using System.Text;
using Ble.Interfaces;
using Garmin.Ble.Lib.Messages;
using GDI.Proto.ConnectIQHTTP;
using GDI.Proto.Core;
using GDI.Proto.DataTransfer;
using GDI.Proto.DataTypes;
using GDI.Proto.DeviceStatus;
using GDI.Proto.FindMyWatch;
using GDI.Proto.Smart;
using Google.Protobuf;
using NeoSmart.AsyncLock;

namespace Garmin.Ble.Lib.Devices.Garmin;

public class Swim2Device : ICommunicator
{
    private readonly ILogger _log;
    private readonly GfdiPacketParser _gfdiPacketParser;

    public Swim2Device(ILogger log)
    {
        _log = log;
        _gfdiPacketParser = new GfdiPacketParser(log);
        _fileUploadQueue = new FileUploadQueue(this, log);
        _fileDownloadQueue = new FileDownloadQueue(this, _log);
    }

    public void UploadFile(int dataType, int subType, int fileIdentifier, String targetPath, Stream data)
    {
        _fileUploadQueue.queueCreateFile((int)data.Length, dataType, subType, fileIdentifier, targetPath, data);
    }

    public async Task OnValueIn(IGattCharacteristic sender, IGattCharacteristicValueEventArgs e)
    {
        var data = e.Value;
        if (data.Length == 0)
        {
            _log.Debug("No data received on change of characteristic {0}", await sender.GetUUIDAsync());
            // return true;
        }

        if (data.Length >= 2 && data[0] == 0 && data[1] == 1)
            ProcessRegisterServiceResponse(data);
        else if (data.Length >= 2 && data[0] == 0 && data[1] == 6)
        {
            await SendRawMessage(new RegisterForServiceRequest(ServiceType.GFDI).Packet);
//            SendRawMessage(new RegisterForServiceRequest(ServiceType.KEEP_ALIVE).packet);
        }
        else
        {
            var channelId = BinaryUtils.ReadByte(data, 0);
            var messageFromService = _channelIdToService.ContainsKey(channelId) ? _channelIdToService[channelId] : 0;
            var messageData = new Span<Byte>(data, 1, data.Length - 1).ToArray();
            switch (messageFromService)
            {
                // case ServiceType.REAL_TIME_HR:
                //     processRealtimeHeartRate(messageData);
                //     break;
                // case ServiceType.REAL_TIME_STEPS:
                //     processRealtimeSteps(messageData);
                //     break;
                // case ServiceType.REAL_TIME_CALORIES:
                //     processRealtimeCalories(messageData);
                //     break;
                // case ServiceType.REAL_TIME_FLOORS:
                //     processRealtimeStairs(messageData);
                //     break;
                // case ServiceType.REAL_TIME_INTENSITY:
                //     processRealtimeIntensityMinutes(messageData);
                //     break;
                // case ServiceType.REAL_TIME_HRV:
                //     handleRealtimeHeartbeat(messageData);
                //     break;
                case ServiceType.GFDI:
                    await ProcessGfdiBytes(messageData);
                    break;
                default:
                    _log.Debug("Unknown data received: {0}", await sender.GetUUIDAsync(), BitConverter.ToString(data));
                    break;
            }
        }

        // return true;
    }

    private async Task ProcessGfdiBytes(byte[] data)
    {
        try
        {

            _gfdiPacketParser.ReceivedBytes(new ReadOnlySpan<byte>(data));
            _log.DebugInZone(DebugZones.BytesBle, "Received {0} GFDI bytes: {1}", data.Length, BitConverter.ToString(data.ToArray()));
            while (_gfdiPacketParser.RetrievePacket() is { } packet)
            {
                _log.DebugInZone(DebugZones.Package, "Processing a {0}B GFDI packet {1}", packet.Length, BitConverter.ToString(packet));
                await ProcessGfdiPacket(packet);
            }
        }
        catch (InvalidCastException e)
        {
            _log.Error("Decode error while processing GFDI Packet.");
            PostGfdiMessage(new ResponseMessage(GarminConstants.MESSAGE_NA, GarminConstants.STATUS_DECODE_ERROR));
        }
    }

    private async Task ProcessGfdiPacket(byte[] packet)
    {
        var size = BinaryUtils.ReadShort(packet, 0);
        if (size != packet.Length)
        {
            _log.Error("Received GFDI packet with invalid length: {0} vs {1}", size, packet.Length);
            PostGfdiMessage(new ResponseMessage(GarminConstants.MESSAGE_NA, GarminConstants.STATUS_LENGTH_ERROR));

            return;
        }

        var crc = BinaryUtils.ReadShort(packet, packet.Length - 2);
        var correctCrc = Crc.Calc(packet, 0, packet.Length - 2);
        if (crc != correctCrc)
        {
            _log.Error("Received GFDI packet with invalid CRC: {0} vs {1}", crc, correctCrc);
            PostGfdiMessage(new ResponseMessage(GarminConstants.MESSAGE_NA, GarminConstants.STATUS_LENGTH_ERROR));
            return;
        }

        var messageType = BinaryUtils.ReadShort(packet, 2);
        switch (messageType)
        {
            case GarminConstants.MESSAGE_RESPONSE:
                ProcessResponseMessage(ResponseMessage.ParsePacket(packet), packet);
                break;

            case GarminConstants.MESSAGE_FILE_TRANSFER_DATA:
                _fileDownloadQueue.OnFileTransferData(FileTransferDataMessage.parsePacket(packet));
                break;

            case GarminConstants.MESSAGE_DEVICE_INFORMATION:
                ProcessDeviceInformationMessage(DeviceInformationMessage.ParsePacket(packet));
                break;

            // case GarminConstants.MESSAGE_WEATHER_REQUEST:
            //     processWeatherRequest(WeatherRequestMessage.parsePacket(packet));
            //     break;
            //
            // case GarminConstants.MESSAGE_MUSIC_CONTROL_CAPABILITIES:
            //     processMusicControlCapabilities(MusicControlCapabilitiesMessage.parsePacket(packet));
            //     break;
            //
            // case GarminConstants.MESSAGE_CURRENT_TIME_REQUEST:
            //     processCurrentTimeRequest(CurrentTimeRequestMessage.parsePacket(packet));
            //     break;
            //
            case GarminConstants.MESSAGE_SYNC_REQUEST: 
                ProcessSyncRequest(SyncRequestMessage.parsePacket(packet));
                break;
            //
            // case GarminConstants.MESSAGE_FIND_MY_PHONE:
            //     processFindMyPhoneRequest(FindMyPhoneRequestMessage.parsePacket(packet));
            //     break;
            //
            // case GarminConstants.MESSAGE_CANCEL_FIND_MY_PHONE:
            //     processCancelFindMyPhoneRequest();
            //     break;
            //
            // case GarminConstants.MESSAGE_NOTIFICATION_SERVICE_SUBSCRIPTION:
            //     processNotificationServiceSubscription(NotificationServiceSubscriptionMessage.parsePacket(packet));
            //     break;
            //
            // case GarminConstants.MESSAGE_GNCS_CONTROL_POINT_REQUEST:
            //     processGncsControlPointRequest(GncsControlPointMessage.parsePacket(packet));
            //     break;

            case GarminConstants.MESSAGE_CONFIGURATION:
                ProcessConfigurationMessage(ConfigurationMessage.parsePacket(packet));
                break;

            case GarminConstants.MESSAGE_PROTOBUF_REQUEST:
                await ProcessProtobufRequest(ProtobufMessageBase.parsePacket<ProtobufRequestMessage>(packet));
                break;

            default:
                _log.Info("Unknown message type {0}: {1}", messageType, BitConverter.ToString(packet.ToArray()));
                break;
        }
    }
    private void ProcessSyncRequest(SyncRequestMessage requestMessage) {
        var filetypesSb = new StringBuilder();
        foreach (var ft in requestMessage.fileTypes)
        {
            if (filetypesSb.Length > 0) filetypesSb.Append(", ");
            filetypesSb.Append(ft);
        }
        _log.Info("Processing sync request message: option={0}, types: {1}", requestMessage.option, filetypesSb);
            PostGfdiMessage(new ResponseMessage(GarminConstants.MESSAGE_SYNC_REQUEST, GarminConstants.STATUS_ACK));
        // if (requestMessage.option != SyncRequestMessage.OPTION_INVISIBLE) {
        //     doSync();
        // }
    }

    private async Task ProcessProtobufRequest(ProtobufRequestMessage requestMessage)
    {
        _log.DebugInZone(DebugZones.Protobuf, "Received protobuf request #{0}, {1}B@{2}/{3}: {4}", requestMessage.RequestId,
            requestMessage.ProtobufDataLength, requestMessage.DataOffset, requestMessage.TotalProtobufLength,
            BitConverter.ToString(requestMessage.MessageBytes));

        // if (_fileDownloadQueue.InProgress)
        // {
        //     _log.Info($"Downloading is in Progress. Sending  NAK to {requestMessage.MessageId}");
        //    PostGfdiMessage(new ResponseMessage(GarminConstants.MESSAGE_PROTOBUF_REQUEST, GarminConstants.STATUS_NAK));
        //    return; 
        // }

        Smart smart;
        try
        {
            smart = Smart.Parser.ParseFrom(requestMessage.MessageBytes);
            _log.Info($"Got a protobuf message: {smart}");
        }
        catch (InvalidProtocolBufferException e)
        {
            _log.Error("Failed to parse protobuf message ({0}): {1}", e,
                BitConverter.ToString(requestMessage.MessageBytes));
            _log.Info($"Sending DECODE_ERROR to {requestMessage.MessageId}");
            PostGfdiMessage(new ResponseMessage(GarminConstants.MESSAGE_PROTOBUF_REQUEST, GarminConstants.STATUS_DECODE_ERROR));
            return;
        }

        var processed = false;
        if (smart.DeviceStatusService != null)
        {
            ProcessProtobufDeviceStatusResponse(smart.DeviceStatusService);
            processed = true;
        }

        if (smart.FindMyWatchService != null)
        {
            ProcessProtobufFindMyWatchResponse(smart.FindMyWatchService);
            processed = true;
        }

        if (smart.CoreService != null)
        {
            processed = ProcessProtobufCoreRequest(smart.CoreService);
        }

        if (smart.ConnectIqHttpService != null)
        {
            processed = await ProcessConnectIqHttpServiceRequest(smart.ConnectIqHttpService, requestMessage.RequestId);
        }

        if (smart.DataTransferService != null)
        {
            processed = ProcessDataTransferRequest(smart.DataTransferService, requestMessage.RequestId);
        }

        if (processed)
        {
            _log.Info($"Sending ACK to {requestMessage.MessageId}");
            PostGfdiMessage( new ResponseMessage(GarminConstants.MESSAGE_PROTOBUF_REQUEST, GarminConstants.STATUS_ACK));
        }
        else
        {
            _log.Warn("Unknown protobuf request: {0}", smart);
            _log.Info($"Sending NAK to {requestMessage.MessageId}");
            PostGfdiMessage(new ResponseMessage(GarminConstants.MESSAGE_PROTOBUF_REQUEST, GarminConstants.STATUS_NAK));
        }
    }

    private bool ProcessDataTransferRequest(DataTransferService smartDataTransferService, int requestId)
    {
        if (smartDataTransferService.DataDownloadRequest != null)
        {
            return  ProcessDataDownloadRequest(smartDataTransferService.DataDownloadRequest, requestId);
        }

        return false;
    }

    private bool ProcessDataDownloadRequest(DataDownloadRequest dataDownloadRequest, int requestId)
    {
        _chunkCount++;
        var chunkSize = _lastOffset == dataDownloadRequest.Offset 
                ? Math.Min(100, dataDownloadRequest.MaxChunkSize/2) 
                : Math.Min(220, dataDownloadRequest.MaxChunkSize);

        _lastOffset = dataDownloadRequest.Offset;
        
        var chunk = _xferQueue.GetChunk(dataDownloadRequest.Id, dataDownloadRequest.Offset, chunkSize);
        var smart = new Smart
        {
            DataTransferService = new DataTransferService()
            {
                DataDownloadResponse = new DataDownloadResponse
                {
                    Id = dataDownloadRequest.Id,
                    Payload = ByteString.CopyFrom(chunk),
                    Offset = dataDownloadRequest.Offset,
                    Status = DataDownloadResponse.Types.Status.Success
                }
            }
        };
        _log.Info($"Sending protobuf message #{requestId}: ChunkSize = {chunkSize} {smart}");
        if (_xferQueue.IsLastChunk(dataDownloadRequest.Id, dataDownloadRequest.Offset, chunkSize))
            _log.Info($"Last Chunk #{_chunkCount} was send. Failures count: {_sendFailuresCount}");
        SendProtobufResponseMessage(smart.ToByteArray(), requestId);
        return true;
    }

    private async Task<bool> ProcessConnectIqHttpServiceRequest(ConnectIQHTTPService message, int messageId)
    {
        if (message.RawResourceRequest != null)
            return await ProcessRawResourceRequest(message.RawResourceRequest, messageId);
        return false;
    }

    private async Task<bool> ProcessRawResourceRequest(RawResourceRequest message, int messageId)
    {
        switch (message.Url)
        {
            case "https://api.gcs.garmin.com/ephemeris/cpe/sony?coverage=WEEKS_1":
                return await UpdateAGps(messageId);
            case "https://services.garmin.com/api/oauth/token":
            case "https://services.garmin.com/oauthTokenExchangeService/connectToIT":
                SendFakeOauthResponse(messageId);
                return true;
            default:
                return false;
        }
    }

    private void SendFakeOauthResponse(int messageId)
    {
        _log.Info($"Sending Fake OAuth...");
        const string fakeOauth =
            """{"access_token":"t","token_type":"Bearer","expires_in":7776000,"scope":"GCS_EPHEMERIS_SONY_READ","refresh_token":"r","refresh_token_expires_in":"31536000","customerId":"c"}""";

        var smart = new Smart
        {
            ConnectIqHttpService = new ConnectIQHTTPService()
            {
                RawResourceResponse = new RawResourceResponse()
                {
                    Status = RawResourceResponse.Types.ResponseStatus.Ok,
                    HttpStatusCode = 200,
                    ResourceData = ByteString.CopyFromUtf8(fakeOauth)
                }
            }
        };
        _log.DebugInZone(DebugZones.Protobuf, $"Sending protobuf message: {smart}");
        SendProtobufResponseMessage(smart.ToByteArray(), messageId);

    }

    private int _lastProtobufRequestId;

    private void SendProtobufMessage(byte[] protobufMessage, int responseToId = 0)
    {
        int GetNextProtobufRequestId()
        {
            _lastProtobufRequestId = (_lastProtobufRequestId + 1) % 65536;
            return _lastProtobufRequestId;
        }

        var id = responseToId > 0 ? responseToId : GetNextProtobufRequestId();
        _log.DebugInZone(DebugZones.Protobuf, "Sending {0}B protobuf message #{1}: {2}", protobufMessage.Length, id,
            BitConverter.ToString(protobufMessage));
        PostGfdiMessage(new ProtobufRequestMessage(id, 0, protobufMessage.Length, protobufMessage.Length,
            protobufMessage).BuildPacket());
    }

    public void SendProtobufResponseMessage(byte[] protobufMessage, int responseToId = 0)
    {
        _log.DebugInZone(DebugZones.Protobuf, "Sending {0}B protobuf Response message #{1}: {2}", protobufMessage.Length,
            responseToId,
            BitConverter.ToString(protobufMessage));
        PostGfdiMessage(new ProtobufResponseMessage(responseToId, 0, protobufMessage.Length,
            protobufMessage.Length,
            protobufMessage).BuildPacket());
    }

    private async Task<bool> UpdateAGps(int messageId)
    {
        _log.Info("Updating AGPS...");
        if (!await DownloadAGpsData()) return false;

        var streamToTransfer = File.OpenRead(AGpsFileName);

        var smart = new Smart
        {
            ConnectIqHttpService = new ConnectIQHTTPService
            {
                RawResourceResponse = new RawResourceResponse
                {
                    Status = RawResourceResponse.Types.ResponseStatus.Ok,
                    HttpStatusCode = 200,
                    XferData = new DataTransferItem
                    {
                        Id = _xferQueue.Add(streamToTransfer),
                        Size = (uint)streamToTransfer.Length
                    }
                }
            }
        };
        _log.Info($"Sending protobuf message: {smart}");
        SendProtobufResponseMessage(smart.ToByteArray(), messageId);
        return true;
    }

    const string AGpsFileName = "downloads/cpe.bin";

    private readonly AsyncLock _downloadAGpsDataLocker = new();

    public async Task<bool> DownloadAGpsData()
    {
        using (await _downloadAGpsDataLocker.LockAsync())
        {
            Directory.CreateDirectory("downloads");

            if (File.GetLastWriteTime(AGpsFileName) < DateTime.Today)
            {
                _log.Info("Downloading AGPS file.");
                var client = new HttpClient();
                client.BaseAddress = new Uri(@"https://api.gcs.garmin.com/ephemeris/cpe/");
                try
                {
                    await using var httpStream = await client.GetStreamAsync("sony?coverage=WEEKS_1");
                    await using var fileStream = File.OpenWrite(AGpsFileName);
                    await httpStream.CopyToAsync(fileStream);
                    await httpStream.DisposeAsync();
                }
                catch (Exception e)
                {
                    _log.Error("Download AGps data is faulted... ", e);
                    return false;
                }
            }
            else
            {
                _log.Info("Using cached AGPS file.");
            }

            return true;
        }
    }

    private bool ProcessProtobufCoreRequest(CoreService coreService)
    {
        if (coreService.SyncResponse != null)
        {
            var syncResponse = coreService.SyncResponse;
            _log.Info("Received sync status: {0}", syncResponse.Status);
        }

        _log.Warn("Unknown CoreService response: {0}", coreService);
        return false;
    }

    private void ProcessProtobufDeviceStatusResponse(DeviceStatusService deviceStatusService)
    {
        if (deviceStatusService.RemoteDeviceBatteryStatusResponse != null)
        {
            var batteryStatusResponse = deviceStatusService.RemoteDeviceBatteryStatusResponse;
            var batteryLevel = batteryStatusResponse.CurrentBatteryLevel;
            _log.Info("Received remote battery status {0}: level={1}", batteryStatusResponse.Status, batteryLevel);
            // var batteryEvent = new GBDeviceEventBatteryInfo();
            // batteryEvent.level = (short) batteryLevel;
            // handleGBDeviceEvent(batteryEvent);
            return;
        }

        if (deviceStatusService.ActivityStatusResponse != null)
        {
            var activityStatusResponse = deviceStatusService.ActivityStatusResponse;
            _log.Info("Received activity status: {0}", activityStatusResponse.ActivityStatus);
            return;
        }

        _log.Warn("Unknown DeviceStatusService response: {0}", deviceStatusService);
    }

    private void ProcessProtobufFindMyWatchResponse(FindMyWatchService findMyWatchService)
    {
        if (findMyWatchService.CancelRequest != null)
        {
            _log.Info("Watch search cancelled, watch found");
            // GBApplication.deviceService().onFindDevice(false);
            return;
        }

        if (findMyWatchService.CancelResponse != null || findMyWatchService.FindResponse != null)
        {
            _log.Debug("Received findMyWatch response");
            return;
        }

        _log.Warn("Unknown FindMyWatchService response: {0}", findMyWatchService);
    }


    private void ProcessConfigurationMessage(ConfigurationMessage configurationMessage)
    {
        this._capabilities = GarminCapabilityHelper.setFromBinary(configurationMessage.ConfigurationPayload);

        _log.Info("Received configuration message; capabilities: {0}",
            GarminCapabilityHelper.setToString(_capabilities));

        // prepare and send response
        PostGfdiMessage(
            new ResponseMessage(GarminConstants.MESSAGE_CONFIGURATION, GarminConstants.STATUS_ACK));

        // and report our own configuration/capabilities
        var ourCapabilityFlags = GarminCapabilityHelper.setToBinary(GarminConstants.OUR_CAPABILITIES);
        PostGfdiMessage(new ConfigurationMessage(ourCapabilityFlags));

        // // initialize current time and settings
        SendCurrentTime();
        // sendSettings();
        //
        // // and everything is ready now
        SendSyncReady();
        // requestBatteryStatusUpdate();
        // sendFitDefinitions();
        // sendFitConnectivityMessage();
        // requestSupportedFileTypes();
    }

    private void SendSyncReady()
    {
        PostGfdiMessage(new SystemEventMessage(GarminSystemEventType.SYNC_READY, 0));
    }

    private void SendCurrentTime()
    {
        var settings = new Dictionary<GarminDeviceSetting, object>(3);
        var now = DateTime.Now;

        var garminTimestamp = (int)((DateTimeOffset)now).ToUnixTimeSeconds() - GarminConstants.GARMIN_TIME_EPOCH;
        if (TimeZoneInfo.Local.IsDaylightSavingTime(now))
            garminTimestamp += SecInHour;
        settings.Add(GarminDeviceSetting.CURRENT_TIME, garminTimestamp);

        var timeZoneOffset = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
        settings.Add(GarminDeviceSetting.TIME_ZONE_OFFSET, timeZoneOffset);

        _log.Info("Setting time to {0}, tzOffset={1} (DST={2})",
            garminTimestamp,
            timeZoneOffset,
            TimeZoneInfo.Local.IsDaylightSavingTime(now) ? 1 : 0);
        PostGfdiMessage(new SetDeviceSettingsMessage(settings));
    }

    private const int SecInHour = 3600;

    private void ProcessDeviceInformationMessage(DeviceInformationMessage deviceInformationMessage)
    {
        _log.Info(
            "Received device information: protocol {0}, product {1}, unit {2}, SW {3}, max packet {4}, BT name {5}, device name {6}, device model {7}",
            deviceInformationMessage.ProtocolVersion, deviceInformationMessage.ProductNumber,
            deviceInformationMessage.UnitNumber, deviceInformationMessage.getSoftwareVersionStr(),
            deviceInformationMessage.MaxPacketSize, deviceInformationMessage.BluetoothFriendlyName,
            deviceInformationMessage.DeviceName, deviceInformationMessage.DeviceModel);

        _maxPacketSize = deviceInformationMessage.MaxPacketSize;

        // prepare and send response
        var protocolVersionSupported = deviceInformationMessage.ProtocolVersion / 100 == 1;
        if (!protocolVersionSupported)
        {
            _log.Error($"Unsupported protocol version {deviceInformationMessage.ProtocolVersion}");
        }

        var protocolFlags = protocolVersionSupported ? 1 : 0;
        var deviceInformationResponseMessage = new DeviceInformationResponseMessage(
            GarminConstants.STATUS_ACK, 113, -1, -1, 6336, -1, "MyPhone", "MyPhone", "MyPhone", protocolFlags);

        PostGfdiMessage(deviceInformationResponseMessage);
    }

    private void ProcessResponseMessage(ResponseMessage responseMessage, byte[] packet)
    {
        switch (responseMessage.RequestId)
        {
            case GarminConstants.MESSAGE_DIRECTORY_FILE_FILTER_REQUEST:
                ProcessDirectoryFileFilterResponse(DirectoryFileFilterResponseMessage.parsePacket(packet));
                break;
            case GarminConstants.MESSAGE_DOWNLOAD_REQUEST:
                _fileDownloadQueue.OnDownloadRequestResponse(DownloadRequestResponseMessage.parsePacket(packet));
                break;
            case GarminConstants.MESSAGE_UPLOAD_REQUEST:
                _fileUploadQueue.onUploadRequestResponse(UploadRequestResponseMessage.parsePacket(packet));
                break;
            case GarminConstants.MESSAGE_FILE_TRANSFER_DATA:
                _fileUploadQueue.onFileTransferResponse(FileTransferDataResponseMessage.parsePacket(packet));
                break;
            case GarminConstants.MESSAGE_CREATE_FILE_REQUEST:
                _fileUploadQueue.onCreateFileRequestResponse(CreateFileResponseMessage.parsePacket(packet));
                break;
            case GarminConstants.MESSAGE_FIT_DEFINITION:
                ProcessFitDefinitionResponse(FitDefinitionResponseMessage.parsePacket(packet));
                break;
            // case GarminConstants.MESSAGE_FIT_DATA:
            //     processFitDataResponse(FitDataResponseMessage.parsePacket(packet));
            //     break;
            case GarminConstants.MESSAGE_PROTOBUF_REQUEST:
                ProcessProtobufRequestResponse(ProtobufRequestResponseMessage.parsePacket(packet));
                break;
            case GarminConstants.MESSAGE_PROTOBUF_RESPONSE:
                ProcessProtobufResponseResponse(responseMessage);
                break;

            // case GarminConstants.MESSAGE_DEVICE_SETTINGS:
            //     processDeviceSettingsResponse(SetDeviceSettingsResponseMessage.parsePacket(packet));
            //     break;
            // case GarminConstants.MESSAGE_SYSTEM_EVENT:
            //     processSystemEventResponse(SystemEventResponseMessage.parsePacket(packet));
            //     break;
            // case GarminConstants.MESSAGE_SUPPORTED_FILE_TYPES_REQUEST:
            //     processSupportedFileTypesResponse(SupportedFileTypesResponseMessage.parsePacket(packet));
            //     break;
            // case GarminConstants.MESSAGE_GNCS_DATA_SOURCE:
            //     gncsDataSourceQueue.responseReceived(GncsDataSourceResponseMessage.parsePacket(packet));
            //     break;
            // case GarminConstants.MESSAGE_AUTH_NEGOTIATION:
            //     processAuthNegotiationRequestResponse(AuthNegotiationResponseMessage.parsePacket(packet));
            default:
                _log.Info("Received response to message {0}: {1}", responseMessage.RequestId,
                    responseMessage.getStatusStr());
                break;
        }
    }

    private void ProcessProtobufResponseResponse(ResponseMessage responseMessage)
    {
        _log.Info(
            $"Received response to ProtobufResponse {responseMessage.RequestId}: {responseMessage.getStatusStr()} ");

        if (responseMessage.Status == GarminConstants.STATUS_LENGTH_ERROR)
        {
            _sendFailuresCount++;
        }
    }

    private void ProcessProtobufRequestResponse(ProtobufRequestResponseMessage responseMessage)
    {
        _log.Info("Received response to protobuf message #{0}@{1}:  status={2}, error={3}", responseMessage.requestId,
            responseMessage.dataOffset, responseMessage.protobufStatus, responseMessage.error);
    }


    private void ProcessDirectoryFileFilterResponse(DirectoryFileFilterResponseMessage responseMessage)
    {
        if (responseMessage.status == GarminConstants.STATUS_ACK && responseMessage.response ==
            DirectoryFileFilterResponseMessage.RESPONSE_DIRECTORY_FILTER_APPLIED)
        {
            _log.Info(
                "Received response to directory file filter request: {0}/{1}, requesting download of directory data",
                responseMessage.status, responseMessage.response);
             DownloadDirectoryData();
        }
        else
        {
            _log.Error("Directory file filter request failed: {0}/{1}", responseMessage.status,
                responseMessage.response);
        }
    }

    private void DownloadDirectoryData()
    {
        PostGfdiMessage(new DownloadRequestMessage(0, 0, DownloadRequestMessage.REQUEST_NEW_TRANSFER, 0, 0));
    }

    private void ProcessFitDefinitionResponse(FitDefinitionResponseMessage responseMessage)
    {
        _log.Info("Received response to FIT definition message: status={0}, FIT response={1}",
            responseMessage.status,
            responseMessage.fitResponse);
    }

    public void PostGfdiMessage(byte[] messageBytes)
    {
        _gfdiCommunicator.PostGfdiMessage(messageBytes);
    }

    public void PostGfdiMessage(IGfdiMessage message)
    {
        _gfdiCommunicator.PostGfdiMessage(message);
    }

    private async Task SendRawMessage(byte[] messageBytes)
    {
        await _outCharacteristic.WriteValueAsync(messageBytes, new Dictionary<string, object>());
    }

    readonly Dictionary<ServiceType, byte> _serviceToChannelId =
        new();

    readonly Dictionary<byte, ServiceType> _channelIdToService =
        new();

    private int _maxPacketSize;
    private IGattCharacteristic _outCharacteristic;
    private HashSet<GarminCapability> _capabilities;
    private readonly FileUploadQueue _fileUploadQueue;
    private readonly XferQueue _xferQueue = new();
    private readonly FileDownloadQueue _fileDownloadQueue;

    private uint _lastOffset = int.MaxValue;
    private int _sendFailuresCount = 0;
    private int _chunkCount = 0;
    private  GfdiCommunicator _gfdiCommunicator;

    public async Task Init(IBleDevice bleDevice)
    {
        var multiChannelService = await bleDevice.GetServiceAsync(GarminConstants.UUID_SERVICE_GARMIN_3.ToString());
        var inCharacteristic = await multiChannelService.GetCharacteristicAsync(GarminConstants.UUID_CHARACTERISTIC_GARMINSWIM2_GFDI_RECEIVE.ToString());
        inCharacteristic.Value += OnValueIn;
        var outCharacteristic = await multiChannelService.GetCharacteristicAsync(GarminConstants.UUID_CHARACTERISTIC_GARMINSWIM2_GFDI_SEND.ToString());
        _outCharacteristic = outCharacteristic;
        _log.Debug("Send InitConnection Message");
        await SendRawMessage(new InitConnectionMessage().Packet);
    }

    private void ProcessRegisterServiceResponse(byte[] data)
    {
        if (data.Length >= 14)
        {
            // long clientId = BinaryUtils.ReadLong(data, 2);
            var serviceTypeId = BinaryUtils.ReadShort(data, 10);
            // int shouldBeZero = BinaryUtils.ReadByte(data, 12);
            var channelId = BinaryUtils.ReadByte(data, 13);
            var serviceType = (ServiceType)serviceTypeId;
            if (!_serviceToChannelId.ContainsKey(serviceType))
                _serviceToChannelId.Add(serviceType, channelId);

            if (!_channelIdToService.ContainsKey(channelId))
                _channelIdToService.Add(channelId, serviceType);

            if (serviceTypeId == (int)ServiceType.GFDI)
            {
                _gfdiCommunicator = new GfdiCommunicator(channelId, _log, _outCharacteristic);
            }

            _log.Info("Service " + serviceType + " Added on Channel " + channelId);
        }
        else
            _log.Debug("handleRegisterServiceResponse: wrong data length.");
    }

    public void DownloadFile(int index, string filename)
    {
        _fileDownloadQueue.DownloadFile(index, filename);
    }

    public void DownloadGarminXml()
    {
        _log.Info("Requesting Download Garmin.xml");
        _fileDownloadQueue.DownloadFile(GarminConstants.GARMIN_DEVICE_XML_FILE_INDEX, "Garmin.xml");
    }

    private void ListFiles(int filterType)
    {
        _fileDownloadQueue.ListFiles(filterType);
    }

    public void DownloadAllActivities()
    {
        _fileDownloadQueue.ListFiles(DirectoryFileFilterRequestMessage.FILTER_NO_FILTER);
    }
}

internal class XferQueue
{
    private uint _nextId;
    private readonly Dictionary<uint, BinaryReader> _dict = new();

    public uint Add(Stream stream)
    {
        _dict.Add(_nextId, new BinaryReader(stream));
        return _nextId++;
    }

    public byte[] GetChunk(uint id, uint offset, uint maxChunkSize)
    {
        var reader = _dict[id];
        reader.BaseStream.Position = offset;
        return reader.ReadBytes((int)maxChunkSize);
    }

    public bool IsLastChunk(uint id, uint offset, uint chunkSize)
    {
        return _dict[id].BaseStream.Length <= offset + chunkSize;
    }
}

public interface ICommunicator
{
    void PostGfdiMessage(byte[] messageBytes);
    void PostGfdiMessage(IGfdiMessage message);
}
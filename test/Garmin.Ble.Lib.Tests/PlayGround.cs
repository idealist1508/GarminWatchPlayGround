using System;
using System.IO;
using System.Threading.Tasks;
using Garmin.Ble.Lib.Messages;
using GDI.Proto.ConnectIQHTTP;
using GDI.Proto.DataTransfer;
using GDI.Proto.DataTypes;
using GDI.Proto.Smart;
using Google.Protobuf;
using Xunit.Abstractions;

namespace Garmin.Ble.Lib.Tests;

using Xunit;

public class PlayGround
{
    private readonly ITestOutputHelper _testOutputHelper;
    private GfdiPacketParser _gfdiPacketParser;
    private ILogger _log;

    public PlayGround(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private void ProcessGfdiBytes(Span<byte> data)
    {
        _gfdiPacketParser.ReceivedBytes(data);
        _log.DebugInZone("bytes", "Received {0} GFDI bytes: {1}", data.Length, BitConverter.ToString(data.ToArray()));
        byte[]? packet;
        while ((packet = _gfdiPacketParser.RetrievePacket()) != null)
        {
            _log.DebugInZone("bytes", "Processing a {0}B GFDI packet {1}", packet.Length,
                BitConverter.ToString(packet));
            processGfdiPacket(packet);
        }
    }

    private void processGfdiPacket(Span<Byte> packet)
    {
        var size = BinaryUtils.ReadShort(packet, 0);
        if (size != packet.Length)
        {
            _log.Error("Received GFDI packet with invalid length: {0} vs {1}", size, packet.Length);
            // return;
        }

        var crc = BinaryUtils.ReadShort(packet, packet.Length - 2);
        var correctCrc = Crc.Calc(packet, 0, packet.Length - 2);
        if (crc != correctCrc)
        {
            _log.Error("Received GFDI packet with invalid CRC: {0} vs {1}", crc, correctCrc);
            // return;
        }

        var messageType = BinaryUtils.ReadShort(packet, 2);
        switch (messageType)
        {
            case GarminConstants.MESSAGE_RESPONSE:
                _log.Info($"{ResponseMessage.ParsePacket(packet)}");
                break;

            case GarminConstants.MESSAGE_FILE_TRANSFER_DATA:
                _log.Info($"{FileTransferDataMessage.parsePacket(packet)}");
                break;

            case GarminConstants.MESSAGE_DEVICE_INFORMATION:
                _log.Info($"{DeviceInformationMessage.ParsePacket(packet)}");
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
            // case GarminConstants.MESSAGE_SYNC_REQUEST:
            //     processSyncRequest(SyncRequestMessage.parsePacket(packet));
            //     break;
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
                _log.Info($"{ConfigurationMessage.parsePacket(packet)}");
                break;

            case GarminConstants.MESSAGE_PROTOBUF_REQUEST:
                var protobufRequestMessage = ProtobufMessageBase.parsePacket<ProtobufRequestMessage>(packet);
                _log.Info(protobufRequestMessage.ToString());
                _pbmessage.Write(protobufRequestMessage.MessageBytes);
                break;
            case GarminConstants.MESSAGE_PROTOBUF_RESPONSE:
                var protobufResponseMessage = ProtobufMessageBase.parsePacket<ProtobufResponseMessage>(packet);
                _log.Info(protobufResponseMessage.ToString());
                _pbmessage.Write(protobufResponseMessage.MessageBytes);

                break;

            default:
                _log.Info("Unknown message type {0}: {1}", messageType, BitConverter.ToString(packet.ToArray()));
                break;
        }
    }

    readonly MemoryStream _pbmessage = new();

    [Fact]
    public void str()
    {
        var s = new String[]
        {
            "\n\u0016GDIConnectIQHTTP.proto\u0012\u0017GDI.Proto.ConnectIQHTTP\u001a\u0012GDIDataTypes.proto\"¹\u0007\n\u0014ConnectIQHTTPService\u0012N\n\u0017connect_iq_http_request\u0018\u0001 \u0001(\u000b2-.GDI.Proto.ConnectIQHTTP.ConnectIQHTTPRequest\u0012P\n\u0018connect_iq_http_response\u0018\u0002 \u0001(\u000b2..GDI.Proto.ConnectIQHTTP.ConnectIQHTTPResponse\u0012P\n\u0018connect_iq_image_request\u0018\u0003 \u0001(\u000b2..GDI.Proto.ConnectIQHTTP.ConnectIQImageRequest\u0012R\n\u0019connect_iq_image_response\u0018\u0004 \u0001(\u000b2/.GDI.Proto.ConnectIQHTTP.ConnectIQImageResponse\u0012I\n\u0014raw_resource_request\u0018\u0005 \u0001(\u000b2+.GDI.Proto.ConnectIQHTTP.RawResourceRequest\u0012K\n\u0015raw_resource_response\u0018\u0006 \u0001(\u000b2,.GDI.Proto.ConnectIQHTTP.RawResourceResponse\u0012P\n\u0018connect_iq_oauth_request\u0018\u0007 \u0001(\u000b2..GDI.Proto.ConnectIQHTTP.ConnectIQOAuthRequest\u0012R\n\u0019connect_iq_oauth_response\u0018\b \u0001(\u000b2/.GDI.Proto.ConnectIQHTTP.ConnectIQOAuthResponse\u0012a\n!connect_iq_oauth_complete_request\u0018\t \u0001(\u000b26.GDI.Proto.ConnectIQHTTP.ConnectIQOAuthCompleteRequest\u0012c\n\"connect_iq_oauth_complete_response\u0018\n \u0001(\u000b27.GDI.Proto.ConnectIQHTTP.ConnectIQOAuthCompleteResponse\u0012S\n\u0019open_webpage_notification\u0018\u000b \u0001(\u000b20.GDI.Proto.ConnectIQHTTP.OpenWebpageNotification\"\u0086\u0005\n\u0014ConnectIQHTTPRequest\u0012\u000b\n\u0003url\u0018\u0001 \u0002(\t\u0012M\n\u000bhttp_method\u0018\u0002 \u0001(\u000e28.GDI.Proto.ConnectIQHTTP.ConnectIQHTTPRequest.HTTPMethod\u0012\u001a\n\u0012http_header_fields\u0018\u0003 \u0001(\f\u0012\u0011\n\thttp_body\u0018\u0004 \u0001(\f\u0012\u001b\n\u0013max_response_length\u0018\u0005 \u0001(\r\u00124\n&include_http_header_fields_in_response\u0018\u0006 \u0001(\b:\u0004true\u0012%\n\u0016compress_response_body\u0018\u0007 \u0001(\b:\u0005false\u0012Q\n\rresponse_type\u0018\b \u0001(\u000e2:.GDI.Proto.ConnectIQHTTP.ConnectIQHTTPRequest.ResponseType\u0012Q\n\u0007version\u0018\t \u0001(\u000e25.GDI.Proto.ConnectIQHTTP.ConnectIQHTTPRequest.Version:\tVERSION_1\"V\n\nHTTPMethod\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0007\n\u0003GET\u0010\u0001\u0012\u0007\n\u0003PUT\u0010\u0002\u0012\b\n\u0004POST\u0010\u0003\u0012\n\n\u0006DELETE\u0010\u0004\u0012\t\n\u0005PATCH\u0010\u0005\u0012\b\n\u0004HEAD\u0010\u0006\"B\n\fResponseType\u0012\b\n\u0004JSON\u0010\u0000\u0012\u000f\n\u000bURL_ENCODED\u0010\u0001\u0012\u000e\n\nPLAIN_TEXT\u0010\u0002\u0012\u0007\n\u0003XML\u0010\u0003\"'\n\u0007Version\u0012\r\n\tVERSION_1\u0010\u0000\u0012\r\n\tVERSION_2\u0010\u0001\"î\u0004\n\u0015ConnectIQHTTPResponse\u0012M\n\u0006status\u0018\u0001 \u0001(\u000e2=.GDI.Proto.ConnectIQHTTP.ConnectIQHTTPResponse.ResponseStatus\u0012\u0018\n\u0010http_status_code\u0018\u0002 \u0001(\u0005\u0012\u0011\n\thttp_body\u0018\u0003 \u0001(\f\u0012\u001a\n\u0012http_header_fields\u0018\u0004 \u0001(\f\u0012\u0015\n\rinflated_size\u0018\u0005 \u0001(\r\u0012Q\n\rresponse_type\u0018\u0006 \u0001(\u000e2:.GDI.Proto.ConnectIQHTTP.ConnectIQHTTPRequest.ResponseType\"Ò\u0002\n\u000eResponseStatus\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0006\n\u0002OK\u0010d\u0012*\n%INVALID_HTTP_HEADER_FIELDS_IN_REQUEST\u0010È\u0001\u0012!\n\u001cINVALID_HTTP_BODY_IN_REQUEST\u0010É\u0001\u0012#\n\u001eINVALID_HTTP_METHOD_IN_REQUEST\u0010Ê\u0001\u0012\u001e\n\u0019NETWORK_REQUEST_TIMED_OUT\u0010¬\u0002\u0012*\n%INVALID_HTTP_BODY_IN_NETWORK_RESPONSE\u0010\u0090\u0003\u00123\n.INVALID_HTTP_HEADER_FIELDS_IN_NETWORK_RESPONSE\u0010\u0091\u0003\u0012\u001f\n\u001aNETWORK_RESPONSE_TOO_LARGE\u0010\u0092\u0003\u0012\u0015\n\u0010INSECURE_REQUEST\u0010é\u0007\"û\u0001\n\u0015ConnectIQImageRequest\u0012\u000b\n\u0003url\u0018\u0001 \u0002(\t\u0012\u0012\n\npartNumber\u0018\u0002 \u0002(\r\u0012\u0011\n\tmax_width\u0018\u0003 \u0001(\r\u0012\u0012\n\nmax_height\u0018\u0004 \u0001(\r\u0012\u0010\n\bmax_size\u0018\u0005 \u0001(\r\u0012\u000f\n\u0007palette\u0018\u0006 \u0001(\f\u0012K\n\tdithering\u0018\u0007 \u0001(\u000e28.GDI.Proto.ConnectIQHTTP.ConnectIQImageRequest.Dithering\"*\n\tDithering\u0012\b\n\u0004NONE\u0010\u0000\u0012\u0013\n\u000fFLOYD_STEINBERG\u0010\u0001\"©\u0002\n\u0016ConnectIQImageResponse\u0012N\n\u0006status\u0018\u0001 \u0001(\u000e2>.GDI.Proto.ConnectIQHTTP.ConnectIQImageResponse.ResponseStatus\u0012\u0018\n\u0010http_status_code\u0018\u0002 \u0001(\u0005\u0012\u0012\n\nimage_data\u0018\u0003 \u0001(\f\u0012\r\n\u0005width\u0018\u0004 \u0001(\r\u0012\u000e\n\u0006height\u0018\u0005 \u0001(\r\u0012\u0015\n\rinflated_size\u0018\u0006 \u0001(\r\"[\n\u000eResponseStatus\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0006\n\u0002OK\u0010d\u0012\u001e\n\u0019NETWORK_REQUEST_TIMED_OUT\u0010È\u0001\u0012\u0014\n\u000fIMAGE_TOO_LARGE\u0010¬\u0002\"å\u0004\n\u0012RawResourceRequest\u0012\u000b\n\u0003url\u0018\u0001 \u0002(\t\u0012\u0010\n\bmax_size\u0018\u0002 \u0001(\r\u0012K\n\u0006method\u0018\u0003 \u0001(\u000e26.GDI.Proto.ConnectIQHTTP.RawResourceRequest.HTTPMethod:\u0003GET\u0012\f\n\u0004body\u0018\u0004 \u0001(\t\u0012G\n\u0007headers\u0018\u0005 \u0003(\u000b26.GDI.Proto.ConnectIQHTTP.RawResourceRequest.HTTPHeader\u0012\u0015\n\ruse_data_xfer\u0018\u0006 \u0001(\b\u0012\u0010\n\braw_body\u0018\u0007 \u0001(\f\u0012H\n\u0005forms\u0018\b \u0003(\u000b29.GDI.Proto.ConnectIQHTTP.RawResourceRequest.MultipartForm\u001a(\n\nHTTPHeader\u0012\u000b\n\u0003key\u0018\u0001 \u0002(\t\u0012\r\n\u0005value\u0018\u0002 \u0002(\t\u001a\u0096\u0001\n\rMultipartForm\u0012\f\n\u0004name\u0018\u0001 \u0001(\t\u0012\u0010\n\bfilename\u0018\u0002 \u0001(\t\u0012\u0014\n\fcontent_type\u0018\u0003 \u0001(\t\u0012\u0015\n\rresource_data\u0018\u0004 \u0001(\f\u00128\n\txfer_data\u0018\u0005 \u0001(\u000b2%.GDI.Proto.DataTypes.DataTransferItem\"V\n\nHTTPMethod\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0007\n\u0003GET\u0010\u0001\u0012\u0007\n\u0003PUT\u0010\u0002\u0012\b\n\u0004POST\u0010\u0003\u0012\n\n\u0006DELETE\u0010\u0004\u0012\t\n\u0005PATCH\u0010\u0005\u0012\b\n\u0004HEAD\u0010\u0006\"¾\u0003\n\u0013RawResourceResponse\u0012K\n\u0006status\u0018\u0001 \u0001(\u000e2;.GDI.Proto.ConnectIQHTTP.RawResourceResponse.ResponseStatus\u0012\u0018\n\u0010http_status_code\u0018\u0002 \u0001(\u0005\u0012\u0015\n\rresource_data\u0018\u0003 \u0001(\f\u00128\n\txfer_data\u0018\u0004 \u0001(\u000b2%.GDI.Proto.DataTypes.DataTransferItem\u0012H\n\u0007headers\u0018\u0005 \u0003(\u000b27.GDI.Proto.ConnectIQHTTP.RawResourceResponse.HTTPHeader\u001a(\n\nHTTPHeader\u0012\u000b\n\u0003key\u0018\u0001 \u0002(\t\u0012\r\n\u0005value\u0018\u0002 \u0002(\t\"{\n\u000eResponseStatus\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0006\n\u0002OK\u0010d\u0012\u001e\n\u0019NETWORK_REQUEST_TIMED_OUT\u0010È\u0001\u0012\u0013\n\u000eFILE_TOO_LARGE\u0010¬\u0002\u0012\u001f\n\u001aDATA_TRANSFER_ITEM_FAILURE\u0010\u0090\u0003\"ä\u0002\n\u0015ConnectIQOAuthRequest\u0012\u0013\n\u000binitial_url\u0018\u0001 \u0002(\t\u0012\u001e\n\u0016initial_url_parameters\u0018\u0002 \u0001(\f\u0012\u001c\n\u0014parameters_encrypted\u0018\u0003 \u0001(\b\u0012\u001d\n\u0015parameters_compressed\u0018\u0004 \u0001(\b\u0012\u0012\n\nresult_url\u0018\u0005 \u0002(\t\u0012P\n\u000bresult_type\u0018\u0006 \u0001(\u000e2;.GDI.Proto.ConnectIQHTTP.ConnectIQOAuthRequest.ResultFormat\u0012\u0013\n\u000bresult_keys\u0018\u0007 \u0001(\f\u0012\u0010\n\bapp_name\u0018\b \u0001(\t\u0012\u0010\n\bapp_uuid\u0018\t \u0002(\f\u0012\u0012\n\nstore_uuid\u0018\n \u0001(\f\"&\n\fResultFormat\u0012\u0007\n\u0003URL\u0010\u0000\u0012\r\n\tBODY_JSON\u0010\u0001\"º\u0001\n\u0016ConnectIQOAuthResponse\u0012N\n\u0006status\u0018\u0001 \u0002(\u000e2>.GDI.Proto.ConnectIQHTTP.ConnectIQOAuthResponse.ResponseStatus\"P\n\u000eResponseStatus\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0006\n\u0002OK\u0010\u0001\u0012\u000f\n\u000bBAD_REQUEST\u0010\u0002\u0012\u0018\n\u0014NOTIFICATION_FAILURE\u0010\u0003\"\u0095\u0002\n\u001dConnectIQOAuthCompleteRequest\u0012U\n\u0006status\u0018\u0001 \u0002(\u000e2E.GDI.Proto.ConnectIQHTTP.ConnectIQOAuthCompleteRequest.ResponseStatus\u0012\u0018\n\u0010http_status_code\u0018\u0002 \u0001(\u0005\u0012\u0010\n\bapp_uuid\u0018\u0003 \u0002(\f\u0012\f\n\u0004data\u0018\u0004 \u0001(\f\u0012\u0011\n\tencrypted\u0018\u0005 \u0001(\b\u0012\u0012\n\ncompressed\u0018\u0006 \u0001(\b\"<\n\u000eResponseStatus\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0006\n\u0002OK\u0010\u0001\u0012\u0015\n\u0011WEB_REQUEST_ERROR\u0010\u0002\"\u009f\u0001\n\u001eConnectIQOAuthCompleteResponse\u0012V\n\u0006status\u0018\u0001 \u0002(\u000e2F.GDI.Proto.ConnectIQHTTP.ConnectIQOAuthCompleteResponse.ResponseStatus\"%\n\u000eResponseStatus\u0012\u000b\n\u0007UNKNOWN\u0010\u0000\u0012\u0006\n\u0002OK\u0010\u0001\"H\n\u0017OpenWebpageNotification\u0012\u000b\n\u0003url\u0018\u0001 \u0002(\t\u0012\u000e\n\u0006params\u0018\u0002 \u0001(\f\u0012\u0010\n\bapp_name\u0018\u0003 \u0001(\tB5\n\u001acom.garmin.proto.generatedB\u0015GDIConnectIQHTTPProtoH\u0003"
        };
        foreach (var s1 in s)
        {
            _testOutputHelper.WriteLine(s1);
        }
    }

    [Fact]
    public void DecodeWhatISend()
    {
        _log = new XunitConsoleLogger(_testOutputHelper);
        _gfdiPacketParser = new GfdiPacketParser(_log);

        var p1c1 =
            "00-07-81-01-B4-13-90-01-01-01-01-03-6D-01-01-03-6D-01-01-0A-3A-EA-02-12-E7-02-08-01-10-02-18-0A-22-DE-02-2A-12-A0-02-BC-E9-01-04-60-57-57-02-04-02-12-48-63-9F-CA-F8-B1-5E-A3-04-0D-B9-02-02-AC-84-72-F8-E3-0A-2A-04-C9-20-CE-01-A5-D7-23-F8-71-7F-AB-03-CE-46-97-01-74-F6-DE-F7-BA-52-28-03-6E-6B-5E-01-E7-3C-A4-F7-D3-26-A1-02-91-D3-23-01-7D-F8-73-F7-7E-AE-16-02-32-CC-E7-04-B0-58-57-04-FF-FF-3C-2C-63-2A-97-EA-65-E3-30-07-BF-8E-46-05-6D-5C-D6-E9-AB-BA-EA-04-C3-AB-37-04-2C-4B-5E-E9-35-D1-94-02-74-01-1B-03-EC-8C-31-E9-20-CA-36-10-0D-1E-F4-01-A3-C1-51-E9-0C-97-D8-FD-2A-CB-C6-B8-0B-82-BF-E9-EC-5C-82-FB-BF-01-97-FF-A6-52-7A-EA-46-56-3C-F9-72-DC-68-FE-1F-9B-80-EB-79-B3-0E-F7-A4-88-40-FD-62-A2-CF-EC-A8-79-01-F5-61-36-22-FC-A2-8F-63-EE-EC-60-1C-F3-93-07-12-FB-6A-70-37-F0-4E-B2-66-F1-A7-FE-13-FA-D4-43-45-F2-2B-26-E7-EF-FA-EC-2B-F9-F4-0A-86-F4-7F-C3-A3-EE-4B-61-5D-F8-8B-DE-F1-F6-A2-C0-A1-ED-73-96-AB-F7-18-0A-80-F9-2A-66-E5-EC-B5-62-19-F7-51-2C-27-FC-92-F4-71-EC-06-28-A9-F6-EC-5C-DD-FE-9D-8D-49-EC-AE-C5-5C-F6-7F-57-98-01-5F-22-6D-EC-D8-8B-35-F6-F2-AA-4D-04-0A-67-DC-EC-99-31-34-F6-8F-EC-F2-06-9E-CD-95-ED-0D-CE-58-F6-80-58-57-02-01-02-0C-09-CF-22-36-02-9A-23-FC-DA-00";
        ProcessGfdiBytes(BinUtils.hexStringToByteArray(p1c1));
        // ProcessGfdiBytes(hexStringToByteArray(p1c2));
        // ProcessGfdiBytes(hexStringToByteArray(p1c3));
        // ProcessGfdiBytes(hexStringToByteArray(p1c4));
        _pbmessage.Position = 0;
        var s = Smart.Parser.ParseFrom(_pbmessage);
        _testOutputHelper.WriteLine(s.ToString());
    }

    [Fact]
    public void DecodeLogRawResponse()
    {
        _log = new XunitConsoleLogger(_testOutputHelper);
        _gfdiPacketParser = new GfdiPacketParser(_log);

        var p1c1 =
            " 0 6 8c 1 b4 13 38 1 1 1 1 3 13 5 1  3 78 1 1        " +
            " d 12 90 a 32 8d a 8 64 10 c8 1 1a 4 22 6 8 ff 10    " +
            " 80 f0 3 2a 1e a d 63 61 63 68 65 2d 63 6f  6e 74 72 6f        " +
            " 6c 12 d 6d 61 78 2d 61 67 65 3d 31 34 34 30 30 2a 1a a  " +
            "  f 43 46 2d 43 61 63 68 65 2d 53 74 61 74 75 73 12 7 44 " +
            " 59 4e 41 4d 49 43 2a 1e a 6 43 46 2d 52 41 59 12 14 36     " +
            " 64 61 37 31 30 61 36 35 65 65 64 36 39 33 66 2d 46 52 41     " +
            " 2a 18 a a 43 6f 6e 6e 65 63 74 69 6f 6e 12 a 6b 65 65    " +
            " 70 2d 61 6c 69 76 65 2a 17 a e 43 6f 6e 74  65 6e 74 2d       " +
            " 4c 65 6e 67 74 68 12 5 36 33 34 38 38 2a 28 a c 43 6f   " +
            " 6e 74 65 6e 74 2d 54 79 70 65 12 18 61 70 70 6c 69 63 61    " +
            " 74 69 6f 6e 2f 6f 63 74 65 74 2d 73 74 72 65  61 6d 2a 25         " +
            " a 4 44 61 74 65 12 1d 54 75 65 2c 20 30 38 20 46 65 62  " +
            " 20 32 30 32 32 20 31 38 3a 35 34 3a 31 31 20  47 4d 54 2a          " +
            " 2e a 4 65 74 61 67 12 26 22 66 32 34 37 32  66 65 61 2d           " +
            " 37 35 36 32 2d 34 6c 39 32 30 2d 38 38 38 32  2d 39 39 34     " +
            " 65 64 62 66 33 38 37 63 66 22 2a 64 a 9 45 78 70 65 63       " +
            " 74 2d 43 54 12 57 6d 61 78 2d 61 67 65 3d 36 30 34 38 30       " +
            "  30 2c 20 72 65 70 6f 72 74 2d 75 72 69 3d 22   68 74 74 70    " +
            " 73 3a 2f 2f 72 65 70 6f 72 74 2d 75 72 69 2e  63 6c 6f 75      " +
            "   64 66 6c 61 72 65 2e 63 6f 6d 2f 63 64 6e 2d  63 67 4c 6   " +
            " 0 " +
            "  ";
        var p1c2 =
            " 0 6 8c 1 b4 13 38 3 78 1 1 3 13 5 1  3 78 1 1    " +
            " ff 69 2f 62 65 61 63 6f 6e 2f 65 78 70 65 63  74 2d 63 74      " +
            " 22 2a 46 a 3 4e 45 4c 12 3f 7b 22 73 75 63  63 65 73 73      " +
            " 5f 66 72 61 63 74 69 6f 6e 22 3a 30 2e 30 31 2c 22 72 65     " +
            "  70 6f 72 74 5f 74 6f 22 3a 22 63 66 2d 6e 65  6c 22 2c 22    " +
            " 6d 61 78 5f 61 67 65 22 3a 36 30 34 38 30 30  7d 2a 87 2    " +
            " a 9 52 65 70 6f 72 74 2d 54 6f 12 f9 1 7b  22 65 6e 64     " +
            " 70 6f 69 6e 74 73 22 3a 5b 7b 22 75 72 6c 22 3a 22 68 74     " +
            " 74 70 73 3a 5c 2f 5c 2f 61 2e 6e 65 6c 2e 63  6c 6f 75 64     " +
            " 66 6c 61 72 65 2e 63 6f 6d 5c 2f 72 65 70 6f  72 74 5c 2f     " +
            "  76 33 3f 73 3d 76 78 47 54 61 63 4d 67 7a 53   79 45 79 77  " +
            " 77 78 4e 73 61 79 6a 4a 56 53 4d 31 6d 56 53  47 4c 55 4e   " +
            " 4c 35 39 70 25 32 42 74 72 76 77 4d 47 49 7a  4a 6f 73 34     " +
            " 54 6b 31 58 52 69 4d 45 4a 6c 62 61 30 53 34  59 57 56 53    " +
            " 76 70 39 55 4c 6c 63 39 7d 6e 4c 32 66 6b 78  38 37 66 25    " +
            "  32 42 45 6b 63 54 55 65 54 32 59 32 72 6a 65 48 58 4b 41        " +
            " 62 5a 45 6a 4e 44 61 79 59 50 6f 67 58 74 6e  65 63 38 75  " +
            " 63 4a 4c 33 77 73 49 66 63 31 41 25 33 44 25 33 44 22 7d   " +
            " 5d 2c 22 67 72 6f 75 70 22 3a 22 63 66 2d 6e   65 6c 22 2c    " +
            " 22 6d 61 78 5f 61 67 65 22 3a 36 30 34 38 30   30 7d 2a 14           " +
            " a 6 53 65 72 76 65 72 12 a 63 6c 6f 75 64 66 6c 9d e2  0 ";

        var p1c3 =
            " 0 6 8c 1 b4 13 38 3 f0 2 1 3 13 5 1 3 78 1 1   " +
            "  ff 61 72 65 2a 90 1 a a 53 65 74 2d 43 6f 6f 6b 69 65   " +
            " 12 81 1 5f 5f 63 66 72 75 69 64 3d 66 30 64 30 37 32 37 " +
            " 33 35 33 30 61 33 32 33 32 37 31 63 39 64 64 32 66 66 66       " +
            " 33 33 33 34 35 62 39 39 61 39 38 64 30 37 2d 31 36 34 34  " +
            " 33 34 36 34 35 31 3b 20 70 61 74 68 3d 2f 3b  20 64 6f 6d    " +
            " 61 69 6e 3d 2e 61 70 69 2e 67 63 73 2e 67 61 72 6d 69 6e   " +
            " 2e 63 6f 6d 3b 20 48 74 74 70 4f 6e 6c 79 3b   20 53 65 63   " +
            " 75 72 65 3b 20 53 61 6d 65 53 69 74 65 3d 4e 6f 6e 65 2a    " +
            "  4a a 19 73 74 72 69 63 74 2d 74 72 61 6e 73 70 6f 72 74 " +
            " 2d 73 65 63 75 72 69 74 79 12 2d 6d 61 78 2d 61 67 65 3d    " +
            " 31 36 30 30 30 30 30 30 3b 20 69 6e 63 6c 75 64 65 53 75    " +
            " 62 44 6f 6d 61 69 6e 73 3b 20 70 72 65 6c 6f 61 64 3b 2a   " +
            " 2a a 19 58 2d 41 6e 64 72 6f 69 64 2d 52 65 63 65 69 76    " +
            "  65 64 2d 4d 69 6c 6c 69 7d 73 12 d 31 36 34   34 33 34 36      " +
            "  34 35 32 34 30 32 2a 28 a 19 58 2d 41 6e 64  72 6f 69 64        " +
            " 2d 52 65 73 70 6f 6e 73 65 2d 53 6f 75 72 63  65 12 b 4e   " +
            " 45 54 57 4f 52 4b 20 32 30 30 2a 27 a 1b 58  2d 41 6e 64    " +
            " 72 6f 69 64 2d 53 65 6c 65 63 74 65 64 2d 50  72 6f 74 6f    " +
            " 63 6f 6c 12 8 68 74 74 70 2f 31 2e 31 2a 26 a 15 58 2d     " +
            " 41 6e 64 72 6f 69 64 2d 53 65 6e 74 2d 4d 69  6c 6c f1 79    0 " +
            "";
        var p1c4 =
            " 0 2 bf 4 b4 13 38 3 68 4 1 3 13 5 1 2 ab 1 1     " +
            " ae 69 73 12 d 31 36 34 34 33 34 36 34 35 32 31 38 31 2a   " +
            " 21 a 16 78 2d 63 6f 6e 74 65 6e 74 2d 74 79  70 65 2d 6f        " +
            " 70 74 69 6f 6e 73 12 7 6e 6f 73 6e 69 66 66 2a 17 a f  " +
            "  78 2d 66 72 61 6d 65 2d 6f 70 74 69 6f 6e 73  12 4 44 45    " +
            " 4e 59 2a 39 a 11 78 2d 76 63 61 70 2d 72 65  71 75 65 73    " +
            " 74 2d 69 64 12 24 33 34 30 38 31 38 63 37 2d 62 33 33 35    " +
            " 2d 34 39 32 32 2d 37 62 63 65 2d 33 62 39 62  66 66 63 64     " +
            " 34 31 31 62 2a 21 a 10 78 2d 78 73 73 2d 70  72 6f 74 65     " +
            " 63 74 69 6f 6e 12 d 31 3b 20 6d 6f 64 65 3d 62 6c 6f 63     " +
            "  6b 19 71 0     " +
            "  " +
            "";
        ProcessGfdiBytes(BinUtils.hexStringToByteArray(p1c1));
        ProcessGfdiBytes(BinUtils.hexStringToByteArray(p1c2));
        ProcessGfdiBytes(BinUtils.hexStringToByteArray(p1c3));
        ProcessGfdiBytes(BinUtils.hexStringToByteArray(p1c4));
        _pbmessage.Position = 0;
        var s = Smart.Parser.ParseFrom(_pbmessage);
        _testOutputHelper.WriteLine(s.ToString());
    }

    [Fact]
    public void DecodeLogDataTransfer()
    {
        _log = new XunitConsoleLogger(_testOutputHelper);
        _gfdiPacketParser = new GfdiPacketParser(_log);

        var p1c1 =
            "  0 6 81 1 b4 13 39 1 1 1 1 3 6d 1 1  3 6d 1 1   " +
            "  a 3a ea 2 12 e7 2 8 1 10 2 18 a 22 de 2 2a 12 a0  " +
            " 2 b4 f1 1 4 e0 52 57 2 4 2 12 4c 7d 83 7 fa 43 42  " +
            " ea 5 22 70 90 2 a2 90 95 f9 df e 84 5 17 22 64 2 df   " +
            "  c6 2b f9 ea e6 16 5 45 cf 34 2 63 9f ca f8 b1 5e a3 4 " +
            "  d b9 2 2 ac 84 72 f8 e3 a 2a 4 c9 20 ce 1 a5 d7 23  " +
            " f8 71 7f ab 3 ce 46 97 1 90 54 57 4 ff ff 3c 88 e5 a7       " +
            "  51 15 7c 24 2c 9 b3 84 bb 2 20 a8 4b 14 a1 30 3f b b5 " +
            " ef d5 3 82 eb 5 13 9 83 2e d 74 26 e4 4  ef 63 85 11     " +
            "  27 cb f4 e a5 20 e3 5 96 73 cf f 7f 50 8d 10 26 17 d0   " +
            "  6 bf db e9 d 86 f0 f3 11 77 84 a8 7 80 ae  da b 23 1c  " +
            "  25 13 c4 24 6a 8 f7 42 a8 9 6f d5 1d 14 d3 f5 12 9 56 " +
            " 2b 59 7 28 ae db 14 2c 37 a1 9 4d 2c f4 4  2e c7 5c 15  " +
            " 9b 6a 13 a 16 35 80 2 1d d1 9f 15 3b 55 68   a 9c 57 4      " +
            "  6c 29 e a4 15 10 1 9f a 25 c0 87 fd 1 55  69 15 38 bf  " +
            " b6 a f8 ab 11 fb a7 14 f0 14 9f 2a af a 80 5e a9 f8 ca  " +
            " 58 39 14 19 2b 88 a 9c 14 56 f6 63 ce 46 13 be f8 41 a    " +
            "  bc f5 1e f4 f1 c7 1a 12 45 1f dd 9 c1 2 b f2 eb 40 b8    " +
            "  10 2d 81 5a 9 79 2 21 f0 b5 df 22 f 52 5a  bb 8 6 6c   " +
            " 67 ee 93 f5 5e d be 41 1 8 60 54 57 2 1 2 c 9 dc      " +
            " 36 36 2 24 36 23 1 0    " +
            "  " +
            "  " +
            "";

        var p1c2 =
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "0";

        var p1c3 =
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "  " +
            "0";


        ProcessGfdiBytes(BinUtils.hexStringToByteArray(p1c1));
        // ProcessGfdiBytes(hexStringToByteArray(p1c2));
        // ProcessGfdiBytes(hexStringToByteArray(p1c3));
        // ProcessGfdiBytes(hexStringToByteArray(p1c4));
        _pbmessage.Position = 0;
        var s = Smart.Parser.ParseFrom(_pbmessage);
        _testOutputHelper.WriteLine(s.ToString());
    }

    [Fact]
    public void TestTimezone()
    {
        DisplayDateWithTimeZoneName(DateTime.Now, TimeZoneInfo.Local);
    }
    private void DisplayDateWithTimeZoneName(DateTime date1, TimeZoneInfo timeZone)
    {
        var log = new XunitConsoleLogger(_testOutputHelper);
        log.Info("The time is {0:t} on {0:d} {1}",
            date1,
            timeZone.IsDaylightSavingTime(date1) ? timeZone.DaylightName : timeZone.StandardName);
    }
}
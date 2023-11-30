namespace Garmin.Ble.Lib.Devices.Garmin;

public abstract record DebugZones
{
    public const string FileTransfer = "filetransfer";
    public const string Package = "package";
    public const string Protobuf = "protobuf";
    public const string MessageSend = "message.send";
    public const string BytesBle = "bytes.ble";
    public const string PackageSend = "package.send";
    public const string BytesBleSend = "bytes.ble.send";
    public const string FileTransferProgress = "Transfer.Progress";
    public const string MessageSendBytes = "message.send.bytes";
}
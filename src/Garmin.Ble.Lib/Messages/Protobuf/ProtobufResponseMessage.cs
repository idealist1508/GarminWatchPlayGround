namespace Garmin.Ble.Lib.Messages;

public class ProtobufResponseMessage:ProtobufMessageBase
{
    public const int MESSAGE_ID = GarminConstants.MESSAGE_PROTOBUF_RESPONSE;
    
    public ProtobufResponseMessage(int requestId, int dataOffset, int totalProtobufLength, int protobufDataLength,
        ReadOnlySpan<byte> messageBytes): base(MESSAGE_ID, requestId, dataOffset, totalProtobufLength, protobufDataLength, messageBytes)
    {
    }

    public ProtobufResponseMessage()
    {
    }
}
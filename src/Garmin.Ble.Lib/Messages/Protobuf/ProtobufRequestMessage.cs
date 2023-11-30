namespace Garmin.Ble.Lib.Messages;

public class ProtobufRequestMessage:ProtobufMessageBase {
    public const int MESSAGE_ID = GarminConstants.MESSAGE_PROTOBUF_REQUEST;
    
    public ProtobufRequestMessage(int requestId, int dataOffset, int totalProtobufLength, int protobufDataLength, ReadOnlySpan<byte> messageBytes): 
        base(MESSAGE_ID, requestId, dataOffset, totalProtobufLength, protobufDataLength, messageBytes) {
    }

    // public static ProtobufRequestMessage parsePacket(ReadOnlySpan<byte> packet) {
    //     MessageReader reader = new MessageReader(ref packet, 4);
    //     int requestID = reader.readShort();
    //     int dataOffset = reader.readInt();
    //     int totalProtobufLength= reader.readInt();
    //     int protobufDataLength = reader.readInt();
    //     var messageBytes = reader.readBytes(protobufDataLength);
    //     return new ProtobufRequestMessage(requestID, dataOffset, totalProtobufLength, protobufDataLength, messageBytes);
    // }
    public ProtobufRequestMessage()
    {
    }
}
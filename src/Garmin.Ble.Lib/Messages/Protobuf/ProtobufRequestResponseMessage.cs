namespace Garmin.Ble.Lib.Messages;

public class ProtobufRequestResponseMessage {
    public static int NO_ERROR = 0;
    public static int UNKNOWN_REQUEST_ID = 100;
    public static int DUPLICATE_PACKET = 101;
    public static int MISSING_PACKET = 102;
    public static int EXCEEDED_TOTAL_PROTOBUF_LENGTH = 103;
    public static int PROTOBUF_PARSE_ERROR = 200;
    public static int UNKNOWN_PROTOBUF_MESSAGE = 201;

    public int status;
    public int requestId;
    public int dataOffset;
    public int protobufStatus;
    public int error;

    public ProtobufRequestResponseMessage(int status, int requestId, int dataOffset, int protobufStatus, int error) {
        this.status = status;
        this.requestId = requestId;
        this.dataOffset = dataOffset;
        this.protobufStatus = protobufStatus;
        this.error = error;
    }

    public static ProtobufRequestResponseMessage parsePacket(ReadOnlySpan<byte> packet) {
        MessageReader reader = new MessageReader(ref packet, 4);
        int requestMessageID = reader.readShort();
        int status = reader.readByte();
        int requestID = reader.readShort();
        int dataOffset = reader.readInt();
        int protobufStatus = reader.readByte();
        int error = reader.readByte();

        return new ProtobufRequestResponseMessage(status, requestID, dataOffset, protobufStatus, error);
    }
}
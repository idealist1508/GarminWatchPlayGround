namespace Garmin.Ble.Lib.Messages;

public class FitDefinitionResponseMessage {
    public int requestID;
    public int status;
    public int fitResponse;

    public FitDefinitionResponseMessage(int requestID, int status, int fitResponse) {
        this.requestID = requestID;
        this.status = status;
        this.fitResponse = fitResponse;
    }

    public static FitDefinitionResponseMessage parsePacket(ReadOnlySpan<byte> packet) {
        MessageReader reader = new MessageReader(ref packet, 4);
        int requestID = reader.readShort();
        int status = reader.readByte();
        int fitResponse = reader.readByte();

        return new FitDefinitionResponseMessage(requestID, status, fitResponse);
    }

    public static int RESPONSE_APPLIED = 0;
    public static int RESPONSE_NOT_UNIQUE = 1;
    public static int RESPONSE_OUT_OF_RANGE = 2;
    public static int RESPONSE_NOT_READY = 3;
}
namespace Garmin.Ble.Lib.Messages;

public class DirectoryFileFilterResponseMessage {
    public int status;
    public int response;

    public static int RESPONSE_DIRECTORY_FILTER_APPLIED = 0;
    public static int RESPONSE_FAILED_TO_APPLY_DIRECTORY_FILTER = 1;

    public DirectoryFileFilterResponseMessage(int status, int response) {
        this.status = status;
        this.response = response;
    }

    public static DirectoryFileFilterResponseMessage parsePacket(ReadOnlySpan<byte> packet) {
        MessageReader reader = new MessageReader(ref packet, 4);
        int requestID = reader.readShort();
        int status = reader.readByte();
        int response = reader.readByte();

        return new DirectoryFileFilterResponseMessage(status, response);
    }
}
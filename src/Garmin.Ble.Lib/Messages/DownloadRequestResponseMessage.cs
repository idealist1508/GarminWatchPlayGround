namespace Garmin.Ble.Lib.Messages;

public class DownloadRequestResponseMessage {
    public int status;
    public int response;
    public int fileSize;

    public static byte RESPONSE_DOWNLOAD_REQUEST_OKAY = 0;
    public static byte RESPONSE_DATA_DOES_NOT_EXIST = 1;
    public static byte RESPONSE_DATA_EXISTS_BUT_IS_NOT_DOWNLOADABLE = 2;
    public static byte RESPONSE_NOT_READY_TO_DOWNLOAD = 3;
    public static byte RESPONSE_REQUEST_INVALID = 4;
    public static byte RESPONSE_CRC_INCORRECT = 5;
    public static byte RESPONSE_DATA_REQUESTED_EXCEEDS_FILE_SIZE = 6;

    public DownloadRequestResponseMessage(int status, int response, int fileSize) {
        this.status = status;
        this.response = response;
        this.fileSize = fileSize;
    }

    public static DownloadRequestResponseMessage parsePacket(ReadOnlySpan<byte> packet) {
        MessageReader reader = new MessageReader(ref packet, 4);
        int requestID = reader.readShort();
        int status = reader.readByte();
        int response = reader.readByte();
        int fileSize = reader.readInt();

        return new DownloadRequestResponseMessage(status, response, fileSize);
    }
}
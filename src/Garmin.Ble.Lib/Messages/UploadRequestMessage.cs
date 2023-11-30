namespace Garmin.Ble.Lib.Messages;

public class UploadRequestMessage {
    public byte[] packet;

    public UploadRequestMessage(int fileIndex, int dataOffset, int maxSize, int crcSeed) {
        var writer = new MessageWriter(18);
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_UPLOAD_REQUEST);
        writer.WriteShort(fileIndex);
        writer.WriteInt(maxSize);
        writer.WriteInt(dataOffset);
        writer.WriteShort(crcSeed);
        writer.WriteShort(0); // CRC will be filled below
        var packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.packet = packet;
    }
}
public class CreateFileResponseMessage {
    public static byte RESPONSE_FILE_CREATED_SUCCESSFULLY = 0;
    public static byte RESPONSE_FILE_ALREADY_EXISTS = 1;
    public static byte RESPONSE_NOT_ENOUGH_SPACE = 2;
    public static byte RESPONSE_NOT_SUPPORTED = 3;
    public static byte RESPONSE_NO_SLOTS_AVAILABLE_FOR_FILE_TYPE = 4;
    public static byte RESPONSE_NOT_ENOUGH_SPACE_FOR_FILE_TYPE = 5;

    public int status;
    public int response;
    public int fileIndex;
    public int dataType;
    public int subType;
    public int fileNumber;

    public CreateFileResponseMessage(int status, int response, int fileIndex, int dataType, int subType, int fileNumber) {
        this.status = status;
        this.response = response;
        this.fileIndex = fileIndex;
        this.dataType = dataType;
        this.subType = subType;
        this.fileNumber = fileNumber;
    }

    public static CreateFileResponseMessage parsePacket(ReadOnlySpan<byte> packet) {
        var reader = new MessageReader(ref packet, 6);
        var status = reader.readByte();
        var response = reader.readByte();
        var fileIndex = reader.readShort();
        var dataType = reader.readByte();
        var subType = reader.readByte();
        var fileNumber = reader.readShort();

        return new CreateFileResponseMessage(status, response, fileIndex, dataType, subType, fileNumber);
    }
}
public class UploadRequestResponseMessage {
    public static byte RESPONSE_UPLOAD_REQUEST_OKAY = 0;
    public static byte RESPONSE_DATA_FILE_INDEX_DOES_NOT_EXIST = 1;
    public static byte RESPONSE_DATA_FILE_INDEX_EXISTS_BUT_IS_NOT_WRITEABLE = 2;
    public static byte RESPONSE_NOT_ENOUGH_SPACE_TO_COMPLETE_WRITE = 3;
    public static byte RESPONSE_REQUEST_INVALID = 4;
    public static byte RESPONSE_NOT_READY_TO_UPLOAD = 5;
    public static byte RESPONSE_CRC_INCORRECT = 6;

    public int status;
    public int response;
    public int dataOffset;
    public int maxFileSize;
    public int crcSeed;

    public UploadRequestResponseMessage(int status, int response, int dataOffset, int maxFileSize, int crcSeed) {
        this.status = status;
        this.response = response;
        this.dataOffset = dataOffset;
        this.maxFileSize = maxFileSize;
        this.crcSeed = crcSeed;
    }

    public static UploadRequestResponseMessage parsePacket(ReadOnlySpan<byte> packet) {
        MessageReader reader = new MessageReader(ref packet, 6);
        int status = reader.readByte();
        int response = reader.readByte();
        int dataOffset = reader.readInt();
        int maxFileSize = reader.readInt();
        int crcSeed = reader.readInt();

        return new UploadRequestResponseMessage(status, response, dataOffset, maxFileSize, crcSeed);
    }
}

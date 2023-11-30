namespace Garmin.Ble.Lib.Messages;

public class FileTransferDataResponseMessage : IGfdiMessage{
    public static byte RESPONSE_TRANSFER_SUCCESSFUL = 0;
    public static byte RESPONSE_RESEND_LAST_DATA_PACKET = 1;
    public static byte RESPONSE_ABORT_DOWNLOAD_REQUEST = 2;
    public static byte RESPONSE_ERROR_CRC_MISMATCH = 3;
    public static byte RESPONSE_ERROR_DATA_OFFSET_MISMATCH = 4;
    public static byte RESPONSE_SILENT_SYNC_PAUSED = 5;

    public int status;
    public int response;
    public int nextDataOffset;

    public byte[] Packet { get; }

    public FileTransferDataResponseMessage(int status, int response, int nextDataOffset) {
        this.status = status;
        this.response = response;
        this.nextDataOffset = nextDataOffset;

        MessageWriter writer = new MessageWriter();
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_RESPONSE);
        writer.WriteShort(GarminConstants.MESSAGE_FILE_TRANSFER_DATA);
        writer.WriteByte(status);
        writer.WriteByte(response);
        writer.WriteInt(nextDataOffset);
        writer.WriteShort(0); // CRC will be filled below
        byte[] packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.Packet = packet;
    }

    public static FileTransferDataResponseMessage parsePacket(ReadOnlySpan<byte> packet) {
        MessageReader reader = new MessageReader(ref packet, 6);
        int status = reader.readByte();
        int response = reader.readByte();
        int nextDataOffset = reader.readInt();

        return new FileTransferDataResponseMessage(status, response, nextDataOffset);
    }
}
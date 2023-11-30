namespace Garmin.Ble.Lib.Messages;

public class FileTransferDataMessage {
    public int flags;
    public int crc;
    public int dataOffset;
    public byte[] data;

    public byte[] packet;

    public FileTransferDataMessage(int flags, int crc, int dataOffset, ReadOnlySpan<byte> data) {
        this.flags = flags;
        this.crc = crc;
        this.dataOffset = dataOffset;
        this.data = data.ToArray();

        MessageWriter writer = new MessageWriter();
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_FILE_TRANSFER_DATA);
        writer.WriteByte(flags);
        writer.WriteShort(crc);
        writer.WriteInt(dataOffset);
        writer.WriteBytes(data);
        writer.WriteShort(0); // CRC will be filled below
        byte[] packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.packet = packet;
    }

    public static FileTransferDataMessage parsePacket(ReadOnlySpan<byte> packet) {
        MessageReader reader = new MessageReader(ref packet, 4);

        int flags = reader.readByte();
        int crc = reader.readShort();
        int dataOffset = reader.readInt();
        int dataSize = packet.Length - 13;
        ReadOnlySpan<byte> data = reader.readBytes(dataSize);

        return new FileTransferDataMessage(flags, crc, dataOffset, data);
    }
}
namespace Garmin.Ble.Lib.Messages;

public interface IGfdiMessage
{
    byte[] Packet { get; }
}

public class DirectoryFileFilterRequestMessage : IGfdiMessage
{
    public static int FILTER_NO_FILTER = 0;
    public static int FILTER_DEVICE_DEFAULT_FILTER = 1;
    public static int FILTER_CUSTOM_FILTER = 2;
    public static int FILTER_PENDING_UPLOADS_ONLY = 3;

    public byte[] Packet { get; }

    public DirectoryFileFilterRequestMessage(int filterType) {
        MessageWriter writer = new MessageWriter(7);
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_DIRECTORY_FILE_FILTER_REQUEST);
        writer.WriteByte(filterType);
        writer.WriteShort(0); // CRC will be filled below
        byte[] packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.Packet = packet;
    }
}
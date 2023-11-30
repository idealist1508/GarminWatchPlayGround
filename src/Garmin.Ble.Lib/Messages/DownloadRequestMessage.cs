namespace Garmin.Ble.Lib.Messages;

public class DownloadRequestMessage :IGfdiMessage{
    public static int REQUEST_CONTINUE_TRANSFER = 0;
    public static int REQUEST_NEW_TRANSFER = 1;

    public byte[] Packet { get; }

    public DownloadRequestMessage(int fileIndex, int dataOffset, int request, int crcSeed, int dataSize) {
        MessageWriter writer = new MessageWriter(19);
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_DOWNLOAD_REQUEST);
        writer.WriteShort(fileIndex);
        writer.WriteInt(dataOffset);
        writer.WriteByte(request);
        writer.WriteShort(crcSeed);
        writer.WriteInt(dataSize);
        writer.WriteShort(0); // CRC will be filled below
        byte[] packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.Packet = packet;
    }
}
namespace Garmin.Ble.Lib.Messages;

public class CreateFileRequestMessage {
    public readonly byte[] Packet;

    public CreateFileRequestMessage(int fileSize, int dataType, int subType, int fileIdentifier, int subTypeMask, int numberMask, String path) {
        var writer = new MessageWriter();
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_CREATE_FILE_REQUEST);
        writer.WriteInt(fileSize);
        writer.WriteByte(dataType);
        writer.WriteByte(subType);
        writer.WriteShort(fileIdentifier);
        writer.WriteByte(0); // reserved
        writer.WriteByte(subTypeMask);
        writer.WriteShort(numberMask);
        writer.WriteString(path);
        writer.WriteShort(0); // CRC will be filled below
        var packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        Packet = packet;
    }
}
namespace Garmin.Ble.Lib.Messages;

public class SystemEventMessage: IGfdiMessage
{
    public byte[] Packet { get; }

    public SystemEventMessage(GarminSystemEventType eventType, Object value) {
        MessageWriter writer = new MessageWriter();
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_SYSTEM_EVENT);
        writer.WriteByte((int)eventType);
        if (value is String s) {
            writer.WriteString(s);
        } else if (value is int i) {
            writer.WriteByte(i);
        } else {
            throw new ArgumentException("Unsupported event value type " + value);
        }
        writer.WriteShort(0); // CRC will be filled below
        byte[] packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.Packet = packet;
    }
}
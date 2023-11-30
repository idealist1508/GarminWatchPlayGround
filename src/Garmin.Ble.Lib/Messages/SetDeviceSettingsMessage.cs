namespace Garmin.Ble.Lib.Messages;

public class SetDeviceSettingsMessage: IGfdiMessage
{
    public byte[] Packet { get; }

    public SetDeviceSettingsMessage(Dictionary<GarminDeviceSetting, Object> settings) {
        int settingsCount = settings.Count;
        if (settingsCount == 0) throw new ArgumentException("Empty settings");
        if (settingsCount > 255) throw new ArgumentException("Too many settings");

        MessageWriter writer = new MessageWriter();
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_DEVICE_SETTINGS);
        writer.WriteByte(settings.Count);
        foreach (var settingPair in settings)
        {
            var setting = (int) settingPair.Key;
            writer.WriteByte(setting);
            Object value = settingPair.Value;
            if (value is String s) {
                writer.WriteString(s);
            } else if (value is int i) {
                writer.WriteByte(4);
                writer.WriteInt(i);
            } else if (value is bool b) {
                writer.WriteByte(1);
                writer.WriteByte(b ? 1 : 0);
            } else {
                throw new ArgumentException("Unsupported setting value type " + value);
            }
        }
        writer.WriteShort(0); // CRC will be filled below
        byte[] packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.Packet = packet;
    }
}
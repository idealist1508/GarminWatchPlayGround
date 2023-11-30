namespace Garmin.Ble.Lib.Messages;

public class ConfigurationMessage: IGfdiMessage
{
    public byte[] ConfigurationPayload { get; }
    public byte[] Packet { get; }

    public ConfigurationMessage(ReadOnlySpan<byte> configurationPayload)
    {
        if (configurationPayload.Length > 255) throw new ArgumentException("Too long payload");
        ConfigurationPayload = configurationPayload.ToArray();

        var writer = new MessageWriter(7 + configurationPayload.Length);
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_CONFIGURATION);
        writer.WriteByte(configurationPayload.Length);
        writer.WriteBytes(configurationPayload);
        writer.WriteShort(0); // CRC will be filled below
        var packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.Packet = packet;
    }

    public static ConfigurationMessage parsePacket(ReadOnlySpan<byte> packet)
    {
        var reader = new MessageReader(ref packet, 4);
        var payloadSize = reader.readByte();
        return new ConfigurationMessage(packet.Slice(5, payloadSize));
    }
}
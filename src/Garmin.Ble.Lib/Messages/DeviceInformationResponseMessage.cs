using System.Text;

namespace Garmin.Ble.Lib.Messages;

public class DeviceInformationResponseMessage: IGfdiMessage
{
    public byte[] Packet { get; }

    public DeviceInformationResponseMessage(int status, int protocolVersion, int productNumber, int unitNumber,
        int softwareVersion, int maxPacketSize, string bluetoothFriendlyName, string deviceName, string deviceModel,
        int protocolFlags)
    {
        var writer = new MessageWriter();
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_RESPONSE);
        writer.WriteShort(GarminConstants.MESSAGE_DEVICE_INFORMATION);
        writer.WriteByte(status);
        writer.WriteShort(protocolVersion);
        writer.WriteShort(productNumber);
        writer.WriteInt(unitNumber);
        writer.WriteShort(softwareVersion);
        writer.WriteShort(maxPacketSize);
        writer.WriteString(bluetoothFriendlyName);
        writer.WriteString(deviceName);
        writer.WriteString(deviceModel);
        writer.WriteByte(protocolFlags);
        writer.WriteShort(0); // CRC will be filled below
        var pkg = writer.GetBytes();
        BinaryUtils.WriteShort(pkg, 0, pkg.Length);
        BinaryUtils.WriteShort(pkg, pkg.Length - 2, Crc.Calc(pkg, 0, pkg.Length - 2));
        Packet = pkg;
    }
}
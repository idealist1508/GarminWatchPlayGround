namespace Garmin.Ble.Lib.Messages;

public class DeviceInformationMessage
{
    public readonly int ProtocolVersion;
    public readonly int ProductNumber;
    public readonly uint UnitNumber;
    public readonly int SoftwareVersion;
    public readonly int MaxPacketSize;
    public readonly String BluetoothFriendlyName;
    public readonly String DeviceName;
    public readonly String DeviceModel;

    public DeviceInformationMessage(int protocolVersion, int productNumber, uint unitNumber, int softwareVersion,
        int maxPacketSize, String bluetoothFriendlyName, String deviceName, String deviceModel)
    {
        this.ProtocolVersion = protocolVersion;
        this.ProductNumber = productNumber;
        this.UnitNumber = unitNumber;
        this.SoftwareVersion = softwareVersion;
        this.MaxPacketSize = maxPacketSize;
        this.BluetoothFriendlyName = bluetoothFriendlyName;
        this.DeviceName = deviceName;
        this.DeviceModel = deviceModel;
    }

    public static DeviceInformationMessage ParsePacket(ReadOnlySpan<byte> packet)
    {
        //skip Length (2Byte) and MessageId (2Bytes)
        var reader = new MessageReader(ref packet, 4);
        var protocolVersion = reader.readShort();
        var productNumber = reader.readShort();
        var unitNumber = (uint)(reader.readInt());
        var softwareVersion = reader.readShort();
        var maxPacketSize = reader.readShort();
        var bluetoothFriendlyName = reader.readString();
        var deviceName = reader.readString();
        var deviceModel = reader.readString();

        return new DeviceInformationMessage(protocolVersion, productNumber, unitNumber, softwareVersion,
            maxPacketSize, bluetoothFriendlyName, deviceName, deviceModel);
    }

    public String getSoftwareVersionStr()
    {
        var softwareVersionMajor = SoftwareVersion / 100;
        var softwareVersionMinor = SoftwareVersion % 100;
        return $"{softwareVersionMajor}.{softwareVersionMinor}";
    }
}
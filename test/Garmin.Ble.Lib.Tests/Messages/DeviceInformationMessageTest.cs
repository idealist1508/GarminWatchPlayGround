using Garmin.Ble.Lib.Messages;
using Xunit;

namespace Garmin.Ble.Lib.Tests.Messages;

public class DeviceInformationMessageTest
{
    [Fact]
    public void TestParsePacket()
    {
        var message = DeviceInformationMessage.ParsePacket(new byte[] { 0x20, 0x00, 0xA0, 0x13, 0x6F, 0x00, 0x4D, 0x0D, 0x80, 0x88, 0x8F, 0xC8, 0x0E, 0x01, 0x90, 0x01, 0x06, 0x53, 0x77, 0x69, 0x6d, 0x20, 0x32, 0x04, 0x53, 0x77, 0x69, 0x6d, 0x01, 0x32, 0xd5, 0xab });
        Assert.Equal("2", message.DeviceModel);
        Assert.Equal("Swim", message.DeviceName);
        Assert.Equal(3405, message.ProductNumber);
        Assert.Equal(111, message.ProtocolVersion);
        Assert.Equal(270, message.SoftwareVersion);
        Assert.Equal(3364849792, message.UnitNumber);
        Assert.Equal("Swim 2", message.BluetoothFriendlyName);
        Assert.Equal(400, message.MaxPacketSize);
    }
}
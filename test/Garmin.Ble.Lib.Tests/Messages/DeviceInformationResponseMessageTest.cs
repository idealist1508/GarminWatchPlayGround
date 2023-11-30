using Garmin.Ble.Lib.Messages;
using Xunit;

namespace Garmin.Ble.Lib.Tests.Messages;

public class DeviceInformationResponseMessageTest
{
    [Fact]
    public void TestCreate()
    {
        var message = new DeviceInformationResponseMessage(
            GarminConstants.STATUS_ACK, 
            113, 
            -1, 
            -1, 
            6336, 
            -1,
            "BV4900", 
            "Blackview", 
            "BV4900", 
            1);
        
        Assert.NotNull(message.Packet);
        Assert.Equal(
            new byte[]
            {
                0x2E, 0x00, 0x88, 0x13, 0xA0, 0x13, 0x00, 0x71, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC0, 0x18,
                0xFF, 0xFF, 0x06, 0x42, 0x56, 0x34, 0x39, 0x30, 0x30, 0x09, 0x42, 0x6C, 0x61, 0x63, 0x6B, 0x76, 0x69,
                0x65, 0x77, 0x06, 0x42, 0x56, 0x34, 0x39, 0x30, 0x30, 0x01, 0xC3, 0x88
            }, message.Packet);
    }
}
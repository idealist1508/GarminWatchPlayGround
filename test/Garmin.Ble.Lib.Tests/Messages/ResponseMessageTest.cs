using Garmin.Ble.Lib.Messages;
using Xunit;

namespace Garmin.Ble.Lib.Tests.Messages;

public class ResponseMessageTest
{
    [Fact]
    public void TestParsePacket()
    {
        var message = ResponseMessage.ParsePacket(new byte[] {0x2e, 0x00, 0x88, 0x13, 0xa0, 0x13, 00 });
        Assert.Equal(GarminConstants.STATUS_ACK, message.Status);
        Assert.Equal(GarminConstants.MESSAGE_DEVICE_INFORMATION, message.RequestId);
    }
}
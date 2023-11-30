using Garmin.Ble.Lib.Messages;
using Xunit;

namespace Garmin.Ble.Lib.Tests.Messages;

public class RegisterForServiceRequestTest
{
    [Fact]
    public void TestCreate()
    {
        var message = new RegisterForServiceRequest(ServiceType.KEEP_ALIVE);
        
        Assert.NotNull(message.Packet);
        Assert.Equal(
            new byte[]
            {
                0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  0x16, 0x00, 0x00
            }, message.Packet);
    }
}
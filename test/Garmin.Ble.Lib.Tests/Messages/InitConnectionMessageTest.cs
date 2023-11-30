using Garmin.Ble.Lib.Messages;
using Xunit;

namespace Garmin.Ble.Lib.Tests.Messages;

public class InitConnectionMessageTest
{
    [Fact]
    public void TestCreate()
    {
        var message = new InitConnectionMessage();
        Assert.Equal(new byte[]{0,5,1,0,0,0,0,0,0,0,0,0,0}, message.Packet);
    }
}
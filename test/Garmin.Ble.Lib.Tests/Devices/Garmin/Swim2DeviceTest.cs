using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ble.Interfaces;
using Garmin.Ble.Lib.Devices.Garmin;
using Garmin.Ble.Lib.Messages;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Garmin.Ble.Lib.Tests.Devices.Garmin;

public class Swim2DeviceTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private const int GfdiChannel = 0xc3;
    private Mock<IGattCharacteristic> InCharacteristic { get; }
    private Mock<IGattCharacteristicValueEventArgs> RegisterServiceGfdiResponseEventArgs { get; }
    private Mock<IGattCharacteristic> OutCharacteristics { get; }
    private Swim2Device Device { get; }

    public Swim2DeviceTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        InCharacteristic = new Mock<IGattCharacteristic>();
        InCharacteristic
            .Setup(c => c.GetUUIDAsync())
            .ReturnsAsync(() => "GattUIID");

        RegisterServiceGfdiResponseEventArgs = new Mock<IGattCharacteristicValueEventArgs>();
        RegisterServiceGfdiResponseEventArgs
            .Setup(args => args.Value)
            .Returns(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, (byte)ServiceType.GFDI, 0, 0, GfdiChannel });

        Device = new Swim2Device(new XunitConsoleLogger(_testOutputHelper));
        OutCharacteristics = new Mock<IGattCharacteristic>();
        OutCharacteristics
            .Setup(c => c.WriteValueAsync(It.IsAny<byte[]>(), It.IsAny<IDictionary<string, object>>()))
            .Callback(
                (byte[] a, IDictionary<string, object> b) => _testOutputHelper.WriteLine(BitConverter.ToString(a)));

        var multiChannelService = new Mock<IGattService>();
        multiChannelService
            .Setup(service => service.GetCharacteristicAsync(It.IsAny<string>()))
            .ReturnsAsync(OutCharacteristics.Object);
        
        var bleDevice = new Mock<IBleDevice>();
        bleDevice
            .Setup(d => d.GetServiceAsync(It.IsAny<string>()))
            .ReturnsAsync(multiChannelService.Object);

        Device.Init(bleDevice.Object);
        Device.OnValueIn(InCharacteristic.Object, RegisterServiceGfdiResponseEventArgs.Object);
    }


    [Fact]
    public void TestSendMessageSmall()
    {
        Device.PostGfdiMessage(new byte[] { 0x21 });
        OutCharacteristics.Verify(
            x => x.WriteValueAsync(new byte[] { GfdiChannel, 0x00, 0x02, 0x21, 0x00 },
                new Dictionary<string, object>()), Times.Once);
    }
    
    [Fact]
    public void TestSendMessageLarge()
    {
        var package = new byte[25];
        Array.Fill(package, (byte)0x21);

        var messagePart1 = new byte[20];
        Array.Fill(messagePart1, (byte)0x21);
        messagePart1[0] = GfdiChannel;
        messagePart1[1] = 0x0;
        messagePart1[2] = 26;

        var messagePart2 = new byte[10];
        Array.Fill(messagePart2, (byte)0x21);
        messagePart2[0] = GfdiChannel;
        messagePart2[9] = 0;

        Device.PostGfdiMessage(package);

        OutCharacteristics.Verify(x => x.WriteValueAsync(messagePart1, new Dictionary<string, object>()), Times.Once);
        OutCharacteristics.Verify(x => x.WriteValueAsync(messagePart2, new Dictionary<string, object>()), Times.Once);
    }
}
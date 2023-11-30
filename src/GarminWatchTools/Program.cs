using Ble.Linux;
using Garmin.Ble.Lib;
using Garmin.Ble.Lib.Devices.Garmin;
using vestervang.DotNetBlueZ;

const string MyWatchMac = "E6:EA:17:A8:92:E0";
var deviceAddress = MyWatchMac;
var timeout = TimeSpan.FromSeconds(15);

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.ExceptionObject);

var adapters = await BlueZManager.GetAdaptersAsync();
if (adapters.Count == 0)
{
    throw new Exception("No Bluetooth adapters found.");
}

var adapter = adapters.First();

if (!await adapter.GetPoweredAsync())
    await adapter.SetPoweredAsync(true);

var device = await adapter.GetDeviceAsync(deviceAddress);
if (device == null)
{
    Console.WriteLine(
        $"Bluetooth peripheral with address '{deviceAddress}' not found. Use `bluetoothctl` or Bluetooth Manager to scan and possibly pair first.");
    return;
}

Console.WriteLine("Connecting...");
await device.ConnectAsync();
await device.WaitForPropertyValueAsync("Connected", value: true, timeout);
Console.WriteLine($"Connected to {await device.GetNameAsync()}.");

await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);

var logger = new ConsoleLogger();
logger.DisableDebugInZone(DebugZones.PackageSend);
logger.DisableDebugInZone(DebugZones.BytesBleSend);
logger.DisableDebugInZone(DebugZones.Package);
logger.DisableDebugInZone(DebugZones.Protobuf);
logger.DisableDebugInZone(DebugZones.BytesBle);
logger.DisableDebugInZone(DebugZones.MessageSend);
logger.DisableDebugInZone(DebugZones.MessageSendBytes);
logger.DisableDebugInZone(DebugZones.FileTransferProgress);
var swim2 = new Swim2Device(logger);
Task.Run(async () => await swim2.DownloadAGpsData());
await swim2.Init(new LinuxBleDevice(device));

Console.WriteLine("Press Enter to read file list:");
Console.ReadLine();
// await swim2.DownloadGarminXml();
// await swim2.DownloadFile(5);

 swim2.DownloadAllActivities();

while (true)
    await Task.Delay(1000);
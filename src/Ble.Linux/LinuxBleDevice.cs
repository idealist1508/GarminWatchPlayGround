using Ble.Interfaces;
using vestervang.DotNetBlueZ;

namespace Ble.Linux;

public class LinuxBleDevice : IBleDevice
{
    private readonly Device _bleDeviceImplementation;

    public LinuxBleDevice(Device device)
    {
        _bleDeviceImplementation = device;
    }

    public async Task<IGattService> GetServiceAsync(string serviceUUID)
    {
        return new LinuxBleService(await _bleDeviceImplementation.GetServiceAsync(serviceUUID));
    }

    public Task DisconnectAsync()
    {
        return _bleDeviceImplementation.DisconnectAsync();
    }

    public Task ConnectAsync()
    {
        return _bleDeviceImplementation.ConnectAsync();
    }

    public Task ConnectProfileAsync(string UUID)
    {
        return _bleDeviceImplementation.ConnectProfileAsync(UUID);
    }

    public Task DisconnectProfileAsync(string UUID)
    {
        return _bleDeviceImplementation.DisconnectProfileAsync(UUID);
    }

    public Task PairAsync()
    {
        return _bleDeviceImplementation.PairAsync();
    }

    public Task CancelPairingAsync()
    {
        return _bleDeviceImplementation.CancelPairingAsync();
    }

    public Task<T> GetAsync<T>(string prop)
    {
        return _bleDeviceImplementation.GetAsync<T>(prop);
    }

    public Task SetAsync(string prop, object val)
    {
        return _bleDeviceImplementation.SetAsync(prop, val);
    }
}
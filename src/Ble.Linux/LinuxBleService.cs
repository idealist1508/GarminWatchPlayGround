using Ble.Interfaces;
using vestervang.DotNetBlueZ;

namespace Ble.Linux;

public class LinuxBleService : IGattService
{
    private readonly IGattService1 _gattServiceImplementation;

    public LinuxBleService(IGattService1 getServiceAsync)
    {
        _gattServiceImplementation = getServiceAsync;
    }

    public async Task<IGattCharacteristic> GetCharacteristicAsync(string characteristicUUID)
    {
        return new LinuxGattCharacteristic(await _gattServiceImplementation.GetCharacteristicAsync(characteristicUUID));
    }

    public Task<T> GetAsync<T>(string prop)
    {
        return _gattServiceImplementation.GetAsync<T>(prop);
    }

    public Task SetAsync(string prop, object val)
    {
        return _gattServiceImplementation.SetAsync(prop, val);
    }
}
using Ble.Interfaces;
using NeoSmart.AsyncLock;
using vestervang.DotNetBlueZ;
using GattCharacteristicEventHandlerAsync = Ble.Interfaces.GattCharacteristicEventHandlerAsync;

namespace Ble.Linux;

public class LinuxGattCharacteristic : IGattCharacteristic
{
    private readonly GattCharacteristic _gattCharacteristicImplementation;

    public LinuxGattCharacteristic(GattCharacteristic getCharacteristicAsync)
    {
        _gattCharacteristicImplementation = getCharacteristicAsync;
    }

    public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
    {
        return _gattCharacteristicImplementation.ReadValueAsync(Options);
    }


    private readonly AsyncLock writeLock = new();

    public async Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
    {
        using (await writeLock.LockAsync())
        {
            await _gattCharacteristicImplementation.WriteValueAsync(Value, Options);
            await Task.Delay(1);
        }
    }

    public Task<string> GetUUIDAsync()
    {
        return _gattCharacteristicImplementation.GetUUIDAsync();
    }

    private event GattCharacteristicEventHandlerAsync? Value;

    event GattCharacteristicEventHandlerAsync? IGattCharacteristic.Value
    {
        add
        {
            this.Value += value;
            _gattCharacteristicImplementation.Value += OnValueIn;
        }
        remove
        {
            _gattCharacteristicImplementation.Value -= OnValueIn;
            this.Value -= value;
        }
    }

    private Task? OnValueIn(GattCharacteristic sender, GattCharacteristicValueEventArgs e)
    {
        return Value?.Invoke(this, new LinuxGattCharacteristicEventArgs(e));
    }
}
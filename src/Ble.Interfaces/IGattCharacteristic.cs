namespace Ble.Interfaces;

public interface IGattCharacteristic
{
    Task<byte[]> ReadValueAsync(IDictionary<string, object> Options);
    Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options);
    public Task<string> GetUUIDAsync();
    public event GattCharacteristicEventHandlerAsync Value;
}
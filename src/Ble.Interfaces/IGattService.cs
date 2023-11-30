namespace Ble.Interfaces;

public interface IGattService
{
    Task<IGattCharacteristic> GetCharacteristicAsync(string characteristicUUID);
    Task<T> GetAsync<T>(string prop);
    Task SetAsync(string prop, object val);
}
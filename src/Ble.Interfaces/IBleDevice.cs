namespace Ble.Interfaces;

public interface IBleDevice
{
    Task<IGattService> GetServiceAsync(string serviceUUID);
    Task DisconnectAsync();
    Task ConnectAsync();
    Task ConnectProfileAsync(string UUID);
    Task DisconnectProfileAsync(string UUID);
    Task PairAsync();
    Task CancelPairingAsync();
    Task<T> GetAsync<T>(string prop);
    Task SetAsync(string prop, object val);
}
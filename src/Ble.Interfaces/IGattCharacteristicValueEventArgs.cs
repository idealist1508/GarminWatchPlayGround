namespace Ble.Interfaces;

public interface IGattCharacteristicValueEventArgs
{
    public byte[] Value { get; }
}
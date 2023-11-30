namespace Ble.Interfaces;

public delegate Task? GattCharacteristicEventHandlerAsync(
    IGattCharacteristic sender,
    IGattCharacteristicValueEventArgs eventArgs);
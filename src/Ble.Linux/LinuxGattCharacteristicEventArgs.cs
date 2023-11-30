using Ble.Interfaces;
using vestervang.DotNetBlueZ;

namespace Ble.Linux;

internal class LinuxGattCharacteristicEventArgs : IGattCharacteristicValueEventArgs
{
    private readonly GattCharacteristicValueEventArgs _gattCharacteristicValueEventArgsImplementation;

    public LinuxGattCharacteristicEventArgs(GattCharacteristicValueEventArgs gattCharacteristicValueEventArgs)
    {
        _gattCharacteristicValueEventArgsImplementation = gattCharacteristicValueEventArgs;
    }

    public byte[] Value => _gattCharacteristicValueEventArgsImplementation.Value;
}
namespace Garmin.Ble.Lib;

public static class Crc {
    private static readonly int[] Magics = {0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400};

    public static int Calc(ReadOnlySpan<byte> data, int offset, int length) {
        return Calc(0, data, offset, length);
    }

    public static int Calc(int initialCrc, ReadOnlySpan<byte> data, int offset, int length) {
        var crc = initialCrc;
        for (var i = offset; i < offset + length; ++i) {
            int b = data[i];
            crc = (((crc >> 4) & 4095) ^ Magics[crc & 15]) ^ Magics[b & 15];
            crc = (((crc >> 4) & 4095) ^ Magics[crc & 15]) ^ Magics[(b >> 4) & 15];
        }
        return crc;
    }
}
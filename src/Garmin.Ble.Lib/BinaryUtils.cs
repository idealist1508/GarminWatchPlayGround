namespace Garmin.Ble.Lib;

public static class BinaryUtils {
    public static byte ReadByte(ReadOnlySpan<byte> array, int offset) {
        return array[offset];
    }

    public static int ReadShort(ReadOnlySpan<byte> array, int offset) {
        return array[offset]  | (array[offset + 1]  << 8);
    }

    public static int ReadInt(ReadOnlySpan<byte> array, int offset) {
        return array[offset] | (array[offset + 1] << 8) | (array[offset + 2] << 16) | (array[offset + 3] << 24);
    }

    public static long ReadLong(ReadOnlySpan<byte> array, int offset) {
        return (long) (array[offset] | (array[offset + 1] << 8) | (array[offset + 2] << 16) | (array[offset + 3] << 24)) |
               ((long) (array[offset + 4]  | (array[offset + 5] << 8) | (array[offset + 6] << 16) | (array[offset + 7] << 24)) << 32);
    }

    public static void WriteByte(byte[] array, int offset, int value) {
        array[offset] = (byte) value;
    }

    public static void WriteShort(byte[] array, int offset, int value) {
        array[offset] = (byte) value;
        array[offset + 1] = (byte) (value >> 8);
    }

    public static void WriteInt(byte[] array, int offset, int value) {
        array[offset] = (byte) value;
        array[offset + 1] = (byte) (value >> 8);
        array[offset + 2] = (byte) (value >> 16);
        array[offset + 3] = (byte) (value >> 24);
    }

    public static void WriteLong(byte[] array, int offset, long value) {
        array[offset] = (byte) value;
        array[offset + 1] = (byte) (value >> 8);
        array[offset + 2] = (byte) (value >> 16);
        array[offset + 3] = (byte) (value >> 24);
        array[offset + 4] = (byte) (value >> 32);
        array[offset + 5] = (byte) (value >> 40);
        array[offset + 6] = (byte) (value >> 48);
        array[offset + 7] = (byte) (value >> 56);
    }
}
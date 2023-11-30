using System;

namespace Garmin.Ble.Lib.Tests;

public class BinUtils
{
    public static byte[] hexStringToByteArray(String s)
    {
        var bytes = s.Split(new Char[] { '\t', ' ' , '-'},
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        int len = bytes.Length;
        byte[] data = new byte[len];
        for (int i = 0; i < len; i++)
        {
            String bs = bytes[i].Length == 1 ? "0" + bytes[i] : bytes[i];
            data[i] = (byte)((Uri.FromHex(bs[0]) << 4) + Uri.FromHex(bs[1]));
        }

        return data;
    }
}
namespace Garmin.Ble.Lib.Messages;

public class DirectoryData
{
    public List<DirectoryEntry> entries;

    public DirectoryData(List<DirectoryEntry> entries)
    {
        this.entries = entries;
    }

    public static DirectoryData parse(ReadOnlySpan<byte> bytes)
    {
        int size = bytes.Length;
        if ((size % 16) != 0) throw new ArgumentException("Invalid directory data length");
        int count = (size - 16) / 16;
        MessageReader reader = new MessageReader(ref bytes, 16);
        List<DirectoryEntry> entries = new();
        for (int i = 0; i < count; ++i)
        {
            int fileIndex = reader.readShort();
            int fileDataType = reader.readByte();
            int fileSubType = reader.readByte();
            int fileNumber = reader.readShort();
            int specificFlags = reader.readByte();
            int fileFlags = reader.readByte();
            int fileSize = reader.readInt();

            var fileDate = DateTimeOffset.FromUnixTimeSeconds((reader.readInt() + GarminConstants.GARMIN_TIME_EPOCH));
            // DateTime fileDate = new DateTime(GarminTimeUtils.garminTimestampToJavaMillis(reader.readInt()));

            entries.Add(new DirectoryEntry(fileIndex, fileDataType, fileSubType, fileNumber, specificFlags, fileFlags,
                fileSize, fileDate.DateTime));
        }

        return new DirectoryData(entries);
    }
}
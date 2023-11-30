namespace Garmin.Ble.Lib.Messages;

public class DirectoryEntry
{
    public int fileIndex;
    public int fileDataType;
    public int fileSubType;
    public int fileNumber;
    public int specificFlags;
    public int fileFlags;
    public int fileSize;
    public DateTime fileDate;

    public DirectoryEntry(int fileIndex, int fileDataType, int fileSubType, int fileNumber, int specificFlags,
        int fileFlags, int fileSize, DateTime fileDate)
    {
        this.fileIndex = fileIndex;
        this.fileDataType = fileDataType;
        this.fileSubType = fileSubType;
        this.fileNumber = fileNumber;
        this.specificFlags = specificFlags;
        this.fileFlags = fileFlags;
        this.fileSize = fileSize;
        this.fileDate = fileDate;
    }
}
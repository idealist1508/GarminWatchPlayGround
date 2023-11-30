namespace Garmin.Ble.Lib.Messages;

public class SyncRequestMessage {
    public static int OPTION_MANUAL = 0;
    public static int OPTION_INVISIBLE = 1;
    public static int OPTION_VISIBLE_AS_NEEDED = 2;

    public int option;
    public HashSet<GarminConstants.GarminMessageType> fileTypes;

    public SyncRequestMessage(int option, HashSet<GarminConstants.GarminMessageType> fileTypes) {
        this.option = option;
        this.fileTypes = fileTypes;
    }

    public static SyncRequestMessage parsePacket(ReadOnlySpan<byte> packet) {
        var reader = new MessageReader(ref packet, 4);
        var option = reader.readByte();
        var bitMaskSize = reader.readByte();
        var longBits = reader.readBytesTo(bitMaskSize, new byte[8], 0);
        var bitMask = BinaryUtils.ReadLong(longBits, 0);

        var fileTypes = new HashSet<GarminConstants.GarminMessageType>();
        foreach (var messageType in Enum.GetValues<GarminConstants.GarminMessageType>()){
            var mask = 1L << (int)messageType;
            if ((bitMask & mask) != 0) {
                fileTypes.Add(messageType);
                bitMask &= ~mask;
            }
        }
        if (bitMask != 0) {
            throw new IllegalStateException("Unknown bit mask " + BitConverter.ToSingle(longBits, 0));
        }

        return new SyncRequestMessage(option, fileTypes);
    }
}
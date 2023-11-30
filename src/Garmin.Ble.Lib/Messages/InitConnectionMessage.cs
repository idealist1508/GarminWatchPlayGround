namespace Garmin.Ble.Lib.Messages;

public class InitConnectionMessage {
    public readonly byte[] Packet;

    public InitConnectionMessage(){
        var writer = new MessageWriter(13);
        writer.WriteByte(0);
        writer.WriteByte(5);
        writer.WriteByte(1);
        writer.WriteLong(0);
        writer.WriteShort(0);
        Packet = writer.GetBytes();
    }
}
namespace Garmin.Ble.Lib.Messages;

public class ResponseMessage: IGfdiMessage {
    public byte[] Packet { get; }

    public readonly int RequestId;
    public readonly int Status;
    
    public ResponseMessage(int requestId, int status) {
        this.RequestId = requestId;
        this.Status = status;
        MessageWriter writer = new MessageWriter(9);
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(GarminConstants.MESSAGE_RESPONSE);
        writer.WriteShort(requestId);
        writer.WriteByte(status);
        writer.WriteShort(0); // CRC will be filled below
        byte[] packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        this.Packet = packet;
    }

    public static ResponseMessage ParsePacket(ReadOnlySpan<byte> packet) {
        //skip Length (2Byte) and MessageId (2Bytes)
        var reader = new MessageReader(ref packet, 4);
        var requestId = reader.readShort();
        var status = reader.readByte();

        return new ResponseMessage(requestId, status);
    }

    public String getStatusStr() {
        switch (Status) {
            case GarminConstants.STATUS_ACK:
                return "ACK";
            case GarminConstants.STATUS_NAK:
                return "NAK";
            case GarminConstants.STATUS_UNSUPPORTED:
                return "UNSUPPORTED";
            case GarminConstants.STATUS_DECODE_ERROR:
                return "DECODE ERROR";
            case GarminConstants.STATUS_CRC_ERROR:
                return "CRC ERROR";
            case GarminConstants.STATUS_LENGTH_ERROR:
                return "LENGTH ERROR";
            default:
                return String.Format("Unknown status %x", Status);
        }
    }

    public override string ToString()
    {
        return $"{nameof(ResponseMessage)}: RequestId = {RequestId}, Status = {getStatusStr()}";
    }
}
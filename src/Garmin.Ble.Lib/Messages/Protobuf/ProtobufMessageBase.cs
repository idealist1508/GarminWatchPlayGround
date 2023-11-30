namespace Garmin.Ble.Lib.Messages;

public class ProtobufMessageBase {
    public ProtobufMessageBase()
    {
    }

    public int RequestId { get; set;  }
    public int DataOffset { get; set;  }
    public int TotalProtobufLength { get; set;  }
    public int ProtobufDataLength { get; set;  }
    public byte[] MessageBytes { get; set;  }
    public int MessageId { get; private set; }

    public ProtobufMessageBase(int messageId, int requestId, int dataOffset, int totalProtobufLength, int protobufDataLength, ReadOnlySpan<byte> messageBytes) {
        RequestId = requestId;
        DataOffset = dataOffset;
        TotalProtobufLength = totalProtobufLength;
        ProtobufDataLength = protobufDataLength;
        MessageBytes = messageBytes.ToArray();
        MessageId = messageId;

    }

    public byte[] BuildPacket()
    {
        var writer = new MessageWriter();
        writer.WriteShort(0); // packet size will be filled below
        writer.WriteShort(MessageId);
        writer.WriteShort(RequestId);
        writer.WriteInt(DataOffset);
        writer.WriteInt(TotalProtobufLength);
        writer.WriteInt(ProtobufDataLength);
        writer.WriteBytes(MessageBytes);
        writer.WriteShort(0); // CRC will be filled below
        var packet = writer.GetBytes();
        BinaryUtils.WriteShort(packet, 0, packet.Length);
        BinaryUtils.WriteShort(packet, packet.Length - 2, Crc.Calc(packet, 0, packet.Length - 2));
        return packet;
    }

    public static T parsePacket<T>(ReadOnlySpan<byte> packet) where T:ProtobufMessageBase, new()
    {
        var reader = new MessageReader(ref packet, 2);
        var messageId = reader.readShort();
        var requestID = reader.readShort();
        var dataOffset = reader.readInt();
        var totalProtobufLength= reader.readInt();
        var protobufDataLength = reader.readInt();
        var messageBytes = reader.readBytes(protobufDataLength);
        return new T{MessageId = messageId, RequestId = requestID, DataOffset = dataOffset, TotalProtobufLength = totalProtobufLength, ProtobufDataLength = protobufDataLength, MessageBytes = messageBytes.ToArray()};
    }

    public override string ToString()
    {
        return $"messageid: {MessageId}, requestId: {RequestId}, dataoffset: {DataOffset}, totalProtobufLength: {TotalProtobufLength}, protobufDatLength: {ProtobufDataLength}, messageByte = {BitConverter.ToString(MessageBytes)}";
    }
}
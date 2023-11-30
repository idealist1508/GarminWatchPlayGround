using System.Text;

namespace Garmin.Ble.Lib.Messages;

public ref struct MessageReader {
    private readonly ReadOnlySpan<byte> data;
    public int Position { get; private set; }

    public MessageReader(ReadOnlySpan<byte> data)
    {
        Position = 0;
        this.data = data;
    }

    public MessageReader(ref ReadOnlySpan<Byte> data, int skipOffset) {
        this.data = data;
        this.Position = skipOffset;
    }

    public bool Eof() {
        return Position >= data.Length;
    }

    public void skip(int offset) {
        if (Position + offset > data.Length) throw new IllegalStateException();
        Position += offset;
    }

    public int readByte() {
        if (Position + 1 > data.Length) throw new IllegalStateException();
        var result = BinaryUtils.ReadByte(data, Position);
        ++Position;
        return result;
    }

    public int readShort() {
        if (Position + 2 > data.Length) throw new IllegalStateException();
        var result = BinaryUtils.ReadShort(data, Position);
        Position += 2;
        return result;
    }

    public int readInt() {
        if (Position + 4 > data.Length) throw new IllegalStateException();
        var result = BinaryUtils.ReadInt(data, Position);
        Position += 4;
        return result;
    }

    public long readLong() {
        if (Position + 8 > data.Length) throw new IllegalStateException();
        var result = BinaryUtils.ReadLong(data, Position);
        Position += 8;
        return result;
    }

    public String readString() {
        var size = readByte();
        if (Position + size > data.Length) throw new IllegalStateException();
        var result = Encoding.UTF8.GetString(data.ToArray(), Position, size);
        Position += size;
        return result;
    }

    public ReadOnlySpan<byte> readBytes(int size) {
        if (Position + size > data.Length) throw new IllegalStateException();
        var result = data.Slice(Position, size);
        Position += size;
        return result;
    }

    public byte[] readBytesTo(int size, byte[] buffer, int offset) {
        if (offset + size > buffer.Length) throw new ArgumentException("offset + size > buffer.Length");
        if (Position + size > data.Length) throw new IllegalStateException();
        
        Array.Copy(data.ToArray(), Position, buffer, offset, size);
        Position += size;
        return buffer;
    }
}
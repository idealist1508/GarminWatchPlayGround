using System.Text;

namespace Garmin.Ble.Lib;

public class MessageWriter {
    private const int DefaultBufferSize = 16384;

    private readonly byte[] _buffer;
    private int _position;

    public MessageWriter() : this(DefaultBufferSize){
    }

    public MessageWriter(int bufferSize) {
        this._buffer = new byte[bufferSize];
    }

    public void WriteByte(int value) {
        if (_position + 1 > _buffer.Length) throw new Exception();
        BinaryUtils.WriteByte(_buffer, _position, value);
        ++_position;
    }

    public void WriteShort(int value) {
        if (_position + 2 > _buffer.Length) throw new Exception();
        BinaryUtils.WriteShort(_buffer, _position, value);
        _position += 2;
    }

    public void WriteInt(int value) {
        if (_position + 4 > _buffer.Length) throw new Exception();
        BinaryUtils.WriteInt(_buffer, _position, value);
        _position += 4;
    }

    public void WriteLong(long value) {
        if (_position + 8 > _buffer.Length) throw new Exception();
        BinaryUtils.WriteLong(_buffer, _position, value);
        _position += 8;
    }

    public void WriteString(string value) {
        var bytes = Encoding.UTF8.GetBytes(value);
        var size = bytes.Length;
        if (size > 255) throw new Exception("Too long string");
        if (_position + 1 + size > _buffer.Length) throw new Exception();
        WriteByte(size);
        Array.Copy(bytes, 0, _buffer, _position, size);
        _position += size;
    }

    public byte[] GetBytes()
    {
        if (_position == _buffer.Length)
            return _buffer;
        else
        {
            var result = (byte[])_buffer.Clone();
            Array.Resize(ref result, _position);
            return result;
        }
    }

    public byte[] PeekBytes() {
        return _buffer;
    }

    public int GetSize() {
        return _position;
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes) {
        WriteBytes(bytes, 0, bytes.Length);
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes, int offset, int size) {
        if (_position + size > _buffer.Length) throw new Exception();
        Array.Copy(bytes.ToArray(), offset, _buffer, _position, size);
        _position += size;
    }
}
namespace Garmin.Ble.Lib;

/// <summary>
/// Parser of GFDI messages embedded in COBS packets.
/// 
/// COBS ensures there are no embedded NUL bytes inside the packet data, and wraps the message into NUL framing bytes.
/// </summary>
public class GfdiPacketParser
{
    private readonly ILogger _log;

    private static readonly long _bufferTimeout = 1500L;

    private MemoryStream _buffer;
    private byte[]? _packet;
    private MemoryStream _packetBuffer;
    private int _bufferPos;
    private DateTime _lastUpdate;
    private bool _insidePacket;

    public GfdiPacketParser(ILogger log)
    {
        _log = log;
        _buffer = new MemoryStream();
        _packetBuffer = new MemoryStream();
    }

    public void Reset()
    {
        _buffer = new MemoryStream();
        _bufferPos = 0;
        _insidePacket = false;
        _packet = null;
        _packetBuffer = new MemoryStream();
    }

    public void ReceivedBytes(ReadOnlySpan<byte> bytes)
    {
        var now = DateTime.Now;
        if ((now - _lastUpdate).TotalMilliseconds > _bufferTimeout)
        {
            Reset();
        }

        _lastUpdate = now;
        var bufferSize = _buffer.Length;
        _buffer.Position = _buffer.Length;
        _buffer.Write(bytes);
        ParseBuffer();
    }

    public byte[]? RetrievePacket()
    {
        var resultPacket = _packet;
        _packet = null;
        ParseBuffer();
        return resultPacket;
    }

    private void ParseBuffer()
    {
        if (_packet != null)
        {
            // packet is waiting, unable to parse more
            return;
        }

        if (_bufferPos >= _buffer.Length)
        {
            // nothing to parse
            return;
        }

        var startOfPacket = !_insidePacket;
        if (startOfPacket)
        {
            byte b;
            _buffer.Position = _bufferPos; //todo check.
            var unexpectedNonZeroByteWasFound = false;
            while (_bufferPos < _buffer.Length && (b = (byte)_buffer.ReadByte()) != 0 & _bufferPos++ > 0)
            {
                unexpectedNonZeroByteWasFound = true; 
                if (_log.IsDebugEnabled())
                {
                    _log.Debug("Unexpected non-zero byte while looking for framing: {0}", b.ToString("X"));
                }
            }

            if (unexpectedNonZeroByteWasFound)
            {
                throw new InvalidDataException("Unexpected non-zero byte while looking for framing.");
            }

            if (_bufferPos >= _buffer.Length)
            {
                // nothing to parse
                return;
            }

            _insidePacket = true;
        }

        var endedWithFullChunk = false;
        while (_bufferPos < _buffer.Length)
        {
            var chunkSize = -1;
            var chunkStart = _bufferPos;
            var pos = _bufferPos;
            _buffer.Position = pos; //todo: check
            while (pos < _buffer.Length && ((chunkSize = (_buffer.ReadByte() & 0xFF)) == 0 & pos++ > 0) &&
                   startOfPacket)
            {
                // skip repeating framing bytes (?)
                _bufferPos = pos;
                chunkStart = pos;
            }

            if (startOfPacket && pos >= _buffer.Length)
            {
                // incomplete framing, needs to wait for more data and try again
                _buffer = new MemoryStream();
                _buffer.WriteByte(0);
                _bufferPos = 0;
                _insidePacket = false;
                return;
            }

            if (chunkSize < 0)
                throw new Exception("chunkSize < 0");

            if (chunkSize == 0)
            {
                // end of packet
                // drop the last zero
                if (endedWithFullChunk)
                {
                    // except when it was explicitly added (TODO: ugly, is it correct?)
                    _packet = _packetBuffer.ToArray();
                }
                else
                {
                    _packetBuffer.SetLength(_packetBuffer.Length - 1);
                    _packet = _packetBuffer.ToArray();
                    // Array.Resize(ref _packet, _packetBuffer.Length - 1);
                }

                _packetBuffer = new MemoryStream();
                _insidePacket = false;

                if (_bufferPos == _buffer.Length - 1)
                {
                    _buffer = new MemoryStream();
                    _bufferPos = 0;
                }
                else
                {
                    // TODO: Re-alloc buffer down
                    ++_bufferPos;
                }

                return;
            }

            if (chunkStart + chunkSize > _buffer.Length)
            {
                // incomplete chunk, needs to wait for more data
                // _log.Debug($"Packetbuffer so far: {BitConverter.ToString(_packetBuffer.ToArray())}");
                return;
            }

            // completed chunk
            var packetPos = _packetBuffer.Length;
            var realChunkSize = chunkSize < 255 ? chunkSize : chunkSize - 1;
            _packetBuffer.SetLength(packetPos + realChunkSize);
            _packetBuffer.Position = packetPos;
            _packetBuffer.Write(_buffer.GetBuffer().AsSpan(chunkStart + 1, chunkSize - 1));
            _bufferPos = chunkStart + chunkSize;

            endedWithFullChunk = chunkSize == 255;
            startOfPacket = false;
        }
    }

    public static byte[] WrapMessageToPacket(ReadOnlySpan<byte> message)
    {
        var outputStream =
            new ByteArrayOutputStream(message.Length + 2 + (message.Length + 253) / 254);
        outputStream.WriteAsByte(0);
        var chunkStart = 0;
        for (var i = 0; i < message.Length; ++i)
        {
            if (message[i] == 0)
            {
                chunkStart = AppendChunk(message, outputStream, chunkStart, i);
            }
        }

        if (chunkStart <= message.Length)
        {
            AppendChunk(message, outputStream, chunkStart, message.Length);
        }

        outputStream.WriteAsByte(0);
        return outputStream.ToArray();
    }

    private static int AppendChunk(ReadOnlySpan<byte> message, ByteArrayOutputStream outputStream, int chunkStart, int messagePos)
    {
        var chunkLength = messagePos - chunkStart;
        while (true)
        {
            if (chunkLength >= 255)
            {
                // write 255-byte chunk
                outputStream.WriteAsByte(255);
                outputStream.Write(message.Slice(chunkStart, 254));
                chunkLength -= 254;
                chunkStart += 254;
            }
            else
            {
                // write chunk from chunkStart to here
                outputStream.WriteAsByte(chunkLength + 1);
                outputStream.Write(message.Slice(chunkStart, chunkLength));
                chunkStart = messagePos + 1;
                break;
            }
        }

        return chunkStart;
    }
}
using System.Collections.Concurrent;
using Ble.Interfaces;
using Garmin.Ble.Lib.Messages;

namespace Garmin.Ble.Lib.Devices.Garmin;

public class GfdiCommunicator: ICommunicator
{
    private readonly IGattCharacteristic _outCharacteristic;
    private readonly byte _gfdiChannelId;
    private readonly ILogger _log;
    private readonly BlockingCollection<(byte[], string)> _gfdiMessageQueue = new();
    private CancellationTokenSource _sendGfdiMessageCts = new();

    private void OnStart()
    {
        foreach (var message in _gfdiMessageQueue.GetConsumingEnumerable(CancellationToken.None))
        {
            _log.DebugInZone(DebugZones.MessageSend, $"Sending... {message.Item2}");
            SendGfdiMessage(message.Item1).GetAwaiter().GetResult();
        }
    } 
    public GfdiCommunicator(byte gfdiChannelId, ILogger log, IGattCharacteristic outCharacteristic)
    {
        _gfdiChannelId = gfdiChannelId;
        _log = log;
        _outCharacteristic = outCharacteristic;
        var thread = new Thread(OnStart)
        {
            IsBackground = true
        };
        thread.Start();
    }

    public void PostGfdiMessage(byte[] messageBytes)
    {
        _gfdiMessageQueue.Add((messageBytes, ""));
    }

    public void PostGfdiMessage(IGfdiMessage message)
    {
        _gfdiMessageQueue.Add((message.Packet, message.ToString()??""));
    }
    public void PostImmideatlyGfdiMessage(IGfdiMessage message)
    {
        //todo:insert
        // _gfdiMessageQueue. ((message.Packet, message.ToString()??""));
    }

    private async Task SendGfdiMessage(byte[] message)
    {
        _sendGfdiMessageCts = new CancellationTokenSource();
        await SendGfdiMessage(message, _sendGfdiMessageCts.Token);
    }

    private async Task SendGfdiMessage(byte[] messageBytes, CancellationToken ct)
    {
        _log.DebugInZone(DebugZones.MessageSendBytes, $"Sending GFDI message: {BitConverter.ToString(messageBytes)}");
        var ch = await _outCharacteristic.GetUUIDAsync();
        var packet = GfdiPacketParser.WrapMessageToPacket(messageBytes);
        _log.DebugInZone(DebugZones.PackageSend, $"Sending GFDI package: {BitConverter.ToString(packet)}");
        var remainingBytes = packet.Length;
        var position = 0;
        while (remainingBytes > 0)
        {
            if (ct.IsCancellationRequested) return;
            
            var fragmentStream = new MemoryStream();
            fragmentStream.WriteByte(_gfdiChannelId);
            var count = Math.Min(remainingBytes, GarminConstants.MAX_WRITE_SIZE - 1);
            fragmentStream.Write(packet, position, count);
            var bytes = fragmentStream.ToArray();
            _log.DebugInZone(DebugZones.BytesBleSend, $"write to ch {ch} bytes: {BitConverter.ToString(bytes)}");
            
            if (ct.IsCancellationRequested) return;
            
            await _outCharacteristic.WriteValueAsync(bytes, new Dictionary<string, object>());
            position += count;
            remainingBytes -= count;
        }
    }

    public void Cancel()
    {
        _sendGfdiMessageCts.Cancel();
    }
}
namespace Garmin.Ble.Lib.Messages;

public class RegisterForServiceRequest
{
    public readonly byte[] Packet;

    public RegisterForServiceRequest(ServiceType serviceType)
    {
        var writer = new MessageWriter(13);
        writer.WriteByte(0);
        writer.WriteByte(0);
        const int clientId = 1;
        writer.WriteLong(clientId);
        writer.WriteByte((int)serviceType);
        writer.WriteByte(0);
        writer.WriteByte(0);
        Packet = writer.GetBytes();
    }

}
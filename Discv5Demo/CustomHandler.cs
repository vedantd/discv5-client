using Lantern.Discv5.WireProtocol.Messages;
using System.Text;



public class CustomHandler : ITalkReqAndRespHandler
{
    public byte[][] HandleRequest(byte[] protocol, byte[] request)
    {
        return new[] { request };
    }

    public byte[] HandleResponse(byte[] response)
    {
        return response;
    }
    public string? LastResponse { get; private set; }
}

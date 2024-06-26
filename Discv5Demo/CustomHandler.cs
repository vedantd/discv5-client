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
        Console.WriteLine("Received TalkResp: {0}", Encoding.UTF8.GetString(response));
        return response;
    }
}

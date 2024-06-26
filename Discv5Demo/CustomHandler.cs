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
        // Log the raw bytes and their length
        Console.WriteLine("Received TalkResp (raw bytes): {0}", BitConverter.ToString(response));
        Console.WriteLine("Received TalkResp (length): {0}", response.Length);
        
        // Convert the response to a string and store it
        var responseString = Encoding.UTF8.GetString(response);
        Console.WriteLine("Received TalkResp (decoded): {0}", responseString);
        
        LastResponse = responseString;
        return response;
    }
    public string? LastResponse { get; private set; }
}

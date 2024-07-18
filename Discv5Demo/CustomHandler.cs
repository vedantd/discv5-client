using Lantern.Discv5.WireProtocol.Messages;
using System.Text;


public class CustomHandler : ITalkReqAndRespHandler
{
    public byte[][] HandleRequest(byte[] protocol, byte[] request)
    {
        // Deserialize the request and handle it accordingly
        // var messageUnion = SSZ.Deserialize<MessageUnion>(request);
        // if (messageUnion != null && messageUnion.Selector == 0)
        // {
        //     // Handle Ping message
        //     var pingMessage = (Ping)messageUnion.Value;
        //     // Respond with a Pong message
        //     var pongMessage = new Pong(pingMessage.EnrSeq, pingMessage.CustomPayload);
        //     var responseUnion = new MessageUnion(1, pongMessage);
        //     var serializedResponse = SSZ.Serialize(responseUnion);
        //     return new[] { serializedResponse };
        // }
        return new[] { request };
    }

    public byte[] HandleResponse(byte[] response)
    {
        // Deserialize the response and handle it accordingly
        // var messageUnion = SSZ.Deserialize<MessageUnion>(response);
        // if (messageUnion != null && messageUnion.Selector == 1)
        // {
        //     // Handle Pong message
        //     var pongMessage = (Pong)messageUnion.Value;
        //     LastResponse = $"Pong received: ENR Seq {pongMessage.EnrSeq}";
        //     Console.WriteLine(LastResponse);
        // }
        return response;
    }

    public string? LastResponse { get; private set; }
}

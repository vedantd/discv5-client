using Lantern.Discv5.WireProtocol.Messages;
using System;

public class CustomHandler : ITalkReqAndRespHandler
{
    private const ushort PROTOCOL_ID = 0x500A; // Execution State Network

    public byte[][]? HandleRequest(byte[] protocol, byte[] request)
    {
        if (BitConverter.ToUInt16(protocol, 0) == PROTOCOL_ID)
        {
            var messageUnion = (MessageUnion)SSZ.Deserialize(request, typeof(MessageUnion));
            if (messageUnion.Selector == 0) // Ping
            {
                var ping = (Ping)messageUnion.Value;
                Console.WriteLine($"Received Ping with ENR sequence number: {ping.EnrSeq}");
                
                // Create and serialize Pong response
                var pong = new Pong(ping.EnrSeq, ping.CustomPayload);
                var pongMessage = new MessageUnion(1, pong);
                var serializedResponse = SSZ.Serialize(pongMessage);
                return new byte[][] { serializedResponse };
            }
            else
            {
                Console.WriteLine("Received unexpected message type");
                return null;
            }
        }
        else
        {
            Console.WriteLine($"Received request with unknown protocol: 0x{BitConverter.ToUInt16(protocol, 0):X4}");
            return null;
        }
    }

    public byte[]? HandleResponse(byte[] response)
    {
        var messageUnion = (MessageUnion)SSZ.Deserialize(response, typeof(MessageUnion));
        if (messageUnion.Selector == 1) // Pong
        {
            var pong = (Pong)messageUnion.Value;
            Console.WriteLine($"Received Pong with ENR sequence number: {pong.EnrSeq}");
            return response;
        }
        else
        {
            Console.WriteLine("Received unexpected response type");
            return null;
        }
    }
}
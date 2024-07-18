using System;
using Nethermind.Serialization.Ssz;

public class Pong
{
    public ulong EnrSeq { get; set; }
    public byte[] CustomPayload { get; set; }

    public Pong(ulong enrSeq, byte[] customPayload)
    {
        EnrSeq = enrSeq;
        CustomPayload = customPayload;
    }
}

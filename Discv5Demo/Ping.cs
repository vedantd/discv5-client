using System;

public class Ping
{
    public ulong EnrSeq { get; set; }
    public byte[] CustomPayload { get; set; }

    public Ping(ulong enrSeq, byte[] customPayload)
    {
        EnrSeq = enrSeq;
        CustomPayload = customPayload;
    }
}

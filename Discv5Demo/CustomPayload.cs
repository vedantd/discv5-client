using System;
public class CustomPayload
{
    public byte[] DataRadius { get; set; }

    public CustomPayload(byte[] dataRadius)
    {
        DataRadius = dataRadius;
    }
}

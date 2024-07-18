using System;
using Nethermind.Serialization.Ssz;


public class MessageUnion
{
    public int Selector { get; set; }
    public object Value { get; set; }

    public MessageUnion(int selector, object value)
    {
        Selector = selector;
        Value = value;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

public static class SSZ
{
    private const int BYTES_PER_LENGTH_OFFSET = 4;

    public static byte[] SerializeUInt(ulong value, int size)
    {
        if (!new[] { 8, 16, 32, 64, 128, 256 }.Contains(size))
            throw new ArgumentException("Invalid size for uint serialization.");

        return BitConverter.GetBytes(value).Take(size / 8).ToArray();
    }

    public static byte[] SerializeContainer(params object[] container)
    {
        List<byte[]> fixedParts = new List<byte[]>();
        List<byte[]> variableParts = new List<byte[]>();

        foreach (var field in container)
        {
            var serialized = Serialize(field);
            if (field is byte[])
            {
                variableParts.Add(serialized);
                fixedParts.Add(new byte[BYTES_PER_LENGTH_OFFSET]); // Empty array instead of null
            }
            else
            {
                fixedParts.Add(serialized);
            }
        }

        int fixedLengthSum = fixedParts.Sum(part => part.Length);
        int currentOffset = fixedLengthSum;

        for (int i = 0; i < fixedParts.Count; i++)
        {
            if (fixedParts[i].Length == BYTES_PER_LENGTH_OFFSET && fixedParts[i].All(b => b == 0))
            {
                fixedParts[i] = SerializeUInt((ulong)currentOffset, 32);
                currentOffset += variableParts[0].Length;
            }
        }

        return fixedParts.Concat(variableParts).SelectMany(x => x).ToArray();
    }

    public static byte[] Serialize(object value)
    {
        switch (value)
        {
            case ulong u:
                return SerializeUInt(u, 64);
            case byte[] b:
                return b;
            case Ping p:
                return SerializeContainer(p.EnrSeq, p.CustomPayload);
            case Pong p:
                return SerializeContainer(p.EnrSeq, p.CustomPayload);
            case MessageUnion m:
                return SerializeUInt((ulong)m.Selector, 8).Concat(Serialize(m.Value)).ToArray();
            default:
                throw new ArgumentException("Unsupported type for serialization");
        }
    }

    public static ulong DeserializeUInt(byte[] data, int size)
    {
        if (!new[] { 8, 16, 32, 64, 128, 256 }.Contains(size))
            throw new ArgumentException("Invalid size for uint deserialization.");

        Array.Resize(ref data, 8);
        return BitConverter.ToUInt64(data, 0);
    }

public static T? DeserializeContainer<T>(byte[] data) where T : class
{
    var type = typeof(T);
    var properties = type.GetProperties();
    var constructorParams = new object[properties.Length];
    int offset = 0;

    Console.WriteLine($"Deserializing {type.Name}. Data length: {data.Length}");

    for (int i = 0; i < properties.Length; i++)
    {
        var property = properties[i];
        Console.WriteLine($"Processing property: {property.Name}");
        
        if (property.PropertyType == typeof(ulong))
        {
            constructorParams[i] = DeserializeUInt(data.Skip(offset).Take(8).ToArray(), 64);
            Console.WriteLine($"Deserialized ulong: {constructorParams[i]}");
            offset += 8;
        }
        else if (property.PropertyType == typeof(byte[]))
        {
            var lengthOffset = (int)DeserializeUInt(data.Skip(offset).Take(4).ToArray(), 32);
            Console.WriteLine($"Length offset: {lengthOffset}");
            var length = data.Length - lengthOffset;  // Calculate length correctly
            Console.WriteLine($"Calculated length: {length}");
            constructorParams[i] = data.Skip(lengthOffset).Take(length).ToArray();
            Console.WriteLine($"Deserialized byte array length: {((byte[])constructorParams[i]).Length}");
            offset += 4;
        }
    }

    var result = Activator.CreateInstance(typeof(T), constructorParams) as T;
    if (result == null)
    {
        Console.WriteLine($"Failed to create instance of {type.Name}");
        return null;
    }
    return result;
}
    public static object? Deserialize(byte[] data, Type type)
    {
        Console.WriteLine($"Deserializing type: {type.Name}. Data length: {data.Length}");
    
        if (type == typeof(ulong))
            return DeserializeUInt(data, 64);
        if (type == typeof(byte[]))
            return data;
        if (type == typeof(Ping))
            return DeserializeContainer<Ping>(data);
        if (type == typeof(Pong))
            return DeserializeContainer<Pong>(data);
        if (type == typeof(MessageUnion))
        {
            var selector = (int)DeserializeUInt(data.Take(1).ToArray(), 8);
            Console.WriteLine($"MessageUnion selector: {selector}");
            var messageData = data.Skip(1).ToArray();
            Console.WriteLine($"Message data length: {messageData.Length}");
            object? value = selector == 0 
                ? DeserializeContainer<Ping>(messageData)
                : DeserializeContainer<Pong>(messageData);
            return value != null ? new MessageUnion(selector, value) : null;
        }
        throw new ArgumentException("Unsupported type for deserialization");
    }
}
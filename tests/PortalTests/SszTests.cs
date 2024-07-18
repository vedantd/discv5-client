using NUnit.Framework;
using System;
using System.Linq;

[TestFixture]
public class SszTests
{
    [Test]
    public void SerializePingMessage()
    {
        ulong enrSeq = 1;
        var dataRadiusBytes = new byte[32];
        dataRadiusBytes[0] = 0xFE;
        for (int i = 1; i < 32; i++) dataRadiusBytes[i] = 0xFF;
        var ping = new Ping(enrSeq, dataRadiusBytes);
        var message = new MessageUnion(0, ping);

        var serialized = SSZ.Serialize(message);

        var expectedOutput = "0001000000000000000c000000feffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        var expectedBytes = StringToByteArray(expectedOutput);

        Console.WriteLine($"Expected Output (Hex): {BitConverter.ToString(expectedBytes).Replace("-", "")}");
        Console.WriteLine($"Actual Output (Hex): {BitConverter.ToString(serialized).Replace("-", "")}");
        Console.WriteLine($"Expected Length: {expectedBytes.Length}");
        Console.WriteLine($"Actual Length: {serialized.Length}");

        Assert.AreEqual(expectedBytes, serialized);
    }

    [Test]
    public void SerializePongMessage()
    {
        ulong enrSeq = 1;
        var dataRadiusBytes = new byte[32];
        for (int i = 0; i < 31; i++) dataRadiusBytes[i] = 0xFF;
        dataRadiusBytes[31] = 0x7F;
        var pong = new Pong(enrSeq, dataRadiusBytes);
        var message = new MessageUnion(1, pong);

        var serialized = SSZ.Serialize(message);

        var expectedOutput = "0101000000000000000c000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f";
        var expectedBytes = StringToByteArray(expectedOutput);

        Console.WriteLine($"Expected Output (Hex): {BitConverter.ToString(expectedBytes).Replace("-", "")}");
        Console.WriteLine($"Actual Output (Hex): {BitConverter.ToString(serialized).Replace("-", "")}");
        Console.WriteLine($"Expected Length: {expectedBytes.Length}");
        Console.WriteLine($"Actual Length: {serialized.Length}");

        Assert.AreEqual(expectedBytes, serialized);
    }

    private static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length / 2)
                         .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                         .ToArray();
    }

    [Test]
    public void DeserializePingMessage()
    {
        var input = "0001000000000000000c000000feffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        var inputBytes = StringToByteArray(input);

        var deserialized = (MessageUnion)SSZ.Deserialize(inputBytes, typeof(MessageUnion));

        Assert.AreEqual(0, deserialized.Selector);
        Assert.IsInstanceOf<Ping>(deserialized.Value);

        var ping = (Ping)deserialized.Value;
        Assert.AreEqual(1UL, ping.EnrSeq);

        var expectedCustomPayload = new byte[32];
        expectedCustomPayload[0] = 0xFE;
        for (int i = 1; i < 32; i++) expectedCustomPayload[i] = 0xFF;
        Assert.AreEqual(expectedCustomPayload, ping.CustomPayload);
    }
    [Test]
    public void DeserializePongMessage()
    {
        var input = "0101000000000000000c000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f";
        var inputBytes = StringToByteArray(input);

        var deserialized = (MessageUnion)SSZ.Deserialize(inputBytes, typeof(MessageUnion));

        Assert.AreEqual(1, deserialized.Selector);
        Assert.IsInstanceOf<Pong>(deserialized.Value);

        var pong = (Pong)deserialized.Value;
        Assert.AreEqual(1UL, pong.EnrSeq);

        var expectedCustomPayload = new byte[32];
        for (int i = 0; i < 31; i++) expectedCustomPayload[i] = 0xFF;
        expectedCustomPayload[31] = 0x7F;
        Assert.AreEqual(expectedCustomPayload, pong.CustomPayload);
    }

    [Test]
    public void SerializeDeserializePingMessageRoundTrip()
    {
        ulong enrSeq = 1;
        var dataRadiusBytes = new byte[32];
        dataRadiusBytes[0] = 0xFE;
        for (int i = 1; i < 32; i++) dataRadiusBytes[i] = 0xFF;
        var ping = new Ping(enrSeq, dataRadiusBytes);
        var message = new MessageUnion(0, ping);

        var serialized = SSZ.Serialize(message);
        var deserialized = (MessageUnion)SSZ.Deserialize(serialized, typeof(MessageUnion));

        Assert.AreEqual(message.Selector, deserialized.Selector);
        Assert.IsInstanceOf<Ping>(deserialized.Value);

        var deserializedPing = (Ping)deserialized.Value;
        Assert.AreEqual(ping.EnrSeq, deserializedPing.EnrSeq);
        Assert.AreEqual(ping.CustomPayload, deserializedPing.CustomPayload);
    }


    [Test]
    public void SerializeDeserializePongMessageRoundTrip()
    {
        ulong enrSeq = 1;
        var dataRadiusBytes = new byte[32];
        for (int i = 0; i < 31; i++) dataRadiusBytes[i] = 0xFF;
        dataRadiusBytes[31] = 0x7F;
        var pong = new Pong(enrSeq, dataRadiusBytes);
        var message = new MessageUnion(1, pong);

        var serialized = SSZ.Serialize(message);
        var deserialized = (MessageUnion)SSZ.Deserialize(serialized, typeof(MessageUnion));

        Assert.AreEqual(message.Selector, deserialized.Selector);
        Assert.IsInstanceOf<Pong>(deserialized.Value);

        var deserializedPong = (Pong)deserialized.Value;
        Assert.AreEqual(pong.EnrSeq, deserializedPong.EnrSeq);
        Assert.AreEqual(pong.CustomPayload, deserializedPong.CustomPayload);
    }

    
}
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lantern.Discv5.Enr;
using Lantern.Discv5.Enr.Entries;
using Lantern.Discv5.WireProtocol;
using Lantern.Discv5.WireProtocol.Connection;
using Lantern.Discv5.WireProtocol.Session;
using Lantern.Discv5.WireProtocol.Table;
using Lantern.Discv5.WireProtocol.Utility;



class Program
{
    private const ushort PROTOCOL_ID = 0x500A; // Execution State Network

    static async Task Main()
    {
        var bootstrapEnrs = new[]
        {
            // Add your bootstrap ENR strings here
            "enr:-Ku4QImhMc1z8yCiNJ1TyUxdcfNucje3BGwEHzodEZUan8PherEo4sF7pPHPSIB1NNuSg5fZy7qFsjmUKs2ea1Whi0EBh2F0dG5ldHOIAAAAAAAAAACEZXRoMpD1pf1CAAAAAP__________gmlkgnY0gmlwhBLf22SJc2VjcDI1NmsxoQOVphkDqal4QzPMksc5wnpuC3gvSC8AfbFOnZY_On34wIN1ZHCCIyg",
            "enr:-Le4QPUXJS2BTORXxyx2Ia-9ae4YqA_JWX3ssj4E_J-3z1A-HmFGrU8BpvpqhNabayXeOZ2Nq_sbeDgtzMJpLLnXFgAChGV0aDKQtTA_KgEAAAAAIgEAAAAAAIJpZIJ2NIJpcISsaa0Zg2lwNpAkAIkHAAAAAPA8kv_-awoTiXNlY3AyNTZrMaEDHAD2JKYevx89W0CcFJFiskdcEzkH_Wdv9iW42qLK79ODdWRwgiMohHVkcDaCI4I"
        };

        var connectionOptions = new ConnectionOptions
        {
            UdpPort = new Random().Next(1, 65535)
        };

        var sessionOptions = SessionOptions.Default;
        var tableOptions = new TableOptions(bootstrapEnrs);
        var enr = new EnrBuilder()
            .WithIdentityScheme(sessionOptions.Verifier, sessionOptions.Signer)
            .WithEntry(EnrEntryKey.Id, new EntryId("v4"))
            .WithEntry(EnrEntryKey.Secp256K1, new EntrySecp256K1(sessionOptions.Signer.PublicKey));

        var services = new ServiceCollection();
        services.AddLogging(config => config.AddConsole());
        services.AddSingleton(enr);

        var customHandler = new CustomHandler();

        var builder = new Discv5ProtocolBuilder(services)
            .WithConnectionOptions(connectionOptions)
            .WithTableOptions(tableOptions)
            .WithSessionOptions(sessionOptions)
            .WithEnrBuilder(enr)
            .WithTalkResponder(customHandler);

        var serviceProvider = services.BuildServiceProvider();
        var discv5Protocol = builder.Build();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        try
            {
                await discv5Protocol.InitAsync();

                var randomNodeId = RandomUtility.GenerateRandomData(32);
                await discv5Protocol.DiscoverAsync(randomNodeId);

                var activeNodes = discv5Protocol.GetActiveNodes;
                foreach (var node in activeNodes)
                {
                    var foundNodes = await discv5Protocol.SendFindNodeAsync(node, randomNodeId);
                    if (foundNodes == null) continue;

                    foreach (var foundEnr in foundNodes)
                    {
                        Console.WriteLine($"Found node with ENR: {foundEnr}");

                        var protocol = BitConverter.GetBytes(PROTOCOL_ID);
                        
                        // Create and serialize Ping message
                        var customPayload = new byte[32]; // Initialize with appropriate data
                        var ping = new Ping(1, customPayload); // Example ENR seq number and custom payload
                        var pingMessage = new MessageUnion(0, ping);
                        var request = SSZ.Serialize(pingMessage);

                        var response = await discv5Protocol.SendTalkReqAsync(foundEnr, protocol, request);

                        if (response != null)
                        {
                            Console.WriteLine("TALKREQ sent successfully and received response.");
                            // var responseMessage = (MessageUnion)SSZ.Deserialize(response, typeof(MessageUnion));
                            // if (responseMessage.Selector == 1) // Pong
                            // {
                            //     var pong = (Pong)responseMessage.Value;
                            //     Console.WriteLine($"Received Pong with ENR sequence number: {pong.EnrSeq}");
                                
                            //     // Process the custom payload if needed
                            //     // var receivedCustomPayload = pong.CustomPayload;
                            //     // Process receivedCustomPayload as needed
                            // }
                            // else
                            // 
                            //     Console.WriteLine("Received unexpected response type");
                            // }

                            var allNodes = discv5Protocol.GetAllNodes;
                            activeNodes = discv5Protocol.GetActiveNodes;
                            Console.WriteLine($"There are {allNodes.Count()} nodes, of which {activeNodes.Count()} are active.");
                        }
                        else
                        {
                            Console.WriteLine("Failed to send TALKREQ or receive response.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during discovery.");
            }
            finally
            {
                await discv5Protocol.StopAsync();
            }
    }
}
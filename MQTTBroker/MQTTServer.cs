using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Text;

namespace MQTTServer
{
    public class MQTTServerWrapper
    {
        private readonly MqttFactory mqttFactory;
        private MqttServer? server;
        private readonly int port;
        public MQTTServerWrapper(int port = 1883) 
        {
            mqttFactory = new MqttFactory();
            this.port = port;
        }

        private MqttServerOptions CreateOptions()
        {
            return new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(port)
                .Build();
        }

        public async Task InitServerWithNoOptions()
        {
            var mqttServerOptions = CreateOptions();
            server = mqttFactory.CreateMqttServer(mqttServerOptions);

            server.ClientConnectedAsync += OnClientConnected;
            server.InterceptingPublishAsync += OnMessageIntersepted;

            await server.StartAsync();
            Console.WriteLine($"Server started and listening on port {port}");
        }

        private Task OnMessageIntersepted(InterceptingPublishEventArgs arg)
        {
            Console.WriteLine($"Message received from client {arg.ClientId}:\n" +
                $"Topic:{arg.ApplicationMessage.Topic}\n" +
                $"Message: {Encoding.Default.GetString(arg.ApplicationMessage.PayloadSegment)}");

            return Task.CompletedTask;
        }

        private Task OnClientConnected(ClientConnectedEventArgs arg)
        {
            Console.WriteLine($"Client {arg.ClientId} connected to server");
            return Task.CompletedTask;
        }

        public void SetServerValidation(List<string> validIds, List<string> validNames)
        {
            if (server != null && !server.IsStarted)
            {
                server.ValidatingConnectionAsync += e =>
                {
                    if (!validIds.Contains(e.ClientId))
                    {
                        e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    }
                    else if (!validNames.Contains(e.UserName))
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    return Task.CompletedTask;
                };
            }
            else
            {
                Console.WriteLine("Server already started, can't set validation handler");
            }
        }

        public async Task PublishMessageFromBroker(string topic, string payload, string senderClientId)
        {
            if (server != null && server.IsStarted)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();

                await server.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message)
                    {
                        SenderClientId = senderClientId
                    });
            }
            else
            {
                Console.WriteLine("Cant publish because server is not created or started.");
            }
        }

        public void GetNotifiedWhenClientReceivedMessage()
        {
            if (server != null && !server.IsStarted)
            {
                server.ClientAcknowledgedPublishPacketAsync += e =>
                {
                    Console.WriteLine($"Client '{e.ClientId}' acknowledged packet {e.PublishPacket.PacketIdentifier} with topic '{e.PublishPacket.Topic}'");

                    // It is also possible to read additional data from the client response. This requires casting the response packet.
                    var qos1AcknowledgePacket = e.AcknowledgePacket as MqttPubAckPacket;
                    Console.WriteLine($"QoS 1 reason code: {qos1AcknowledgePacket?.ReasonCode}");

                    var qos2AcknowledgePacket = e.AcknowledgePacket as MqttPubCompPacket;
                    Console.WriteLine($"QoS 2 reason code: {qos1AcknowledgePacket?.ReasonCode}");

                    return Task.CompletedTask;
                };
            }
            else
            {
                Console.WriteLine("Server already started, can't set notify handler");
            }
        }
    }
}
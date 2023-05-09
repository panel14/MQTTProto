using MQTTnet;
using MQTTnet.Client;
using System.Drawing;
using System.Text;

namespace MQTTClient
{
    public class MQTTClientWrapper
    {
        private readonly MqttFactory mqttFactory;
        private readonly IMqttClient client;
        private readonly string clientId;
        private readonly string clientServer;
        private readonly int? port;
        private readonly CancellationToken token;

        public MQTTClientWrapper(string server = "localhost", int? port = null, CancellationToken token = default)
        {
            mqttFactory = new MqttFactory();
            client = mqttFactory.CreateMqttClient();
            clientId = Guid.NewGuid().ToString();
            clientServer = server;
            this.port = port;
            this.token = token;
        }

        public async Task<string> InitAndConnectClient()
        {
            var options = new MqttClientOptionsBuilder()
                            .WithClientId(clientId)
                            .WithTcpServer(clientServer, port)
                            .WithCleanSession(true)
                            .Build();

            client.ConnectedAsync += OnConnected;
            client.DisconnectedAsync += OnDisconnected;
            client.ApplicationMessageReceivedAsync += OnMessageReceive;

            await client.ConnectAsync(options, token);
            return clientId;
        }

        private Task OnMessageReceive(MqttApplicationMessageReceivedEventArgs args)
        {
            byte[] message = args.ApplicationMessage.PayloadSegment.ToArray();

            Console.WriteLine($"Message received to {clientId}");

            Console.WriteLine($"Message: {Encoding.Default.GetString(args.ApplicationMessage.PayloadSegment)}");

            using (var ms = new MemoryStream(message, 0, message.Length))
            {
                var image = Image.FromStream(ms);
                if (image != null)
                {
                    image.Save($"{args.ApplicationMessage.Topic}_image.jpg");
                }
            }

            Console.WriteLine($"Image {args.ApplicationMessage.Topic}_image saved from message");

            return Task.CompletedTask;
        }

        private Task OnConnected(MqttClientConnectedEventArgs args)
        {
            Console.WriteLine($"Client with ID:{clientId} connected");
            return Task.CompletedTask;
        }

        private Task OnDisconnected(MqttClientDisconnectedEventArgs args)
        {
            Console.WriteLine($"Client with ID:{clientId} disconnected");
            return Task.CompletedTask;
        }
        

        public async Task PublishMessageAsync(string topic, string message)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                                        .WithTopic(topic)
                                        .WithPayload(message)
                                        .Build();

            await client.PublishAsync(applicationMessage, token);
            Console.WriteLine($"Message is published by client {clientId}");
        }

        public async Task SubscribeAsync(string topicFilter)
        {
            var mqttSubscribeOptions = new MqttTopicFilterBuilder()
                .WithTopic(topicFilter)
                .Build();
            
            await client.SubscribeAsync(mqttSubscribeOptions, token);
            Console.WriteLine($"Client {clientId} subscribed on topic: {topicFilter}");
        }

        public async Task UnsubscribeAsync(string topicFilter)
        {
            var mqttUnsubOptions = mqttFactory.CreateUnsubscribeOptionsBuilder()
                .WithTopicFilter(topicFilter) 
                .Build();

            await client.UnsubscribeAsync(mqttUnsubOptions, token);
            Console.WriteLine($"Client {clientId} subscribed from topic: {topicFilter}");
        }

        public async Task DisconnectClient()
        {
            var disconnectOprions = new MqttClientDisconnectOptionsBuilder()
                .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                .Build();

            await client.DisconnectAsync(disconnectOprions);
        }
    }
}
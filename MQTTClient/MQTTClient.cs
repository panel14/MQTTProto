using MQTTnet;
using MQTTnet.Client;

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

        public async Task InitAndConnectClient()
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
            Console.WriteLine("Connection is successfully.");
        }

        private Task OnMessageReceive(MqttApplicationMessageReceivedEventArgs arg)
        {
            Console.WriteLine($"Message received to {clientId}");
            Console.WriteLine($"Message: {arg.ApplicationMessage}");
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
        

        public async void PublishMessageAsync(string topic, string message)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                                        .WithTopic("topic")
                                        .WithPayload(topic)
                                        .Build();

            await client.PublishAsync(applicationMessage, token);
            Console.WriteLine($"Message is published by client {clientId}");
        }

        public async void SubscribeAsync(string topicFilter)
        {
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(topicFilter);
                    })
                .Build();
            
            await client.SubscribeAsync(mqttSubscribeOptions, token);
            Console.WriteLine($"Client {clientId} subscribed on topic: {topicFilter}");
        }

        public async void UnsubscribeAsync(string topicFilter)
        {
            var mqttUnsubOptions = mqttFactory.CreateUnsubscribeOptionsBuilder()
                .WithTopicFilter(topicFilter) 
                .Build();

            await client.UnsubscribeAsync(mqttUnsubOptions, token);
            Console.WriteLine($"Client {clientId} subscribed from topic: {topicFilter}");
        }
    }
}
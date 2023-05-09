using MQTTClient;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using MQTTServer;

Console.WriteLine("Choice mode:\n" +
    "1 - init mqtt client\n" +
    "2 - init mqtt broker (server)\n" +
    "q/quit - quit program");

string? command = Console.ReadLine();
int port = 1883;

if (command != "q" || command != "quit")
{
    switch (command)
    {
        case "1":
            Console.WriteLine("Write topic");
            string topic =  Console.ReadLine();

            var client = new MQTTClientWrapper("broker.hivemq.com", port, CancellationToken.None);
            string id = await client.InitAndConnectClient();
            await client.SubscribeAsync(topic);

            Console.WriteLine("Press any key to close window");
            Console.ReadKey();
            break;
        case "2":
            var server = new MQTTServerWrapper(port);
            await server.InitServerWithNoOptions();

            

            Console.WriteLine("Press any key to close window");
            Console.ReadKey();
            break;
        default:
            Console.WriteLine("Unknown command");
            break;
    }
}
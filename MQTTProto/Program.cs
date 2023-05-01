using MQTTnet.Extensions.ManagedClient;

Console.WriteLine("Choice mode:\n" +
    "1 - init mqtt client\n" +
    "2 - init mqtt broker (server)\n" +
    "q/quit - quit program");

string? command = Console.ReadLine();

while (command != "q" || command != "quit")
{
    switch (command)
    {
        case "1":
            break;
        case "2":
            break;
        default:
            Console.WriteLine("Unknown command");
            break;
    }
}

static void InteractiveClient(IManagedMqttClient client)
{

}

static void InteractiveBroker()
{

}
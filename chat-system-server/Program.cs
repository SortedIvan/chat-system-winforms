using chat_system_server;

class Program
{
    public static async Task Main(string[] args)
    {
        // Sockets are the programming interface that is between the 
        // application layer and the transport layer

        // We simply need to open an http connection when it is requested
        // and then expect data from the client
        // the port nr can be seen as the "application" nr

        Server server = new Server(8888, "127.0.0.1");
        await server.RunServer(10);

    }
}
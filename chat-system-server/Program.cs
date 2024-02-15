using chat_system_server;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    public static async Task Main(string[] args)
    {
        /*
           - We simply need to open an http connection when it is requested
            and then expect data from the client
            the port nr can be seen as the "application" nr
           - Sockets are the programming interface that is between the 
            application layer and the transport layer
         */

        /*
          Have two threads:
          one for processing the adding of new clients and running the server socket
          another for handling messages and client interaction
        */

        Server server = new Server(8888, "127.0.0.1");
        await server.RunServer(10);

    }

}
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chat_system_server
{
    public class Server
    {
        /*
          TCP based servers have two sockets, one for entry
          and one that is dynamically created
          entry listens to connections and creates a new socket when needed
         */
        private Socket entry;
        private IPEndPoint serverEndpoint;
        private int port;
        private string ip;
        private List<Socket> clientConnections;
        private bool configured = false;
        private bool serverRunning = false;
        private CancellationTokenSource tokenSource;
        private CancellationToken cancellationToken;

        public Server(int port, string ip)
        {
            try
            {
                clientConnections = new List<Socket>(); // Initialize the clients array

                this.port = port;
                this.ip = ip;

                
                serverEndpoint = new IPEndPoint(
                    IPAddress.Parse(ip),
                    port
                );

                // Create the entry socket
                entry = new Socket(SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket with the ip information:
                entry.Bind(serverEndpoint);

                tokenSource = new CancellationTokenSource();
                cancellationToken = tokenSource.Token;

                configured = true;
                serverRunning = true;
            }
            catch(Exception ex) {
                Console.WriteLine(ex.Message);
                configured = false;
            }
        }

        public async Task<bool> RunServer(int connections)
        {
            if (configured)
            {
                // How many connections can the server handle
                entry.Listen(connections);


                while (true)
                {
                    Console.WriteLine("hi");
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    /*
                        1) check whether the client is first time connection
                        this is done by checking what the client sends initially
                        2) Have a second thread that deals with broadcasting and accepting messages from clients
                        3) Have some kind of system that lets clients disconnect
                     */

                    Console.WriteLine("Waiting for a client to connect");

                    Socket client = await entry.AcceptAsync(cancellationToken);
                    // As soon as we receive a client, pass it onto a client handle method

                    await HandleClient(client);
                }
            }
            return false;
        }

        // We use the C# ecosystem to start a task and handle the client seperately
        private async Task HandleClient(Socket client)
        {
            // Check if the client has already been added
            if (!CheckClientAlreadyExists(client))
            {
                clientConnections.Add(client);
            }
            else
            {
                return;
            }
         
            var buffer = new byte[1_024];
            var initialMessageRec = await client.ReceiveAsync(buffer, SocketFlags.None);
            var responseFromClient = Encoding.UTF8.GetString(buffer, 0, initialMessageRec);

           

            JObject json = JObject.Parse(responseFromClient);


            Console.WriteLine(json["name"]);
            //while (true)
            //{

            //    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            //    var response = Encoding.UTF8.GetString(buffer, 0, received);

            //}

        }

        private bool CheckClientAlreadyExists(Socket client)
        {
            for (int i =0; i < clientConnections.Count; i++)
            {
                if (clientConnections[i].RemoteEndPoint == client.RemoteEndPoint)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetPort()
        {
            return port;
        }
        public void SetPort(int port)
        {
            this.port = port;
        }

        public void SetIP(string ip)
        {
            this.ip = ip;
        }

        public string GetIP()
        {
            return ip;
        }

        public void AddClientConnection(Socket clientConnection)
        {
            clientConnections.Add(clientConnection);
        }

        public List<Socket> GetClientConnections()
        {
            return clientConnections;
        }

        public CancellationToken GetCancellationToken()
        {
            return cancellationToken;
        }

    }
}

using chat_system_server.Models;
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
        private Dictionary<string, User> connectedUsers;
        private bool configured = false;
        private bool serverRunning = false;
        private CancellationTokenSource tokenSource;
        private CancellationToken cancellationToken;

        public Server(int port, string ip)
        {
            try
            {
                connectedUsers = new Dictionary<String,User>(); // Initialize the Users array

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

                    await AcceptClient(client);
                }
            }
            return false;
        }

        // We use the C# ecosystem to start a task and handle the client seperately
        private async Task AcceptClient(Socket client)
        {
            var buffer = new byte[1_024];
            var initialMessageRec = await client.ReceiveAsync(buffer, SocketFlags.None);
            var responseFromClient = Encoding.UTF8.GetString(buffer, 0, initialMessageRec);
            JObject connectedJson = JObject.Parse(responseFromClient);
            Msg initialMsg = new Msg();
            initialMsg.ParseFromJsonAndSet(connectedJson);

            Console.WriteLine(client.RemoteEndPoint.ToString());

            // Check if the user has already been added
            if (CheckUserAlreadyExists(initialMsg.GetUserFrom()))
            {
                ServerResponse response = new ServerResponse();
                response.SetResponseType(ResponseType.NAME_TAKEN);
                await client.SendAsync(Encoding.UTF8.GetBytes(response.ToJsonString()), 0);
                return;
            }

            User user = new User(client.RemoteEndPoint.ToString(), initialMsg.GetUserFrom(), client);

            connectedUsers.Add(initialMsg.GetUserFrom(), user);
            Console.WriteLine(initialMsg.GetUserFrom());

            // Now run the handling of the client in the background
            await Task.Run(async () => await HandleClient(user, client));
        }

        private async Task HandleClient(User user, Socket client)
        {
            var buffer = new byte[1_024];   
            while (true) // Go in a 
            {
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

            }
        }


        private bool CheckUserAlreadyExists(string username)
        {
            if (connectedUsers.ContainsKey(username))
            {
                return true;
            }
            return false;
        }

        

        private async void ProcessGlobalClientMessage(Msg message)
        {
            // Propagate it over all of the connected users
            for (int i = 0; i < connectedUsers.Count; ++i)
            {
                
            }
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


        public CancellationToken GetCancellationToken()
        {
            return cancellationToken;
        }

    }
}

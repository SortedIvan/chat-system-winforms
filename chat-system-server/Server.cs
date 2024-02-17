﻿using chat_system_server.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
        //private Dictionary<string, Task> userTasks;
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
        
        // Main task that runs the server and handles newly connecting clients
        public async Task<bool> RunServer(int connections)
        {
            if (configured)
            {
                // How many connections can the server handle
                entry.Listen(connections);

                while (true)
                {
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
            Msg connectedMsg = ProcessMessageFromClient(buffer, 0, initialMessageRec);

            ServerResponse response = new ServerResponse(); // Initialize a server response obj

            // Check if the action is connecting

            if (connectedMsg.GetActionType() != ActionType.Connect)
            {
                response.SetResponseType(ResponseType.BAD_REQUEST);
                response.SetMessage("User not yet connected. Please connect first");
                await client.SendAsync(Encoding.UTF8.GetBytes(response.ToJsonString()), 0);
                return;
            }

            bool userExists = CheckUserAlreadyExists(connectedMsg.GetUserFrom());

            // Check if the user has already been added
            if (userExists)
            {
                response.SetResponseType(ResponseType.NAME_TAKEN);
                await client.SendAsync(Encoding.UTF8.GetBytes(response.ToJsonString()), 0);
                return;
            }

            if (String.IsNullOrEmpty(connectedMsg.GetUserFrom()))
            {
                response.SetResponseType(ResponseType.BAD_REQUEST);
                response.SetMessage("Please provide a username");
                await client.SendAsync(Encoding.UTF8.GetBytes(response.ToJsonString()), 0);
                return;
            }
            

            User user = new User(client.RemoteEndPoint.ToString(), connectedMsg.GetUserFrom(), client);

            connectedUsers.Add(connectedMsg.GetUserFrom(), user);

            // If everything else above is good:
            response.SetResponseType(ResponseType.OK);
            response.SetMessage("User connected sucessfully");
            await client.SendAsync(Encoding.UTF8.GetBytes(response.ToJsonString()), 0);

            // Create a background task without awaiting for its completion
            Task userTask = HandleClient(user, client);

            // Finally, add the task to a collection in case synchronization is needed
            //userTasks.Add(user.GetUsername(), userTask);
        }

        private async Task HandleClient(User user, Socket client)
        {
            var buffer = new byte[1_024];

            while (true) // Go in a 
            {
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                Msg message = ProcessMessageFromClient(buffer, 0, received);

                Console.WriteLine(user.GetUsername() + ": " + message.GetContent());

            }
        }

        private Msg ProcessMessageFromClient(byte[] bytes, int index, int count)
        {
            var responseFromClient = Encoding.UTF8.GetString(bytes, index, count);
            JObject messageJson = JObject.Parse(responseFromClient);
            Msg message = new Msg();
            message.ParseFromJsonAndSet(messageJson);
            return message;
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

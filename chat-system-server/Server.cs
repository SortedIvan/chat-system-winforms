using chat_system_server.Models;
using Newtonsoft.Json.Linq;
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
            if (configured && serverRunning)
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
                        2) Start a background task for each client that handles incoming messages/etc
                           -> problem with the above: synchronization
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
            ClientMessage connectedMsg = ConverToMessage(buffer, 0, initialMessageRec);

            ServerMessage response = new ServerMessage(); // Initialize a server response obj

            // Check if the action is connecting

            if (connectedMsg.GetActionType() != ActionType.CONNECT)
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
            client.SendAsync(Encoding.UTF8.GetBytes(response.ToJsonString()), 0).Wait();

            // Wait for acknowledgement from client
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            ClientMessage ackn = ConverToMessage(buffer, 0, received);
            
            if (ackn.GetActionType() != ActionType.RECEIVED)
            {
                // remove user if we do not get confirmation
                connectedUsers.Remove(user.GetUsername());
                return;
            }

            connectedUsers[user.GetUsername()].SetIsConnected(true);

            await NotifyAllUsersWhenJoined(user);

            List<string> users = new List<string>();
            foreach (var connectedUser in connectedUsers)
            {
                if (connectedUser.Value.GetUsername() == user.GetUsername())
                {
                    continue;
                }
                users.Add(connectedUser.Value.GetUsername());
            }

            _ = PollInformationForUserJustJoined(users, client);


            // Create a background task without awaiting for its completion
            Task userTask = HandleClient(user, client);
        }

        private async Task NotifyAllUsersWhenJoined(User user)
        {
            ServerMessage clientConnectionMsg = new ServerMessage();
            clientConnectionMsg.SetMessage(user.GetUsername());
            clientConnectionMsg.SetResponseType(ResponseType.USER_JOINED);
            foreach (var connectedUser in connectedUsers)
            {
                await connectedUser
                    .Value.GetClientSocket()
                    .SendAsync(Encoding.UTF8.GetBytes(clientConnectionMsg.ToJsonString()), 0);
            }
        }

        private async Task PollInformationForUserJustJoined(List<string> users, Socket client)
        {
            ServerMessage clientConnectionMsg = new ServerMessage();

            string allUsers = "";

            for (int i = 0; i < users.Count; i++)
            {
                allUsers += users[i];
                allUsers += "|";
            }

            clientConnectionMsg.SetMessage(allUsers);
            clientConnectionMsg.SetResponseType(ResponseType.FIRST_TIME_POLL);
            await client.SendAsync(Encoding.UTF8.GetBytes(clientConnectionMsg.ToJsonString()), 0);
        }

        private async Task HandleClient(User user, Socket client)
        {
            var buffer = new byte[1_024];
            while (true)
            {
               
                int received = await client.ReceiveAsync(buffer, SocketFlags.None);

                if (received == 0)
                {
                    
                    // User closed connection
                    connectedUsers.Remove(user.GetUsername());
                    _ = SendGlobalServerMessage(user.GetUsername() + " has left the chat. Womp!");
                    return;
                }

                

                ClientMessage message = ConverToMessage(buffer, 0, received);
                ProcessMessage(message);
                Console.WriteLine(user.GetUsername() + ": " + message.GetContent());

            }
        }

        private ClientMessage ConverToMessage(byte[] bytes, int index, int count)
        {
            var responseFromClient = Encoding.UTF8.GetString(bytes, index, count);
            JObject messageJson = JObject.Parse(responseFromClient);
            ClientMessage message = new ClientMessage();
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

        private async void ProcessMessage(ClientMessage message)
        {
            Console.WriteLine(message.GetActionType());
            switch (message.GetActionType())
            {
                case ActionType.MESSAGE:
                    ProcessGlobalClientMessage(message);
                    break;
                case ActionType.PRIVATE_MESSAGE:
                    ProcessPrivateClientMessage(message);
                    break;
            }
        }

        private async void ProcessPrivateClientMessage(ClientMessage message)
        {
            string userTo = message.GetUserTo();
            User user;

            if (userTo == null) {
                //Bad input - send back to client
                return;
            }

            if (!connectedUsers.TryGetValue(userTo, out user))
            {
                // No user exists - send back to client
                return;
            }

            ServerMessage pm = new ServerMessage();
            pm.SetMessage("FROM: " + message.GetUserFrom() + message.GetContent());
            pm.SetResponseType(ResponseType.PRIVATE_MESSAGE);
            await user.GetClientSocket().SendAsync(Encoding.UTF8.GetBytes(pm.ToJsonString()), 0);

        }

        private async void ProcessGlobalClientMessage(ClientMessage message)
        {
            foreach (KeyValuePair<string, User> entry in connectedUsers)
            {
                ServerMessage response = new ServerMessage(); // Initialize a server response obj
                response.SetResponseType(ResponseType.GLOBAL_MESSAGE);
                response.SetMessage(message.GetUserFrom() + ": " + message.GetContent());
                await entry.Value
                    .GetClientSocket()
                    .SendAsync(Encoding.UTF8.GetBytes(response.ToJsonString()), 0);
            }
        }

        private async Task SendGlobalServerMessage(string content)
        {
            foreach (KeyValuePair<string, User> entry in connectedUsers)
            {
                if (!entry.Value.GetIsConnected())
                {
                    // This client is not connected yet
                    continue;
                }

                ServerMessage message = new ServerMessage();
                message.SetResponseType(ResponseType.GLOBAL_MESSAGE);
                message.SetMessage(content);
                await entry.Value
                    .GetClientSocket()
                    .SendAsync(Encoding.UTF8.GetBytes(message.ToJsonString()), 0);
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

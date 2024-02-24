using chat_system_client_wpf.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChatClient.System
{
    public class Client
    {
        private Socket clientSocket;
        private bool connected;
        private string username;

        public Client()
        {
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            connected = false;
            username = "";
        }

        public async Task MainClientLoop(ListBox chatBox, ListBox userBox)
        {
            while (connected)
            {
                var responseBuffer = new byte[1_024];
                var received = await clientSocket.ReceiveAsync(responseBuffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(responseBuffer, 0, received);
                _ = ProcessServerMessages(response, chatBox, userBox);
            }
        }

        public void ResetSocket()
        {
            clientSocket.Close();
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        private async Task ProcessServerMessages(string responses, ListBox chatBox, ListBox userBox)
        {
            List<ServerMessage> serverMessages = new List<ServerMessage>();
            string pattern = @"\{(?:[^\{\}]|(?<Open>\{)|(?<-Open>\}))*(?(Open)(?!))\}";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(responses);

            foreach (Match match in matches)
            {
                try
                {
                    JObject json = JObject.Parse(match.Value);
                    ServerMessage servResponse = new ServerMessage();
                    servResponse.ParseFromJsonAndSet(json);
                    serverMessages.Add(servResponse);

                }
                catch (Exception e)
                {

                }
            }

            foreach (var servResponse in serverMessages)
            {
                switch (servResponse.GetResponseType())
                {
                    case ResponseType.GLOBAL_MESSAGE:
                        chatBox.Items.Add(servResponse.GetServerMessage());
                        break;
                    // Keep message and joins seperated for adding the username
                    case ResponseType.USER_JOINED:
                        chatBox.Items.Add(servResponse.GetServerMessage() + " has joined the chat room! Say hi!");
                        userBox.Items.Add(servResponse.GetServerMessage());
                        break;
                    case ResponseType.PRIVATE_MESSAGE:
                        chatBox.Items.Add(servResponse.GetServerMessage());
                        break;
                    case ResponseType.FIRST_TIME_POLL:
                        List<string> users = ParseUsersMessage(servResponse.GetServerMessage());
                        for (int i = 0; i < users.Count; i++)
                        {
                            userBox.Items.Add(users[i]);
                        }
                        break;
                }
            }
        }


        private List<string> ParseUsersMessage(string input)
        {
            List<string> users = new List<string>();
            string currentUser = "";

            for (int i = 0; i < input.Length; i++) {
                if (input[i] == '|') {
                    users.Add(currentUser);
                    currentUser = "";
                    continue;
                }
            }

            if (currentUser.Length > 0)
            {
                users.Add(currentUser);
            }

            return users;
        }

        public Socket GetClientSocket()
        {
            return clientSocket;
        }

        public void SetClientSocket(Socket value)
        {
            clientSocket = value;
        }

        public bool GetConnected()
        {
            return connected;
        }

        public void SetConnected(bool value)
        {
            connected = value;
        }

        public string GetUsername()
        {
            return username;
        }

        public void SetUsername(string value)
        {
            username = value;
        }

        public async Task<ServerMessage> ParseServerMessage()
        {
            var responseBuffer = new byte[1_024];
            var received = await clientSocket.ReceiveAsync(responseBuffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(responseBuffer, 0, received);
            JObject jsonServerResponse = JObject.Parse(response);
            ServerMessage servResponse = new ServerMessage();
            servResponse.ParseFromJsonAndSet(jsonServerResponse);
            return servResponse;
        }
    }
}

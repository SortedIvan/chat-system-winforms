using chat_system_client_wpf.Models;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.Text;
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

        public async Task MainClientLoop(ListBox chatBox)
        {
            while (connected)
            {
                var responseBuffer = new byte[1_024];
                var received = await clientSocket.ReceiveAsync(responseBuffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(responseBuffer, 0, received);
                JObject jsonServerResponse = JObject.Parse(response);

                ServerMessage servResponse = new ServerMessage();
                servResponse.ParseFromJsonAndSet(jsonServerResponse);

                switch (servResponse.GetResponseType())
                {
                    case ResponseType.GLOBAL_MESSAGE:
                    chatBox.Items.Add(servResponse.GetServerMessage());
                    break;
                    case ResponseType.USER_JOINED: // Keep message and joins seperated for further logic
                    chatBox.Items.Add(servResponse.GetServerMessage());
                    break;
                }

            }
        }

        public void ResetSocket()
        {
            clientSocket.Close();
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
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

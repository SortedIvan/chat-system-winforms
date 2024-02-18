using chat_system_winforms.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace chat_system_winforms
{
    public partial class Form1 : Form
    {

        Socket client = new Socket(SocketType.Stream, ProtocolType.Tcp);
        bool connected = false;
        string username = "";

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnJoin_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                return;
            }

            string username = tbUsername.Text;


            IPEndPoint serverEndpoint = new IPEndPoint(
                IPAddress.Parse("127.0.0.1"),
                8888
            );

            await client.ConnectAsync(serverEndpoint);
            Msg message = new Msg(ActionType.Connect, username, " ", " ");
            var messageBytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            _ = await client.SendAsync(messageBytes, SocketFlags.None);

            var responseBuffer = new byte[1_024];
            var received = await client.ReceiveAsync(responseBuffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(responseBuffer, 0, received);
            JObject jsonServerResponse = JObject.Parse(response);

            ServerResponse servResponse = new ServerResponse();
            servResponse.ParseFromJsonAndSet(jsonServerResponse);

            switch (servResponse.GetResponseType())
            {
                case ResponseType.NAME_TAKEN:
                    serverResponseLbl.Text = "NAME TAKEN";
                    break;
                case ResponseType.OK:
                    connected = true;
                    serverResponseLbl.Text = servResponse.GetServerMessage();
                    this.username = username;

                    _ = MainClientLoop();

                    break;
            }

        }

        private async void sendMessageBtn_Click(object sender, EventArgs e)
        {
            string messageContent = tbMessage.Text;
            Msg message = new Msg(ActionType.Message, username, messageContent, " ");
            var messageBytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
        }

        private async Task MainClientLoop()
        {
            while (connected)
            {
                var responseBuffer = new byte[1_024];
                var received = await client.ReceiveAsync(responseBuffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(responseBuffer, 0, received);
                JObject jsonServerResponse = JObject.Parse(response);

                ServerResponse servResponse = new ServerResponse();
                servResponse.ParseFromJsonAndSet(jsonServerResponse);

                if (servResponse.GetResponseType() == ResponseType.OK)
                {
                    chatBox.AppendText(servResponse.GetServerMessage() +  "\n");
                }
                
            }
        }
    }
}
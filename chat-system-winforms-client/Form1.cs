using chat_system_winforms.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace chat_system_winforms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnJoin_Click(object sender, EventArgs e)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEndpoint = new IPEndPoint(
                IPAddress.Parse("127.0.0.1"),
                8888
            );

            await socket.ConnectAsync(serverEndpoint);
            Msg message = new Msg(ActionType.Connect, "Test", "", "");
            var messageBytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            _ = await socket.SendAsync(messageBytes, SocketFlags.None);
        }

        private void sendMessageBtn_Click(object sender, EventArgs e)
        {

        }
    }
}
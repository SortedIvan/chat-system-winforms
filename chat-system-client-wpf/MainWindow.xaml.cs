using chat_system_client_wpf.Models;
using ChatClient.System;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace chat_system_client_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client client;

        public MainWindow()
        {
            InitializeComponent();

            client = new Client();

        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (client.GetConnected())
            {
                return; // The client is already connected
            }

            IPEndPoint serverEndpoint = new IPEndPoint(
                         IPAddress.Parse("127.0.0.1"),
                         8888
                     );

            string username = tbUsername.Text;

            // 1) Attempt to connect
            await client.GetClientSocket().ConnectAsync(serverEndpoint);
            ClientMessage message = new ClientMessage(ActionType.CONNECT, username, " ", " ");
            var messageBytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            _ = await client.GetClientSocket().SendAsync(messageBytes, SocketFlags.None);

            //// The server sends one initial message to indicate whether client is connected or not
            //
            ServerMessage isConnectedMessage = await client.ParseServerMessage();
            
            switch (isConnectedMessage.GetResponseType())
            {
                case ResponseType.NAME_TAKEN:
                    tbServerResponses.AppendText(DateTime.Now.ToString() + '\n');
                    tbServerResponses.AppendText("Name already taken." + '\n');

                    // Reset the socket for another connection attempt
                    client.ResetSocket();

                    break;
                case ResponseType.OK:
                    client.SetConnected(true);
                    tbServerResponses.AppendText(DateTime.Now.ToString() + '\n');
                    tbServerResponses.AppendText(isConnectedMessage.GetServerMessage() + '\n');

                    // Let know the server that we are connected and received its response:
                    ClientMessage ackn = new ClientMessage(ActionType.CONNECT, username, " ", " ");
                    messageBytes = Encoding.UTF8.GetBytes(ackn.ToJsonString());
                    _ = await client.GetClientSocket().SendAsync(messageBytes, SocketFlags.None);
            

                    this.client.SetUsername(username);
                    _ = client.MainClientLoop(tbMain);
                    break;
            }
        }

        private async void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!client.GetConnected())
            {
                return;
            }

            string messageContent = tbMessage.Text;
            ClientMessage message = new ClientMessage(ActionType.MESSAGE, client.GetUsername(), messageContent, "None");
            var messageBytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            _ = await client.GetClientSocket().SendAsync(messageBytes, SocketFlags.None);
        }

        private void tbAllUsers_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        
    }
}

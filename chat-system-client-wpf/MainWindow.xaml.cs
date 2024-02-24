using chat_system_client_wpf.Models;
using ChatClient.System;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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

            // Define a function even for when the client exits the application
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (client.GetConnected())
            {
                // if the client was connected:
                client.GetClientSocket().Shutdown(SocketShutdown.Both);
                client.GetClientSocket().Close();
            }
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

            // The server sends one initial message to indicate whether client is connected or not
            ServerMessage isConnectedMessage = await client.ParseServerMessage();
            
            switch (isConnectedMessage.GetResponseType())
            {
                case ResponseType.NAME_TAKEN:
                    lbServerLogs.Items.Add(DateTime.Now.ToString());
                    lbServerLogs.Items.Add("Name already taken");

                    // Reset the socket for another connection attempt
                    client.ResetSocket();

                    break;
                case ResponseType.OK:
                    client.SetConnected(true);

                    lbServerLogs.Items.Add(DateTime.Now.ToString());
                    lbServerLogs.Items.Add(isConnectedMessage.GetServerMessage());

                    // Let know the server that we are connected and received its response:
                    ClientMessage ackn = new ClientMessage(ActionType.RECEIVED, username, " ", " ");
                    messageBytes = Encoding.UTF8.GetBytes(ackn.ToJsonString());
                    _ = await client.GetClientSocket().SendAsync(messageBytes, SocketFlags.None);
            
                    this.client.SetUsername(username);
                    _ = client.MainClientLoop(lbMain, lbUsers);
                    break;
            }
        }

        private async void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!client.GetConnected())
            {
                return;
            }

            bool privateMessage = tbReciever.Text.Length > 0;

            string messageContent = tbMessage.Text;
            ClientMessage message;

            if (privateMessage)
            {
                message = new ClientMessage(ActionType.PRIVATE_MESSAGE, client.GetUsername(), messageContent, tbReciever.Text);
            }
            else
            {
                message = new ClientMessage(ActionType.MESSAGE, client.GetUsername(), messageContent, "None");
            }

            var messageBytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            _ = await client.GetClientSocket().SendAsync(messageBytes, SocketFlags.None);
        }

        private void tbAllUsers_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void lbMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var addedItems = e.AddedItems;
            if (addedItems.Count <= 0)
            {
                return;
            }

            tbReciever.Text = addedItems[0].ToString();
        }
    }
}

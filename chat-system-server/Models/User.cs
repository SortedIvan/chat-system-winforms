using System.Net.Sockets;

namespace chat_system_server.Models
{
    public class User
    {
        private string address;
        private string username;
        private DateTime firstJoined;
        private DateTime left;
        private List<ClientMessage> globalMessages;
        private List<ClientMessage> privateMessages;
        private Socket clientSocket;
        private bool isConnected = false;


        public User(string address, string username, Socket clientSocket) {
            this.address = address;
            this.username = username;
            this.firstJoined = DateTime.Now;
            this.clientSocket = clientSocket;
        }

        public List<ClientMessage> GetGlobalMessages()
        {
            return globalMessages;
        }

        public List<ClientMessage> GetPrivateMessages()
        {
            return privateMessages;
        }

        public void AddGlobalMessage(ClientMessage message)
        {
            globalMessages.Add(message);
        }

        public void AddPrivateMessage(ClientMessage message)
        {
            privateMessages.Add(message);
        }

        public void SetIsConnected(bool isConnected)
        {
            this.isConnected = isConnected;
        }

        public bool GetIsConnected() {
            return isConnected;
        }

        public Socket GetClientSocket()
        {
            return clientSocket;
        }

        public string GetUsername()
        {
            return username;
        }

       

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chat_system_server.Models
{
    public class User
    {
        private string address;
        private string username;
        private DateTime firstJoined;
        private DateTime left;
        private List<string> chatMessages; // Keeping it simple with only saved string chats for now
        private Socket clientSocket;


        public User(string address, string username, Socket clientSocket) {
            this.address = address;
            this.username = username;
            this.firstJoined = DateTime.Now;
            this.clientSocket = clientSocket;
        }

        public List<string> GetUserChats()
        {
            return chatMessages;
        }

        public void AddChatMessage(string message)
        {
            chatMessages.Add(message);
        }

        public Socket GetClientSocket()
        {
            return clientSocket;
        }


    }
}

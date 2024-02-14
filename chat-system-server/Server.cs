using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        private List<Socket> clientConnections;

        public Server(int port, string ip)
        {
            this.port = port;
            this.ip = ip;

            serverEndpoint = new IPEndPoint(
                new IPAddress(Convert.FromBase64String(ip)),
                port
            );

            // Create the entry socket
            entry = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket with the ip information:
            entry.Bind(serverEndpoint);
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

        public void AddClientConnection(Socket clientConnection)
        {
            clientConnections.Add(clientConnection);
        }

        public List<Socket> GetClientConnections()
        {
            return clientConnections;
        }



    }
}

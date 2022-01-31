using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VaServerAlpha_0._1
{
    public class UnityServer
    {
        private static readonly Socket serverSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 11001;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        public static ServerSettings a;

        static void Main()
        {
            Console.Title = "Server";
            SetupServer();
            a = ServerSettings.SS;
            Console.ReadLine();
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(10);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            clientSockets.Add(socket);

            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;
            var ip = (IPEndPoint)current.RemoteEndPoint;
            string strConnect = ip.Address.ToString();

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }
            byte[] recBuf = new byte[received];
            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            byte[] mac = new byte[12];
            for (int i = 0; i < 12; i++)
            {
                if (i < 12)
                    mac[i] = buffer[i];

            }

            DB.AddClientInfo(ip.ToString(), mac);
            if (DB.ISUserOnline(mac) == false)
                current.Send(Encoding.UTF8.GetBytes("autorize erore"));
            else
            {
                Array.Copy(buffer, recBuf, received);
                string text = Encoding.UTF8.GetString(recBuf);
                Console.WriteLine("Received Text: " + text);
                var Words = text.Split(' ');
                DB.analize(Words[0], Words);
                while (true)
                {
                    if (Client.ReqTempl.dataIsredy)
                    {
                        current.Send(Client.ReqTempl.Serialize());
                        Client.TurnOff();
                        break;
                    }
                }
            }
            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
    }

}


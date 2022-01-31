
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VaServerAlpha_0._1
{
    public class ServerSettings
    {
        
            private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            private static readonly List<Socket> clientSockets = new List<Socket>();
            private const int BUFFER_SIZE = 2048;
            private const int PORT = 100;
            private static readonly byte[] buffer = new byte[BUFFER_SIZE];

            static void Main()
            {
                Console.Title = "Server";
                SetupServer();
                Console.ReadLine(); // When we press enter close everything
                CloseAllSockets();
            }

            private static void SetupServer()
            {
                Console.WriteLine("Setting up server...");
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                serverSocket.Listen(0);
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
                catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
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
                Array.Copy(buffer, recBuf, received);
                string text = Encoding.ASCII.GetString(recBuf);
                Console.WriteLine("Received Text: " + text);

                if (text.ToLower() == "exit") // Client wants to exit gracefully
                {
                    // Always Shutdown before closing
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    clientSockets.Remove(current);
                    Console.WriteLine("Client disconnected");
                    return;
                }


                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
        private static object[] AnalizeReqest(string req,string ip)
        {
            DataTable DT = new DataTable();
            object[] answ = new object[3];
            var words = req.Split(' ');
            switch (words[1])
            {
                case "LOGIN":
                    answ[0] = DB.AutorizeUser(words[3], words[5], words[0]);
                    break;
                case "REGISTR":
                    answ[0] = DB.RegistrateUser(words[3], words[5], words[7], Encoding.ASCII.GetBytes(words[0]));
                    break;
                case "REPORT":
                    switch (words[3])
                    {
                        case "1":
                            answ[1] = DB.GetDataTable(Convert.ToInt32(words[3]), words[5]);
                            break;
                        case "2":
                            answ[1] = DB.GetDataTable(Convert.ToInt32(words[3]),"");
                            break;
                        case "3":
                            var param = words[5].Replace('_', ' ');
                            answ[1] = DB.GetDataTable(Convert.ToInt32(words[3]), param);
                            break;
                        case "4":
                            
                            answ[1] = DB.GetDataTable(Convert.ToInt32(words[3]), words[5]);
                            break;
                        case "5":
                            answ[1] = DB.GetDataTable(Convert.ToInt32(words[3]), "");
                            break;
                    }
                    break;
                case "SAULT":
                    answ[2] = DB.GetSault(words[3]);
                    break;
            }



            return answ;
        }
    }
}

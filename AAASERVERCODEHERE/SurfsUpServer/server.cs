using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SurfsUpServer
{
    class server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, client> clients = new Dictionary<int, client>();
        public delegate void PacketHandler(int id, packet pack);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void start(int maxPlayers, int portNumber)
        {
            MaxPlayers = maxPlayers;
            Port = portNumber;
            InitializeServerData();
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine("TCPListener started");
            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);
            Console.WriteLine($"Server started on {Port} ");
        }
        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from : {client.Client.RemoteEndPoint}");

            for (int i = 1; i <= MaxPlayers; i ++)
            {
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }
            Console.WriteLine("Server is full");
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if(data.Length < 4)
                {
                    return; 
                }
                using (packet pack = new packet(data))
                {
                    int clientID = pack.ReadInt();
                    if(clientID == 0)
                    {
                        return;
                    }

                    if(clients[clientID].udp.endPoint == null)
                    {
                        clients[clientID].udp.Connect(clientEndPoint);
                        return;
                    }

                    if(clients[clientID].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientID].udp.HandleData(pack);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static void SendUDPData(IPEndPoint clientEndPoint, packet pack)
        {
            try
            {
                if(clientEndPoint != null)
                {
                    udpListener.BeginSend(pack.ToArray(), pack.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i ++)
            {
                clients.Add(i, new client(i));
            }
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, serverHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, serverHandle.PlayerMovement }
            };
            Console.WriteLine("Initialization done");
        }
    }
}

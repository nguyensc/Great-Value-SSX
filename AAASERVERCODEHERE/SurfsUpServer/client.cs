using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace SurfsUpServer
{
    class client
    {
        public static int dataBuffersize = 4096;

        public player player;
        public int id;
        public TCP tcp;
        public UDP udp;

        public client(int _id)
        {
            id = _id;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private packet receivedData;
            private byte[] receiveBuffer;
            public TCP(int ID)
            {
                id = ID;
            }

            public void Connect(TcpClient Socket)
            {
                socket = Socket;
                socket.ReceiveBufferSize = dataBuffersize;

                stream = socket.GetStream();

                receivedData = new packet();
                receiveBuffer = new byte[dataBuffersize];

                stream.BeginRead(receiveBuffer, 0, dataBuffersize, ReceiveCallback, null);
                serversend.Welcome(id, "Welcome to [Game name here]");
            }

            public void SendData(packet pack)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(pack.ToArray(), 0, pack.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if(byteLength <= 0)
                    {
                        server.clients[id].Disconnect();
                        return;
                    }
                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));

                    stream.BeginRead(receiveBuffer, 0, dataBuffersize, ReceiveCallback, null);

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    server.clients[id].Disconnect();
                }
            }
            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    threadManager.ExecuteOnMainThread(() =>
                    {
                        using (packet pack = new packet(packetBytes))
                        {
                            int packetID = pack.ReadInt();
                            server.packetHandlers[packetID](id, pack);
                        }
                    });
                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }
                if (packetLength <= 1)
                {
                    return true;
                }
                return false;
            }
            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receiveBuffer = null;
                receivedData = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;
            private int id;

            public UDP(int ID)
            {
                id = ID;
            }
            public void Connect(IPEndPoint endP)
            {
                endPoint = endP;
            }
            public void SendData(packet pack)
            {
                server.SendUDPData(endPoint, pack);
            }
            public void HandleData(packet pack)
            {
                int packetLength = pack.ReadInt();
                byte[] packetBytes = pack.ReadBytes(packetLength);

                threadManager.ExecuteOnMainThread(() =>
                {
                    using (packet pck = new packet(packetBytes))
                    {
                        int packetId = pck.ReadInt();
                        server.packetHandlers[packetId](id, pck);
                    }
                });
            }
            public void Disconnect()
            {
                endPoint = null;
            }
        }

        public void SendIntoGame(string playerName)
        {
            player = new player(id, playerName, new Vector3(-4, 22, -39));

            foreach (client cli in server.clients.Values)
            {
                if(cli.player != null)
                {
                    if(cli.id != id)
                    {
                        serversend.SpawnPlayer(id, cli.player);
                    }
                }
            }
            foreach (client cli in server.clients.Values)
            {
                if(cli.player != null)
                {
                    serversend.SpawnPlayer(cli.id, player);
                }
            }
        }
        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected");

            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}

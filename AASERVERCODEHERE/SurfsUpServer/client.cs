using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SurfsUpServer
{
    class client
    {
        public static int dataBuffersize = 4096;

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
                serversend.UDPTest(id);
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
        }
    }
}

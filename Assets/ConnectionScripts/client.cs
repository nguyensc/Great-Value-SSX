using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;

public class client : MonoBehaviour
{
    public static client instance;
    public static int dataBufferSize = 4096;

    public string ip = "67.167.183.119";
    //public string ip = UIManager.instance.ipField.text; //pull the ip from the field
    public int port = 46400;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(packet pack);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Destroy new instance.");
            Destroy(this);
        }
    }
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }
    private void OnApplicationQuit()
    {
        //runs when app closes
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        isConnected = true;
        Debug.Log("Trying to connect");
        tcp.Connect();

    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private byte[] receivedBuffer;
        private packet receivedData;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receivedBuffer = new byte[dataBufferSize];
            Debug.Log("Starting to connect");
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            Debug.Log("BeginConnect method finished");
        }
        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                Debug.Log("Didn't connect");
                return;
            }
            stream = socket.GetStream();
            receivedData = new packet();
            stream.BeginRead(receivedBuffer, 0, dataBufferSize, ReceiveCallback, null);
            Debug.Log("Beginning to read");
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
                Debug.Log(ex);
                throw;
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }
                byte[] data = new byte[byteLength];
                Array.Copy(receivedBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));

                stream.BeginRead(receivedBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on client connection");
                Disconnect();
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
                        packetHandlers[packetID](pack);
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
        private void Disconnect()
        {
            instance.Disconnect();
            receivedBuffer = null;
            receivedData = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }
        public void Connect(int localPort)
        {
            socket = new UdpClient(localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (packet pack = new packet())
            {
                SendData(pack);
            }
        }
        public void SendData(packet pack)
        {
            try
            {
                pack.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(pack.ToArray(), pack.Length(), null, null);
                }
            }
            catch (Exception ex)
            {

                Debug.Log(ex);
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length <= 4)
                {
                    instance.Disconnect();
                    return;
                }
                HandleData(data);
            }
            catch (Exception ex)
            {
                Disconnect();
                Console.WriteLine(ex);
            }
        }
        private void HandleData(byte[] data)
        {
            using (packet pack = new packet(data))
            {
                int packetLength = pack.ReadInt();
                data = pack.ReadBytes(packetLength);
            }

            threadManager.ExecuteOnMainThread(() =>
            {
                using (packet pack = new packet(data))
                {
                    int packetId = pack.ReadInt();
                    packetHandlers[packetId](pack);
                }
            });
            //This is what actually reads the packet data and calls the appropriate handler. Maybe we need to keep the thread manager around?
        }
        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome},
            {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition},
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation}
        };
        Debug.Log("Initialized client data");
    }
    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            Debug.Log("Disconnected from server");
        }
    }
}

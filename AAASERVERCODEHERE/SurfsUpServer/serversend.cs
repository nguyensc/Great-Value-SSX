using System;
using System.Collections.Generic;
using System.Text;

namespace SurfsUpServer
{
    class serversend
    {
        private static void SendTCPData(int toClient, packet pack)
        {
            pack.WriteLength();
            server.clients[toClient].tcp.SendData(pack);
        }

        private static void SendTCPDataToAll(packet pack)
        {
            pack.WriteLength();
            for (int i = 1; i <= server.MaxPlayers; i++)
            {
                server.clients[i].tcp.SendData(pack);
            }
        }

        private static void SendTCPDataToAll(int except, packet pack)
        {
            pack.WriteLength();
            for (int i = 1; i <= server.MaxPlayers; i++)
            {
                if(i != except)
                {
                    server.clients[i].tcp.SendData(pack);
                }
            }
        }
        private static void SendUDPData(int toClient, packet pack)
        {
            pack.WriteLength();
            server.clients[toClient].udp.SendData(pack);
        }
        private static void SendUDPDataToAll(packet pack)
        {
            pack.WriteLength();
            for (int i = 1; i <= server.MaxPlayers; i++)
            {
                server.clients[i].udp.SendData(pack);
            }
        }

        private static void SendUDPDataToAll(int except, packet pack)
        {
            pack.WriteLength();
            for (int i = 1; i <= server.MaxPlayers; i++)
            {
                if (i != except)
                {
                    server.clients[i].udp.SendData(pack);
                }
            }
        }
        public static void Welcome(int toClient, string msg)
        {
            using (packet pack = new packet((int)ServerPackets.welcome))
            {
                pack.Write(msg);
                pack.Write(toClient);

                SendTCPData(toClient, pack);
            }

        }
        public static void SpawnPlayer(int toClient, player play)
        {
            using (packet pack = new packet((int)ServerPackets.spawnPlayer))
            {
                pack.Write(play.id);
                pack.Write(play.username);
                pack.Write(play.position);
                pack.Write(play.rotation);

                SendTCPData(toClient, pack);
            }
        }
        public static void PlayerPosition(player play)
        {
            using (packet pack = new packet((int)ServerPackets.playerPosition))
            {
                pack.Write(play.id);
                pack.Write(play.position);
                //Console.WriteLine($"Sent position: {play.position} from {play.id}");
                SendUDPDataToAll(pack);
            }
        }

        public static void PlayerRotation(player play)
        {
            using (packet pack = new packet((int)ServerPackets.playerRotation))
            {
                pack.Write(play.id);
                pack.Write(play.rotation);
                //Console.WriteLine($"Sent rotation: {play.position} from {play.id}");
                SendUDPDataToAll(pack);
            }
        }
    }
}

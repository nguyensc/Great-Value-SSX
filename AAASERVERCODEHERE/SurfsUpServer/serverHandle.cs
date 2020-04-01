using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SurfsUpServer
{
    class serverHandle
    {
        public static void WelcomeReceived(int fromClient, packet pack)
        {
            int id = pack.ReadInt();
            string username = pack.ReadString();

            Console.WriteLine($"{server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected and is player {id} with username {username}.");
            if(fromClient != id)
            {
                Console.WriteLine($"Player {username} has assumed the wrong client id");
            }
            server.clients[fromClient].SendIntoGame(username);
        }

        public static void PlayerMovement(int fromClient, packet pack)
        {
            bool[] inputs = new bool[pack.ReadInt()];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = pack.ReadBool();
            }
            Quaternion rotation = pack.ReadQuaternion();
            Vector3 position = pack.ReadVector3();

            //Console.WriteLine($"Position {position} from {fromClient}");

            server.clients[fromClient].player.SetInput(position, rotation);
        }
    }
}

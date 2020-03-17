using System;
using System.Collections.Generic;
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
           //TODO: send player into game
        }
        public static void UDPTestReceived(int fromClient, packet pack)
        {
            string msg = pack.ReadString();
            Console.WriteLine($"UDPTestReceived {msg}");
        }
    }
}

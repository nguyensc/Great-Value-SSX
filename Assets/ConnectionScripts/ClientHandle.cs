using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(packet pack)
    {
        string mesg = pack.ReadString();
        int id = pack.ReadInt();

        Debug.Log($"Message from server: {mesg}");
        client.instance.myId = id;
        clientSend.WelcomeReceived();

        client.instance.udp.Connect(((IPEndPoint)client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(packet pack)
    {
        int id = pack.ReadInt();
        string username = pack.ReadString();
        Vector3 position = pack.ReadVector3();
        Quaternion rotation = pack.ReadQuaternion();


        gameManager.instance.SpawnPlayer(id, username, position, rotation);

    }

    public static void PlayerPosition(packet pack)
    {
        int id = pack.ReadInt();
        Vector3 position = pack.ReadVector3();

        if(client.instance.myId != id)
        {
            gameManager.players[id].transform.position = position;
        }
        
    }
    public static void PlayerRotation(packet pack)
    {
        int id = pack.ReadInt();
        Quaternion rotation = pack.ReadQuaternion();

        if(client.instance.myId != id)
        {
            gameManager.players[id].transform.rotation = rotation;
        }
        
        
    }
    //Might be able to convert this to just one method that takes the entire rigidbody and pulls the rotation and stuff from it.
}

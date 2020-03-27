using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clientSend : MonoBehaviour
{
    private static void SendTCPData(packet pack)
    {
        pack.WriteLength();
        client.instance.tcp.SendData(pack);
        Debug.Log("Tried to send data");
    }

    private static void SendUDPData(packet pack)
    {
        pack.WriteLength();
        client.instance.udp.SendData(pack);
    }
    #region Packets
    public static void WelcomeReceived()
    {
        Debug.Log("Welcome pack recieved");
        using (packet pack = new packet((int)ClientPackets.welcomeReceived))
        {
            pack.Write(client.instance.myId);
            pack.Write(UIManager.instance.usernameField.text);
            //this is where we write the usernameField to the server
            //meanwhile the gameManager writes the ID and username to the input when a player is spawned in
            //we don't need to write the IP to the server, but we do need it for connection
            // // I guess it could be hardcoded but that wouldn't be as cool
            //Especially if my IP is ephemeral, which it probably is
            SendTCPData(pack);
        }
    }

    #endregion

    public static void PlayerMovement(bool[] input)
    {
        //This needs to be modified so that it sends rotation values instead of booleans
        using (packet pack = new packet((int)ClientPackets.playerMovement))
        {
            pack.Write(input.Length);
            foreach (bool apple in input)
            {
                pack.Write(apple);
            }
            //Which means the above can probably just get removed, and replaced with the associated
            //rigidbody below
            //You might be able to send the entire reigidbody on its own and that's it
            //For now, just keep it at this
            //this method should be called in the playerController. The rotation is pulled from the gameManager
            //the game manager gets the data for the players in each client handle method
            pack.Write(gameManager.players[client.instance.myId].transform.rotation);

            SendUDPData(pack);
        }
    }
}

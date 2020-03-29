using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class guiSpeedCounterController : MonoBehaviour
{
    Text text;
    static playerController player;
    static bool playerConnected = false;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        //player = FindObjectOfType<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerConnected)
        {
            text.text = "hVelocity: " + player.hVelocity.ToString() + "\nslopeSpeed: " + player.getSlopeSpeed().ToString() + "\nverticalImpulse: " + player.getVerticalImpulse().ToString() + "\naccel (vector): " + player.GetAccelerationVector().ToString();
            return;
        }
    }

    public static void getPlayer()
    {
        player = FindObjectOfType<playerController>();
        playerConnected = true;
        Debug.Log("Got player for speed counter");
    }
    public static void setPlayerConnectedFalse(int id)
    {
        if(client.instance.myId == id)
        {
            playerConnected = false;
        }
        
    }
}

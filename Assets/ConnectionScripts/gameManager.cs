using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public static Dictionary<int, playerManager> players = new Dictionary<int, playerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefeb;
    public GameObject currentLocalPlayer;
    public bool gotConnection = false;
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
    public void SpawnPlayer(int ID, string username, Vector3 position, Quaternion rotation)
    {
        GameObject playerObj;
        if (ID == client.instance.myId)
        {
            Destroy(currentLocalPlayer);
            playerObj = Instantiate(localPlayerPrefab, position, rotation);
            Debug.Log("Instantiated a player");
        }
        else
        {
            playerObj = Instantiate(playerPrefeb, position, rotation);
        }

        playerObj.GetComponent<playerManager>().id = ID;
        playerObj.GetComponent<playerManager>().username = username;
        players.Add(ID, playerObj.GetComponent<playerManager>());

        guiRightArrowController.setPlayerConnectedFalse(ID);
        guiLeftArrowController.setPlayerConnectedFalse(ID);
        guiSpeedCounterController.setPlayerConnectedFalse(ID);

        guiRightArrowController.getPlayer();
        guiLeftArrowController.getPlayer();
        guiSpeedCounterController.getPlayer();

        //Debug.Log("Got all the players");

        gotConnection = true;
    }

}

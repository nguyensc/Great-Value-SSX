using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public static Dictionary<int, playerManager> players = new Dictionary<int, playerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefeb;
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
            playerObj = Instantiate(localPlayerPrefab, position, rotation);
        }
        else
        {
            playerObj = Instantiate(playerPrefeb, position, rotation);
        }

        playerObj.GetComponent<playerManager>().id = ID;
        playerObj.GetComponent<playerManager>().username = username;
        players.Add(ID, playerObj.GetComponent<playerManager>());
    }

}

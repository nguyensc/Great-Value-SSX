using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guiRightArrowController : MonoBehaviour
{
    static playerController player;
    static RectTransform rect;
    static bool playerConnected = false;

    float deltaPos = 0.5f;
    float offsetx = 0f;
    float originalPosX = 10f;
    
    void Start()
    {
        //player = FindObjectOfType<playerController>();
        rect = GetComponent<RectTransform>();

        //originalPosX = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerConnected)
        {
            if (player.GetAccelerationVector() > 0)
            {
                offsetx = Mathf.Min(offsetx + deltaPos, 75f);
            }
            else if (player.GetAccelerationVector() == 0)
            {
                offsetx = Mathf.Max(offsetx - deltaPos * 2, 0f);
            }
            else
            {
                offsetx = Mathf.Max(offsetx - deltaPos * 10, 0f);
            }
            rect.localPosition = new Vector3(originalPosX + offsetx, rect.localPosition.y, rect.localPosition.z);
        }

    }

    public static void getPlayer()
    {
        player = FindObjectOfType<playerController>();
        playerConnected = true;
        Debug.Log("Got player for right arrow");
    }
    public static void setPlayerConnectedFalse(int id)
    {
        if (client.instance.myId == id)
        {
            playerConnected = false;
        }
    }
}

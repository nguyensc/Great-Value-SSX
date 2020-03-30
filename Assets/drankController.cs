using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drankController : MonoBehaviour
{
    GameObject player;
    playerController playerCtrl;
    
    float targetRotToCamera = 300f;
    float initialRot = 90f;
    float deltaRot = 2f;
    float rotationSpeed = 2f;
    float drankCounter = 0f;
    float drankTimer = 1f;
    
    bool resetRot = false;
    bool dranking = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player1");    
        playerCtrl = player.GetComponent<playerController>();
    }

    void RotateTowardCamera()
    {
        

        transform.RotateAround(transform.position, player.transform.right, -rotationSpeed);

        
    }

    void RotateAway()
    {
        transform.RotateAround(transform.position, player.transform.right, rotationSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        // check to make sure drank not in progress
        if (!dranking)
        {
            drankCounter = drankTimer;
            dranking = playerCtrl.getDrank();
        }
        // other wise go thru drank process
        else
        {
            Debug.Log(transform.rotation.eulerAngles.x);
            if (!resetRot)
            {
                drankCounter-= deltaRot * Time.deltaTime;
                RotateTowardCamera();

                if (drankCounter <= 0)
                {
                    resetRot = true;
                    drankCounter = -drankTimer;
                }
            }
            else
            {
                drankCounter+= deltaRot * Time.deltaTime;
                RotateAway();

                if (drankCounter >= 0)
                {
                    resetRot = false;
                    drankCounter = 0f;
                    dranking = false;

                    // set spawn coords
                    playerCtrl.SetSpawnCoords();
                }
            }
        }
    }
}

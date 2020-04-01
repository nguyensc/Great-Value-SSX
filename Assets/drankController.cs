﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drankController : MonoBehaviour
{
    playerController player;
    float drankTimer = 1f;
    float drankCounter = 0f;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<playerController>();
    }

    void RotateBottle(float dir)
    {
        Vector3 eulers = transform.localRotation.eulerAngles;
        
        float newEuler = 0f;
        if (dir > 0)
        {
            newEuler = Mathf.Min(eulers.x + 50f * Time.deltaTime, 270);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - 1f * Time.deltaTime, transform.localPosition.z);
        }
        else
        {
            newEuler = eulers.x - 50f * Time.deltaTime;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 1f * Time.deltaTime, transform.localPosition.z);
        }
        
        Quaternion newRotation = Quaternion.Euler(new Vector3(newEuler, eulers.y, eulers.z));
        transform.Rotate(Vector3.right, dir * 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.paused)
        {
            Debug.Log(transform.localPosition.y);
            if (drankCounter <= 0f)
            {

                if (drankCounter < -0.1f)
                {
                    drankCounter = Mathf.Min(drankCounter + 1f * Time.deltaTime, -0.1f);
                    if (drankCounter == -0.1f)
                    {
                        RotateBottle(0.25f);
                    }
                    else
                    {
                        RotateBottle(1);
                    }
                }      

                if (player.getDrank())
                {
                    drankCounter = drankTimer;
                }
            }
            else
            {
                if (drankCounter > 0.1f)
                {
                    drankCounter = Mathf.Max(drankCounter - 1f * Time.deltaTime, 0.1f);
                    RotateBottle(-1);
                }
                else if (!player.getDrank())
                {
                    drankCounter = -drankTimer;
                }
            }
        }
    }
}

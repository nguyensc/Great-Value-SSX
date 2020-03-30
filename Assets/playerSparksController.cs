using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerSparksController : MonoBehaviour
{
    GameObject player;
    playerController pController;
    ParticleSystem sparks;

    void Start()
    {
        sparks = GetComponent<ParticleSystem>();
        player = GameObject.FindWithTag("Player1");
        pController = player.GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pController.onRail)
        {
            sparks.Play();
        }
        else
        {
            sparks.Stop();
        }
        transform.position = player.transform.position + player.transform.forward * 3 + Vector3.down;        
    }
}

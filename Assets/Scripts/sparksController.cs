using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sparksController : MonoBehaviour
{
    playerController player;
    ParticleSystem ps;

    void Start()
    {
        player = FindObjectOfType<playerController>();
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
    }

    void Update()
    {
        if (player.onRail)
        {
            if (ps.isStopped)
            {
                ps.Play();
            }
        }
        else
        {
            if (ps.isPlaying)
            {
                //ps.Clear();
                ps.Stop();
            }

        }
        
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedLineController : MonoBehaviour
{
    playerController player;
    ParticleSystem ps;
    void Start()
    {
        player = FindObjectOfType<playerController>();
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
    }

    // Update is called once per frame
    void Update()
    {

        if (player.getCurrentVelocity() > 30)
        {
            if (ps.isStopped)
            {
                ps.Play();
            }
        }
        else if (ps.isPlaying)
        {
            ps.Clear();
            ps.Stop();
        }
        
    }
}

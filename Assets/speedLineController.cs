using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedLineController : MonoBehaviour
{
    playerController player;
    ParticleSystem ps;

    Vector3 initLocalPosition;
    Vector3 initEulerAngles;
    void Start()
    {
        player = FindObjectOfType<playerController>();
        ps = GetComponent<ParticleSystem>();
        ps.Stop();

        initLocalPosition = transform.localPosition;
        initEulerAngles = transform.eulerAngles;
    }

    void rotateToUpright()
    {
        Vector3 eulers = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(new Vector3(-90, eulers.y, eulers.z));
        transform.localPosition += Vector3.up;
    }

    void resetLocalTransform()
    {
        transform.localPosition = initLocalPosition;
        transform.localEulerAngles = initEulerAngles;
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
        else if (player.getVerticalImpulse() < 0)
        {
            if (ps.isStopped)
           {
                rotateToUpright();
                ps.Play();
            }
            
        }
        else if (ps.isPlaying)
        {
            resetLocalTransform();

            ps.Clear();
            ps.Stop();
        }
        
    }
}

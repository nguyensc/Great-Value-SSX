using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rightCrossHairManager : MonoBehaviour
{
    PlayerCharacterController player;
    Vector3 initialPosition;
    Vector3 initialScale;
    void Start()
    {
        player = FindObjectOfType<PlayerCharacterController>();
        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.rotation > 0)
        {
            transform.position = new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z);
            transform.localScale += transform.localScale * (player.ps_currDirectionAccel / 50);
        }
        else
        {
            transform.position = initialPosition;
            transform.localScale = initialScale;
        }

    }
}

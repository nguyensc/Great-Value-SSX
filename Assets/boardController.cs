using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boardController : MonoBehaviour
{
    playerController player;
    float forwardOffset = 0f;
    float maxForwardOffset = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (player.GetAccelerationVector() == 0)
        {
            forwardOffset = Mathf.Min(forwardOffset + 0.25f * Time.deltaTime, maxForwardOffset);
        }
        else
        {
            forwardOffset = Mathf.Max(forwardOffset - 0.25f * Time.deltaTime, 0f);
        }

        transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 0.5f, player.transform.position.z) + player.transform.forward * forwardOffset;       
    }
}

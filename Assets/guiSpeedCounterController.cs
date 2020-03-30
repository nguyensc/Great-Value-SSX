using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class guiSpeedCounterController : MonoBehaviour
{
    Text text;
    playerController player;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        player = FindObjectOfType<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "hVelocity: " + player.getCurrentVelocity().ToString() + "\nslopeSpeed: " + player.getSlopeSpeed().ToString() + "\nverticalImpulse: " + player.getVerticalImpulse().ToString() + "\naccel (vector): " + player.GetAccelerationVector().ToString();
        return;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class guiScoreController : MonoBehaviour
{
    GameObject p;
    playerController pController;
    Text scoreText;
    void Start()
    {
        p = GameObject.Find("Player");
        pController = p.GetComponent<playerController>();
        scoreText = GetComponent<Text>();
    }

    void Update()
    {
        int newScore = (int)pController.getScore();
        scoreText.text = newScore.ToString();
    }
}

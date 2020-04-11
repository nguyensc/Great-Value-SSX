using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteText : MonoBehaviour
{
    bool muted = false;
    Text text;
    void Start()
    {
        text = GetComponent<Text>();
    }

    public void UpdateMuteText()
    {
        muted = !muted;
        if (muted)
        {
            text.text = "UNMUTE";
        }
        else
        {
            text.text = "MUTE";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

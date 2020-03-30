using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class guiRail : MonoBehaviour
{
    Text text;
    playerController player;
    float[] colors = {255f, 255f, 255f};

    int currentDeltaColor = 0;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        player = FindObjectOfType<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "";

        for (int i=0; i<3; ++i)
        {
            if (i == currentDeltaColor)
            {
                colors[i] = colors[i] - 15f;  

                if (colors[i] <= 128f)
                {
                    colors[i] = 255f;
                    currentDeltaColor = (currentDeltaColor + 1) % 2;
                }

                continue;
            }
            
        }

        text.color = new Color(colors[0], colors[1], colors[2], 255f);
        
        text.text = player.spinCounter.ToString();

        if (player.getOnRail())
        {
            text.text = "NASTY GRIND BROH\n";
            

        }

        return;
    }
}

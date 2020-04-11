using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class guiNotifierController : MonoBehaviour
{
    GameObject p;
    playerController pController;
    Text notifText;
    Color curr;
    float fadeTimer = 5f;
    float fadeCounter = 0f;

    float alpha = 0f;
    bool moved = true;

    AudioSource sketchyLandingSound;
    AudioSource[] sounds;
    int[] pausedSounds;

    void Start()
    {
        p = GameObject.Find("Player");
        pController = p.GetComponent<playerController>();
        notifText = GetComponent<Text>();
    
        Color curr = Color.white;

        sounds = GetComponents<AudioSource>();
        sketchyLandingSound = sounds[0];
        pausedSounds = new int[sounds.Length];
        resetPausedSounds();
        
    }

    void resetPausedSounds()
    {
        foreach (int i in pausedSounds)
        {
            pausedSounds[i] = 0;
        }
    }

    void Update()
    {
        if (!pController.paused)
        {
            if (moved)
            {
                // resume any effects audio
                foreach (int i in pausedSounds)
                {
                    sounds[i].UnPause();
                    pausedSounds[i] = 0;
                }

                Vector3 pos = transform.localPosition;
                transform.localPosition = new Vector3(pos.x, 0, pos.z);
                moved = false;
            }

            string newNotification = pController.getCurrentNotification();

            // update the display notification only if it is new
            if (newNotification != notifText.text)
            {
                if (newNotification.ToString().ToLower().StartsWith("sketch"))
                {
                    sketchyLandingSound.Play();
                }

                notifText.text = newNotification.ToString();
                curr.a = 0; // make transparent
                fadeCounter = 1;
            }
            else
            {
                
                // make notification slowly appear
                if (fadeCounter > 0)
                {
                    alpha += 3f * Time.deltaTime;
                    if (alpha >= 1)
                    {
                        fadeCounter = -1;
                    }
                }
                else if (fadeCounter < 0)
                {
                    if (fadeCounter <= -2)
                    {
                        alpha -= 1f * Time.deltaTime;
                        if (alpha <= 0)
                        {
                            fadeCounter = 0;
                        }
                    }
                    // let the text display solidly for a second
                    else
                    {
                        fadeCounter -= 3f * Time.deltaTime;
                    }
                }
            }
                        
        }
        else
        {
            // move the text out of view when paused so buttons can be clicked
            if  (!moved)
            {
                int i = 0;
                // pause any effects audio
                foreach (AudioSource s in sounds)
                {
                    if (s.isPlaying)
                    {
                        pausedSounds[i] = 1;
                        s.Pause();
                    }
                    ++i;
                }

                Vector3 pos = transform.localPosition;
                transform.localPosition = new Vector3(pos.x, -500, pos.z);
                moved = true;
            }
        }

        alpha = Mathf.Clamp(alpha, 0f, 1f);
        Color col = Color.white;
        col.a = alpha;
        notifText.color = col;
    }
}

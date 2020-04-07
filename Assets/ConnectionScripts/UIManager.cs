using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public InputField usernameField;
    public bool isConnected = false;

    public AudioMixer masterMixer;
    playerController pCtrl;

    Canvas c;

    AudioSource pauseSound;
    AudioSource resumeSound;

    AudioSource[] music;
    int currentSong = 0;

    public void Start()
    {
        AudioSource[] sounds = GetComponents<AudioSource>();
        pauseSound = sounds[0];
        resumeSound = sounds[1];

        music = new AudioSource[sounds.Length - 2];
        for (int i=2; i<sounds.Length; ++i)
        {
            music[i-2] = sounds[i];
        }
        
        currentSong = (int)Random.Range(0, music.Length);
        music[currentSong].Play();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            c = GetComponent<Canvas>();
            pCtrl = FindObjectOfType<playerController>();
        }
        else if (instance != this)
        {
            Debug.Log("Destroy new instance.");
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        Debug.Log("The button was pressed");
        startMenu.SetActive(false);
        usernameField.interactable = false;
        client.instance.ipField.interactable = false;
        isConnected = true;

        client.instance.onStart();
        client.instance.ConnectToServer();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            c.enabled = !c.enabled;

            if (c.enabled)
            {
                masterMixer.SetFloat("cutoff", 1000);
                pauseSound.Play();
            }
            
        }

        if (c.enabled)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (pCtrl.paused && !c.enabled)
        {
            resumeSound.Play();
            masterMixer.SetFloat("cutoff", 22000);
        }

        pCtrl.paused = c.enabled;
    }
}

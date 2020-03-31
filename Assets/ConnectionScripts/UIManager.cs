using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public InputField usernameField;
    public InputField ipField;

    playerController pCtrl;

    Canvas c;

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
        ipField.interactable = false;

        client.instance.ConnectToServer();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            c.enabled = !c.enabled;
        }

        if (c.enabled)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        pCtrl.paused = c.enabled;
    }
}

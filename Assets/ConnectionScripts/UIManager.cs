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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
        //Debug.Log("Button was pressed");
        client.instance.ConnectToServer();
    }
}

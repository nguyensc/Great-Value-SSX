using UnityEngine;

public class camController : MonoBehaviour
{
    playerController player;

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked; // hide the cursor
        player = FindObjectOfType<playerController>();
    }
    
    void Update()
    {
        /*
        // Exit Sample  
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
            #endif
        }
        */
        
    }
}

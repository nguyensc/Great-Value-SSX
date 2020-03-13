using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboText : MonoBehaviour
{
    float x;
    float y;
    float z;
    RoboController roboController;
    PlayerCharacterController player;
    TextMesh txtMesh;
    string[] text = {"","Hello world!"};
    int textToDisplay = 0;

    void Start()
    {
        txtMesh = FindObjectOfType<TextMesh>();
        textToDisplay = 0;
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;

        roboController = FindObjectOfType<RoboController>();
        player = FindObjectOfType<PlayerCharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        textToDisplay = 1;
    }

    private void OnTriggerExit(Collider other)
    {
        textToDisplay = 0;
    }

    void UpdatePositions() {
        transform.position = new Vector3(x, y, z);
    }

    // Update is called once per frame
    void Update()
    {
        txtMesh.text = text[textToDisplay];

        if (textToDisplay > 0)
        {
            Vector3 relative = transform.InverseTransformPoint(player.transform.position);
            float angle1 = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;

            relative = transform.InverseTransformPoint(roboController.transform.position);
            float angle2 = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;

            roboController.Turn(0, angle1 - 180 + angle2, 0);

        }
        else 
        {
            Vector3 relative = transform.InverseTransformPoint(roboController.transform.position);
            float angle2 = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;

            roboController.Turn(0, 0, 0);
        }

        UpdatePositions();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIController : MonoBehaviour
{
    camController camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = FindObjectOfType<camController>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = camera.transform.position + camera.transform.forward*2;
        transform.rotation = camera.transform.rotation;
    }
}

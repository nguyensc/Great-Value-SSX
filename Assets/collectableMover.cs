using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectableMover : MonoBehaviour
{
    float rotationSpeed = 50f;
    float gravity = 0.0025f;
    float currHeight = 0f;
    float maxHeight = 0.5f;
    float minHeight = 0f;

    void Start()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit collectableHit, 10f);
        minHeight = collectableHit.transform.position.y + 1f;
        currHeight = minHeight;
        maxHeight = minHeight + maxHeight;
        transform.position = new Vector3(transform.position.x, minHeight, transform.position.z); 
    }

    // Update is called once per frame
    void Update()
    {
        if (currHeight > maxHeight || currHeight < minHeight)
        {
            gravity *= -1f;
        }

        currHeight += gravity;
        transform.position += Vector3.up * gravity; 

        transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}

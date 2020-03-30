using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guiLeftArrowController : MonoBehaviour
{
    playerController player;
    RectTransform rect;

    float deltaPos = 25f;
    float offsetx = 0f;
    float originalPosX = -10f;
    
    void Start()
    {
        player = FindObjectOfType<playerController>();
        rect = GetComponent<RectTransform>();

        //originalPosX = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (player.GetAccelerationVector() < 0)
        {
            offsetx = Mathf.Max(offsetx - deltaPos * Time.deltaTime, -100f); 
        }
        else if (player.GetAccelerationVector() == 0)
        {
            offsetx = Mathf.Min(offsetx + deltaPos * 2 * Time.deltaTime, 0f);
        }
        else
        {
            offsetx = Mathf.Min(offsetx + deltaPos * 5 * Time.deltaTime, 0f); 
        } 
        rect.localPosition = new Vector3(originalPosX + offsetx, rect.localPosition.y, rect.localPosition.z);  
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class leftMomentumController : MonoBehaviour
{
    GameObject player;
    playerController playerCtrl;
    
    int numMarkers;
    GameObject[] leftMarkers;

    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player");
        //playerCtrl = player.GetComponent<playerController>();
        playerCtrl = FindObjectOfType<playerController>();

        numMarkers = 7;

        leftMarkers = new GameObject[numMarkers];
        for (int i=0; i<numMarkers; ++i)
        {
            leftMarkers[i] = GameObject.FindWithTag("LMarker" + i.ToString());
            
            // shift their positions over a bit
            RectTransform rect = leftMarkers[i].GetComponent<RectTransform>();
            rect.localPosition = new Vector3(rect.localPosition.x - 20 - 20 * i, rect.localPosition.y, rect.localPosition.z);
            
            leftMarkers[i].SetActive(false);
        }
    
    }

    public void updateMarkers(int threshold, bool show)
    {
        leftMarkers[threshold].SetActive(show);
    }

    void resetMarkers()
    {
        foreach (GameObject marker in leftMarkers)
        {
            marker.SetActive(false);
        } 
    }

    void Update()
    {
        if (!playerCtrl.onRamp)
        {
            resetMarkers();
        }
        return;
    }
}

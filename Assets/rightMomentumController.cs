using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rightMomentumController : MonoBehaviour
{
    GameObject player;
    playerController playerCtrl;
    
    int numMarkers;
    GameObject[] rightMarkers;

    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player");
        //playerCtrl = player.GetComponent<playerController>();
        playerCtrl = FindObjectOfType<playerController>();

        numMarkers = 7;

        rightMarkers = new GameObject[numMarkers];
        for (int i=0; i<numMarkers; ++i)
        {
            rightMarkers[i] = GameObject.FindWithTag("Marker" + i.ToString());
            
            // shift their positions over a bit
            RectTransform rect = rightMarkers[i].GetComponent<RectTransform>();
            rect.localPosition = new Vector3(rect.localPosition.x + 20 + 20 * i, rect.localPosition.y, rect.localPosition.z);
            
            rightMarkers[i].SetActive(false);
        }
    
    }

    public void updateMarkers(int threshold, bool show)
    {
        rightMarkers[threshold].SetActive(show);
    }

    void resetMarkers()
    {
        foreach (GameObject marker in rightMarkers)
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

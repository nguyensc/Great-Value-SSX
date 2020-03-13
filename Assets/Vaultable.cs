using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vaultable : MonoBehaviour
{
    Ray r;
    PlayerCharacterController m_PlayerCharacterController;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCharacterController = FindObjectOfType<PlayerCharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 a = transform.position;
        a.y += 3.5f;
        a.z += 1f;        
        Vector3 b = transform.right;
        b.x += 2f;

        for (float i=1f; i>0.1f; i-=0.1f)
        {
            r.origin = a;
            r.direction = b;

            Debug.DrawRay(a, b, Color.red);
            if (Physics.Raycast(r, 10))
            {
                Debug.Log("HIT");
                m_PlayerCharacterController.canVault = true;
                break;
            }
            else
                m_PlayerCharacterController.canVault = false;

            a.z -= 0.1f;
        }

        

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class WallRun : MonoBehaviour
{
    private bool isWallR = false;
    private bool isWallL = false;
    private RaycastHit hitR;
    private RaycastHit hitL;
    private int jumpCount = 0;
    private RigidbodyFirstPersonController rc;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rc = GetComponent<RigidbodyFirstPersonController>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rc.Grounded)
        {
            jumpCount = 0;
        }
        if (Input.GetKey(KeyCode.Space) && !rc.Grounded && jumpCount <= 1)
        {
            if (Physics.Raycast(transform.position, transform.right, out hitR, 1))
            {
                if (hitR.transform.tag == "Wall")
                {
                    isWallR = true;
                    isWallL = false;
                    jumpCount += 1;
                    rb.useGravity = false; //Really this should be a small number, not totally off
                    StartCoroutine(afterRun());

                }
            }
            if (Physics.Raycast(transform.position, -transform.right, out hitL, 1))
            {
                if (hitL.transform.tag == "Wall")
                {
                    isWallR = false;
                    isWallL = true;
                    jumpCount += 1;
                    rb.useGravity = false; //Really this should be a small number, not totally off
                    StartCoroutine(afterRun());

                }
            }
        }
    }
    IEnumerator afterRun()
    {
        yield return new WaitForSeconds(1.0f);
        isWallR = false;
        isWallL = false;
        rb.useGravity = true;
    }
}

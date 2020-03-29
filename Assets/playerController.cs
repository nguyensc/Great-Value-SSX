using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    Rigidbody rb;
    Camera camera;

    GameObject[] guiRightMarkers;
    GameObject[] guiLeftMarkers;

    Vector3 groundNormal;

    public enum trickStates {
        NONE,
        ON_RAMP,
        SETUP_SPIN,
        SPIN
    }
    public trickStates trickState;
    float prevAccelDir = 1f;
    public float spinDebug = 0f;
    int spinSetupTimer = 3;
    int spinTimer = 3;
    public int spinCounter = 0;
    float currentSpinRot = 0f;
    float currentSpinVector = 1f;
    Quaternion preSpinRotation; 

    public float currentMouseAccel = 0f;
    public float hVelocity = 0f;
    float hVelocityMin = 0f;
    float normalRotationSpeed = 100f;
    float slopeRotationSpeed = 85f;
    float rotationSpeed = 100f;
    float accel = 0f;
    float accelDir = 1f;
    float verticalImpulse = 0f;
    float slopeSpeed = 0f;
    float smoothRot = 0.3f;
    float angle = 0f;
    float maxAirRotation = 35f;
    float gravity = 5f;

    float initialVelocity = 0f;
    float finalVelocity = 100f;
    float currentHVelocity = 0f;
    float initialAcceleration = 0f;
    float finalAcceleration = 10f;
    float currentAcceleration = 0f;
    float deceleration = 30f;
    float momentum = 0f;
    float dischargedMomentum = 0f;
    float initialMomentum = 0f;
    float finalMomentum = 5f;
    float rightMomentum = -1f;
    float leftMomentum = -1f;
    float totalSpinRot = 0f;

    bool onGround = false;
    bool onRail = false;
    bool onRamp = false;
    bool crouched = false;

    // Start is called before the first frame update
    void Start()
    {
        // set up rigidbody interface
        rb = GetComponent<Rigidbody>();

        // set up camera
        camera = FindObjectOfType<Camera>();

        // set up gui stuff
        guiRightMarkers = new GameObject[6];
        for (int i=0; i<6; ++i)
        {
            guiRightMarkers[i] = GameObject.FindWithTag("Marker" + i.ToString());
            // shift their positions over a bit
            RectTransform rect = guiRightMarkers[i].GetComponent<RectTransform>();
            rect.localPosition = new Vector3(rect.localPosition.x + 20 + 20 * i, rect.localPosition.y, rect.localPosition.z);
            // hide each marker
            guiRightMarkers[i].SetActive(false);
        }

        guiLeftMarkers = new GameObject[6];
        for (int i=0; i<6; ++i)
        {
            guiLeftMarkers[i] = GameObject.FindWithTag("LMarker" + i.ToString());
            // shift their positions over a bit
            RectTransform rect = guiLeftMarkers[i].GetComponent<RectTransform>();
            rect.localPosition = new Vector3(rect.localPosition.x - 20 - 20 * i, rect.localPosition.y, rect.localPosition.z);
            // hide each marker
            guiLeftMarkers[i].SetActive(false);
        }

        groundNormal = Vector3.up;

        trickState = trickStates.NONE;
    }


    // returns accel with the context of accelDir
    // mostly used by the arrows in gui
    public float GetAccelerationVector()
    {
        return currentAcceleration * accelDir;
    }

    public float getSlopeSpeed()
    {
        return slopeSpeed;
    }

    public float getVerticalImpulse()
    {
        return verticalImpulse;
    }

    public bool getOnRail()
    {
        return onRail;
    }

    // works like copysign but will return a 0 if the input is 0
    float GetZero(float f)
    {
        if (f == 0f)
        {
            return 0f;
        }
        else
        {
            return Mathf.Sign(f);
        }
    }

    bool CollectableCheck()
    {
        if (Physics.SphereCast(transform.position, 3f, transform.forward, out RaycastHit sphereHit, 0.25f)){
            if (sphereHit.transform.tag == "Collectable")
            {
                Destroy(sphereHit.transform.gameObject);
                return true;
            }
        }
        return false;
    }

    bool GroundCheck()
    {
        
        smoothRot = 0.0025f;
        Vector3 groundCheckDir;
        // if previously on the ground use the cameras down direction to raycast
        if (onGround)
        {
            groundCheckDir = -camera.transform.up;
        }
        // otherwise raycast in the world's down direction
        else
        {
            groundCheckDir = Vector3.down;
        }
        // this is because for quarter pipes to work properly they need to raycast in the last user's down dir won't work for
        // the world's down dir
        
        onGround = false; // reset grounded bool
        onRail = false;
        onRamp = false;

        // Pretty sure this ray is not used 
        Ray r = new Ray();
        r.origin = transform.position;
        r.direction = -camera.transform.up;
        
        Debug.DrawRay(r.origin, r.direction, Color.magenta, 1f); 
        
        if (Physics.Raycast(camera.transform.position, -camera.transform.up, out RaycastHit hit, 1f)){
            onGround = true;
            groundNormal = hit.normal;
            
            // check for rail(grinding)
            if (hit.transform.tag == "Rail")
            {
                onRail = true;
            }
            else if (hit.transform.tag == "Ramp")
            {
                onRamp = true;
            }

        }
        return onGround;
    }

    bool CollisionCheck()
    {
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 1f)){
            groundNormal = Vector3.up;

            trickState = trickStates.NONE;

            return true;
        }

        return false;
    }

    void calculateVerticalImpulse()
    {
        if (onGround)
        {
            if (angle == 0)
            {
                verticalImpulse = 0f;
            }
            else
            {
                verticalImpulse = (Mathf.Sin(angle * Mathf.Deg2Rad) * currentHVelocity * 5f) * Time.deltaTime;
            }
        }
        else
        {
            // make the amount of upwards speed decrease, eventually becoming negative and add onto gravity's effect
            verticalImpulse = Mathf.Lerp(verticalImpulse, dischargedMomentum + verticalImpulse - rb.mass, 0.25f);
        }
        
    }

    void CalculateAcceleration()
    {
        prevAccelDir = accelDir;

        // movement speed calculations
        if (Mathf.Sign(currentMouseAccel) != accelDir)
        {
            // player mouse rotation direction has changed
            currentAcceleration = initialAcceleration;
            accelDir *= -1;
        }
        else if (Mathf.Abs(currentMouseAccel) <= 0f)
        {
            currentAcceleration = initialAcceleration;
        }
        else
        {
            currentAcceleration = Mathf.Min(currentAcceleration + (5f * Time.deltaTime), finalAcceleration);
        }

        // angle speed calculations
        if (angle > 0)
        {
            currentAcceleration += (gravity * (Mathf.Sin(angle * Mathf.Deg2Rad) - Mathf.Cos(angle * Mathf.Deg2Rad) * 0.15f)) * Time.deltaTime;
        }

    }

    void CalculateMomentum()
    {
        if (dischargedMomentum > 0)
        {
            dischargedMomentum = Mathf.Max(dischargedMomentum - 1f, initialMomentum);

        }
        else
        {
            if (currentAcceleration <= 0)
            {
                dischargedMomentum = momentum;
                momentum = 0;
            }
            else
            {
                momentum = Mathf.Min(momentum + 1f, finalMomentum) * Time.deltaTime;
            }
        }
    }


    void Update()
    {
        // initial world detections here
        GroundCheck();
        CollectableCheck();

        // trick state machine
        switch (trickState)
        {
            case (trickStates.NONE):
                // when the player is on a ramp enter a new state
                if (onRamp)
                {
                    trickState = trickStates.ON_RAMP;
                }
                break;
            
            case (trickStates.ON_RAMP):
                // reset back to the NONE state if no ramp is detected 
                if (!onRamp)
                {
                    // back side spin condition
                    if (rightMomentum > leftMomentum && leftMomentum >= 1f && rightMomentum >= 3f)
                    {
                        currentSpinRot = 360;
                        currentSpinVector = 1f;
                        trickState = trickStates.SPIN;
                    }
                    // front side spin condition
                    else if (leftMomentum > rightMomentum && rightMomentum >= 1f && leftMomentum >= 3f)
                    {
                        currentSpinRot = 360;
                        currentSpinVector = -1f;
                        trickState = trickStates.SPIN;
                    }
                    // no trick conditions met
                    else
                    {
                        trickState = trickStates.NONE;
                        rightMomentum = -1f;
                        leftMomentum = -1f;
                    }

                    // rehide each momentum marker
                    foreach (GameObject marker in guiRightMarkers)
                    {
                        marker.SetActive(false);
                    }

                    foreach (GameObject marker in guiLeftMarkers)
                    {
                        marker.SetActive(false);
                    }

                    
                }
                else
                {
                    // increase the right moment for acceleration in right dir
                    if (accelDir > 0 && currentAcceleration > 0)
                    {
                        rightMomentum = Mathf.Min(rightMomentum + 0.25f, 5.5f);
                        // display a new marker when rightMomentum is a new whole number
                        if ((rightMomentum % 1f) == 0)
                        {
                            guiRightMarkers[(int)rightMomentum].SetActive(true);
                        }

                        // hide the furtherest marker when leftMomentum subtracted is a new whole number
                        if ((leftMomentum % 1f) == 0 && leftMomentum >= 0)
                        {
                            guiLeftMarkers[(int)leftMomentum].SetActive(false);

                        }
                        leftMomentum = Mathf.Max(leftMomentum - 0.25f, 0f); 
                    }
                    // increase the left moment for acceleration in left dir
                    else if (accelDir < 0 && currentAcceleration > 0)
                    {
                        leftMomentum = Mathf.Min(leftMomentum + 0.25f, 5.5f);
                        // display a new marker when rightMomentum is a new whole number
                        if ((leftMomentum % 1f) == 0)
                        {
                            guiLeftMarkers[(int)leftMomentum].SetActive(true);
                        }

                        // hide the furtherest marker when leftMomentum subtracted is a new whole number
                        if ((rightMomentum % 1f) == 0 && rightMomentum >= 0)
                        {
                            guiRightMarkers[(int)rightMomentum].SetActive(false);
                        }
                        rightMomentum = Mathf.Max(rightMomentum - 0.25f, 0f); 

                    }
                    // moving without any horizontal mouse acceleration
                    else
                    {
                        rightMomentum = Mathf.Max(rightMomentum - 0.25f, 0f);
                        if ((rightMomentum % 1f) == 0)
                        {
                            guiRightMarkers[(int)rightMomentum].SetActive(false);
                        }

                        leftMomentum = Mathf.Max(leftMomentum - 0.25f, 0f);
                        if ((leftMomentum % 1f) == 0)
                        {
                            guiLeftMarkers[(int)leftMomentum].SetActive(false);
                        }
                    }
                }
                break;

            case (trickStates.SPIN):
                if (currentSpinRot <= 0 || onGround)
                {
                    rightMomentum = -1f;
                    leftMomentum = -1f;
                    currentSpinRot = 0f;
                    //transform.rotation = preSpinRotation;
                    trickState = trickStates.NONE;
                }
                else
                {
                    //transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) * (currentSpinRot * Mathf.Deg2Rad));
                    
                    currentSpinRot -= 5;
                    
                    transform.RotateAround(transform.position, transform.up, currentSpinVector * currentSpinRot * (rotationSpeed / 100) * Time.deltaTime * currentMouseAccel);

                    rb.velocity += (Vector3.up * verticalImpulse + Physics.gravity * rb.mass) * Time.deltaTime; 

                    calculateVerticalImpulse();

                    return;
                }
                break; 
        }

        rotationSpeed = normalRotationSpeed; // reset the speed of horizontal look rotation

        // precalculate any values affected by the player's current angle
        if (groundNormal == Vector3.up)
        {

            if (onGround)
            {
                slopeSpeed = 0f;
            }
            
            smoothRot = 0.3f;
        }
        else
        {
            rotationSpeed = slopeRotationSpeed; // slow the speed of horizontal look rotation

            slopeSpeed = Mathf.Lerp(slopeSpeed, Mathf.Sign(angle) * Mathf.Abs(Mathf.Sin(angle) / 2f), 0.01f); // gradually increase the speed caused by a sloped surface
        }
        
        // get inputs
        currentMouseAccel = Input.GetAxis("Mouse X");
        
        transform.RotateAround(transform.position, Vector3.up, currentMouseAccel * rotationSpeed * Time.deltaTime); // rotate player horizontally

        // handle crouching
        if (onGround)
        {
            if (!crouched)
            {
                verticalImpulse = 0f;
                if (Input.GetMouseButtonDown(0) == true)
                {
                    crouched = true;
                }
            }
            else
            {
                verticalImpulse += 0.1f * Mathf.Abs(slopeSpeed) * hVelocity;
                if (Input.GetMouseButtonUp(0) == true)
                {
                    onGround = false;
                }
            }
        }
        else
        {
            crouched = false;
        }

        CalculateAcceleration();
        CalculateMomentum();
        calculateVerticalImpulse();

        // calculate any angle caused by a slope
        angle = Vector3.SignedAngle(Vector3.up, groundNormal, transform.right);
        
        // no friction or slopeSpeed slow down when on a rail
        if (currentAcceleration > 0)
        {
            currentHVelocity = currentHVelocity + (currentAcceleration * Time.deltaTime);
            currentHVelocity = Mathf.Clamp(currentHVelocity, initialVelocity, finalVelocity);
        }
        else
        {
            if (currentAcceleration < 0)
            {
                currentHVelocity = currentHVelocity + (currentAcceleration * Time.deltaTime);
                currentHVelocity = Mathf.Clamp(currentHVelocity, -5f, finalVelocity);
            }
            else
            {
                float dragForce = deceleration;
                if (onRail)
                {
                    dragForce = 0f;
                }
                else if (!onGround)
                {
                    dragForce = 1f;
                }
                currentHVelocity = currentHVelocity - (dragForce * Time.deltaTime);
                currentHVelocity = Mathf.Clamp(currentHVelocity, initialVelocity, finalVelocity);
            }            
        }

        // set rigidbody velocities
        rb.angularVelocity = Vector3.zero; // we are not using angular velocity so just reset it (makes sure player rigidbody doesnt tip over!!)
        rb.velocity = camera.transform.forward * (currentHVelocity) - camera.transform.forward * (slopeSpeed); // set horizontal velocity to the current velocity
        
        // additional context based movement calculations
        if (!onGround)
        {
            rb.velocity += Vector3.up * verticalImpulse + Physics.gravity * rb.mass; // calculate the vertical velocity at this moment in the air

            groundNormal = Vector3.up; // reset the angle to align with, so the player is not stuck as the angle from the previous slope
            
            // slowly rotate forwards while in the air
            Vector3 eulers = this.transform.rotation.eulerAngles;

            float newEulerX = Mathf.Min(eulers.x + 0.1f, maxAirRotation);

            Quaternion newRotation = Quaternion.Euler(new Vector3(newEulerX,eulers.y,eulers.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.5f);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, groundNormal)), 0.75f);
        }
    }
}

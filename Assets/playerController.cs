using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    Rigidbody rb;
    public Camera camera;

    leftMomentumController leftMomentumCtrl;

    rightMomentumController rightMomentumCtrl;

    Vector3 groundNormal;

    // trick state vars
    public enum trickStates {
        NONE,
        ON_RAMP,
        SETUP_SPIN,
        SPIN
    }
    public trickStates trickState;
    public int spinCounter = 0;
    float currentSpinRot = 0f;
    float currentSpinVector = 1f;
    float rightMomentum = -1f;
    public float leftMomentum = -1f;
    int rightMomentumThreshold = 0;
    public int leftMomentumThreshold = 0;

    // movement vars
    public float currentMouseAccel = 0f;
    float prevMouseAccel = 0f;
    float normalRotationSpeed = 100f;
    float slopeRotationSpeed = 85f;
    float rotationSpeed = 200f;
    float accel = 0f;
    float accelDir = -1f;
    float verticalImpulse = 0f;
    float slopeSpeed = 0f;
    float smoothRot = 0.3f;
    float angle = 0f;
    float maxAirRotation = 35f;
    float gravity = 5f;
    float inputTimer = 0.005f;
    float inputCounter = 0.005f;

    float initialVelocity = 0f;
    float finalVelocity = 100f;
    float currentHVelocity = 0f;
    float initialAcceleration = 0f;
    float finalAcceleration = 5f;
    float accelerationResetTimer = 1f;
    float accelerationResetCounter = 1f;
    float currentAcceleration = 0f;
    float currentAngularAcceleration = 0f;
    float deceleration = 30f;
    
    // non trick-momentum vars
    float momentum = 0f;
    float dischargedMomentum = 0f;
    float initialMomentum = 0f;
    float finalMomentum = 5f;

    
    // spawn related vars
    Vector3 spawnCoords;
    Vector3 spawnNormal;
    Quaternion spawnRot;
    bool drank = false;
    public bool canDrank = true;
    float drankHeldDown = 0f;
    float spawnHVelocity = 0f;
    float spawnAccelDir = 1f;
    float spawnAcceleration = 0f;
    float spawnVerticalImpulse = 0f;

    public bool onRail = false;
    bool onGround = false;
    public bool onRamp = false;
    bool onPipe = false;
    bool crouched = false;
    bool online = false;
    public bool paused = false;

    public bool[] toSend =
    {
            //We don't want anything in here because the server is not deciding the position of the player
    };

    // Start is called before the first frame update
    void Start()
    {
        // set up rigidbody interface
        rb = GetComponent<Rigidbody>();

        //have a rigidbody g

        // set up camera
        camera = FindObjectOfType<Camera>();

        leftMomentumCtrl = FindObjectOfType<leftMomentumController>();
        rightMomentumCtrl = FindObjectOfType<rightMomentumController>();

        // set up gui stuff
        /*
        guiRightMarkers = new GameObject[7];
        for (int i=0; i<=6; ++i)
        {
            guiRightMarkers[i] = GameObject.FindWithTag("Marker" + i.ToString());
            // shift their positions over a bit
            if (guiRightMarkers[i] == null)
            {
                Debug.Log("GUI object is null");
            }
            RectTransform rect = guiRightMarkers[i].GetComponent<RectTransform>();
            rect.localPosition = new Vector3(rect.localPosition.x + 20 + 20 * i, rect.localPosition.y, rect.localPosition.z);
            // hide each marker
            guiRightMarkers[i].SetActive(false);
        }
        */

        spawnCoords = transform.position + Vector3.up;
        spawnRot = transform.rotation;
        spawnNormal = Vector3.up;

        groundNormal = Vector3.up;

        trickState = trickStates.NONE;
    }


    /** Getters **/

    // returns accel with the context of accelDir
    // mostly used by the arrows in gui
    public float GetAccelerationVector()
    {
        return currentAcceleration * accelDir;
    }

    public int getTrickState()
    {
        return (int)trickState;
    }

    public float getCurrentVelocity()
    {
        return currentHVelocity;
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

    public bool getDrank()
    {
        return drank;
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

    /*** world state checks ***/

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
        
        if (Physics.Raycast(camera.transform.position + camera.transform.up * 0.1f, -camera.transform.up, out RaycastHit hit, 1f)){
            onGround = true;
            onPipe = false;
            groundNormal = hit.normal;
            
            switch(hit.transform.tag)
            {
                case "Rail":
                    onRail = true;
                    break;
                case "Ramp":
                    onRamp = true;
                    break;
                case "Pipe":
                    onPipe = true;
                    break;
            }

        }
        return onGround;
    }

    bool CollisionCheck()
    {
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 2)){
            groundNormal = Vector3.up;

            trickState = trickStates.NONE;

            return true;
        }

        return false;
    }

    /*** movement calculations ***/

    void CalculateVerticalImpulse()
    {
        if (onGround)
        {
            if (onRamp || onPipe || onRail)
            {
                //verticalImpulse += (Mathf.Sin(Mathf.Abs(angle) * Mathf.Deg2Rad) * Mathf.Max(currentHVelocity, 1f)) * Time.deltaTime;
                verticalImpulse = (Mathf.Sin(Mathf.Abs(angle) * Mathf.Deg2Rad) * Mathf.Max(currentHVelocity, 1f));
            }
            else
            {
                verticalImpulse = 0f;
            }
        }
        else
        {
            if (crouched)
            {
                Debug.Log("HIT");
                verticalImpulse -= 10f;
                currentHVelocity -= 5f;
                currentSpinRot = 1f;
            }

            // make the amount of upwards speed decrease, eventually becoming negative and add onto gravity's effect
            verticalImpulse -= gravity * Time.deltaTime;
        }
        
    }

    void CalculateAcceleration()
    {
        
        if (Mathf.Abs(currentMouseAccel) > 0)
        {
            if (Mathf.Sign(currentMouseAccel) == accelDir)
            {
                prevMouseAccel = currentMouseAccel;
                currentAcceleration = Mathf.Min(currentAcceleration + 3f * Time.deltaTime, finalAcceleration);
                accelerationResetCounter = accelerationResetTimer;
            }
            else
            {
                currentAcceleration = initialAcceleration;
                accelDir *= -1;
            }
        }
        else
        {
            if (accelerationResetCounter <= 0)
            {
                currentAcceleration = initialAcceleration;
            }
            accelerationResetCounter -= 5 * Time.deltaTime;
        }

        // angle speed calculations
        if (Mathf.Abs(angle) > 0)
        {
            currentAngularAcceleration = (rb.mass * gravity * (Mathf.Sin(angle * Mathf.Deg2Rad) - Mathf.Cos(angle * Mathf.Deg2Rad) * 1f)) * Time.deltaTime * Mathf.Sign(angle);
        }
        else
        {
            currentAngularAcceleration = 0f;
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


    void CalculateVelocity()
    {
        // no friction or slopeSpeed slow down when on a rail
        if (currentAcceleration > 0)
        {
            currentHVelocity = currentHVelocity + (currentAcceleration + currentAngularAcceleration) * Time.deltaTime;
            currentHVelocity = Mathf.Clamp(currentHVelocity, initialVelocity, finalVelocity);
        }
        else
        {
            if (Mathf.Abs(angle) > 5)
            {
                currentHVelocity = currentHVelocity + currentAngularAcceleration * Time.deltaTime;
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
    }

    /*** etc ***/
    public void SetSpawnCoords()
    { 
        spawnCoords = transform.position + Vector3.up;
        spawnRot = transform.rotation;
        spawnHVelocity = currentHVelocity;
        spawnAccelDir = accelDir;
        spawnAcceleration = currentAcceleration;
        spawnVerticalImpulse = verticalImpulse;
        spawnNormal = groundNormal;
    }

    public void Respawn()
    {
        transform.position = spawnCoords;
        transform.rotation = spawnRot;
        currentHVelocity = spawnHVelocity;
        accelDir = spawnAccelDir;
        currentAcceleration = spawnAcceleration;
        verticalImpulse = spawnVerticalImpulse;
        groundNormal = spawnNormal;
    }

    void GetInputs()
    {
        crouched = Input.GetMouseButtonDown(0);

        if (inputCounter <= 0)
        {  
            currentMouseAccel = Input.GetAxis("Mouse X");
            inputCounter = inputTimer;
        }
        else
        {
            inputCounter -= 1 * Time.deltaTime;
        }
        

        drank = false;
        if (Input.GetKey(KeyCode.Tab) && canDrank && trickState != trickStates.SPIN)
        {
            drankHeldDown += 1f * Time.deltaTime;
            if (drankHeldDown < 1)
            {
                drank = true; 
            }
            else
            {
                canDrank = false;
                drank = false;
                drankHeldDown = 0f;
                SetSpawnCoords();
            }
        }
        else if (drankHeldDown < 0.5 && drankHeldDown > 0f)
        {
            canDrank = false;
            drankHeldDown = 0f;
            Respawn();
        }
        else if (drankHeldDown > 0)
        {
            canDrank = false;
        }
        
    }


    void Update()
    {
        if (paused)
        {
            rb.velocity = Vector3.down * 0;
            return;
        }
        // initial world detections here
        GroundCheck();
        CollectableCheck();

        // get inputs
        GetInputs();

        
        // trick state machine
        switch (trickState)
        {
            case (trickStates.NONE):               
                // when the player is on a ramp enter a new state
                if (onRamp)
                {
                    // ensure both right/left momentum are reset
                    leftMomentum = -1f; 
                    rightMomentum = -1f;
                    rightMomentumThreshold = (int)rightMomentum + 1;
                    leftMomentumThreshold = (int)leftMomentum + 1;
                    trickState = trickStates.ON_RAMP;
                }
                break;
            
            case (trickStates.ON_RAMP):
                // reset back to the NONE state if no ramp is detected 
                if (!onRamp)
                {
                    // back side spin condition
                    if (leftMomentum > rightMomentum && rightMomentum >= 2f && accelDir < 0)
                    {
                        currentSpinRot = 360;
                        currentSpinVector = -1f;
                        trickState = trickStates.SPIN;
                    }
                    // front side spin condition
                    else if (leftMomentum < rightMomentum && leftMomentum >= 2f && accelDir > 0)
                    {
                        currentSpinRot = 360;
                        currentSpinVector = 1f;
                        trickState = trickStates.SPIN;
                    }
                    // no trick conditions met
                    else
                    {
                        trickState = trickStates.NONE;
                        rightMomentum = -1f;
                        leftMomentum = -1f;
                    }
                }
                else
                {
                    float deltaMomentum = Mathf.Max(Mathf.Abs(currentMouseAccel) * 10f, 5f) * Time.deltaTime;
                    float deltaDemomentum = 5f * Time.deltaTime;

                    // increase the right moment for acceleration in right dir
                    if (accelDir > 0 && currentAcceleration > 0)
                    {
                        rightMomentum = Mathf.Min(rightMomentum + deltaMomentum, 6f);
                        // display a new marker when rightMomentum is a new whole number
                        if (rightMomentum >= rightMomentumThreshold)
                        {
                            rightMomentumThreshold = Mathf.Clamp(rightMomentumThreshold, 0, 6); // ensure correct data range
                            // display marker
                            rightMomentumCtrl.updateMarkers(rightMomentumThreshold, true);
                            ++rightMomentumThreshold;
                        }

                        // hide the furtherest marker when leftMomentum subtracted is a new whole number
                        if (leftMomentum < leftMomentumThreshold)
                        {
                            leftMomentumThreshold = Mathf.Clamp(leftMomentumThreshold, 0, 6); // ensure correct data range
                            leftMomentumCtrl.updateMarkers(leftMomentumThreshold, false);
                            --leftMomentumThreshold;
                        }
                        leftMomentum = Mathf.Max(leftMomentum - deltaDemomentum, 0f); 
                    }
                    // increase the left moment for acceleration in left dir
                    else if (accelDir < 0 && currentAcceleration > 0)
                    {
                        leftMomentum = Mathf.Min(leftMomentum + deltaMomentum, 6f);
                        // display a new marker when rightMomentum is a new whole number
                        if (leftMomentum >= leftMomentumThreshold)
                        {
                            leftMomentumThreshold = Mathf.Clamp(leftMomentumThreshold, 0, 6); // ensure correct data range
                            leftMomentumCtrl.updateMarkers(leftMomentumThreshold, true);
                            ++leftMomentumThreshold;
                        }

                        // hide the furtherest marker when leftMomentum subtracted is a new whole number
                        if (rightMomentum < rightMomentumThreshold)
                        {
                            rightMomentumThreshold = Mathf.Clamp(rightMomentumThreshold, 0, 6); // ensure correct data range
                            rightMomentumCtrl.updateMarkers(rightMomentumThreshold, false);
                            --rightMomentumThreshold;
                        }
                        rightMomentum = Mathf.Max(rightMomentum - deltaDemomentum, 0f); 

                    }
                    // moving without any horizontal mouse acceleration
                    else
                    {
                        rightMomentum = Mathf.Max(rightMomentum - deltaDemomentum, 0f);
                        if (rightMomentum < rightMomentumThreshold)
                        {
                            rightMomentumThreshold = Mathf.Clamp(rightMomentumThreshold, 0, 6); // ensure correct data range
                            rightMomentumCtrl.updateMarkers(rightMomentumThreshold, false);
                            --rightMomentumThreshold;
                        }

                        leftMomentum = Mathf.Max(leftMomentum - deltaDemomentum, 0f);
                        if (leftMomentum < leftMomentumThreshold)
                        {
                            leftMomentumThreshold = Mathf.Clamp(leftMomentumThreshold, 0, 6); // ensure correct data range
                            leftMomentumCtrl.updateMarkers(leftMomentumThreshold, false);
                            --leftMomentumThreshold;
                        }
                    }
                }
                break;

            case (trickStates.SPIN):
                // rotation work
                if (currentSpinRot <= 0 || onGround)
                {
                    rightMomentum = -1f;
                    leftMomentum = -1f;
                    rightMomentumThreshold = (int)rightMomentum + 1;
                    leftMomentumThreshold = (int)leftMomentum + 1;
                    currentSpinRot = 0f;
                    // if not a smooth landing, reduce velocity
                    if (Mathf.Abs(Mathf.Abs(camera.transform.rotation.eulerAngles.y) - Mathf.Abs(transform.rotation.eulerAngles.y)) > 21f)
                    {
                        currentHVelocity /= 2;
                        currentAcceleration /= 2;
                    }
                    //reset the camera rotation
                    camera.transform.rotation = transform.rotation;

                    //transform.rotation = preSpinRotation;
                    trickState = trickStates.NONE;
                }
                else
                {
                    //transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) * (currentSpinRot * Mathf.Deg2Rad));
                    currentSpinRot -= 50 * Time.deltaTime;
                    
                    camera.transform.RotateAround(transform.position, transform.up, currentSpinVector * currentSpinRot * (rotationSpeed / 300) * Time.deltaTime * 2.5f);

                    rb.velocity += (Vector3.up * verticalImpulse + Physics.gravity * rb.mass) * Time.deltaTime * 2; 

                    CalculateVerticalImpulse();

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
        
        Vector3 yaxis = Vector3.up;
        if (onPipe)
        {
            yaxis = transform.up;
        }

        // rotate player horizontally based on input
        transform.RotateAround(transform.position, yaxis, currentMouseAccel * rotationSpeed * Time.deltaTime); 

        // handle crouching
        /*
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
                verticalImpulse += 0.1f * Mathf.Abs(slopeSpeed) * currentHVelocity;
                if (Input.GetMouseButtonUp(0) == true)
                {
                    onGround = false;
                }
            }
        }
        */

        // calculate any angle caused by a slope
        angle = Vector3.SignedAngle(Vector3.up, groundNormal, transform.right);

        // calculate current movement values
        CalculateAcceleration();
        
        CalculateMomentum();
        
        CalculateVerticalImpulse();
        
        CalculateVelocity();
        
        // set rigidbody velocities
        rb.angularVelocity = Vector3.zero; // we are not using angular velocity so just reset it (makes sure player rigidbody doesnt tip over!!)
        rb.velocity = camera.transform.forward * (currentHVelocity); // set horizontal velocity to the current velocity
        
        // additional context based movement calculations
        if (!onGround)
        {
            rb.velocity += Vector3.up * verticalImpulse + Physics.gravity * rb.mass; // calculate the vertical velocity at this moment in the air

            groundNormal = Vector3.up; // reset the angle to align with, so the player is not stuck as the angle from the previous slope
            
            Vector3 eulers = this.transform.rotation.eulerAngles;

            if (onPipe)
            {
                rb.velocity = new Vector3(0, rb.velocity.y + 3f, 0) - transform.up;
                currentAcceleration = 0;
                /*
                if (verticalImpulse < -1)
                {
                    transform.RotateAround(transform.position, transform.up, accelDir * 2f * rotationSpeed * Time.deltaTime);
                    rb.velocity -= transform.up;
                }
                */
                
                 
                transform.rotation = Quaternion.Euler(eulers + Vector3.down);
                return;
            }
            
            // slowly rotate forwards while in the air            
            float newEulerX = Mathf.Min(eulers.x + 3f * Time.deltaTime, maxAirRotation);

            Quaternion newRotation = Quaternion.Euler(new Vector3(newEulerX,eulers.y,eulers.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.25f);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(Vector3.Cross(camera.transform.right, groundNormal));
            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(camera.transform.right, groundNormal)), 1f);
        }
    }

    void FixedUpdate()
    {
        if (gameManager.instance.gotConnection)
        {
            clientSend.PlayerMovement(toSend);
        }
        
    }
}

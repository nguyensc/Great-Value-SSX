using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    Rigidbody rb;
    Camera camera;

    Vector3 groundNormal;

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
    float fric = 0.025f;
    float deltaFric = 0.01f;
    float maxFriction = 0.1f;

    bool onGround = false;

    // Start is called before the first frame update
    void Start()
    {
        // set up rigidbody interface
        rb = GetComponent<Rigidbody>();

        // set up camera
        camera = FindObjectOfType<Camera>();

        groundNormal = Vector3.up;
    }


    // returns accel with the context of accelDir
    // mostly used by the arrows in gui
    public float GetAccelerationVector()
    {
        return accel * accelDir;
    }

    public float getSlopeSpeed()
    {
        return slopeSpeed;
    }

    public float getVerticalImpulse()
    {
        return verticalImpulse;
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

        Ray r = new Ray();
        r.origin = transform.position;
        r.direction = -camera.transform.up;
        
        Debug.Log(r.direction);
        Debug.DrawRay(r.origin, r.direction, Color.magenta, 1f); 
        
        if (Physics.Raycast(camera.transform.position, -camera.transform.up, out RaycastHit hit, 1.125f)){
            onGround = true;
            groundNormal = hit.normal;
            smoothRot = 0.3f;
        }
        return onGround;
    }

    bool CollisionCheck()
    {
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 0.001f)){
            groundNormal = Vector3.up;
            return true;
        }

        return false;
    }

    void CalculateAcceleration()
    {
        // movement speed calculations
        if (Mathf.Sign(currentMouseAccel) != accelDir)
        {
            // player mouse rotation direction has changed
            accel = 0f;
            accelDir *= -1;
        }
        else if (Mathf.Abs(currentMouseAccel) > 0f)
        {
            accel = Mathf.Min(accel + 0.0025f, 0.85f); // base acceleration increase for any proper mouse rotation

            // boost the acceleration increase when there is a smooth rotation
            if (Mathf.Abs(currentMouseAccel) < 1f)
            {
                accel = Mathf.Min(accel + 0.0005f, 0.85f);
            }
        }
        else
        {
            accel = 0f; // reset the acceleration
        }

    }

    // Update is called once per frame
    void Update()
    {

        //if menu is open, rb.velocity = Vector3.zero;
        //return
        GroundCheck();
        CollectableCheck();

        rotationSpeed = normalRotationSpeed; // reset the speed of horizontal look rotation

        if (groundNormal == Vector3.up)
        {

            if (onGround)
            {
                slopeSpeed = 0f;
                verticalImpulse = 0f;
            }
            
            smoothRot = 0.3f;
        }
        else
        {
            rotationSpeed = slopeRotationSpeed; // slow the speed of horizontal look rotation

            slopeSpeed = Mathf.Lerp(slopeSpeed, Mathf.Sign(angle) * Mathf.Abs(Mathf.Sin(angle) / 2f), 0.01f); // gradually increase the speed caused by a sloped surface
            verticalImpulse = Mathf.Clamp(-1*Mathf.Sign(angle) * Mathf.Abs(Mathf.Sin(angle)) * hVelocity, -5f, 12f); // what the player's vertical velocity will be once in air
        }
        
        currentMouseAccel = Input.GetAxis("Mouse X");
        //Send the above input to server

        transform.RotateAround(transform.position, Vector3.up, currentMouseAccel * rotationSpeed * Time.deltaTime);

        CalculateAcceleration();

        // calculate any acceleration caused by a slope
        angle = Vector3.SignedAngle(Vector3.up, groundNormal, transform.right);
        
        hVelocity = Mathf.Max(hVelocity + accel - 0.05f * GetZero(hVelocity), hVelocityMin);        
        hVelocity = Mathf.Min(hVelocity, 20f) + slopeSpeed;

        // check for and handle head on collisions the rigidbody hits
        if (CollisionCheck())
        {
            hVelocity = hVelocity / -1.2f;
            hVelocityMin = -10f;
        }
        else if (hVelocity > 0f)
        {
            hVelocityMin = 0f;
        }

        // set rigidbody velocities
        rb.angularVelocity = Vector3.zero; // we are not using angular velocity so just reset it (makes sure player rigidbody doesnt tip over!!)
        rb.velocity = camera.transform.forward * (hVelocity) - camera.transform.forward * (slopeSpeed); // set velocity to the current velocity
        
        // additional context based movement calculations
        if (!onGround)
        {
            rb.velocity += Vector3.up * verticalImpulse + Physics.gravity * rb.mass;
            groundNormal = Vector3.up;
            
            // slowly rotate forwards while in the air
            Vector3 eulers = this.transform.rotation.eulerAngles;
            Quaternion newRotation = Quaternion.Euler(new Vector3(eulers.x + 0.25f,eulers.y,eulers.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.75f);

            verticalImpulse = Mathf.Max(verticalImpulse - rb.mass, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, groundNormal)), 0.75f);
        }
        //Send transform.rotation to server? Probably more like rb.rotation
        //Send rb.position to the server
    }
}

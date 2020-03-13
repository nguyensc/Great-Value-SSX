using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
public class PlayerCharacterController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the main camera used for the player")]
    public Camera playerCamera;
    [Tooltip("Audio source for footsteps, jump, etc...")]
    public AudioSource audioSource;

    [Header("General")]
    [Tooltip("Force applied downward when in the air")]
    public float grav = 2f;
    public float weight = 2f;
    public float gravityDownForce = 9.98f * 2f;

    [Tooltip("Physic layers checked to consider the player grounded")]
    public LayerMask groundCheckLayers = -1;
    [Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
    public float groundCheckDistance = 0.05f;

    [Header("Movement")]
    [Tooltip("Max movement speed when grounded (when not sprinting)")]
    public float maxSpeedOnGround = 10f;
    [Tooltip("Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
    public float movementSharpnessOnGround = 1;
    [Tooltip("Max movement speed when crouching")]
    [Range(0, 1)]
    public float maxSpeedCrouchedRatio = 0.5f;
    [Tooltip("Max movement speed when not grounded")]
    public float maxSpeedInAir = 10f;
    [Tooltip("Acceleration speed when in the air")]
    public float accelerationSpeedInAir = 5f;
    [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
    public float sprintSpeedModifier = 2f;
    [Tooltip("Height at which the player dies instantly when falling off the map")]
    public float killHeight = -50f;

    [Header("Rotation")]
    [Tooltip("Rotation speed for moving the camera")]
    public float rotationSpeed = 100f;
    [Range(0.1f, 1f)]
    [Tooltip("Rotation speed multiplier when aiming")]
    public float aimingRotationMultiplier = 0.4f;

    [Header("Jump")]
    [Tooltip("Force applied upward when jumping")]
    public float jumpForce = 9f;

    [Header("Stance")]
    [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
    public float cameraHeightRatio = 0.9f;
    [Tooltip("Height of character when standing")]
    public float capsuleHeightStanding = 1.8f;
    [Tooltip("Height of character when crouching")]
    public float capsuleHeightCrouching = 0.9f;
    [Tooltip("Speed of crouching transitions")]
    public float crouchingSharpness = 10f;

    [Header("Audio")]
    [Tooltip("Amount of footstep sounds played when moving one meter")]
    public float footstepSFXFrequency = 1f;
    [Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
    public float footstepSFXFrequencyWhileSprinting = 1f;
    [Tooltip("Sound played for footsteps")]
    public AudioClip footstepSFX;
    [Tooltip("Sound played when jumping")]
    public AudioClip jumpSFX;
    [Tooltip("Sound played when landing")]
    public AudioClip landSFX;
    [Tooltip("Sound played when taking damage froma fall")]
    public AudioClip fallDamageSFX;

    [Header("Fall Damage")]
    [Tooltip("Whether the player will recieve damage when hitting the ground at high speed")]
    public bool recievesFallDamage;
    [Tooltip("Minimun fall speed for recieving fall damage")]
    public float minSpeedForFallDamage = 50f;
    [Tooltip("Fall speed for recieving th emaximum amount of fall damage")]
    public float maxSpeedForFallDamage = 80f;
    [Tooltip("Damage recieved when falling at the mimimum speed")]
    public float fallDamageAtMinSpeed = 0f;
    [Tooltip("Damage recieved when falling at the maximum speed")]
    public float fallDamageAtMaxSpeed = 0f;

    public bool canVault = false;

    public UnityAction<bool> onStanceChanged;

    public Vector3 characterVelocity { get; set; }
    public bool isGrounded { get; private set; }
    public bool hasJumpedThisFrame { get; private set; }
    public bool isDead { get; private set; }
    public bool isCrouching { get; private set; }
    public float RotationMultiplier
    {
        get
        {
            if (m_WeaponsManager.isAiming)
            {
                return aimingRotationMultiplier;
            }

            return 1f;
        }
    }

    Health m_Health;
    PlayerInputHandler m_InputHandler;
    CharacterController m_Controller;
    PlayerWeaponsManager m_WeaponsManager;
    WeaponController m_WepController;
    Actor m_Actor;
    Vector3 m_GroundNormal;
    Vector3 m_CharacterVelocity;
    Vector3 m_LatestImpactSpeed;
    float m_LastTimeJumped = 0f;
    float m_CameraVerticalAngle = 0f;
    float m_footstepDistanceCounter;
    float m_TargetCharacterHeight;
    Vector3 vaultOrigin;
    Vector3 vaultPosition;

    // wall running
    RaycastHit wallcheckR;
    RaycastHit wallcheckL;
    bool isWallR;
    bool isWallL;
    float wallRunVelocity = 0;

    int state = 0;

    // <<< PERFECT STRIDE >>>
    float ps_velocity = 1f;
    Vector3 ps_airVelocity;
    float ps_friction = 0.1f;
    public float ps_currDirectionAccel = 0f;
    float ps_currDirection = 1f;
    float slopeDir = 1;
    public Vector3 verticalImpulse = Vector3.up;
    float slopeImpulse = 1f;
    bool onPipe = false;
    bool onSlope = false;
    float ps_pipeSlopeRotation = 0f;
    public float rotation = 0f;
    public Quaternion ps_initalRotation;


    const float k_JumpGroundingPreventionTime = 0.4f;
    const float k_GroundCheckDistanceInAir = 0.001f;

    void Start()
    {
        // fetch components on the same gameObject
        m_Controller = GetComponent<CharacterController>();
        DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerCharacterController>(m_Controller, this, gameObject);

        m_InputHandler = GetComponent<PlayerInputHandler>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerCharacterController>(m_InputHandler, this, gameObject);

        m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerCharacterController>(m_WeaponsManager, this, gameObject);

        m_Health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerCharacterController>(m_Health, this, gameObject);

        m_Actor = GetComponent<Actor>();
        DebugUtility.HandleErrorIfNullGetComponent<Actor, PlayerCharacterController>(m_Actor, this, gameObject);

        m_WepController = FindObjectOfType<WeaponController>();

        m_Controller.enableOverlapRecovery = true;

        m_Health.onDie += OnDie;

        // force the crouch state to false when starting
        SetCrouchingState(false, true);
        UpdateCharacterHeight(true);

        /// PERFECT STRIDE
        ps_initalRotation = Quaternion.LookRotation(transform.forward, transform.up);
    }

    void Update()
    {
        // check for Y kill
        if (!isDead && transform.position.y < killHeight)
        {
            m_Health.Kill();
        }

        hasJumpedThisFrame = false;

        // state machine
        switch (state)
        {
            case 0:
                bool wasGrounded = isGrounded;
                GroundCheck();

                // landing
                if (isGrounded && !wasGrounded)
                {
                    // Fall damage

                    float fallSpeed = -Mathf.Min(characterVelocity.y, m_LatestImpactSpeed.y);
                    float fallSpeedRatio = (fallSpeed - minSpeedForFallDamage) / (maxSpeedForFallDamage - minSpeedForFallDamage);
                    if (recievesFallDamage && fallSpeedRatio > 0f)
                    {
                        //float dmgFromFall = Mathf.Lerp(fallDamageAtMinSpeed, fallDamageAtMaxSpeed, fallSpeedRatio);
                        //m_Health.TakeDamage(dmgFromFall, null);

                        // fall damage SFX
                        // audioSource.PlayOneShot(fallDamageSFX);
                    }
                    else
                    {
                        // land SFX
                        audioSource.PlayOneShot(landSFX);
                    }
                }

                // crouching
                if (m_InputHandler.GetCrouchInputDown())
                {
                    SetCrouchingState(!isCrouching, false);
                }

                UpdateCharacterHeight(false);

                HandleCharacterMovement();
                break;

            case 1:
                vaultPosition = transform.position;
                if (vaultPosition.z < vaultOrigin.z - 2f)
                {
                    state = 0;
                    canVault = false;
                }
                else if (vaultPosition.z < vaultOrigin.z - 1f)
                    transform.position = new Vector3(vaultPosition.x, vaultPosition.y, vaultPosition.z - 0.25f);
                else
                    transform.position = new Vector3(vaultPosition.x, vaultPosition.y + 0.1f, vaultPosition.z - 0.1f);
                break;

        }

    }

    void OnDie()
    {
        isDead = true;

        // Tell the weapons manager to switch to a non-existing weapon in order to lower the weapon
        m_WeaponsManager.SwitchToWeaponIndex(-1, true);
    }

    void GroundCheck()
    {
        // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
        float chosenGroundCheckDistance = isGrounded ? (m_Controller.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;

        // reset values before the ground check
        isGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
        {
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height), m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, groundCheckLayers, QueryTriggerInteraction.Ignore))
            {
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;
                onPipe = hit.transform.tag == "Pipe";

                Debug.Log(Vector3.Dot(hit.normal, transform.up));

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    isGrounded = true;

                    // handle snapping to the ground
                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }

    void HandleCharacterMovement()
    {
        // horizontal character rotation
        {
            // rotate the transform with the input speed around its local Y axis
            transform.Rotate(new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal() * rotationSpeed * RotationMultiplier), 0f), Space.Self);
        }

        // vertical camera rotation
        {
            // add vertical inputs to the camera's vertical angle
            m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * rotationSpeed * RotationMultiplier;

            // limit the camera's vertical angle to min/max
            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

            // apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
            playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
        }



        // character movement handling
        bool isSprinting = m_InputHandler.GetSprintInputHeld();
        {
            if (isSprinting)
            {
                if (!isGrounded)
                {
                    maxSpeedInAir += 1f;
                }

                isSprinting = SetCrouchingState(false, false);
            }

            float speedModifier = isSprinting ? sprintSpeedModifier : 1f;

            // converts move input to a worldspace vector based on our character's transform orientation
            Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());

            // handle grounded movement
            if (isGrounded)
            {
                // check for able to start wallrunning
                if (isSprinting)
                {
                    if (Physics.Raycast(transform.position, transform.right, out wallcheckR, 5))
                    {
                        if (wallcheckR.transform.tag == "Wall")
                        {
                            isWallR = true;
                            isWallL = false;
                        }
                    }
                    else if (Physics.Raycast(transform.position, -transform.right, out wallcheckL, 5))
                    {
                        if (wallcheckL.transform.tag == "Wall")
                        {
                            isWallL = true;
                            isWallR = false;
                        }
                    }
                    else
                    {
                        isWallR = false;
                        isWallL = false;
                    }
                }
                else
                {
                    isWallR = false;
                    isWallL = false;
                }

                // calculate the desired velocity from inputs, max speed, and current slope
                Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier;

                // reduce speed if crouching by crouch speed ratio
                if (isCrouching)
                    targetVelocity *= maxSpeedCrouchedRatio;
                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) * targetVelocity.magnitude;

                // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);

                // jumping
                if (isGrounded && m_InputHandler.GetJumpInputDown())
                {
                    if (canVault)
                    {
                        state = 1;
                        vaultOrigin = transform.position;
                    }
                    // force the crouch state to false
                    else if (SetCrouchingState(false, false))
                    {
                        // start by canceling out the vertical component of our velocity
                        characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);


                        if (isWallL || isWallR)
                        {
                            wallRunVelocity = 0;
                            characterVelocity += Vector3.up * (jumpForce / 2);
                        }
                        else
                        {
                            // then, add the jumpSpeed value upwards
                            characterVelocity += Vector3.up * jumpForce;
                        }

                        // play sound
                        audioSource.PlayOneShot(jumpSFX);

                        // remember last time we jumped because we need to prevent snapping to ground for a short time
                        m_LastTimeJumped = Time.time;
                        hasJumpedThisFrame = true;

                        // Force grounding to false
                        isGrounded = false;
                        m_GroundNormal = Vector3.up;
                    }
                }

                // footsteps sound
                float chosenFootstepSFXFrequency = (isSprinting ? footstepSFXFrequencyWhileSprinting : footstepSFXFrequency);
                if (m_footstepDistanceCounter >= 1f / chosenFootstepSFXFrequency)
                {
                    m_footstepDistanceCounter = 0f;
                    audioSource.PlayOneShot(footstepSFX);
                }

                // keep track of distance traveled for footsteps sound
                m_footstepDistanceCounter += characterVelocity.magnitude * Time.deltaTime;
            }
            // handle air movement
            else
            {

                // add air acceleration
                characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

                // limit air speed to a maximum, but only horizontally
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                if (wallRunVelocity >= 0)
                {
                    characterVelocity += Vector3.down * wallRunVelocity * Time.deltaTime;
                    wallRunVelocity += 0.25f;
                    if (wallRunVelocity >= gravityDownForce)
                    {
                        wallRunVelocity = -1;
                    }
                }
                else
                {
                    // apply the gravity to the velocity
                    characterVelocity += -transform.up * gravityDownForce * Time.deltaTime;
                }
            }
        }

        // apply the final calculated velocity value as a character movement
        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);


        // <<< PERFECT STRIDE >>>
        rotation = m_InputHandler.GetLookInputsHorizontal(); // get amount plyaer has looked

        if (rotation == 0)
        {
            ps_currDirectionAccel = 0f;
        }
        else if (Mathf.Sign(ps_currDirection) != Mathf.Sign(rotation))
        {
            ps_currDirectionAccel = 0f;
            ps_currDirection *= -1;
        }
        else
        {
            ps_currDirectionAccel = Mathf.Min(ps_currDirectionAccel + 0.03f, 0.15f);
        }

        ps_velocity += ps_currDirectionAccel;
        characterVelocity = transform.forward * ps_velocity;

        // handle rotation
        float slopeAngle = Mathf.Round(Vector3.Angle(transform.up, m_GroundNormal));

        // handle movement
        if (!isGrounded)
        {
            
            if (slopeImpulse > 1)
            {
               // verticalImpulse = new Vector3(0f, Mathf.Max(verticalImpulse.y * slopeImpulse - 1f, -10f), 0f);
                characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                slopeImpulse = 0f;
                transform.rotation = Quaternion.LookRotation(transform.up, -transform.forward);
            } else if (slopeImpulse < 1)
            {
                Quaternion rot = Quaternion.LookRotation(-transform.up, transform.forward);
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.02f);
            }
            verticalImpulse = new Vector3(0f, Mathf.Max(verticalImpulse.y-1f, -10f), 0f);

            characterVelocity += verticalImpulse;
            ps_velocity = Mathf.Max(ps_velocity - ps_friction / 2, ps_friction / 2);
        }
        else
        {

            ps_velocity = Mathf.Max(ps_velocity - ps_friction, ps_friction);
            verticalImpulse = transform.up * Mathf.Abs(Mathf.Sin(slopeAngle) * ps_velocity);
            ps_currDirectionAccel = Mathf.Max(ps_currDirectionAccel + Mathf.Abs(Mathf.Sin(slopeAngle)) * slopeDir, 0f); 
            /*
            if (onPipe)
            {
                characterVelocity = new Vector3(Mathf.Max(characterVelocity.x - 5f, 0f), characterVelocity.y + slopeImpulse, Mathf.Max(characterVelocity.z - 5f, 0f));
                slopeImpulse += 1f;
                Quaternion rot = Quaternion.LookRotation(transform.up, -transform.forward);
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.05f);
            }
            else
            {
                if (slopeImpulse < 1)
                {
                    slopeImpulse++;
                }
            }
            */
        }

        float prevY = transform.position.y;

        m_Controller.Move(characterVelocity * Time.deltaTime);

        if (prevY > transform.position.y)
            slopeDir = 1;
        else if (prevY < transform.position.y)
            slopeDir = -1;
        else slopeDir = 0;


        // detect obstructions to adjust velocity accordingly
        m_LatestImpactSpeed = Vector3.zero;
        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius, characterVelocity.normalized, out RaycastHit hit, characterVelocity.magnitude * Time.deltaTime, -1, QueryTriggerInteraction.Ignore))
        {
            // We remember the last impact speed because the fall damage logic might need it
            m_LatestImpactSpeed = characterVelocity;

            characterVelocity = Vector3.ProjectOnPlane(characterVelocity, hit.normal);
            
        }
    }

    // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        float maxSlope = m_Controller.slopeLimit;
        maxSlope = 190;
        return Vector3.Angle(transform.up, normal) <= maxSlope;
    }

    // Gets the center point of the bottom hemisphere of the character controller capsule    
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_Controller.radius);
    }

    // Gets the center point of the top hemisphere of the character controller capsule    
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_Controller.radius));
    }

    // Gets a reoriented direction that is tangent to a given slope
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    void UpdateCharacterHeight(bool force)
    {
        // Update height instantly
        if (force)
        {
            m_Controller.height = m_TargetCharacterHeight;
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * cameraHeightRatio;
            m_Actor.aimPoint.transform.localPosition = m_Controller.center;
        }
        // Update smooth height
        else if (m_Controller.height != m_TargetCharacterHeight)
        {
            // resize the capsule and adjust camera position
            m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, crouchingSharpness * Time.deltaTime);
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, Vector3.up * m_TargetCharacterHeight * cameraHeightRatio, crouchingSharpness * Time.deltaTime);
            m_Actor.aimPoint.transform.localPosition = m_Controller.center;
        }
    }

    // returns false if there was an obstruction
    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        // set appropriate heights
        if (crouched)
        {
            m_TargetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            // Detect obstructions
            if (!ignoreObstructions)
            {
                Collider[] standingOverlaps = Physics.OverlapCapsule(
                    GetCapsuleBottomHemisphere(),
                    GetCapsuleTopHemisphere(capsuleHeightStanding),
                    m_Controller.radius,
                    -1,
                    QueryTriggerInteraction.Ignore);
                foreach (Collider c in standingOverlaps)
                {
                    if (c != m_Controller)
                    {
                        return false;
                    }
                }
            }

            m_TargetCharacterHeight = capsuleHeightStanding;
        }

        if (onStanceChanged != null)
        {
            onStanceChanged.Invoke(crouched);
        }

        isCrouching = crouched;
        return true;
    }
}

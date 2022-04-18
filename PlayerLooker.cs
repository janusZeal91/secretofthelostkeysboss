using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerLooker : MonoBehaviour
{
    [HideInInspector]
    public bool _isLookingUp = false;
    [HideInInspector]
    public bool _isLookingDown = false;
    private bool hasReturnedToPosition = true;
    private bool isGrounded = false;
    private bool isFalling = false;
    private bool lockLooker = false;
    private float buttonTimer;
    private Animator _animator;
    private Transform cameraCheckTransform;
    public CinemachineVirtualCamera virtualCamera;
    public GameObject cameraCheck;
    private PlayerMovement playerMovement;
    public Rigidbody2D playerRigidBody;
    private float originalHeightDeathZone = 0f;


    //speed of movement for camera when looking up or down
    [Range(0.0f, 10.0f)] // create a slider in the editor and set limits on moveSpeed
    public float cameraSpeedMovement = 1f;
    [Range(0.0f, 10.0f)]
    public float lookingOffset = 2.0f;


    public bool LockLooker
    {
        set
        {
            this.lockLooker = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        InitPlayerReferences();
    }

    private void InitPlayerReferences()
    {

        cameraCheckTransform = cameraCheck.GetComponent<Transform>();
        _animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        isFalling = playerMovement.isFalling;
    }

    private void SetNewDeadZoneHeight()
    {

        originalHeightDeathZone = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 0.1f;
    }

    private void RestoreDeadZoneHeight()
    {
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = originalHeightDeathZone;
    }



    // Update is called once per frame
    void Update()
    {
        if (!lockLooker && !playerMovement.IsRunning)
        {
            if (CrossPlatformInputManager.GetButton("LookUp") && playerMovement.isGrounded && !_isLookingDown)
            {

                if (-2f <= playerMovement.yVelocity && playerMovement.yVelocity <= 2f)
                {
                    buttonTimer += Time.deltaTime;
                    if (buttonTimer > 0.3f)
                    {

                        if (!_isLookingUp)
                        {
                            _animator.SetBool("LookingUp", true);
                            _isLookingUp = true;
                            SetNewDeadZoneHeight();
                            //Change rigidbody to dynamic to stop moving in platforms.
                            playerRigidBody.constraints = RigidbodyConstraints2D.FreezePositionX |
                                   RigidbodyConstraints2D.FreezeRotation;
                            hasReturnedToPosition = false;
                        }
                        LookUp();
                    }

                }


            }

            if (CrossPlatformInputManager.GetButton("LookDown") && playerMovement.isGrounded && !_isLookingUp)
            {
                _animator.SetBool("LookingDown", true);
                if (-2f <= playerMovement.yVelocity && playerMovement.yVelocity <= 2f)
                {

                    playerMovement.yVelocity = 0;
                    buttonTimer += Time.deltaTime;

                    if (!_isLookingDown)
                    {
                        playerMovement.yVelocity = 0;
                        _isLookingDown = true;
                        SetNewDeadZoneHeight();
                        playerRigidBody.constraints = RigidbodyConstraints2D.FreezePositionX |
                               RigidbodyConstraints2D.FreezeRotation;

                        hasReturnedToPosition = false;
                    }
                    if (buttonTimer > 1f)
                    {
                        LookDown();
                    }
                }
            }



            if (CrossPlatformInputManager.GetButtonUp("LookUp") && _isLookingUp)
            {
                if (!hasReturnedToPosition)
                {
                    _animator.SetBool("LookingUp", false);
                    buttonTimer = 0f;
                    playerRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
                    hasReturnedToPosition = true;
                }

                StartCoroutine(ReturnCameraFromLookingUp());
            }
            if (CrossPlatformInputManager.GetButtonUp("LookDown") && _isLookingDown)
            {
                _animator.SetBool("LookingDown", false);
                if (!hasReturnedToPosition)
                {

                    buttonTimer = 0f;
                    playerRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
                    hasReturnedToPosition = true;

                }
                StartCoroutine(ReturnCameraFromLookingDown());
            }
        }
    }

    //function to manage the camera when player wants to look up
    //Smoothly moves the camera based on variable speed movement  * delta time by each frame
    private void LookUp()
    {


        if (cameraCheckTransform.position.y <= (this.GetComponent<Transform>().position.y + lookingOffset))
        {
            cameraCheckTransform.position += Vector3.up * (Time.deltaTime * cameraSpeedMovement);

        }

    }

    //funcion to manage the camera when player wants to look down
    private void LookDown()
    {
        if (cameraCheckTransform.position.y >= (this.GetComponent<Transform>().position.y - lookingOffset))
        {
            cameraCheckTransform.position += Vector3.down * (Time.deltaTime * cameraSpeedMovement);

        }



    }
    private IEnumerator ReturnCameraFromLookingUp()
    {
        do
        {
            cameraCheckTransform.position += Vector3.down * (Time.deltaTime * cameraSpeedMovement);
            yield return null;

        } while (cameraCheckTransform.position.y > (this.GetComponent<Transform>().position.y));
        cameraCheckTransform.position = this.GetComponent<Transform>().position;
        _isLookingUp = false;
        hasReturnedToPosition = false;
        RestoreDeadZoneHeight();
    }

    private IEnumerator ReturnCameraFromLookingDown()
    {
        do
        {
            cameraCheckTransform.position += Vector3.up * (Time.deltaTime * cameraSpeedMovement);
            yield return null;
        } while (cameraCheckTransform.position.y < (this.GetComponent<Transform>().position.y));
        cameraCheckTransform.position = this.GetComponent<Transform>().position;
        _isLookingDown = false;
        hasReturnedToPosition = false;
        RestoreDeadZoneHeight();

    }

}

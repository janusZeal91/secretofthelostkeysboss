using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class IceMagic : MonoBehaviour
{
    [Range(0, 20f)]
    public float maxPiecesPerAttack = 10f;
    public AudioClip magicSFX;
    private Animator playerCastingAnimator;
    public Transform startMagicPoint;
    public Transform endMagicPoint;
    public Transform roofCheckerRayCastLimit;
    public Transform rayCastOrigin;
    private float xMinRange;
    private float xMaxRange;
    private float yMinRange;
    private float yMaxRange;
    public GameObject[] icePieces; // what prefabs to spawn
    private CharacterController2D playerController;
    private PlayerMovement playerMovement;
    private MPBarManager mpManager;
    [Range(0, 100)]
    public int mPConsumption = 40;
    private bool isAttacking = false;
    private bool hasReducedMP = false;
    private int groundLayer;
    private bool isIceMagicEnabled = false;
    private Vector2 magicCastDirection;

    void Awake()
    {

        playerCastingAnimator = GetComponent<Animator>();
        this.SetCurrentPositionOfMagicTransforms();
        groundLayer = LayerMask.GetMask("Ground");

    }

    private void SetMagicCastDirection()
    {
        if (this.gameObject.GetComponent<Transform>().localScale.x < 0)
        {
            magicCastDirection = -Vector2.right;
        }
        else
        {
            magicCastDirection = Vector2.right;
        }
    }

    private void SetCurrentPositionOfMagicTransforms()
    {
        xMinRange = startMagicPoint.transform.position.x;
        xMaxRange = endMagicPoint.transform.position.x;
        yMinRange = startMagicPoint.transform.position.y;
        yMaxRange = endMagicPoint.transform.position.y;

    }
    private void SetCurrentPositionOfMagicTransforms(Transform newPositions)
    {

        xMinRange = startMagicPoint.transform.position.x;
        xMaxRange = endMagicPoint.transform.position.x;
        yMinRange = newPositions.position.y - 1f;
        yMaxRange = newPositions.position.y - 1f;

    }

    private void SetNewXLimitValueForMagic(float newXValue)
    {
        xMaxRange = newXValue;
    }

    // Use this for initialization
    void Start()
    {
        this.playerController = this.gameObject.GetComponent<CharacterController2D>();
        this.mpManager = this.gameObject.GetComponent<MPBarManager>();
        this.playerMovement = this.GetComponent<PlayerMovement>();
        isIceMagicEnabled = GameManager.gm.hasIceMagic;
    }

    // Update is called once per frame
    void Update()
    {
        if (isIceMagicEnabled)
        {
            if (playerController.playerCanMove)
            {
                //If the player pushes magic button and is not already casting then cast ice attack
                if (CrossPlatformInputManager.GetButtonDown("IceMagic") && !isAttacking && HasMPAvailable())
                {
                    isAttacking = true;
                    playerMovement.SetMagicCastingFlag(true);
                    playerCastingAnimator.SetTrigger("CastingIce");
                    SetMagicCastDirection();
                    StartCoroutine(RestorePlayerMovement());
                    CheckUpperRoofHigh();           
                    if (IsThereDistanceAvailableForMagic())
                    {   
                        ReduceMP();
                        PlaySound(magicSFX);
                        StartCoroutine(CastIceMagic());
                    }
                    else
                    {
                        SendNoSpaceMsgToPlayer();
                    }
                }
            }
        }
    }

    private void SendNoSpaceMsgToPlayer()
    {
        if (PlayerPrefManager.GetLanguage() == 1)
        {
            playerController.SendMsgToPlayerHUD("No space available for spell!");
        }
        else
        {
            playerController.SendMsgToPlayerHUD("¡No hay espacio suficente para el hechizo!");
        }
    }

    private IEnumerator CastIceMagic()
    {
        int castPiecesCounter = 0;
        do
        {
            CastIcePieces();
            castPiecesCounter++;
            yield return null;
        } while (castPiecesCounter <= maxPiecesPerAttack);

        isAttacking = false;
        hasReducedMP = false;
    }

    private void ReduceMP()
    {
        if (!hasReducedMP)
        {
            mpManager.ReduceMPBar(mPConsumption);
            hasReducedMP = true;
        }
    }


    private void CheckUpperRoofHigh()
    {
        //Use of raycast in order to set a line of view from player to ground
        RaycastHit2D Ray = Physics2D.Linecast(rayCastOrigin.transform.position,
            roofCheckerRayCastLimit.transform.position, groundLayer);
        if (Ray.collider != null)
        {   //Get current Transform of hit object 
            Transform currentHitTransform = Ray.transform;
            //Set new position vectors for ice magic 
            SetCurrentPositionOfMagicTransforms(currentHitTransform);
        }
        else
        {
            SetCurrentPositionOfMagicTransforms();

        }

    }

    private bool IsThereDistanceAvailableForMagic()
    {
        RaycastHit2D Ray = Physics2D.Raycast(this.GetComponent<Transform>().position,
            magicCastDirection, 50f, groundLayer);

        /*Calculate distance beteween min x limit of magic attack and the point of horizontal
         * raycast hit. The current distance should be equal or greater than the min distance required for attack
         This is done in order to prevent the player cast ice magic while next to a wall*/
        if (Ray.collider != null)
        {
            float minXDistanceForMagic = 2f;
            float normalXDistanceForMagic =
                Mathf.Abs(endMagicPoint.transform.localPosition.x - startMagicPoint.transform.localPosition.x);
            float currentXDistance = Mathf.Abs(Ray.transform.localPosition.x - this.gameObject.transform.localPosition.x);

            if (currentXDistance >= minXDistanceForMagic)
            {
                /* Check new x limit for magic. If x distance is less than the normal x distance for attack 
                 * update magic to perform attack only to the distance in x determined by the RayCast collision*/
                if (currentXDistance < normalXDistanceForMagic)
                {
                    SetNewXLimitValueForMagic(Ray.transform.position.x);
                }
                return true;
            }
            else
            {
                isAttacking = false;
                return false;
            }
        }
        else
        {

            return true;
        }
    }

    //PlaySound
    IEnumerator RestorePlayerMovement()
    {
        yield return new WaitForSeconds(0.4f);
        playerMovement.SetMagicCastingFlag(false);
    }

    private void PlaySound(AudioClip audioSFX)
    {
        // play sound effect if set
        if (audioSFX)
        {

            this.gameObject.GetComponent<AudioSource>().PlayOneShot(audioSFX);

        }

    }


    private void CastIcePieces()
    {
        Vector2 spawnPosition;
        // get a random position between the specified ranges
        spawnPosition.x = Random.Range(xMinRange, xMaxRange);
        spawnPosition.y = Random.Range(yMinRange, yMaxRange);

        // determine which object to spawn
        int objectToSpawn = Random.Range(0, icePieces.Length);

        // actually spawn the game object
        GameObject spawnedObject = Instantiate(icePieces[objectToSpawn], spawnPosition, transform.rotation) as GameObject;


    }

    //Checker of amount of MP Available

    bool HasMPAvailable()
    {
        bool hasMP = true;
        if (mpManager.GetMPAmount() < mPConsumption)
        {
            hasMP = false;

        }
        return hasMP;
    }
}

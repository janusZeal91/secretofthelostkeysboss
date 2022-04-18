using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AudioSource))]
public class MirageAttack : MonoBehaviour
{
    [Header("Set in Inspector")]
    [Range(0, 5)]
    [SerializeField]
    private int attackLimit = 3;
    [Tooltip("Target to chase player during mirage attach stage")]
    [SerializeField]
    private Transform targetToChase;
    [SerializeField]
    [Range(1f, 10f)]
    private float chaseSpeed = 2f;
    [Range(1f, 10f)]
    [SerializeField]
    private float spawnHeight = 5f;
    [Range(1f, 5f)]
    [SerializeField]
    private float horizontalSpawnOffset = 2f;
    [Header("FireBall Configuration")]
    [SerializeField]
    private GameObject fireBall;
    [SerializeField]
    [Range(0, 4)]
    private int fireBallCastNumber;
    [SerializeField]
    [Range(0, 10)]
    private int fireBallSpeed = 5;
    [SerializeField]
    [Range(0, 10)]
    [Tooltip("Time to wait in seconds since boss hits ground from spawn to cast fire attack")]
    private float fireBallCooldown = 3f;
    [SerializeField]
    private AudioClip fireBallSFX;
    private Rigidbody2D bossRigidbody;
    private Animator bossAnimator;
    private int attackCounter = 0;
    private bool startChasing = false;
    private BossMovement bossMovement;
    private static int[] multipliers = { -1, 1 };
    private static float leftAreaXLimit;
    private static float rightAreaXLimit;
    [Header("Environment")]
    [Tooltip("Waypoints left limit should always be at 0 index of array")]
    public Transform[] bossAreaLimits;
    public AudioClip mirageSFX;
    private bool isCastingFireball;
    private RegGroundChecker regGroundChecker;


    private void Start()
    {
        //Register groundedCallback
        RegGroundChecker.regGroundChecker.callbackGroundedDel += ExecuteFireball;
    }

    private void OnDestroy()
    {
        RegGroundChecker.regGroundChecker.callbackGroundedDel -= ExecuteFireball;

    }
    private void ExecuteFireball()
    {
        StartCoroutine(InitFireBallTimer());
    }

    public int AttackCounter
    {
        get
        {
            return this.attackCounter;
        }
    }


    public int AttackLimit
    {
        get
        {
            return this.attackLimit;
        }
    }


    public bool StartChasing
    {
        get
        {
            return this.startChasing;
        }
        set
        {
            this.startChasing = value;
        }
    }

    private void Awake()
    {
        this.bossAnimator = this.GetComponent<Animator>();
        this.bossRigidbody = this.GetComponent<Rigidbody2D>();
        this.bossMovement = this.GetComponent<BossMovement>();
        this.regGroundChecker = this.GetComponent<RegGroundChecker>();
        this.InitBossAreaLimits();

    }

    private void InitBossAreaLimits()
    {
        if (bossAreaLimits.Length >= 2)
        {
            leftAreaXLimit = this.bossAreaLimits[0].position.x;
            rightAreaXLimit = this.bossAreaLimits[1].position.x;
        }
        else
        {
            leftAreaXLimit = 999;
            rightAreaXLimit = 999;
            Debug.LogWarning("One or more boss area limits are not asigned to the object!");
        }
    }



    private void RestartAttackCounter()
    {
        this.attackCounter = 0;
    }


    private void NormalChase()
    {

        //get the distance between the chaser and the target
        float distance = Vector2.Distance(this.GetComponent<Transform>().position,
            targetToChase.position);
        //so long as the chaser is farther away than the max distance, move towards it at rate speed.
        if (distance > 0f)
            this.GetComponent<Transform>().position =
                Vector2.MoveTowards(this.GetComponent<Transform>().position,
                new Vector2(targetToChase.position.x, transform.position.y),
                chaseSpeed * Time.deltaTime);

    }

    private void SetStateWalkingAnimation(bool state)
    {
        bossAnimator.SetBool("Moving", state);
    }

    private IEnumerator CastFireBalls()
    {
        isCastingFireball = true;
        SetStateWalkingAnimation(false);
        yield return null;
        bossAnimator.SetTrigger("CastFire");
        yield return new WaitForSeconds(0.5f);
        this.GetComponent<AudioSource>().PlayOneShot(fireBallSFX);
        for (int i = 0; i < fireBallCastNumber; i++)
        {
            SpawnFireBall(this.fireBall);
            yield return null;
        }
        isCastingFireball = false;

    }

    private void SpawnFireBall(GameObject fireBall)
    {
        Vector3 bossLocalScale = this.GetComponent<Transform>().localScale;
        GameObject newFireBall;
        Vector3 directionOfAttack = GetRandomFireAngleOfAttack();

        if ((bossLocalScale.x > 0f))
        {
            fireBall.transform.localScale = -transform.localScale;
            newFireBall = Instantiate(fireBall, this.transform.position, Quaternion.identity) as GameObject;
            newFireBall.GetComponent<Rigidbody2D>().AddForce(directionOfAttack * fireBallSpeed * 100, ForceMode2D.Force);


        }
        else if ((bossLocalScale.x < 0f))
        {
            fireBall.transform.localScale = -transform.localScale;
            newFireBall = Instantiate(fireBall, this.transform.position, Quaternion.identity) as GameObject;
            newFireBall.GetComponent<Rigidbody2D>().AddForce(-(directionOfAttack * fireBallSpeed * 100), ForceMode2D.Force);

        }
    }

    private Vector3 GetRandomFireAngleOfAttack()
    {
        /*In order to make the attack range direction of 90 degrees over Y Axis 
        the direction Vector will take values between -1 and 1 for "y" component to get any 
       angle within that range*/
        float randomYValue = Random.Range(-0.1f, 0.2f);
        Vector3 randomPosition = new Vector3(1, randomYValue, 0);
        return randomPosition;
    }

    public IEnumerator StartMirageAttack(bool isFirstTime)
    {
        BossRegManager.bossRegManager.currentBossAttack = BossRegManager.BossAttacks.Mirage;
        this.startChasing = true;
        if (!isFirstTime)
        {
            this.chaseSpeed = 1f;
            yield return new WaitForSeconds(2f);
            this.chaseSpeed = 3.5f;
        }
        
    }

    public void StopMirageAttack()
    {
        startChasing = false;
        StopAllCoroutines();     
        RestartAttackCounter();
           
    }

    public void SpawnBoss(Transform playerTransform)
    {

        StopCoroutine(InitFireBallTimer());
        this.GetComponent<AudioSource>().PlayOneShot(mirageSFX);
        //Get random -1, 0 or 1 in order to determine in random spawn is made to the left, right of above the player
        int randomIndex = Random.Range(0, 2);
        float posibleOffset = playerTransform.position.x + (horizontalSpawnOffset * multipliers[randomIndex]);
        ///Check that spawn is not performed outside of boss area limits
        if (posibleOffset < leftAreaXLimit)
        {
            if (Mathf.Abs(leftAreaXLimit - playerTransform.position.x) <= 0.5f)
            {
                posibleOffset = leftAreaXLimit + horizontalSpawnOffset;
            }
            else
            {
                posibleOffset = leftAreaXLimit;
            }
            
        }
        if (posibleOffset > rightAreaXLimit)
        {
            if (Mathf.Abs(rightAreaXLimit - playerTransform.position.x) <= 0.5f)
            {
                posibleOffset = rightAreaXLimit - horizontalSpawnOffset;
            }
            else
            {
                posibleOffset = rightAreaXLimit;
            }          
        }
        //Set new position  
        Vector3 newPosition = new Vector3(
            posibleOffset,
            playerTransform.position.y + spawnHeight, 0);
        this.GetComponent<Transform>().position = newPosition;
        bossRigidbody.velocity = Vector2.zero;
        attackCounter++;
               
    }

  

    public IEnumerator InitFireBallTimer()
    {
        yield return new WaitForSeconds(fireBallCooldown);
        StartCoroutine(CastFireBalls());
    }

    
    private void Update()
    {
        if (BossRegManager.bossRegManager.currentBossAttack == BossRegManager.BossAttacks.Mirage)
        {
            
            if (startChasing)
            {
                
                if (regGroundChecker.isGrounded)
                {
                    this.bossMovement.StopFlip = false;
                    if (!isCastingFireball)
                    {
                        
                        NormalChase();                                     
                        SetStateWalkingAnimation(true);
                    }
                    else
                    {
                        SetStateWalkingAnimation(false);
                    }                                       
                }
                else
                {
                    this.bossMovement.StopFlip = true;
                    SetStateWalkingAnimation(false);
                }
            }
        }
    }


    









}

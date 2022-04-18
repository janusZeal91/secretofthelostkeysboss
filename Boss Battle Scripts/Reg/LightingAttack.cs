using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(AudioSource))]
public class LightingAttack : MonoBehaviour
{

    private Transform bossTransform;
    private Animator bossAnimator;
    private Rigidbody2D bossRigidBody;

    [SerializeField]
    [Range(0f, 1f)]
    private float movementAmplitude = 0.5f;
    [SerializeField]
    [Range(0f, 20f)]
    private float frequency = 10f;
    [SerializeField]
    [Tooltip("Attack repetitions")]
    [Range(0, 30)]
    private int maxAttacks = 5;
    [Tooltip("Attack cooldown in seconds")]
    [SerializeField]
    [Range(0f, 20f)]
    private float coolDownDuration = 5f;
    [Tooltip("Restart attack time seconds")]
    [SerializeField]
    [Range(0f, 20f)]
    private float restartTime = 10f;
    [SerializeField]
    [Range(0.1f, 2f)]
    private float movementSpeed = 2f;
    [SerializeField]
    [Range(10f, 50f)]
    private float lightingSpawnHeight = 37f;
    [SerializeField]
    [Range(0f, 20f)]
    private float horizontalAttackRange = 10f;

    private float xdistToWayPoint = 0f;
    private int randomWaypointIndex;
    private Vector3 movementDirection;
    private int attackCounter = 0;
    private Vector3 position;
    private bool isVulnerable = true;
    private bool isAttacking = false;
    private static int [] multipliers = { -1, 1 };

    [Tooltip("Waypoints left limit should always be at 0 index of array")]
    public Transform[] wayPoints;
    public Transform playerTransform;
    public AudioClip lightingSFX;
    public GameObject lighting;

    public bool IsVulnerable
    {
        get
        {
           return this.isVulnerable;
        }
    }


    private void Awake()
    {
        this.bossTransform = this.GetComponent<Transform>();
        this.bossRigidBody = this.GetComponent<Rigidbody2D>();
        this.bossAnimator = this.GetComponent<Animator>();
    }

    private void SpawnBossOnRandomPosition()
    {      
        isVulnerable = false;
        float xleftLimit = wayPoints[0].position.x;
        float xRightLimit = wayPoints[1].position.x;
        float randomXPosition = Random.Range(xleftLimit+0.1f, xRightLimit-0.1f);
        Vector2 newPosition = new Vector2(randomXPosition, wayPoints[0].position.y);
        bossTransform.position = newPosition;
        SetGravityState(false);

    }

    private void PlaySound(AudioClip sfx)
    {
        this.GetComponent<AudioSource>().PlayOneShot(sfx);
    }

    
    private void SetRandomWayPointIndex()
    {
        randomWaypointIndex = Random.Range(0, 2);
    }

    public void StartLightingAttack()
    {
        SetRandomWayPointIndex();
        SetFirstTimeDirectionOfMov();
        SetIdleState();
        SpawnBossOnRandomPosition();             
        position = bossTransform.position;
        isVulnerable = false;
    }

    private void SetIdleState()
    {
        bossAnimator.SetBool("Falling", false);
        bossAnimator.SetBool("Grounded", true);
        bossAnimator.SetBool("Moving", false);
    }

    private void SetFirstTimeDirectionOfMov()
    {
        xdistToWayPoint = wayPoints[randomWaypointIndex].position.x - bossTransform.position.x;
        if (xdistToWayPoint >= 0)
        {
            movementDirection = bossTransform.right;
        }
        else
        {
            movementDirection = bossTransform.right * -1;
        }
    }

    private void SetGravityState(bool affectGravity)
    {
        this.bossRigidBody.gravityScale = affectGravity ? 2.0f : 0f;
    }


    private void ChangeWaypoint()
    {
        if (randomWaypointIndex == 0)
        {
            randomWaypointIndex = 1;
        }
        else
        {
            randomWaypointIndex = 0;
        }

    }

    private void GetNewDirectionOfMovement()
    {
        xdistToWayPoint = wayPoints[randomWaypointIndex].position.x - bossTransform.position.x;
        if (Mathf.Abs(xdistToWayPoint) < 0.1f)
        {
            ChangeWaypoint();
            xdistToWayPoint = wayPoints[randomWaypointIndex].position.x - bossTransform.position.x;
            if (xdistToWayPoint >= 0f)
            {
                movementDirection = bossTransform.right;
            }
            else
            {
                movementDirection = bossTransform.right * -1;
            }
        }
    }

    private void MoveBoss()
    {
        GetNewDirectionOfMovement();   
        position += movementDirection * Time.deltaTime * movementSpeed;
        bossTransform.position = position + bossTransform.up *
            Mathf.Sin(Time.time * frequency) * movementAmplitude;
     
    }

    private void InstatianteLighting()
    {
        int randomIndex = Random.Range(0,2);
        float xSpawningPoint = playerTransform.position.x + (horizontalAttackRange *
            multipliers[randomIndex]);
        //We cast lighting just above of player in case he stops movement 
        if (Mathf.Round(bossRigidBody.velocity.x) ==0f)
        {
            xSpawningPoint = playerTransform.position.x;
            if (Mathf.Round(playerTransform.position.x) <= Mathf.Round(wayPoints[0].position.x))
            {
                xSpawningPoint = wayPoints[0].position.x;
            }
            if(playerTransform.position.x >= wayPoints[1].position.x)
            {
                xSpawningPoint = wayPoints[1].position.x;
            }
            
        }
        
        
        Vector3 newPosition = new Vector3(xSpawningPoint,lightingSpawnHeight);
        Instantiate(lighting, newPosition, Quaternion.identity);
    }

    private IEnumerator PerformLightingAttack()
    {
        isAttacking = true;
        while (attackCounter <= maxAttacks)
        {
            bossAnimator.SetTrigger("CastThunder");
            PlaySound(lightingSFX);
            yield return new WaitForSeconds(0.7f);
            InstatianteLighting();
            attackCounter++;
            yield return new WaitForSeconds(coolDownDuration-0.7f);
        }
        SetGravityState(true);
        StartCoroutine(StopLightingAttack());
    }

    public void StopLightningAttackImmediate()
    {
        bossAnimator.SetBool("Kneel", false);
        StopAllCoroutines();
        isVulnerable = false;
        SetGravityState(true);
        isAttacking = false;
        attackCounter = 0;
        
    }

    private IEnumerator StopLightingAttack()
    {
        isVulnerable = true;     
        isAttacking = false;
        attackCounter = 0;
        bossAnimator.SetBool("Kneel", true);
        yield return new WaitForSeconds(restartTime);
        bossAnimator.SetBool("Kneel", false);
        StartLightingAttack();

    }
 

    // Update is called once per frame
    void LateUpdate()
    {
        if (BossRegManager.bossRegManager.currentBossAttack == BossRegManager.BossAttacks.Lighting
            && !isVulnerable)
        {
            MoveBoss();
            if (!isAttacking)
            {
                StartCoroutine(PerformLightingAttack());
            }
            
        }
    }
}

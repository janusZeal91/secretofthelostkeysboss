using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Animator), typeof(Rigidbody2D))]
public class CloneManager : MonoBehaviour
{



    [SerializeField]
    private GameObject cloneExplosion;
    public Transform playerTransform;
    [Header("Ground Checker")]
    public LayerMask whatIsGround;
    [HideInInspector]
    public bool isGrounded = true;
    [SerializeField]
    private Transform groundChecker;

    private Rigidbody2D bossRigidBody;
    private Animator cloneAnimator;

    private void Awake()
    {
        this.bossRigidBody = this.GetComponent<Rigidbody2D>();
        this.cloneAnimator = this.GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "AttackTrigger")
        {
            DestroyClone();
        }
      
    }



    private void DestroyClone()
    {
        Instantiate(cloneExplosion, this.GetComponent<Transform>().position, Quaternion.identity);
        BossRegManager.bossRegManager.AttackClone();
        Destroy(this.gameObject);
    }

    public void DestroyImmediateClone()
    {
        Instantiate(cloneExplosion, this.GetComponent<Transform>().position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    private void Update()
    {
        CheckGround();
        CheckFalling();
       
    }

    private void CheckFalling()
    {
        if (bossRigidBody.velocity.y >= 0)
        {
            cloneAnimator.SetBool("Falling", false);
        }
        else
        {
            cloneAnimator.SetBool("Falling", true);
        }

    }

    private void CheckGround()
    {
        this.isGrounded = Physics2D.Linecast(this.GetComponent<Transform>().position,
           groundChecker.position, whatIsGround);
        cloneAnimator.SetBool("Grounded", this.isGrounded);
    }

    private void LateUpdate()
    {
        FlipBossSpriteBasedOnPlayerPosition();
        if (BossRegManager.bossRegManager.DestroyAllClones())
        {
            DestroyImmediateClone();
        }
    }

    private void FlipBossSpriteBasedOnPlayerPosition()
    {
        float playerXPosition = playerTransform.position.x;
        float bossXPosition = this.gameObject.GetComponent<Transform>().position.x;
        Vector3 localScale = this.gameObject.GetComponent<Transform>().localScale;
        bool isPlayerToTheRight = false;
        if (playerXPosition <= bossXPosition)
        {
            isPlayerToTheRight = false;
        }
        else
        {
            isPlayerToTheRight = true;
        }

        if (((isPlayerToTheRight) && (localScale.x < 0)) || ((!isPlayerToTheRight) && (localScale.x > 0)))
        {
            localScale.x *= -1;
        }

        this.gameObject.GetComponent<Transform>().localScale = localScale;
    }



}

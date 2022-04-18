using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class RegGroundChecker : MonoBehaviour
{

    public LayerMask whatIsGround;
    [HideInInspector]
    public bool isGrounded = true;
    [SerializeField]
    private Transform groundChecker;
    private Animator bossAnimator;
    private Rigidbody2D bossRigidBody;
    private Vector3 rigidBodyVelocity;
    public delegate void CallbackGroundedDel();
    public delegate void CallbackFlyingDel();
    public CallbackGroundedDel callbackGroundedDel;
    public CallbackFlyingDel callbackFlyingDel;
    private enum RegState { GROUNDED, FLYING };
    private RegState currentState = RegState.GROUNDED;
    public static RegGroundChecker regGroundChecker;
    private RegState CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if (currentState == value)
            {
                return;
            }
            else
            {
                if (currentState == RegState.FLYING)
                {
                    if (callbackGroundedDel != null)
                    {
                        callbackGroundedDel.Invoke();
                    }
                }

            }
            currentState = value;
        }
    } 

    private void SetVelocityOnChange()
    {
        this.rigidBodyVelocity = bossRigidBody.velocity;
        this.rigidBodyVelocity.x = 0;
        bossRigidBody.velocity = this.rigidBodyVelocity;
    }

    private void Awake()
    {
        bossAnimator = this.GetComponent<Animator>();
        bossRigidBody = this.GetComponent<Rigidbody2D>();
        regGroundChecker = this;
        callbackGroundedDel += SetVelocityOnChange;
    }

    private void CheckGround()
    {
      
        this.isGrounded = Physics2D.Linecast(this.GetComponent<Transform>().position,
            groundChecker.position, whatIsGround);
        
        if(BossRegManager.bossRegManager.currentBossAttack == BossRegManager.BossAttacks.Mirage)
        {
            if (isGrounded)
            {
                CurrentState = RegState.GROUNDED;
            }
            else
            {
                CurrentState = RegState.FLYING;
            }
        }
      
        if (BossRegManager.bossRegManager.currentBossAttack != BossRegManager.BossAttacks.Lighting)
        {
            bossAnimator.SetBool("Grounded", this.isGrounded);
        }      
        if (BossRegManager.bossRegManager.currentBossAttack == BossRegManager.BossAttacks.Clone)
        {
            this.rigidBodyVelocity = bossRigidBody.velocity;
            this.rigidBodyVelocity.x = 0;
            bossRigidBody.velocity = this.rigidBodyVelocity;
            return;
        }
        if ((BossRegManager.bossRegManager.currentBossAttack == BossRegManager.BossAttacks.Lighting
            || BossRegManager.bossRegManager.currentBossAttack == BossRegManager.BossAttacks.Finished)
            && this.isGrounded)
        {
            this.rigidBodyVelocity = bossRigidBody.velocity;
            this.rigidBodyVelocity.x = 0;
            bossRigidBody.velocity = this.rigidBodyVelocity;
            return;
        }
    }

   
    private void CheckFalling()
    {
        if (bossRigidBody.velocity.y >= 0)
        {
            bossAnimator.SetBool("Falling", false);
        }
        else
        {
            bossAnimator.SetBool("Falling", true);
        }

    }


    // Update is called once per frame
    void Update()
    {
        CheckGround();
        CheckFalling();

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RegSwordAttack : MonoBehaviour {

    [SerializeField]
    [Range(5, 50)]
    private int attackPower = 10;

    [SerializeField]
    [Range(0.5f, 3f)]
    [Tooltip("Time in seconds to wait until attacking again")]
    private float attackCoolDown = 0.5f;
    private Animator bossAnimator;
    private bool isInCoolDown = false;

    private void Awake()
    {
        this.bossAnimator = this.GetComponent<Animator>();
        
    }

    private void SetAttackAnimation()
    {
        bossAnimator.SetTrigger("Attack");
    }

    private IEnumerator StartCoolDown()
    {
        isInCoolDown = true;
        yield return new WaitForSeconds(attackCoolDown);
        isInCoolDown = false;

    }

    private void StopToAttackPlayer()
    {
        this.GetComponent<BossMovement>().StopToAttackPlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(BossRegManager.bossRegManager.currentBossAttack != BossRegManager.BossAttacks.Finished)
        {
            if (collision.tag == "Player" && !isInCoolDown)
            {
                SetAttackAnimation();
                if (this.gameObject.tag != "Clone")
                {
                    StopToAttackPlayer();
                }
                collision.GetComponent<CharacterController2D>().ApplyDamage(attackPower);
            }
        }      
    }




}

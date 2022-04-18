using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody))]
public class CloneAttack : MonoBehaviour {
    [Header("Set in Inspector")]
    [SerializeField]
    public Transform[] platformsTransform;
    [SerializeField]
    private GameObject cloneReg;
    [SerializeField]
    private GameObject fireBall;
    [SerializeField]
    [Range(0, 10)]
    private int fireBallSpeed = 5;
    [SerializeField]
    private GameObject spawnEffect;

    private Animator bossAnimator;
    private Transform bossTransform;
    private Rigidbody bossRigidBody;

    private int cloneDestroyedCounter = 0;
    private int attackTriggerLayer;
    private int enemyLayer;
    private Vector3 spawnPosition;
    private bool destroyAllClones = false;

    public bool DestroyAllClones
    {
        get
        {
            return this.destroyAllClones;
        }
        set
        {
            this.destroyAllClones = value;
        }
    }


    public int CloneDestroyedCounter
    {
        get
        {
            return this.cloneDestroyedCounter;
        }
    }

    private void Awake()
    {
        bossAnimator = this.GetComponent<Animator>();
        bossTransform = this.GetComponent<Transform>();
        bossRigidBody = this.GetComponent<Rigidbody>();
        attackTriggerLayer = LayerMask.NameToLayer("AttackTrigger");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    private Vector3 GetSpawnPosition(int index)
    {
        spawnPosition = platformsTransform[index].position;
        spawnPosition.y += 3f;
        return spawnPosition;
    }

    private IEnumerator SpawnClones()
    {
        Physics2D.IgnoreLayerCollision(attackTriggerLayer, enemyLayer, true);
        int realRegSpawnIndex = Random.Range(0, platformsTransform.Length);
        Debug.Log("Current spawn index: " + realRegSpawnIndex);
        for (int i = 0; i < platformsTransform.Length; i++)
        {
            platformsTransform[i].gameObject.SetActive(true);
            if (i == realRegSpawnIndex)
            {
                
                this.GetComponent<Transform>().position =
                    GetSpawnPosition(i);
            }
            else
            {
                GameObject regClone = Instantiate(cloneReg, GetSpawnPosition(i), Quaternion.identity);
                Instantiate(spawnEffect, GetSpawnPosition(i), Quaternion.identity);
                regClone.SetActive(true);
            }
            yield return null;
        }
        Physics2D.IgnoreLayerCollision(attackTriggerLayer, enemyLayer, false);
    }

    private void DeactivatePlatforms()
    {
        foreach(Transform platformT in platformsTransform)
        {
            platformT.gameObject.SetActive(false);
        }

    }

    public void StartCloneAttack()
    {
        RestartAnimationState();
        StartCoroutine(SpawnClones());
    }

    private void RestartAnimationState()
    {
        this.bossAnimator.ResetTrigger("Moving");
        this.bossAnimator.ResetTrigger("Falling");
    }



    private void SetRigidbodyState(bool state)
    {
        bossRigidBody.isKinematic = state;
        
    }

    public void DestroyClone()
    {
        cloneDestroyedCounter++;
        if (cloneDestroyedCounter >= 3)
        {         
            StartCoroutine(PerformFireExplosion());                       
        }
    }

    public IEnumerator PerformFireExplosion()
    {
        bossAnimator.SetTrigger("CastFire");
        yield return new WaitForSeconds(0.8f);
        float rotation = 0f;
        for(int i=0; i <= 4; i++)
        {
            
            GameObject newFireBall = Instantiate(fireBall, this.GetComponent<Transform>().position,
                Quaternion.Euler(0,0, rotation));
            newFireBall.GetComponent<Rigidbody2D>().AddForce(-(newFireBall.GetComponent<Transform>().right
                * fireBallSpeed * 100), ForceMode2D.Force);
            if (i % 2 == 0)
            {
                rotation += 90;
            }
            else
            {
                rotation += 45;
            }        
            yield return null;
        }
        StopCloneAttack();
        BossRegManager.bossRegManager.StartMirageAttack(false);
    }

    public void StopCloneAttack()
    {
        destroyAllClones = true;
        cloneDestroyedCounter = 0;
        DeactivatePlatforms();

    }
	
   
}

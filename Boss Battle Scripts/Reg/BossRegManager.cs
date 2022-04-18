using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossRegManager : MonoBehaviour
{

    [SerializeField]
    private AudioClip bossFightSFX;
    [SerializeField]
    private AudioClip anxietyThemeSFX;
    public SpriteRenderer regSpriteRenderer;
    public AudioSource mainAudioSource;
    public Transform playerTransform;
    public Transform bossTransform;
    public GameObject regBossGUI;
    public Transform lastBossSpawnPoint;
    public Transform lastPlayerSpawnPoint;
    public GameObject[] lastPlatforms;
    public GameObject[] bossOuterLimits;
    public GameObject portal;
    public GameObject lastDialogTrigger;
    public GameObject lastActionTrigger;
    public GameObject dialogBox;
    public MirageAttack mirageAttack;
    public CloneAttack cloneAttack;
    public LightingAttack lightingAttack;
    public Animator bossAnimator;
    public Rigidbody2D bossRigidBody;
    public GameObject regDissapearEffect;
    public GameObject regAppearEffect;

    public static BossRegManager bossRegManager;
    [HideInInspector]
    public enum BossAttacks { Mirage, Lighting, Clone, Finished };
    [HideInInspector]
    public BossAttacks currentBossAttack;
    public GameObject bossStatusTextUI;
    public CharacterController2D playerManager;
    public GameObject bossExplosion;
    private int playerLayer;
    private int enemyLayer;
    private int attackTriggerLayer;



    private void Awake()
    {
        bossRegManager = this.GetComponent<BossRegManager>();
        this.playerLayer = LayerMask.NameToLayer("Player");
        this.enemyLayer = LayerMask.NameToLayer("Enemy");
        this.attackTriggerLayer = LayerMask.NameToLayer("AttackTrigger");
    }

    public void StartBossMusic()
    {
        this.PlayMusic(bossFightSFX, 0.4f);
    }

    public void StartNormalMusic()
    {
        this.PlayMusic(anxietyThemeSFX, 0.4f);
    }

    public void StartMirageAttack(bool isFirstTime)
    {
        StartCoroutine(mirageAttack.StartMirageAttack(isFirstTime));
    }

    public IEnumerator SpawnBoss()
    {
        if (mirageAttack.AttackCounter <= mirageAttack.AttackLimit)
        {
            SetBossSpawnEffect(false);
            yield return new WaitForSeconds(0.4f);
            mirageAttack.SpawnBoss(playerTransform);
            SetBossSpawnEffect(true);
        }
        else
        {
            StopMirageAttack();
            SetBossSpawnEffect(false);
            yield return new WaitForSeconds(0.4f);
            StartLightningAttack();
            SetBossSpawnEffect(true);

        }
    }

    private void SetBossSpawnEffect(bool isSpawn)
    {
        GameObject spawnEffecct = isSpawn ? regAppearEffect : regDissapearEffect;
        Instantiate(spawnEffecct, bossTransform.position, Quaternion.identity);
        regSpriteRenderer.enabled = isSpawn;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, !isSpawn);
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, !isSpawn);

    }

    private void StartLightningAttack()
    {
        currentBossAttack = BossAttacks.Lighting;
        lightingAttack.StartLightingAttack();
    }


    public void AttackRealClone()
    {
        cloneAttack.StopCloneAttack();
        SendMessageToPlayerHUD();
        StartMirageAttack(false);

    }


    private void StopLightingAttack()
    {

        currentBossAttack = BossAttacks.Clone;
        lightingAttack.StopLightningAttackImmediate();
        StartCloneAttack();

    }

    public bool DestroyAllClones()
    {
        return cloneAttack.DestroyAllClones;
    }

    private void StartCloneAttack()
    {
        cloneAttack.DestroyAllClones = false;
        string message = PlayerPrefManager.GetLanguage() == 1 ?
            "Reg is vulnerable only to physical attacks!" : "¡Reg es vulnerable solo a ataques físicos!";
        SendMessageToPlayerHUD(message);
        cloneAttack.StartCloneAttack();

    }

    public int GetCloneDeathCounter()
    {
        return cloneAttack.CloneDestroyedCounter;
    }

    public void AttackClone()
    {
        cloneAttack.DestroyClone();
    }

    public void AttackOnLighting()
    {
        if (lightingAttack.IsVulnerable)
        {
            StopLightingAttack();
        }
        else
        {
            if (PlayerPrefManager.GetLanguage() == 1)
            {
                SendBossStatusMessage("Reg's lightning protects him from damage!");
            }
            else
            {
                SendBossStatusMessage("¡El poder de Reg lo protege de daños!");
            }
        }
    }

    public IEnumerator SendBossStatusMessage(string message)
    {
        bossStatusTextUI.SetActive(true);
        bossStatusTextUI.GetComponent<Text>().text = message;
        yield return new WaitForSeconds(5f);
        bossStatusTextUI.SetActive(false);
    }

    private void SendMessageToPlayerHUD()
    {
        if (PlayerPrefManager.GetLanguage() == 1)
        {
            playerManager.SendMsgToPlayerHUD("Reg's is weak to Eralia Sword power!");
        }
        else
        {
            playerManager.SendMsgToPlayerHUD("¡Reg es débil al poder de la espada de Eralia!");
        }
    }

    public void SendMessageToPlayerHUD(string message)
    {
        playerManager.SendMsgToPlayerHUD(message);
    }






    public void StopMirageAttack()
    {
        mirageAttack.StopMirageAttack();
    }

    private void PlayMusic(AudioClip newMusic, float volume)
    {
        if (mainAudioSource != null)
        {
            mainAudioSource.loop = true;
            mainAudioSource.volume = volume;
            mainAudioSource.clip = newMusic;
            mainAudioSource.Play();
        }

    }

    public void StartKneelAnimation()
    {
        bossAnimator.SetBool("Kneel", true);
    }

    public IEnumerator FinishBossFight()
    {
        cloneAttack.StopCloneAttack();
        ActivateLastPlatform();
        SetStateBossOuterLimits(false);
        regBossGUI.SetActive(false);
        SpawnPlayerOnLastPosition();
        portal.SetActive(true);
        lastDialogTrigger.SetActive(true);
        lastActionTrigger.SetActive(true);
        regSpriteRenderer.enabled = false;
        SetFinalPositionOfDialogBox();
        DisableBossHoriontalMovement();
        yield return new WaitForSeconds(1f);
        regSpriteRenderer.enabled = true;
        SpawnBossInLastPosition();
        SetBossSpawnEffect(true);
        ChangeBossFaceDirection();
        PlayMusic(anxietyThemeSFX, 0.4f);
        SetFinshedBossState();


    }

    private void ChangeBossFaceDirection()
    {
        Vector3 localScale = new Vector3(-1, 1, 1);
        this.bossTransform.localScale = localScale;
    }

    private void SetFinshedBossState()
    {
        this.currentBossAttack = BossAttacks.Finished;
        StartKneelAnimation();
    }

    public void DestroyReg()
    {
        ParticleSystem.MainModule mainModule = this.portal.GetComponent<ParticleSystem>().main;
        mainModule.startSize = 2f;
        this.portal.GetComponent<BoxCollider2D>().enabled = true;
        Instantiate(bossExplosion, bossTransform.position, Quaternion.identity);
        Destroy(bossTransform.gameObject);
    }

    private void SpawnPlayerOnLastPosition()
    {
        Vector3 newSpawnPosition = this.lastPlayerSpawnPoint.position;
        newSpawnPosition.y += 1f;
        playerTransform.position = newSpawnPosition;
    }

    private void SpawnBossInLastPosition()
    {
        Vector3 newSpawnPosition = this.lastBossSpawnPoint.position;
        newSpawnPosition.y += 1f;
        bossTransform.position = newSpawnPosition;
    }

    private void ActivateLastPlatform()
    {
        foreach(GameObject tile in lastPlatforms)
        {
            tile.SetActive(true);
        }
    }

    private void DisableBossHoriontalMovement()
    {
        bossRigidBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void SetFinalPositionOfDialogBox()
    {
        dialogBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -350f, 0);

    }

    private void SetStateBossOuterLimits(bool state)
    {
        foreach(GameObject tile in bossOuterLimits)
        {
            tile.SetActive(state);
        }
    }

    private void StopMovementOnRestart()
    {
        bossAnimator.GetComponent<Animator>().SetBool("Moving", false);
    }

    private void OnDestroy()
    {
        BossMovement.StartEvent -= StopMovementOnRestart;
    }

    private void SetRestartState(bool isRestart)
    {
        if (isRestart)
        {
            
            regBossGUI.SetActive(true);
            regBossGUI.GetComponent<Animator>().SetTrigger("Enter");
            bossTransform.parent.gameObject.SetActive(true);
            BossMovement.StartEvent += StopMovementOnRestart;
        }
        else
        {
            PlayerPrefManager.SetBossRestartState(1);
        }
    }
    public void EnableBossFight(bool isRestart)
    {

        SetRestartState(isRestart);
        this.StartBossMusic();
        SetStateBossOuterLimits(true);
        this.StartMirageAttack(true);
    }
}

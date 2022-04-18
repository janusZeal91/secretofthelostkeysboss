using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{

    [HideInInspector]
    public int scoreUntilCheckpoint = 0;
    [HideInInspector]
    public int livesUntilCheckpoint = 0;
    [HideInInspector]
    public int collectiblesUntilCheckPoint = 0;
    [HideInInspector]
    public int secretZonesUntilCheckpoint = 0;
    [HideInInspector]
    public int scoreToExtraLifeUntilCheckPoint = 0;
    private List<GameObject> collectiblesTakenVault = new List<GameObject>();
    private List<GameObject> secretZonesTriggersVault = new List<GameObject>();
    private List<GameObject> enemiesVault = new List<GameObject>();
    public static CheckpointManager checkpointManager;
  

    private void Awake()
    {
        checkpointManager = this.GetComponent<CheckpointManager>();
    }

    private void Start()
    { 
        livesUntilCheckpoint = PlayerPrefManager.GetLives();

    }

    public void AddEnemyToVault(GameObject enemy)
    {
        enemiesVault.Add(enemy);
    }

    public void AddCollectibleToVault(GameObject collectible)
    {
        collectiblesTakenVault.Add(collectible);
    }

   

    public void AddSecretZoneToVault(GameObject secretZoneTrigger)
    {
        secretZonesTriggersVault.Add(secretZoneTrigger);
    }

    private void RestoreToPreviousCheckpointValues()
    {
       
        GameManager.gm.score = scoreUntilCheckpoint;
        GameManager.gm.counterOfCollectibles = collectiblesUntilCheckPoint;
        GameManager.gm.secretZonesUnlocked = secretZonesUntilCheckpoint;
        GameManager.gm.scoreToReachExtraLife = scoreToExtraLifeUntilCheckPoint;
    }

    public void RestoreAllValuesPreCheckPoint()
    {
        RestoreAllEnemiesState();
        RestoreAllSecretZonesState();
        RestoreAllCollectiblesState();
        RestoreToPreviousCheckpointValues();
    }
   

    private void RestoreAllSecretZonesState()
    {
        foreach(GameObject secretZone in secretZonesTriggersVault)
        {
            secretZone.SetActive(true);
            secretZone.GetComponent<SecretZoneTrigger>().taken = false;
        }
    }

    private void RestoreAllEnemiesState()
    {
        foreach(GameObject enemy in enemiesVault)
        {
            enemy.SetActive(true);
            enemy.GetComponent<SpriteRenderer>().enabled = true;
            if (enemy.GetComponent<BoxCollider2D>() != null)
            {
                enemy.GetComponent<BoxCollider2D>().enabled = true;
            }         
            enemy.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            if (enemy.GetComponent<CircleCollider2D>() != null)
            {
                enemy.GetComponent<CircleCollider2D>().enabled = true;
            }
          
        }
    }
    private void RestoreAllCollectiblesState()
    {
        foreach (GameObject collectible in collectiblesTakenVault)
        {
            if(collectible.tag != "RecoveryItem")
            {
                if (collectible.tag == "Coin")
                {
                    collectible.SetActive(true);                  
                }
                else
                {
                    collectible.SetActive(true);
                    collectible.GetComponent<Transform>().parent.gameObject.SetActive(true);

                }
                collectible.GetComponent<Collectible>().taken = false;
                collectible.GetComponent<SpriteRenderer>().enabled = true;
                collectible.GetComponent<BoxCollider2D>().enabled = true;
            }
            else
            {
                collectible.SetActive(true);                                        
            }
          
        }
    }

    public void SaveParametersUntilCheckpoint()
    {
        //Save last score
        scoreUntilCheckpoint = GameManager.gm.score;
        //Save lives untilCheckpoint
        livesUntilCheckpoint = GameManager.gm.lives;
        //Save collectibleCount 
        collectiblesUntilCheckPoint = GameManager.gm.counterOfCollectibles;
        //Save secretZonesCount 
        secretZonesUntilCheckpoint = GameManager.gm.secretZonesUnlocked;
        //Save scoreToExtraLife
        scoreToExtraLifeUntilCheckPoint = GameManager.gm.scoreToReachExtraLife;

    }

    public void DestroyAllCollectiblesInVault()
    {
        foreach (GameObject collectible in collectiblesTakenVault)
        {
            Destroy(collectible);
        }
        collectiblesTakenVault.Clear();
    }

    public void DestroyAllSecretZonesInVault()
    {
        foreach(GameObject secretZone in secretZonesTriggersVault)
        {
            Destroy(secretZone);
        }
        secretZonesTriggersVault.Clear();
    }

    public void DestroyAllEnemiesInVault()
    {
        try
        {
            foreach (GameObject enemy in enemiesVault)
            {
                Destroy(enemy.GetComponent<Transform>().parent.gameObject);
            }
            enemiesVault.Clear();
        }
        catch(System.Exception e)
        {
            Debug.LogError("Something went bad deleting this enemy: " + e.Message);
        }     
    }
}

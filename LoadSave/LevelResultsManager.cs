using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class LevelResultsManager : MonoBehaviour
{
  
  

    public Text UILevelName;
    public Text UIPlayTime;
    public Text UICollectiblesRate;
    public Text UISecretZonesRate;
    public Text UIRank;
    public GameObject saveConfirmationDialog;
    public GameObject savedGameMessage;
    public GameObject exitConfirmation;
    public GameObject normalMenuPanel;
    public GameObject lastLevelPanel;
    public GameObject exitToWorldMap;
    public GameObject endGameButton;
    public Button[] menuButtons;
    public EventSystem eventSystem;
    
    

    private PlayerSession.Level currentLevel;
    private PlayerSession savedFile;
    private bool isLastLevel = false;

   

    // Use this for initialization
    void Start()
    {
        this.currentLevel = DataPersistenceManager.LoadLevelInfo();
        SetResultsForCurrentLevel(currentLevel);
        AutoSaveGame();
        SetButtonsPanel();
    }

    

    private void SetButtonsPanel()
    {
        PlayerSession playerSession = DataPersistenceManager.LoadGameData();
        if (playerSession.hasBeatedGame)
        {
            isLastLevel = true;
            lastLevelPanel.SetActive(true);
            eventSystem.firstSelectedGameObject = endGameButton;
        }
        else
        {
            isLastLevel = false;
            normalMenuPanel.SetActive(true);
            eventSystem.firstSelectedGameObject = exitToWorldMap;
        }
    }
   
    private void AutoSaveGame()
    {
        int savedRank = DataPersistenceManager.GetSavedRankOfLevel(this.currentLevel.levelNumber);
        if (savedRank >= this.currentLevel.rank)
        {
            SaveGame();
        }
        else
        {          
            DataPersistenceManager.SaveAchievements(AchievementManager.achievementManager_S.GetCompletedAchievements());
        }
            
    }
   
    private void DisableButtonsWhenMenuIsOpen()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].interactable = false;
        }

    }

    private void EnableButtonsOnMenuClosed()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].interactable = true;
        }
    }

    public void SetStatusConfirmationSavedDialog(bool status)
    {
        if (!status)
        {
            StartCoroutine(CloseConfirmationSaveDialog());
        }
        else
        {
            DisableButtonsWhenMenuIsOpen();
            saveConfirmationDialog.SetActive(status);
            SetYesSelectedOnConfirmationSave();
        }

    }
    public void SetStatusExitDialog(bool status)
    {
        if (!status)
        {

            StartCoroutine(CloseConfirmationExitDialog());
        }
        else
        {
            DisableButtonsWhenMenuIsOpen();
            exitConfirmation.SetActive(status);
            SetYesSelectedOnConfirmationExit();
        }
    }

    public void SetStatusSavedGameDialog(bool status)
    {
        if (!status)
        {
            StartCoroutine(CloseSavedGameMessage());
        }
        else
        {
            savedGameMessage.SetActive(status);
            SetOkSelectedOnConfirmationSaved();
        }
       
        
    }
    public IEnumerator CloseConfirmationExitDialog()
    {
        exitConfirmation.GetComponent<Animator>().SetTrigger("Exit");
        yield return new WaitForSeconds(0.5f);
        exitConfirmation.SetActive(false);
        SetWorldMapSelected();
        EnableButtonsOnMenuClosed();
    }
    public IEnumerator CloseConfirmationSaveDialog()
    {
        saveConfirmationDialog.GetComponent<Animator>().SetTrigger("Exit");
        yield return new WaitForSeconds(0.5f);
        saveConfirmationDialog.SetActive(false);          
    }

    public IEnumerator CloseSavedGameMessage()
    {
        savedGameMessage.GetComponent<Animator>().SetTrigger("Exit");
        yield return new WaitForSeconds(0.5f);
        savedGameMessage.SetActive(false);
        SetWorldMapSelected();
        EnableButtonsOnMenuClosed();
    }

    private void SetYesSelectedOnConfirmationSave()
    {
        EventSystem.current.SetSelectedGameObject(
           saveConfirmationDialog.GetComponent<Transform>().Find("YesButton").gameObject
           , null);
    }

    private void SetOkSelectedOnConfirmationSaved()
    {
        EventSystem.current.SetSelectedGameObject(
            savedGameMessage.GetComponent<Transform>().Find("OkButton").gameObject
            , null);
    }

    private void SetYesSelectedOnConfirmationExit()
    {
        EventSystem.current.SetSelectedGameObject(
           exitConfirmation.GetComponent<Transform>().Find("YesButton").gameObject
           , null);

    }

    private void SetWorldMapSelected()
    {
        if (isLastLevel)
        {
            this.eventSystem.SetSelectedGameObject(endGameButton);
        }
        else
        {
            this.eventSystem.SetSelectedGameObject(exitToWorldMap);
        }

     
    }

   
    public void SaveGame()
    {
        DataPersistenceManager.SaveGameData(this.currentLevel);
        StartCoroutine(CloseConfirmationSaveDialog());
        SetStatusSavedGameDialog(true);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void SetResultsForCurrentLevel(PlayerSession.Level level)
    {
        if (PlayerPrefManager.GetLanguage() == 1)
        {
            UILevelName.text = level.levelName + " - Level " + level.levelNumber.ToString();
        }
        else
        {
            UILevelName.text = level.levelSpanishName + " - Nivel " + level.levelNumber.ToString();
        }
       
        UIPlayTime.text =  GetTimeOnFormat(level.playTime);
        UISecretZonesRate.text = level.secretZonesUnlocked + "/" + level.secretZonesToUnlock;
        UICollectiblesRate.text = level.collectiblesTaken + "/" + level.collectiblesToTake;
        SetRank(level);
       

    }

    private string GetTimeOnFormat(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        return timeText;
    }

    private void SetRank(PlayerSession.Level level)
    {
        if (level.rank == 1)
        {
            /*Set Yellow Color*/
            UIRank.color = new Color32(255, 255, 90, 255);
            UIRank.text = GetRank(level.rank);
            
        }
        else
        {
            UIRank.text = GetRank(level.rank);
        }
    }

    private string GetRank(int rankNumber)
    {
        switch (rankNumber)
        {
            case 5:
                return "D";
            case 4:
                return "C";
            case 3:
                return "B";
            case 2:
                return "A";
            case 1:
                return "S";
            default:
                return "N/A";
        }
    }

    
}

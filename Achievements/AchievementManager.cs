using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Steamworks;

public class AchievementManager: MonoBehaviour
{

    public Achievement[] achievements;
    public static AchievementManager achievementManager_S;
    public bool enableAchievements = true;
    private static bool isOnlineAchievementComplete = false;

    private void Awake()
    {
        achievementManager_S = this;
    }
    public Achievement[] GetAchievements()
    {
        PlayerSession savedData = DataPersistenceManager.LoadGameData();
        if (savedData.achievements.Length>0)
        {            
            return savedData.achievements;
        }
        else {
            return achievements;
        }
    }
    


    private static void DisplayAchivementMsg(string title, string description)
    {
        Debug.Log("Achievement: " + title + "/" + description + " Unlocked!");
    }

    public Achievement[] GetCompletedAchievements()
    {       
        Achievement[] currentAchievements = GetAchievements();
        Achievement[] reviewedAchievements = new Achievement[currentAchievements.Length];
        int counter = 0;
        foreach (Achievement achievement in currentAchievements)
        {
            if (achievement.Complete)
            {
                if (enableAchievements)
                {
                    STEAM_SaveAchievement(achievement.achievementId.Trim());
                }              
                reviewedAchievements[counter] = achievement;
                counter++;
                continue;

            }
            else
            {
                switch (achievement.stepType)
                {
                    case Achievement.StepType.time:
                        if (GetCountOfPerfectTimesLevels() >= achievement.stepCount)
                        {
                            achievement.Complete = true;

                        }
                        break;
                    case Achievement.StepType.collectible:
                        if (GetCountOfPerfectCollectiblesLevels() >= achievement.stepCount)
                        {
                            achievement.Complete = true;
                        }
                        break;
                    case Achievement.StepType.secretArea:
                        if (GetCountOfSecretAreasUnlocked() >= achievement.stepCount)
                        {
                            achievement.Complete = true;
                        }
                        break;
                    case Achievement.StepType.levelCompletion:
                        if (CheckLevelCompletion(achievement.stepCount))
                        {
                            achievement.Complete = true;
                        }
                        break;
                    case Achievement.StepType.rank:
                        if (GetCountOfSRanks() >= achievement.stepCount)
                        {
                            achievement.Complete = true;
                        }
                        break;
                }
                if (achievement.Complete)
                {
                    DisplayAchivementMsg(achievement.name, achievement.description);
                    if (enableAchievements)
                    {
                        STEAM_SaveAchievement(achievement.achievementId);
                    }
                    
                }
                reviewedAchievements[counter] = achievement;
                counter++;

            }

        }

        return reviewedAchievements;
    }

   

    public static void STEAM_SaveAchievement(string achievementID)
    {
        
        SteamUserStats.GetAchievement(achievementID, out isOnlineAchievementComplete);
        if (!isOnlineAchievementComplete)
        {
            SteamUserStats.SetAchievement(achievementID);
            SteamUserStats.StoreStats();
        }
       
    }

    //ONLY FOR DEBUG
    public void STEAM_LockAllAchievements()
    {
        Achievement[] currentAchievements = GetAchievements();
        foreach(Achievement achievement in currentAchievements)
        {
            SteamUserStats.ClearAchievement(achievement.achievementId);
            SteamUserStats.StoreStats();

        }
    } 

    
    public static int GetCountOfSRanks()
    {
        PlayerSession savedGame = DataPersistenceManager.LoadGameData();
        int counter = 0;
        foreach(PlayerSession.Level level in savedGame.levels)
        {
            if (level.rank == 1)
            {
                counter++;
            }
        }
        return counter;
    }

    private static bool CheckLevelCompletion(int levelNumber)
    {
        List<PlayerSession.Level> levels = DataPersistenceManager.LoadGameData().levels;
        PlayerSession.Level level = levels.Find(x => x.levelNumber == levelNumber);
        return level == null ? false : true;
    }

    private static int GetCountOfPerfectCollectiblesLevels()
    {
        int counter = 0;
        PlayerSession gameData = DataPersistenceManager.LoadGameData();
        foreach (PlayerSession.Level level in gameData.levels)
        {
            if (level.collectiblesTaken == level.collectiblesToTake)
            {
                counter++;
            }
        }
        return counter;
    }

    private static int GetCountOfPerfectTimesLevels()
    {
        int counter = 0;
        PlayerSession gameData = DataPersistenceManager.LoadGameData();
        foreach (PlayerSession.Level level in gameData.levels)
        {
            if (level.playTime <= level.recordTime)
            {
                counter++;
            }
        }
        return counter;
    }

    private static int GetCountOfSecretAreasUnlocked()
    {
        int counter = 0;
        PlayerSession gameData = DataPersistenceManager.LoadGameData();
        foreach (PlayerSession.Level level in gameData.levels)
        {
            counter += level.secretZonesUnlocked;
        }
        return counter;
    }



}


[System.Serializable]
public class Achievement
{
    public string achievementId;
    public enum StepType
    {
        levelCompletion,
        rank,
        time,
        collectible,
        secretArea,
    }

    public string name;             // The first line of the Achievement Pop-Up 
    public string spanishName;
    public string description;      // The second line of the Achievement Pop-Up
    public string spanishDescription;
    public StepType stepType;        // What type of step triggers this Achievement?
    public int stepCount;           // And how many of that thing do you need?  
    [SerializeField]
    private bool complete = false; // Has the player completed this Achievement

    public bool Complete
    {
        set
        {
            this.complete = value;
        }

        get
        {
            return this.complete;
        }
    }
}

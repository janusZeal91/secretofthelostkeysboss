using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class DataPersistenceManager
{

    private static string gameDataSavePath;
    private static string levelDataSavePath;


    static DataPersistenceManager()
    {

        InitSavePaths();
    }

    public static bool HasAchievedPerfectGame()
    {
        PlayerSession playerSession = LoadGameData();
        foreach (PlayerSession.Level level in playerSession.levels)
        {
            if (level.rank != 1)
            {
                return false;
            }
        }
        return true;
    }

    private static void InitSavePaths()
    {
        gameDataSavePath = Path.Combine(Application.persistentDataPath, "SOTLKgamedata.json");
        levelDataSavePath = Path.Combine(Application.persistentDataPath, "levelresults.json");

    }

    private static void CreateDirectoryIfNeeded(string dataPath)
    {
        try
        {
            if (dataPath != "data")
            {
                if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dataPath));
                }
            }
        }
        catch
        {
            Debug.LogWarning("No se pudo crear el directorio");
        }
    }

    public static bool ExistsLevel(int levelNumber)
    {
        try
        {
            List<PlayerSession.Level> levels = LoadGameData().levels;
            PlayerSession.Level level = levels.Find(lvl => lvl.levelNumber == levelNumber);
            return level == null ? false : true;
        }
        catch (System.NullReferenceException)
        {
            return false;
        }
    }

    public static PlayerSession.Level LoadLevelInfo()
    {
        if (File.Exists(levelDataSavePath))
        {
            string jsonFile = File.ReadAllText(levelDataSavePath);
            PlayerSession.Level level = JsonUtility.FromJson<PlayerSession.Level>(jsonFile);
            return level;
        }
        else
        {
            Debug.LogError("There is not any level data");
            return null;
        }
    }

    public static void SaveLevelInfo(PlayerSession.Level level)
    {
        try
        {
            string jsonLevelFile = JsonUtility.ToJson(level);
            File.WriteAllText(levelDataSavePath, jsonLevelFile);
        }
        catch
        {
            Debug.Log("An error has ocurred saving game Data");
        }
    }

    public static PlayerSession LoadGameData()
    {
        if (File.Exists(gameDataSavePath))
        {
            string jsonFile = File.ReadAllText(gameDataSavePath);
            PlayerSession playerSession = JsonUtility.FromJson<PlayerSession>(jsonFile);
            return playerSession;
        }
        else
        {
            Debug.LogWarning("File data does not exist");
            return null;
        }
    }

    public static int GetSavedRankOfLevel(int levelNumber)
    {
        PlayerSession savedSession = LoadGameData();
        if (savedSession != null)
        {
            foreach (PlayerSession.Level level in savedSession.levels)
            {
                if (level.levelNumber == levelNumber)
                {
                    return level.rank;
                }
            }
            return 5;
        }
        else
        {
            return 5;
        }
    }

    public static void SavePlayerData(int lives, int score, int scoreToExtraLife)
    {
        if (File.Exists(gameDataSavePath))
        {
            PlayerSession savedData = LoadGameData();
            savedData.lives = lives;
            savedData.score = score;
            savedData.scoreToExtraLife = scoreToExtraLife;
            string jsonSavedFile = JsonUtility.ToJson(savedData);
            File.WriteAllText(gameDataSavePath, jsonSavedFile);
        }
        else
        {
            PlayerSession playerSession = new PlayerSession()
            {
                lives = lives,
                score = score,
                scoreToExtraLife = scoreToExtraLife            
            };
            string jsonSavedFile = JsonUtility.ToJson(playerSession);
            File.WriteAllText(gameDataSavePath, jsonSavedFile);
        }

    }

    public static void SaveGameData(PlayerSession.Level currentLevel)
    {
        if (File.Exists(gameDataSavePath))
        {
            PlayerSession savedData = LoadGameData();          
            OverwriteSavedData(savedData, currentLevel);
        }
        else
        {
            PlayerSession newSaveFile = new PlayerSession();
            newSaveFile.levels.Add(currentLevel);
            newSaveFile.hasBeatedGame = currentLevel.levelNumber == 16 ? true : false;
            newSaveFile.lives = PlayerPrefManager.GetLives();
            newSaveFile.score = PlayerPrefManager.GetScore();
            newSaveFile.scoreToExtraLife = PlayerPrefManager.GetScoreToNextLife();
            newSaveFile.powerUps = GetPowerUpsInfo();
            string jsonSavedFile = JsonUtility.ToJson(newSaveFile);
            File.WriteAllText(gameDataSavePath, jsonSavedFile);
            Debug.Log("New level saved!");
        }
        SaveAchievements(AchievementManager.achievementManager_S.GetCompletedAchievements());
    }

    public static void DeleteSaveFile()
    {
        try
        {
            File.Delete(gameDataSavePath);
            File.Delete(levelDataSavePath);
            Debug.Log("Saved file deleted");
        }
        catch (System.Exception)
        {
            Debug.LogError("Cannot delete file...");
        }
    }

    public static void SaveAchievements(Achievement[] achivements)
    {
        if (File.Exists(gameDataSavePath))
        {
            PlayerSession savedFile = LoadGameData();
            savedFile.achievements = achivements;
            string jsonSaveFile = JsonUtility.ToJson(savedFile);
            File.WriteAllText(gameDataSavePath, jsonSaveFile);
            Debug.Log("Achievements saved");

        }
        else
        {
            Debug.LogWarning("Can't save due to a missing previous save file");
        }
    }



    private static void OverwriteSavedData(PlayerSession savedData, PlayerSession.Level currentLevel)
    {
        List<PlayerSession.Level> levels = savedData.levels;
        List<PlayerSession.Level> levelsToIgnore = new List<PlayerSession.Level>();
        foreach (PlayerSession.Level level in levels)
        {
            if (level.levelNumber != currentLevel.levelNumber)
            {
                levelsToIgnore.Add(level);
            }
        }
        levels = levelsToIgnore;
        levels.Add(currentLevel);
        savedData.levels = levels;
        savedData.lives = PlayerPrefManager.GetLives();
        savedData.score = PlayerPrefManager.GetScore();
        savedData.hasBeatedGame = currentLevel.levelNumber == 16;
        savedData.scoreToExtraLife = PlayerPrefManager.GetScoreToNextLife();
        savedData.powerUps = GetPowerUpsInfo();
        string jsonSavedFile = JsonUtility.ToJson(savedData);
        File.WriteAllText(gameDataSavePath, jsonSavedFile);
        Debug.Log("Data overriden and saved!");
    }

    private static PlayerSession.PowerUps GetPowerUpsInfo()
    {
        PlayerSession.PowerUps powerUps = new PlayerSession.PowerUps();

        if (PlayerPrefManager.GetDoubleJumpState() == 1)
        {
            powerUps.doubleJump = true;
        }
        if (PlayerPrefManager.GetFireMagicState() == 1)
        {
            powerUps.fireMagic = true;
        }
        if (PlayerPrefManager.GetIceMagicState() == 1)
        {
            powerUps.iceMagic = true;
        }
        return powerUps;
    }


}

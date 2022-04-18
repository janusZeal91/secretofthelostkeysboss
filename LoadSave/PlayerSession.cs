using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSession {

    public List<Level> levels = new List<Level>();
    public PowerUps powerUps = new PowerUps();
    public float completionRate = 0f;
    public int lives = 0;
    public int score = 0;
    public int scoreToExtraLife = 2000;
    public bool hasBeatedGame = false;
    public Achievement[] achievements;
   
        


    [System.Serializable]
    public class Level
    {
        public int levelNumber = 0;
        public string levelName = "";
        public string levelSpanishName = "";
        public float playTime = 0.0f;
        public float recordTime = 0.0f;
        public int collectiblesTaken = 0;
        public int collectiblesToTake = 0;
        public int secretZonesUnlocked = 0;
        public int secretZonesToUnlock = 0;
        public int finalScoreToNextLife = 2000;
        /*Rank will be transformed from int to string based on following table 
         * 5 - D rank
         * 4 - C rank
         * 3 - B rank
         * 2 - A Rank 
         * 1 - S rank*/
        public int rank = 5;

        public Level(int levelNumber, string levelName, string spanishlevelName ,float playTime, float recordTime, int collectiblesTaken,  int collectiblesToTake,
            int secretZonesUnlocked , int secretZonesToUnlock, int rank)
        {
            this.levelNumber = levelNumber;
            this.levelName = levelName;
            this.levelSpanishName = spanishlevelName;
            this.playTime = playTime;
            this.recordTime = recordTime;
            this.collectiblesTaken = collectiblesTaken;
            this.collectiblesToTake = collectiblesToTake;
            this.secretZonesUnlocked = secretZonesUnlocked;
            this.secretZonesToUnlock = secretZonesToUnlock;
            this.rank = rank;

        }
    }

    [System.Serializable]
    public class PowerUps
    {
        public bool doubleJump = false;
        public bool fireMagic = false;
        public bool iceMagic = false;
        
    }

    

}

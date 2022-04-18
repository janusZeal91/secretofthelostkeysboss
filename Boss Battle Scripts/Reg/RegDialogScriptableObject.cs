using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/RegDialogSO", fileName = "RegDialogSO.asset")]
[System.Serializable]
public class RegDialogScriptableObject : ScriptableObject {

    public Dialog[] startDialogs;
    public Dialog[] endDialogs;
    [System.Serializable]
    public struct Dialog
    {
        public bool isGoodDialog;
        public int stopLineAt;
        public TextAsset dialogAsset;
        public enum Language {SPANISH, ENGLISH};
        public Language dialogLanguage;
    }

    private enum FinalGameRanking { GOOD, BAD};
    private FinalGameRanking currentGameRanking = FinalGameRanking.BAD;

    private void Start()
    {
        this.currentGameRanking = GetFinalRanking();

    }

    private Dialog GetDialogBasedOnLanguage(Dialog[] dialogs, Dialog.Language language, bool isGoodDialog)
    {
        foreach(Dialog dialog in dialogs)
        {
            if (dialog.dialogLanguage == language && dialog.isGoodDialog == isGoodDialog)
            {
                return dialog;
            }
        }
        return dialogs[0];
    }
    public Dialog GetStartDialog(Dialog.Language language)
    {
        this.Start();
        return currentGameRanking == FinalGameRanking.BAD ? 
            GetDialogBasedOnLanguage(startDialogs, language, false) : 
            GetDialogBasedOnLanguage(startDialogs, language, true);
    }

    public Dialog GetEndDialog(Dialog.Language language)
    {
        this.Start();
        return currentGameRanking == FinalGameRanking.BAD ?
            GetDialogBasedOnLanguage(endDialogs, language, false) :
            GetDialogBasedOnLanguage(endDialogs, language, true);
    }

    private FinalGameRanking GetFinalRanking()
    {
        PlayerSession playerSession = DataPersistenceManager.LoadGameData();
        foreach(PlayerSession.Level level in playerSession.levels)
        {
            if(level.rank != 1 && level.levelNumber!=16)
            {
                return FinalGameRanking.BAD;
            }
        }
        return FinalGameRanking.GOOD;
    }
	
}

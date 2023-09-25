using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManagerScript : MonoBehaviour {

    public static GameManagerScript instance;
    public static Lang LMan;
    public string currentLanguage = "English";

    //States of the Battle
    [HideInInspector]
    public enum GameStates
    {
        MAINMENU,
        PAUSED,
        EXPLORATION,
        BATTLE
    }
    public GameStates currentState;
    public GameStates previousState;

    void Awake()
    {
        MakeSingleton();
    }
    // Use this for initialization
    void Start () {
        LMan = new Lang(Application.dataPath + "\\Scripts\\XML\\languages.xml", currentLanguage, false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void MakeSingleton()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetExplorationState()
    {
        previousState = currentState;
        currentState = GameStates.EXPLORATION;
        ExplorerManagerScript.instance.ResumeExploration();

    }

    public void SetBattleState()
    {
        previousState = currentState;
        currentState = GameStates.BATTLE;
    }
}

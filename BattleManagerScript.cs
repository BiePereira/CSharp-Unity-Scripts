using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq.Expressions;
using UnityEngine.SceneManagement;

public class BattleManagerScript : MonoBehaviour
{

    /*-------------------------------------------- Variables ----------------------------------------------*/
    public static BattleManagerScript instance;

    [Header("DEV Mode")]
    public bool developerMode = true;
    private static bool devMode;
        
    //Booleans which helps the renderization and presence of certain characters on the party
    //TODO they shall be inicialized in a function
    [Header("Party Members Active")]
    public bool Kari = true;
    public bool Winter = false;
    public bool Aayala = false;
    public bool Evie = false;
    public bool Malbor = false;

    [Header("Enemies Active")]
    [Tooltip("Number of enemies")]
    public int enemies = 1;

    //States of the Battle
    [HideInInspector]
    public enum BattleStates
    {
        START,
        DIALOGUE,
        PLAYERCHOICE,
        ENDTURNCONFIRMATION,
        ENEMYCHOICE,
        LOSE,
        WIN
    }
    [HideInInspector] public static BattleStates currentState;
    [HideInInspector] public static BattleStates previousState;

    //Arraylist of turns
    [HideInInspector] public List<GameObject> allFighters = new List<GameObject>();
    //Arraylist of allies and enemies disposed in the board and ordered by their positions
    [HideInInspector] public List<GameObject> allPositions = new List<GameObject>();
    //Arraylist of party (portraits)
    [HideInInspector] public List<GameObject> allPortraits = new List<GameObject>();

    [Header("Arena Objects")]
    private AudioSource backTrack;
    [Tooltip("The GameObject where the sprite of the background will be shown")]
    public GameObject background;
    private Sprite sprite;
    private BattlegroundClass arena;

    [Header("End Battle Panel")]
    [Tooltip("The GameObject parent of all End Game Objects")]
    public GameObject endPanel;
    public GameObject endPanelStatic; //TODO this will be removed after the non static reformulation

    private int actionNumber = 0;

    /* -------------------------------- MonoBehaviour Inherited Functions ---------------------------------*/

    /*On awake it's calculated
	- the order of the combat
	- the way it started (preemptive, surprise, etc)
	- so it's created the disposition of the characters and enemies
	*/
    void Awake()
    {
        instance = this;
        currentState = BattleStates.START;
        ActivateCharacters();
    }

    void Start()
    {
        devMode = developerMode;
        endPanelStatic = endPanel;
        Debug.Log("DEVMODE: " + devMode);
        backTrack = GetComponent<AudioSource>();
        //backTrack.Play();

        arena = BattleLocalizationScript.GetArena();
        SetAmbient();

        OrderList();
        TimelineManagerScript.ArrangeTimeline();
    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log(currentState);
    }

    /* -------------------------------------- Customized Functions ----------------------------------------*/

    /* This function order the list of turns for the FIRST time
	 * based in some criteria
	*/
    private void OrderList()
    {


        //TODO organization of list based on agility and the way that the battle started
        /*for(int i = 0; i < allFighters.Count; i++)
        {
            Debug.Log("1st name: " + allFighters[i].GetComponent<CharEntityScript>().name);
            Debug.Log("1st agility: " + allFighters[i].GetComponent<CharEntityScript>().agility);
            Debug.Log("1st totalAgility: " + allFighters[i].GetComponent<CharEntityScript>().totalAgility);
        }*/

        //MAYBE THIS IS THE SOLUTION?
        allFighters.Sort(CompareFightersByAgility);

        /*for (int i = 0; i < allFighters.Count; i++)
        {
            Debug.Log("2nd name: " + allFighters[i].GetComponent<CharEntityScript>().name);
            Debug.Log("2nd agility: " + allFighters[i].GetComponent<CharEntityScript>().agility);
            Debug.Log("2nd totalAgility: " + allFighters[i].GetComponent<CharEntityScript>().totalAgility);
        }*/
        //allFighters.Sort((go1, go2) => - go1.GetComponent<CharEntityScript>().agility.CompareTo(go2.GetComponent<CharEntityScript>().agility));

        //allPositions = allFighters; //referencing
        allPositions = new List<GameObject>(allFighters); //creating a copy
        allPositions.Sort(CompareFightersByPosition);

        /*for (int i = 0; i < allPositions.Count; i++)
        {
            Debug.Log("Name: " + allPositions[i].GetComponent<CharEntityScript>().name);
            Debug.Log("Position: " + allPositions[i].GetComponent<CharEntityScript>().boardPosition);
        }*/

        NextBattleState();
    }

    /* Sorting Function to compare the fighters agility and order the List by Agility
     * */
    private static int CompareFightersByAgility(GameObject a1, GameObject a2)
    {
        int a1Agi = a1.GetComponent<CharEntityScript>().totalAgility;
        int a2Agi = a2.GetComponent<CharEntityScript>().totalAgility;

        //if (a1Agi != a2Agi)
        return -a1Agi.CompareTo(a2Agi);

        /*else
        {
            int luckLv1 = Random.Range(0, a1.GetComponent<CharEntityScript>().luck * 10);
            int luckLv2 = Random.Range(0, a2.GetComponent<CharEntityScript>().luck * 10);

            return luckLv1.CompareTo(luckLv2);
        }*/
    }

    private int CompareFightersByPosition(GameObject a1, GameObject a2)
    {
        int pos1 = a1.GetComponent<CharEntityScript>().boardPosition;
        int pos2 = a2.GetComponent<CharEntityScript>().boardPosition;

        return pos1.CompareTo(pos2);
    }

    /*
     * This function reorder the list so the first one becomes the last and everyone else goes one "step" ahead
     */
    private void ReOrderList()
    {
        int aux;
        GameObject firstTemp = allFighters[0];
        for (int i = 0; i < allFighters.Count - 1; i++)
        {
            aux = i + 1;
            allFighters[i] = allFighters[aux];
        }
        aux = allFighters.Count - 1;
        allFighters[aux] = firstTemp;
    }

    /* Where the characters are activated. This exists because in some battle there will be some allies
	 * which will not be available in the moment, like in the beggining of the game and some other
	 * quests during the progression.
     * Also, this populate the list with all available characters that have their turns to battle.
     * 
     * And, of course, the list will be populated with enemies aswell
	*/
    private void ActivateCharacters()
    {
        GameObject auxChar;
        GameObject auxPort;

        //Kari
        auxChar = GameObject.Find("Kari Noctua");
        auxPort = GameObject.Find("Kari - Portrait");
        allPortraits.Add(auxPort);
        if (Kari)
        {
            allFighters.Add(auxChar);
            auxChar.SetActive(true);

            //allPortraits.Insert(0, auxPort);
            auxPort.SetActive(true);

            GameObject.Find("Kari UI").SetActive(true);
        }
        else
        {
            auxChar.SetActive(false);
            auxPort.SetActive(false);
            GameObject.Find("Kari UI").SetActive(false);

        }

        //Winter
        auxChar = GameObject.Find("Winter Wayne"); //The Char itself
        auxPort = GameObject.Find("Winter - Portrait"); //The Portrait
        allPortraits.Add(auxPort);
        if (Winter)
        {
            allFighters.Add(auxChar);
            auxChar.SetActive(true);

            //allPortraits.Insert(1, auxPort);
            auxPort.SetActive(true);
            GameObject.Find("Winter UI").SetActive(true);
            //GameObject.Find("Winter - Health Bar").SetActive(true);
        }
        else
        {
            auxChar.SetActive(false);
            auxPort.SetActive(false);
            GameObject.Find("Winter UI").SetActive(false);
            //GameObject.Find("Winter - Health Bar").SetActive(false);

        }
        /*
        //Aayala
        auxChar = GameObject.Find("Aayala Stormclaws");
        auxPort = GameObject.Find("Aayala - Portrait");
        if (Aayala)
        {
            allTurns.Add(auxChar);
            auxChar.SetActive(true);

            //allPortraits.Insert(2, auxPort);
            auxPort.SetActive(true);

            GameObject.Find("Aayala - Health Bar").SetActive(true);
        }
        else
        {
            auxChar.SetActive(false);
            auxPort.SetActive(false);
            GameObject.Find("Aayala - Health Bar").SetActive(false);

        }

        //Evie
        auxChar = GameObject.Find("Evie Eileen");
        auxPort = GameObject.Find("Evie - Portrait");
        if (Evie)
        {
            allTurns.Add(auxChar);
            auxChar.SetActive(true);

            //allPortraits.Insert(3, auxPort);
            auxPort.SetActive(true);

            GameObject.Find("Evie - Health Bar").SetActive(true);
        }
        else
        {
            auxChar.SetActive(false);
            auxPort.SetActive(false);
            GameObject.Find("Evie - Health Bar").SetActive(false);

        }

        //Malbor
        auxChar = GameObject.Find("Malbor Na'ill");
        auxPort = GameObject.Find("Malbor - Portrait");
        if (Malbor)
        {
            allTurns.Add(auxChar);
            auxChar.SetActive(true);

            //allPortraits.Insert(4, auxPort);
            auxPort.SetActive(true);

            GameObject.Find("Malbor - Health Bar").SetActive(true);
        }
        else
        {
            auxChar.SetActive(false);
            auxPort.SetActive(false);
            GameObject.Find("Malbor - Health Bar").SetActive(false);

        }
        */

        //TODO list of enemies with portraits and stuff... and activate (or not) them
        //Enemies
        for (int i = 1; i <= enemies; i++)
        {
            allFighters.Add(GameObject.Find("Enemy " + i));
        }

        Debug.Log("LIST OF TURNS - TOTAL: " + allFighters.Count);

    }

    public string GetTurnName()
    {
        return allFighters[0].name;
    }

    public GameObject GetTurnObject()
    {
        return allFighters[0];
    }

    public void NextBattleState()
    {
        if (allFighters[0].CompareTag("PartyMember"))
        {
            SetPlayerChoiceState();
            
        }
        else
        {
            SetEnemyChoiceState();
        }
    }

    /* It's called from the Selection Box OR from the endo of the enemy turn
     * - Gives the information the Matrix need
     * - Rearrange the timeline
	 * TODO In the future will have more structures here
     * TODO Should be called from enemy turn
	 */
    public void NextTurn()
    {
        if (IsEnemiesAlive()){
            //Keeps reordening the fighters list until the owner of the next turn token is alive
            do
            {
                ReOrderList();
            } while (!allFighters[0].GetComponent<CharEntityScript>().alive);

            TimelineManagerScript.ArrangeTimeline();

            allFighters[0].GetComponent<CharEntityScript>().ChargeNewTurnAP();

            NextBattleState();

            ActionBoxManagerScript.ResetActions();
        }
        else
        {
            WinnerPanel();
            StartCoroutine(UnloadBattle());
        }
        
    }

    /* It's called at every action performed
     * It's a function responsible for the logic of attacks, buffs, debuffs, heals, fixes, etc...
     * TODO divide it into many functions, one for Attack (like it is right now), one for buff, one for heal, etc.
     * */
    public string StartAction(CharEntityScript actioner, CharEntityScript target, AttackClass skill)
    {
        //Debug.Log(actioner.name + " agiu e gastou " + skill.apCost + "...");
        //Debug.Log(actioner.name + " tinha " + actioner.currentAP + "...");
        actioner.currentAP = actioner.currentAP - skill.apCost;
        //Debug.Log(actioner.name + " agora tem " + actioner.currentAP + ".");

        string message = "";
        actionNumber++;
        //Debug.Log("TURNO Nº " + actionNumber);
        //Debug.Log("Turno: " + actioner.name + " | Alvo: " + target.name);

        //SKILL USED HAVE PHYSICAL DAMAGE TO TARGET'S HEALTH
        if (skill.physicalDamage > 0)
        {      
            int criticalRandom = Random.Range(0, 100);
            float criticalCriteria = skill.criticalChance * 1.25f;
            //IF CRITICAL
            if (criticalCriteria > criticalRandom){
                float damage = (actioner.totalPhAttack / 1.05f) + skill.physicalDamage;
                damage = (damage * 1.125f) / 2;
                int totalDamage = 0;
                criticalRandom = Random.Range(0, 127);
                damage = (damage / 1.5f) - (((damage / 1.6f) * criticalRandom) / 256);
                totalDamage = Mathf.RoundToInt(damage);
                target.SubHP(totalDamage);

                message = skill.name + " de " + GameManagerScript.LMan.getString(actioner.name) + " infringiu "
                    + totalDamage + " de dano físico CRÍTICO à " + GameManagerScript.LMan.getString(target.name);
            }
            else{
                //DODGE
                //TODO is the attack missable?
                //target's dodge chance
                int dodgeChance = target.totalEvasion + 19;
                dodgeChance = dodgeChance - actioner.totalAccuracy;
                if (dodgeChance < 20)
                {
                    dodgeChance = 20;
                }
                else if (dodgeChance > 49)
                {
                    dodgeChance = 49;
                }
                int randomDodge = Random.Range(0, 100);
                int totalDodge = randomDodge - dodgeChance;

                //if the target evaded
                if (totalDodge <= 0)
                {
                    message = GameManagerScript.LMan.getString(target.name) + " desviou do ataque de "
                        + GameManagerScript.LMan.getString(actioner.name);
                }
                //if the target didn't evaded
                else
                {
                    //Debug.Log("O ataque " + skill.name + " de " + actioner.name + " tem " + skill.physicalDamage + " de dano.");
                    float damage = 0;
                    int totalDamage = 0;
                    float defense = 0;
                    int random;
                    bool glanced = false;

                    message = skill.name + " de " + GameManagerScript.LMan.getString(actioner.name) + " infringiu ";

                    for (int i = 0; i < skill.numberOfHits; i++)
                    {
                        Debug.Log(actioner.name + " hit " + i + ": ");
                        damage = (actioner.totalPhAttack / 1.05f) + skill.physicalDamage;
                        Debug.Log("Attacker Damage: " + actioner.totalPhAttack);
                        Debug.Log("Damage Initial: " + damage);
                        damage = (damage * 1.125f) / 2;

                        defense = target.totalPhDefense / 4f;
                        Debug.Log("Defense: " + defense);

                        random = Random.Range(0, 255);
                        Debug.Log("Random Number: " + random);

                        damage = (damage - defense + (((damage - defense + 1) * random) / 256)) / 2;
                        Debug.Log("Damage Later: " + damage);

                        totalDamage = Mathf.RoundToInt(damage);
                        //Debug.Log("Hit " + i + ": " + totalDamage + " de dano.");


                        randomDodge = Random.Range(0, 100);
                        totalDodge = randomDodge - dodgeChance;
                        //IF GLANCED
                        if (totalDodge <= 8)
                        {
                            totalDamage = Mathf.FloorToInt(damage / 1.25f);
                            glanced = true;
                        }

                        //Security for a non negative damage
                        if(totalDamage < 1)
                        {
                            totalDamage = 1;
                        }

                        target.SubHP(totalDamage);

                        //MESSAGE
                        if(skill.numberOfHits == 1)
                        {
                            message = message + totalDamage;
                        }
                        else if (i == 0)
                        {
                            message = message + "(" + totalDamage;
                        }
                        else
                        {
                            message = message + "+" + totalDamage;
                        }

                        if (glanced)
                        {
                            message = message + " [arranhão]";
                        }

                        if (i == skill.numberOfHits - 1 && skill.numberOfHits != 1)
                        {
                            message = message + ")";
                        }

                        glanced = false;                     

                    }
                    message = message + " de dano físico à " + GameManagerScript.LMan.getString(target.name);
                }
            }
        }

        Debug.Log("totalHP = " + target.currentHP + " and DevMode is " + devMode);
        if(target.currentHP < 1 && !devMode)
        {
            Debug.Log("MORREU remove no battle");
            allFighters.Remove(target.gameObject);
            allPositions.Remove(target.gameObject);
            target.gameObject.GetComponent<CharBattleUIScript>().GetDisabled();
        }

        return message;

    }

    public static void SetPlayerChoiceState()
    {
        previousState = currentState;
        currentState = BattleStates.PLAYERCHOICE;
    }

    public static void SetEnemyChoiceState()
    {
        previousState = currentState;
        currentState = BattleStates.ENEMYCHOICE;
    }

    public static void SetPreviousState()
    {
        currentState = previousState;
    }

    private void SetAmbient()
    {
        sprite = arena.sprite;
        background.GetComponent<SpriteRenderer>().sprite = arena.sprite;

        backTrack.clip = arena.themeSong;
        backTrack.loop = true;
        backTrack.Play();
    }

    private bool IsEnemiesAlive()
    {
        bool isEnemiesAlive = false;
        for(int i = 0; i < allFighters.Count; i++)
        {
            if (allFighters[i].GetComponent<CharEntityScript>().alive &&
                allFighters[i].CompareTag("Enemy"))
            {
                isEnemiesAlive = true;
            }
        }
        return isEnemiesAlive;
    }

    private void WinnerPanel()
    {
        endPanelStatic.SetActive(true);
        //endPanelStatic.GetComponent<>
        previousState = currentState;
        currentState = BattleStates.WIN;
    }

    IEnumerator UnloadBattle()
    {
        yield return new WaitForSeconds(2f); //Just to show a little of loading

        AsyncOperation async = SceneManager.UnloadSceneAsync("Aniba_Tower_XX(battle)");

        GameManagerScript.instance.SetExplorationState();

        while (!async.isDone)
        {
            yield return null;
        }
    }
}

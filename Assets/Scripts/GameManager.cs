using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public bool hasServiceStarted;
    
    [Header("Parameters")]
    [SerializeField] Character[] characters;
    [SerializeField] Bottle[] bottles;
    [SerializeField] int[] dialogueStepTriggers;
    [SerializeField] DialogueCategory[] dialogues;
    [SerializeField] int winThreshold;
    private float drankAnimationDuration = 5f;
    
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI transitionTitle;
    public Canvas endCanvas;
    public Canvas mainMenu;
    public Canvas gameCanvas;
    public Canvas pauseCanvas;
    public Slider drankLevel;
    public Slider[] sliders;

    public float test;

    int currentRound = -1;
    Bottle CurrentBottle;
    List<int> drankCharacterIDs;

    private void Start()
    {
        StartCoroutine(StartServiceRoutine());
        drankLevel.interactable = false;
        foreach (Slider slider in sliders) { slider.interactable = false; }
    }

    private void LateUpdate()
    {
        drankLevel.value = Mathf.MoveTowards(drankLevel.value, test, Time.deltaTime / drankAnimationDuration);
    }

    IEnumerator StartServiceRoutine()
    {
        currentRound++;
        drankCharacterIDs = new List<int>();
        ChangeBottle();
        yield return new WaitForSeconds(0.5f);
        transitionTitle.text = "Service";
        hasServiceStarted = true;
    }

    public void ServeCharacter(int indexCharacter)
    {
        characters[indexCharacter].currentDrinkAmount += CurrentBottle.drinkValue;
        characters[indexCharacter].TimeSinceHasDrank = 0;
        sliders[indexCharacter].value = Mathf.MoveTowards(sliders[indexCharacter].value, characters[indexCharacter].currentDrinkAmount, Time.deltaTime / drankAnimationDuration);
        CurrentBottle.servingSize--;
        drankCharacterIDs.Add(indexCharacter);
        
        if (CurrentBottle.servingSize == 0)
            StartCoroutine(EndServiceRoutine());
    }

    // Character dialogues
    IEnumerator EndServiceRoutine()
    {
        for (int i = 0; i < drankCharacterIDs.Count; i++)
        {
            DisplayDialogue(characters[drankCharacterIDs[i]]);
            yield return new WaitForSeconds(1f);
        }
    }

    void DisplayDialogue(Character character)
    {
        
        //Debug.Log();
    }

    public void EndOfTurn()
    {
        int charaCounter = 0;
        foreach (Character chara in characters)
        {
            if (chara.TimeSinceHasDrank >= chara.timeToSober)
            {
                chara.currentDrinkAmount -= chara.soberUpMultiplier;
                sliders[charaCounter].value = Mathf.MoveTowards(sliders[charaCounter].value, characters[charaCounter].currentDrinkAmount, Time.deltaTime / drankAnimationDuration);
            }
            else if (chara.currentDrinkAmount >= chara.endDrinkTreshold) { GameIsLost(); }

            chara.TimeSinceHasDrank += 1;
            charaCounter++;
        }
    }

    void ChangeBottle()
    {
        CurrentBottle = bottles[currentRound];
        // Animation
    }

    public void GameIsLost()
    {
        endCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        mainMenu.gameObject.SetActive(false);
        gameCanvas.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        pauseCanvas.gameObject.SetActive(false);
        endCanvas.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
        //reset les valeurs / recharger la scene
    }

    public void PauseGame()
    {
        gameCanvas.gameObject.SetActive(!gameCanvas.gameObject.activeSelf);
        pauseCanvas.gameObject.SetActive(!pauseCanvas.gameObject.activeSelf);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
    
    public bool CanServeCharacter(int characterID)
    {
        return !drankCharacterIDs.Contains(characterID);
    }
    
    [ContextMenu("SetDialogues")]
    void SetDialogues()
    {
        TextAsset rawDialogues = Resources.Load<TextAsset>("Dialogues");
        string[] data = rawDialogues.text.Split(new [] { "\n" }, StringSplitOptions.None);
        List<DialogueCategory> newDialogueList = new();

        foreach (string line in data)
        {
            string[] col = line.Split(new [] { "," }, StringSplitOptions.None);
            if (int.TryParse(col[0], out int drinkLevel))
            {
                if (drinkLevel > newDialogueList.Count) newDialogueList.Add(new DialogueCategory());
                newDialogueList[drinkLevel-1].textList.Add(col[1].Replace("\"", ""));
            }
        }
        
        dialogues = newDialogueList.ToArray();
        
        /*
       List<DialogueData> newDialogues = new List<DialogueData>();

       foreach (string line in data)
       {
           string[] col = line.Split(new [] { "," }, StringSplitOptions.None);
           if (int.TryParse(col[0], out int drink))
               newDialogues.Add(new DialogueData(drink, col[1].Replace("\"", "")));
           else
               newDialogues.Add(new DialogueData(-1, col[1].Replace("\"", "")));
       }

       dialogues = newDialogues.ToArray();
       */
    }
}

[System.Serializable]
public class Character
{
    [HideInInspector] public float currentDrinkAmount;
    [HideInInspector] public int TimeSinceHasDrank;
    public int endDrinkThreshold;
    public float soberUpMultiplier;
    public int timeToSober;
}

[System.Serializable]
public struct Bottle
{
    public  int drinkValue;
    public int servingSize;
    private Sprite visual;
}

[System.Serializable]
public class DialogueCategory
{
    public List<string> textList = new ();
}

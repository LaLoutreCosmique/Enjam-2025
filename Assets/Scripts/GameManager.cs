using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
    
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI transitionTitle;

    int currentRound = -1;
    Bottle CurrentBottle;
    List<int> drankCharacterIDs;

    private void Start()
    {
        StartCoroutine(StartServiceRoutine());
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
        foreach (Character chara in characters)
        {
            if (chara.TimeSinceHasDrank >= chara.timeToSober)
            {
                chara.currentDrinkAmount -= chara.soberUpMultiplier;
            }

            chara.TimeSinceHasDrank += 1;
        }
    }

    void ChangeBottle()
    {
        CurrentBottle = bottles[currentRound];
        // Animation
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

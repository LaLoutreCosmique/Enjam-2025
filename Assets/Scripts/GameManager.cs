using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] Character[] characters;
    [SerializeField] Bottles[] bottles;
    [SerializeField] DialogueData[] dialogues;
    private int winTreshold;

    public bool gameIsLost;
    public Bottles currentBottle;

    [System.Serializable]
    public class Character
    {
        [HideInInspector]
        public float currentDrinkAmount;
        public int endDrinkTreshold;
        [HideInInspector]
        public int TimeSinceHasDrank;
        private Dictionary<int, string> dialogueByTreshold;
        public float soberUpMultiplier;
        public int timeToSober;
    }

    [SerializeField]
    public class Bottles
    {
        public  int drinkValue;
        public int servingSize;
        private Sprite visual;
    }


    public void ServeCharacter(int indexCharacter)
    {
        characters[indexCharacter].currentDrinkAmount += currentBottle.drinkValue;
        characters[indexCharacter].TimeSinceHasDrank = 0;
        currentBottle.servingSize -= 1;
        if(currentBottle.servingSize == 0)
        {
            ChangeBottle();
        }
    }

        public void EndOfTurn ()
    {
        foreach (Character chara in characters)
        {
            if (chara.TimeSinceHasDrank >= chara.timeToSober && !gameIsLost)
            {
                chara.currentDrinkAmount -= chara.soberUpMultiplier;
            }
            else if (chara.currentDrinkAmount >= chara.endDrinkTreshold && !gameIsLost) { gameIsLost = true; }

            chara.TimeSinceHasDrank += 1;
        }
    }


    public void ChangeBottle()
    {
        currentBottle = new Bottles();
        //Play animation
    }
    
    [ContextMenu("SetDialogues")]
    void SetDialogues()
    {
        TextAsset rawDialogues = Resources.Load<TextAsset>("Dialogues");
        string[] data = rawDialogues.text.Split(new [] { "\n" }, StringSplitOptions.None);
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
    }
}

[System.Serializable]
public struct DialogueData
{
    public int drinkAmount;
    public string text;

    public DialogueData(int drinkAmount, string text)
    {
        this.drinkAmount = drinkAmount;
        this.text = text;
    }
}

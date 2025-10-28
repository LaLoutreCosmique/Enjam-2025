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

    [System.Serializable]
    public class Character
    {
        [HideInInspector]
        public int currentDrinkAmount;
        public int endDrinkTreshold;
        [HideInInspector]
        public bool hasDrankThisRound;
        private Dictionary<int, string> dialogueByTreshold;
        public float soberUpMultiplier;
    }

    [SerializeField]
    public class Bottles
    {
        public  int drinkValue;
        public int servingSize;
        private Sprite visual;
    }


    public void ServeCharacter(int indexCharacter, int indexBottle)
    {
        characters[indexCharacter].currentDrinkAmount += bottles[indexBottle].drinkValue;
        characters[indexCharacter].hasDrankThisRound = true;
        bottles[indexBottle].servingSize -= 1;
        if(bottles[indexBottle].servingSize == 0)
        {

        }
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

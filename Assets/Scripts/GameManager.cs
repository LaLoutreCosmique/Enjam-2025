using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] Character[] characters;
    [SerializeField] Bottles[] bottles;
    [SerializeField] DialogueData[] dialogues;
    private int winTreshold;
    [HideInInspector]
    public Bottles currentBottle;
    private float drankAnimationDuration = 5f;

    [Header("Réfécences")]
    public Canvas endCanvas;
    public Canvas mainMenu;
    public Canvas gameCanvas;
    public Slider drankLevel;

    public float test;

    private void LateUpdate()
    {
        drankLevel.value = Mathf.MoveTowards(drankLevel.value, test, Time.deltaTime / drankAnimationDuration);
    }

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
            if (chara.TimeSinceHasDrank >= chara.timeToSober)
            {
                chara.currentDrinkAmount -= chara.soberUpMultiplier;
            }
            else if (chara.currentDrinkAmount >= chara.endDrinkTreshold) { GameIsLost(); }

            chara.TimeSinceHasDrank += 1;
        }
    }


    public void ChangeBottle()
    {
        currentBottle = new Bottles();
        //Play animation
    }

    public void GameIsLost()
    {
        endCanvas.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        mainMenu.gameObject.SetActive(false);
        gameCanvas.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        endCanvas.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
        //reset les valeurs / recharger la scene
    }

    public void CloseGame()
    {
        Application.Quit();
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

using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField] Character[] characters;
    [SerializeField] Bottles[] bottles;
    private int winTreshold;

    public bool gameIsLost;
    public Bottles currentBottle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

}

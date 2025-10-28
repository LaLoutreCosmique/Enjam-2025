using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField] Character[] characters;
    [SerializeField] Bottles[] bottles;
    private int winTreshold;
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

}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    public GameObject[] charactersOnTheRight;

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
        int sliderIndex = 0;
        foreach (Slider slider in sliders)
        {
            slider.value = Mathf.MoveTowards(slider.value, characters[sliderIndex].currentDrinkAmount/10, Time.deltaTime / drankAnimationDuration);
            sliderIndex++;
        }
    }

    IEnumerator StartServiceRoutine()
    {
        currentRound++;
        drankCharacterIDs = new List<int>();
        ChangeBottle();
        yield return new WaitForSeconds(0.5f);
        transitionTitle.text = "Service on";
        Debug.Log("--- SERVICE ON ---");
        hasServiceStarted = true;
    }

    public void ServeCharacter(int indexCharacter)
    {
        ToogleCharacter(indexCharacter);
        characters[indexCharacter].currentDrinkAmount += CurrentBottle.drinkValue;
        characters[indexCharacter].TimeSinceHasDrank = 0;
        sliders[indexCharacter].value = Mathf.MoveTowards(sliders[indexCharacter].value, characters[indexCharacter].currentDrinkAmount, Time.deltaTime / drankAnimationDuration);
        CurrentBottle.servingSize--;
        drankCharacterIDs.Add(indexCharacter);
        
        Debug.Log(indexCharacter + "has drank: " + characters[indexCharacter].currentDrinkAmount);
        
        if (CurrentBottle.servingSize == 0)
            StartCoroutine(EndServiceRoutine());
    }

    public void ToogleCharacter(int indexCharacter)
    {
        if (indexCharacter < 2)
        {
            charactersOnTheRight[0].SetActive(true);
            charactersOnTheRight[1].SetActive(true);
            charactersOnTheRight[2].SetActive(false);
            charactersOnTheRight[3].SetActive(false);
        }
        else
        {
            charactersOnTheRight[0].SetActive(false);
            charactersOnTheRight[1].SetActive(false);
            charactersOnTheRight[2].SetActive(true);
            charactersOnTheRight[3].SetActive(true);
        }
        int x = 0;
        foreach (Slider slider in sliders)
        {
            slider.gameObject.SetActive(charactersOnTheRight[x].activeSelf);
            x++;
        }
    }

    // Character dialogues
    IEnumerator EndServiceRoutine()
    {
        transitionTitle.text = "Service off";
        Debug.Log("--- SERVICE OFF ---");

        
        for (int i = 0; i < drankCharacterIDs.Count; i++)
        {
            DisplayDialogue(drankCharacterIDs[i]);
            yield return new WaitForSeconds(1f);
        }
        EndOfTurn();
    }

    void DisplayDialogue(int characterID)
    {
        int characterDrankLevel = Mathf.CeilToInt(characters[characterID].currentDrinkAmount);
        string[] dialogueBag = GetDialoguesByDrankLevel(characterDrankLevel);
        int dialogueIndex = Random.Range(0, dialogueBag.Length);
        
        Debug.Log(dialogueBag[dialogueIndex]);
    }
    
    string[] GetDialoguesByDrankLevel(int characterDrankLevel)
    {
        int selected = 0;
        foreach (int stepTrigger in dialogueStepTriggers)
        {
            if (stepTrigger <= selected) break;
            selected = stepTrigger;
        }

        return dialogues[selected-1].textList.ToArray();
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
            else if (chara.currentDrinkAmount >= chara.endDrinkThreshold)
            {
                GameIsLost();
                return;
            }

            chara.TimeSinceHasDrank += 1;
            charaCounter++;
        }
        
        if (currentRound + 1 >= bottles.Length)
        {
            Debug.Log("WIN GG");
            Debug.Log($"Round : {currentRound} - Total bottles : {bottles.Length}");
            GameIsLost(); // TODO: win condition
            return;
        }
        
        StartCoroutine(StartServiceRoutine());
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
            string[] col = line.Split(new [] { ";" }, StringSplitOptions.None);
            if (int.TryParse(col[0], out int drinkLevel))
            {
                if (drinkLevel > newDialogueList.Count) newDialogueList.Add(new DialogueCategory());
                newDialogueList[drinkLevel-1].textList.Add(col[1].Replace("\"", ""));
            }
        }
        
        dialogues = newDialogueList.ToArray();
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

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] float totalChaos;
    private float drankAnimationDuration = 5f;
    
    
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI transitionTitle;
    public Canvas endCanvas;
    public Canvas mainMenu;
    public Canvas gameCanvas;
    public Canvas pauseCanvas;
    public Slider drankLevel;
    public Slider[] sliders;
    public Slider[] slidersClone;
    public Canvas winCanvas;
    [SerializeField] TextMeshProUGUI glassToFill;
    
    [Header("Dialogue UI References")]
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject dialLeftArrow;
    [SerializeField] GameObject dialRightArrow;
    [SerializeField] GameObject[] dialogueBackgrounds;
    
    [Header("Right screen References")]
    [SerializeField] GameObject upView;
    [SerializeField] GameObject upViewDial;
    [SerializeField] GameObject downView;
    [SerializeField] GameObject downViewDial;
    
    int currentRound = -1;
    Bottle CurrentBottle;
    List<int> drankCharacterIDs;
    string lastDialogue;
    const string GLASS_TO_FILL = "Verres à remplir : ";

    private void Start()
    {
        ClearDialogue();
        StartCoroutine(StartServiceRoutine());
        drankLevel.interactable = false;
        foreach (Slider slider in sliders) { slider.interactable = false; }
        foreach (Slider slider in slidersClone) { slider.interactable = false; }
    }

    private void LateUpdate()
    {
        drankLevel.value = Mathf.MoveTowards(drankLevel.value, totalChaos, Time.deltaTime / 1f);
        int sliderIndex = 0;
        foreach (Slider slider in sliders)
        {
            slider.value = Mathf.MoveTowards(slider.value, characters[sliderIndex].currentDrinkAmount/10, Time.deltaTime / drankAnimationDuration);
            sliderIndex++;
        }
        int sliderCloneIndex = 0;
        foreach (Slider slider in slidersClone) { slider.value = sliders[sliderCloneIndex].value; sliderCloneIndex++; }
    }

    IEnumerator StartServiceRoutine()
    {
        currentRound++;
        drankCharacterIDs = new List<int>();
        ChangeBottle();
        yield return new WaitForSeconds(0.5f);
        transitionTitle.text = "SERVEZ !!";
        Debug.Log("--- SERVICE ON ---");
        hasServiceStarted = true;
    }

    public void ServeCharacter(int indexCharacter)
    {
        ToogleCharacter(indexCharacter);
        characters[indexCharacter].currentDrinkAmount += CurrentBottle.drinkValue;
        totalChaos += CurrentBottle.drinkValue;
        characters[indexCharacter].TimeSinceHasDrank = 0;
        sliders[indexCharacter].value = Mathf.MoveTowards(sliders[indexCharacter].value, characters[indexCharacter].currentDrinkAmount, Time.deltaTime / drankAnimationDuration);
        CurrentBottle.servingSize--;
        drankCharacterIDs.Add(indexCharacter);
        glassToFill.text = GLASS_TO_FILL + CurrentBottle.servingSize;
        
        Debug.Log(indexCharacter + "has drank: " + characters[indexCharacter].currentDrinkAmount);
        
        if (CurrentBottle.servingSize == 0)
            StartCoroutine(EndServiceRoutine());
    }

    void ClearDialogue(bool hideText = true)
    {
        dialRightArrow.SetActive(false);
        dialLeftArrow.SetActive(false);
        foreach (GameObject background in dialogueBackgrounds)
        {
            background.SetActive(false);
        }

        if (hideText)
            dialogueText.gameObject.SetActive(false);
    }

    public void ToogleCharacter(int indexCharacter, bool dial = false)
    {
        ClearDialogue(!dial);
        
        if (indexCharacter < 2)
        {
            if (dial)
            {
                upView.SetActive(false);
                upViewDial.SetActive(true);
                downView.SetActive(false);
                downViewDial.SetActive(false);
            }
            else
            {
                upView.SetActive(true);
                upViewDial.SetActive(false);
                downView.SetActive(false);
                downViewDial.SetActive(false);
            }
        }
        else
        {
            if (dial)
            {
                upView.SetActive(false);
                upViewDial.SetActive(false);
                downView.SetActive(false);
                downViewDial.SetActive(true);
            }
            else
            {
                upView.SetActive(false);
                upViewDial.SetActive(false);
                downView.SetActive(true);
                downViewDial.SetActive(false);
            }
        }
        
        foreach (Slider slider in sliders)
        {
            slider.gameObject.SetActive(indexCharacter % 2 == 0);
        }
    }

    // Character dialogues
    IEnumerator EndServiceRoutine()
    {
        transitionTitle.text = "Attendez...";
        Debug.Log("--- SERVICE OFF ---");
        
        dialogueText.gameObject.SetActive(true);
        
        for (int i = 0; i < drankCharacterIDs.Count; i++)
        {
            DisplayDialogue(drankCharacterIDs[i]);
            yield return new WaitForSeconds(2f);
        }
        EndOfTurn();
    }

    void DisplayDialogue(int characterID)
    {
        int characterDrankLevel = Mathf.CeilToInt(characters[characterID].currentDrinkAmount);
        string[] dialogueBag = GetDialoguesByDrankLevel(characterDrankLevel);

        int dialogueIndex;
        do
        {
            dialogueIndex = Random.Range(0, dialogueBag.Length);
        } while (dialogueBag[dialogueIndex] == lastDialogue);
        
        lastDialogue = dialogueBag[dialogueIndex];
        ToogleCharacter(characterID, true);
        dialogueText.text = dialogueBag[dialogueIndex];
        dialLeftArrow.SetActive(characterID % 2 == 0);
        dialRightArrow.SetActive(characterID % 2 != 0);

        foreach (GameObject background in dialogueBackgrounds)
        {
            background.SetActive(false);
        }
        dialogueBackgrounds[characterID].SetActive(true);
        
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
                totalChaos -= chara.soberUpMultiplier;
                sliders[charaCounter].value = Mathf.MoveTowards(sliders[charaCounter].value, characters[charaCounter].currentDrinkAmount, Time.deltaTime / drankAnimationDuration);
            }
            else if (chara.currentDrinkAmount >= chara.endDrinkThreshold)
            {
                Debug.Log($"CHARACTER {charaCounter} is drunk!!");
                GameIsLost();
                return;
            }
            if(totalChaos >= winThreshold)
            {
                winCanvas.gameObject.SetActive(true);
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
        glassToFill.text = GLASS_TO_FILL + CurrentBottle.servingSize;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

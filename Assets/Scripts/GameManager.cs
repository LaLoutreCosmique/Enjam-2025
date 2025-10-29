using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
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
    public GameObject[] charactersOnTheRight;
    public Slider[] slidersClone;
    public Canvas winCanvas;
    [SerializeField] TextMeshProUGUI glassToFill;
    

    [Header("Sound References")]
    public AudioSource audiosource;
    public AudioClip hoquetHomme1;
    public AudioClip hoquetHomme2;
    public AudioClip hoquetFemme1;
    public AudioClip hoquetFemme2;
    public SoundManagement soundMana;

    [Header("Dialogue UI References")]
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject dialLeftArrow;
    [SerializeField] GameObject dialRightArrow;
    [SerializeField] GameObject dialBackground;

    public float test;

    int currentRound = -1;
    Bottle CurrentBottle;
    List<int> drankCharacterIDs;
    string lastDialogue;
    const string GLASS_TO_FILL = "Verres à remplir : ";

    private void Start()
    {
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
        
        dialRightArrow.SetActive(false);
        dialLeftArrow.SetActive(false);
        dialBackground.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        
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
        soundMana.VerserVin();
        
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
        transitionTitle.text = "Attendez...";
        Debug.Log("--- SERVICE OFF ---");
        
        dialBackground.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        
        for (int i = 0; i < drankCharacterIDs.Count; i++)
        {
            DisplayDialogue(drankCharacterIDs[i]);
            yield return new WaitForSeconds(2f);
        }
        EndOfTurn();
    }

    void hips(int drankLevel)
    {
        float chance = 0f;

        if (drankLevel == 1) chance = 0.25f;
        else if (drankLevel == 2) chance = 0.5f;
        else if (drankLevel == 3) chance = 0.75f;

        if (Random.value < chance)
        {
            int x = Random.Range(0, 4);
            if (x == 0)
            {
                audiosource.clip = hoquetFemme1;
            }
            else if (x == 1) { audiosource.clip = hoquetHomme1; }
            else if (x == 2) { audiosource.clip = hoquetFemme2; }
            else { audiosource.clip = hoquetHomme2; }
            audiosource.Play();
        }
    }

    void DisplayDialogue(int characterID)
    {
        int characterDrankLevel = Mathf.CeilToInt(characters[characterID].currentDrinkAmount);
        string[] dialogueBag = GetDialoguesByDrankLevel(characterDrankLevel);
        hips(characterDrankLevel);


        int dialogueIndex;
        do
        {
            dialogueIndex = Random.Range(0, dialogueBag.Length);
        } while (dialogueBag[dialogueIndex] == lastDialogue);
        
        lastDialogue = dialogueBag[dialogueIndex];
        ToogleCharacter(characterID);
        dialogueText.text = dialogueBag[dialogueIndex];
        dialLeftArrow.SetActive(characterID % 2 == 0);
        dialRightArrow.SetActive(characterID % 2 != 0);
        
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

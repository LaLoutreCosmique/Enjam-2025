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
    public Slider[] slidersClone;
    public Canvas winCanvas;
    [SerializeField] GameObject bottleInfo;
    [SerializeField] TextMeshProUGUI glassToFill;
    [SerializeField] TextMeshProUGUI alcoolRateTxt;
    

    [Header("Sound References")]
    public AudioSource audiosource;
    public AudioClip hoquetHomme1;
    public AudioClip hoquetHomme2;
    public AudioClip hoquetFemme1;
    public AudioClip hoquetFemme2;
    public SoundManagement soundMana;

    [Header("Dialogue UI References")]
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject[] dialogueBackgrounds;
    
    [Header("Right screen References")]
    [SerializeField] GameObject upView;
    [SerializeField] GameObject upViewDial;
    [SerializeField] GameObject downView;
    [SerializeField] GameObject downViewDial;
    
    int currentRound = -1;
    Bottle CurrentBottle;
    List<ClickableCharacter> drankCharacters;
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
            slider.value = Mathf.MoveTowards(slider.value, characters[sliderIndex].currentDrinkAmount / characters[sliderIndex].endDrinkThreshold, Time.deltaTime / drankAnimationDuration);
            sliderIndex++;
        }
        int sliderCloneIndex = 0;
        foreach (Slider slider in slidersClone) { slider.value = sliders[sliderCloneIndex].value; sliderCloneIndex++; }
    }

    IEnumerator StartServiceRoutine()
    {
        currentRound++;
        drankCharacters = new List<ClickableCharacter>();
        ChangeBottle();
        yield return new WaitForSeconds(0.5f);
        transitionTitle.text = "SERVEZ !!";
        Debug.Log("--- SERVICE ON ---");
        hasServiceStarted = true;
    }

    public void ServeCharacter(ClickableCharacter character)
    {
        ToggleCharacter(character.characterIndex);
        CurrentBottle.servingSize--;
        drankCharacters.Add(character);
        glassToFill.text = GLASS_TO_FILL + CurrentBottle.servingSize;
        soundMana.VerserVin();
        
        Debug.Log(character.name + "has drank: " + characters[character.characterIndex].currentDrinkAmount);
        
        if (CurrentBottle.servingSize == 0)
            StartCoroutine(EndServiceRoutine());
    }

    void ClearDialogue(bool hideText = true)
    {
        foreach (GameObject background in dialogueBackgrounds)
        {
            background.SetActive(false);
        }

        if (hideText)
            dialogueText.gameObject.SetActive(false);
    }

    public void ToggleCharacter(int indexCharacter, bool dial = false)
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

        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].gameObject.SetActive(indexCharacter == i);
        }
    }

    // Character dialogues
    IEnumerator EndServiceRoutine()
    {
        transitionTitle.text = "Attendez...";
        Debug.Log("--- SERVICE OFF ---");
        hasServiceStarted = false;
        
        bottleInfo.SetActive(false);
        dialogueText.gameObject.SetActive(true);
        
        for (int i = 0; i < drankCharacters.Count; i++)
        {
            DisplayDialogue(drankCharacters[i].characterIndex);
            characters[drankCharacters[i].characterIndex].currentDrinkAmount += CurrentBottle.drinkValue;
            totalChaos += CurrentBottle.drinkValue;
            characters[drankCharacters[i].characterIndex].TimeSinceHasDrank = 0;
            sliders[drankCharacters[i].characterIndex].value = Mathf.MoveTowards(sliders[drankCharacters[i].characterIndex].value, characters[drankCharacters[i].characterIndex].currentDrinkAmount, Time.deltaTime / drankAnimationDuration);

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
        ToggleCharacter(characterID, true);
        dialogueText.text = dialogueBag[dialogueIndex];

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
            currentRound = 0;
        }

        foreach (ClickableCharacter character in drankCharacters)
        {
            character.Deselect();
        }
        StartCoroutine(StartServiceRoutine());
    }

    void ChangeBottle()
    {
        CurrentBottle = bottles[currentRound];
        glassToFill.text = GLASS_TO_FILL + CurrentBottle.servingSize;
        SetAlcoolRateText();
        // Animation
        bottleInfo.SetActive(true);
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

    private void SetAlcoolRateText()
    {
        string rate = "";

        switch (CurrentBottle.drinkValue)
        {
            case 1:
                rate = "léger";
                break;
            case 2:
                rate = "moyen";
                break;
            case 3:
                rate = "fort";
                break;
            default:
                rate = "très fort";
                break;
        }
        
        alcoolRateTxt.text = "Taux d'alcool : " + rate;
    }
}

[System.Serializable]
public class Character
{
    [HideInInspector] public float currentDrinkAmount;
    [HideInInspector] public int TimeSinceHasDrank;
    public float endDrinkThreshold;
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

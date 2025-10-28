using System;
using System.Collections.Generic;
using UnityEngine;

public static class DiscussionParser
{
    public static DialogueData[] GetDialogues()
    {
        TextAsset rawDialogues = Resources.Load<TextAsset>("Dialogues");
        string[] data = rawDialogues.text.Split(new [] { "\n" }, StringSplitOptions.None);
        List<DialogueData> dialogues = new List<DialogueData>();
        foreach (string line in data)
        {
            string[] col = line.Split(new [] { "," }, StringSplitOptions.None);
            if (int.TryParse(col[0], out int drink))
                dialogues.Add(new DialogueData(drink, col[1].Replace("\"", "")));
            else
                dialogues.Add(new DialogueData(-1, col[1].Replace("\"", "")));
        }
        
        return dialogues.ToArray(); 
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

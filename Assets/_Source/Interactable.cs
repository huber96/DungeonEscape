using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [HideInInspector]
    public string displayName = "DEFAULT";

    public Sprite inventoryImage;
    public string[] synonyms = new string[] { };

    public Dictionary<string, int> commands;
    public Dictionary<int, LineData> answers;

    void Awake()
    {
        displayName = this.gameObject.name;
    }

    public int tryGetInteractionAnswerKey(string[] words)
    {
        int answerKey = -1;

        string[] queryWordsToIgnore = new string[] { "the", "a", "an", "this", "that" };
        bool ignoreThisWord(string word)
        {
            bool ignore = false;
            for (int i = 0; i < queryWordsToIgnore.Length; i++)
            {
                if (word == queryWordsToIgnore[i])
                {
                    ignore = true;
                    break;
                }
            }
            return ignore;
        }

        List<string> verbs = new List<string>();
        for (int i = 0; i < words.Length; i++)
        {
            if (containsName(words[i]) || ignoreThisWord(words[i]))
            {
                continue;
            }
            verbs.Add(words[i].ToLower());
        }

        var query = string.Join(" ", verbs);

        //Debug.Log("__________________");
        //Debug.Log(query);
        //Debug.Log(commands.Count);

        if (!commands.TryGetValue(query, out answerKey))
        {
            answerKey = -1;
        }

        return answerKey;
    }



    public LineData tryGetInteractionAnswer(int answerKey)
    {
        LineData lineData = null;

        if (!answers.TryGetValue(answerKey, out lineData))
        {
            lineData = null;
        }

        return lineData;
    }


    public bool containsName(string inputName)
    {
        int nameCount = synonyms.Length;
        string name = inputName.ToLower();
        bool found = name == this.gameObject.name.ToLower();

        if (!found)
        {
            for (int i = 0; i < nameCount; i++)
            {
                if (name == synonyms[i].ToLower())
                {
                    displayName = synonyms[i];
                    found = true;
                    break;
                }
            }
        }

        return found;
    }
}

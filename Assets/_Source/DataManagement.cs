using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager
{
    public static void loadFiles()
    {
        string json = File.ReadAllText(Application.dataPath + "/_StoryData/English.json");

        var newData = JsonUtility.FromJson<DataStorage>(json);

        var game = GameObject.Find("Game");

        List<Node> activeNodes = new List<Node>(game.GetComponentsInChildren<Node>());
        for (int i = 0; i < newData.nodes.Length; i++)
        {
            bool nodeFound = false;
            var node = newData.nodes[i];

            for (int j = 0; j < activeNodes.Count; j++)
            {
                var synonymsLength = node.synonyms.Length;

                for (int k = 0; k < synonymsLength; k++)
                {
                    if (node.synonyms[k].ToLower() == activeNodes[j].name.ToLower())
                    {
                        nodeFound = true;

                        activeNodes[j].synonyms = node.synonyms;
                        activeNodes[j].arrivalLines = node.arrivalLines;
                        activeNodes[j].visitedLines = node.visitedLines;

                        activeNodes.RemoveAt(j);
                        break;
                    }

                    if (nodeFound)
                    {
                        break;
                    }
                }
            }
        }


        List<Interactable> activeInteractables = new List<Interactable>(game.GetComponentsInChildren<Interactable>());
        for (int i = 0; i < newData.interactables.Length; i++)
        {
            bool interactableFound = false;
            var interactable = newData.interactables[i];

            for (int j = 0; j < activeInteractables.Count; j++)
            {
                var synonymsLength = interactable.synonyms.Length;

                for (int k = 0; k < synonymsLength; k++)
                {
                    if (interactable.synonyms[k].ToLower() == activeInteractables[j].name.ToLower())
                    {
                        interactableFound = true;

                        activeInteractables[j].synonyms = interactable.synonyms;

                        var commandDic = new Dictionary<string, int>();
                        for (int h = 0; h < interactable.commandKeys.Length; h++)
                        {
                            commandDic.Add(interactable.commandKeys[h].ToLower(), interactable.commandValues[h]);
                        }
                        activeInteractables[j].commands = new Dictionary<string, int>(commandDic);

                        var answerDic = new Dictionary<int, LineData>();
                        for (int h = 0; h < interactable.answerKeys.Length; h++)
                        {
                            answerDic.Add(interactable.answerKeys[h], interactable.answerValues[h]);
                        }
                        activeInteractables[j].answers = new Dictionary<int, LineData>(answerDic);

                        activeInteractables.RemoveAt(j);
                        break;
                    }

                    if (interactableFound)
                    {
                        break;
                    }
                }
            }
        }

        game.GetComponent<Game>().choiceData = newData.choices;
        //for (int i = 0; i < newData.choices.Length; i++)
        //{
        //    var choice = newData.choices[i];
        //    Debug.Log(choice.choiceId);
        //    Debug.Log(choice.choiceOption1);
        //    Debug.Log(choice.choiceOption2);
        //    Debug.Log(choice.choiceOption3);

        //    for (int j = 0; j < choice.choiceResult1.Length; j++)
        //    {
        //        Debug.Log(choice.choiceResult1[j].lineString);
        //    }
        //    for (int j = 0; j < choice.choiceResult2.Length; j++)
        //    {
        //        Debug.Log(choice.choiceResult2[j].lineString);
        //    }
        //    for (int j = 0; j < choice.choiceResult3.Length; j++)
        //    {
        //        Debug.Log(choice.choiceResult3[j].lineString);
        //    }
        //}

        game.GetComponent<Game>().startingLines = newData.startingLines;
        game.GetComponent<Game>().commandErrorMessages = newData.errorMessages;
    }
}

public class DataStorage
{
    public NodeData[] nodes;
    public InteractableData[] interactables;
    public ChoiceData[] choices;
    public LineData[] startingLines;
    public string[] errorMessages;

    // new stuff goes here....
}

[System.Serializable]
public class NodeData
{
    public string[] synonyms;
    public LineData[] arrivalLines;
    public LineData[] visitedLines;
}

[System.Serializable]
public class LineData
{
    [TextArea(3, 5)]
    public string lineString;
    public string[] commands;

    public LineData(string line)
    {
        lineString = line;
        commands = null;
    }

    public LineData(string line, string[] comm)
    {
        lineString = line;
        commands = comm;
    }
}

[System.Serializable]
public class InteractableData
{
    public string[] synonyms;   

    public string[] commandKeys;
    public int[] commandValues;
    
    // dic
    public int[] answerKeys;
    public LineData[] answerValues;
}

[System.Serializable]
public class ChoiceData
{
    public string choiceId;

    // uuugly, find scalable solution for future projects :C    (aka find/make a different json parser)

    public string choiceOption1;
    public string choiceOption2;
    public string choiceOption3;

    public LineData[] choiceResult1;
    public LineData[] choiceResult2;
    public LineData[] choiceResult3;
}

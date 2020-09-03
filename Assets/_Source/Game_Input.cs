using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public partial class Game : MonoBehaviour
{
    Dictionary<string, int> basicCommands = new Dictionary<string, int>()
    {
        { "go", 0},
        { "move", 0},
        { "walk", 0},
        { "run", 0},
        { "navigate", 0},
        { "nav", 0},
        {"f", 1},
        {"forward", 1},
        {"forwards", 1},
        {"b", 2},
        {"back", 2},
        {"backward", 2},
        {"backwards", 2},
        {"l", 3},
        {"left", 3},
        {"r", 4},
        {"right", 4},
        {"exit", 5},
        {"quit", 5}
    };

    public LineData[] startingLines;

    void attemptNavigation(Node node)
    {
        if (node == null)
        {
            pushToDisplay(new LineData("Can't go that way."));
        }
        else
        {
            targetNode = node;

            setNavAgentDestination(targetNode.worldPos);
            targetRot = targetNode.worldRot;

            changeState(GameState.Move);
        }
    }

    void processInputText(string inputText)
    {
        if (inputText != "")
        {
            pushToDisplay(new LineData("<color=#" + ColorUtility.ToHtmlStringRGBA(commandColor) + ">" + inputText + "</color>"));

            var words = getCondensedInput(inputText);

            var verbs = new List<string>();
            Node referencedNode = null;
            Interactable referencedInteractable = null;

            for (int i = 0; i < words.Length; i++)
            {
                if (!isNode(words[i], out referencedNode) && !isInteractable(words[i], out referencedInteractable))
                {
                    verbs.Add(words[i]);
                }
            }
            //Debug.Log(string.Join(" ", verbs) + " | RefNode: " + referencedNode + " | RefInt: " + referencedInteractable);


            if (words.Length > 1)
            {
                if (basicCommands.TryGetValue(string.Join(" ", verbs), out var commandKey))
                {
                    switch (commandKey)
                    {
                        case 0:
                            if (referencedNode != null)
                            {
                                attemptNavigation(referencedNode);
                            }
                            break;
                        case 1:
                            attemptNavigation(currentNode.forwardNode);
                            break;
                        case 2:
                            attemptNavigation(currentNode.backNode);
                            break;
                        case 3:
                            attemptNavigation(currentNode.leftNode);
                            break;
                        case 4:
                            attemptNavigation(currentNode.rightNode);
                            break;
                        case 5:
                            quitGame();
                            break;
                        default:
                            pushToDisplay(new LineData(commandErrorMessages[Random.Range(0, commandErrorMessages.Length)]));
                            break;
                    }
                }
                else if (referencedInteractable != null)
                {
                    handleInteractable(verbs.ToArray(), referencedInteractable);
                }
                //else if (words[0] == "set" && (words[1] == "volume" || words[1] == "audio") && int.TryParse(words[2], out var newVal))
                //{
                //    newVal = Mathf.Clamp(newVal, 0, 100);
                //    audioSource.volume = newVal / 100f;
                //    pushToDisplay(new LineData("Setting audio to " + newVal + "%"));
                //}
                else
                {
                    pushToDisplay(new LineData(commandErrorMessages[Random.Range(0, commandErrorMessages.Length)]));
                }
            }
            else
            {
                if (basicCommands.TryGetValue(words[0], out var commandKey))
                {
                    switch (commandKey)
                    {
                        case 1:
                            attemptNavigation(currentNode.forwardNode);
                            break;
                        case 2:
                            attemptNavigation(currentNode.backNode);
                            break;
                        case 3:
                            attemptNavigation(currentNode.leftNode);
                            break;
                        case 4:
                            attemptNavigation(currentNode.rightNode);
                            break;
                        case 5:
                            quitGame();
                            break;
                        default:
                            pushToDisplay(new LineData(commandErrorMessages[Random.Range(0, commandErrorMessages.Length)]));
                            break;
                    }
                }
                else if (referencedInteractable != null)
                {
                    pushToDisplay(new LineData("What about it?"));
                }
                else
                {
                    pushToDisplay(new LineData(commandErrorMessages[Random.Range(0, commandErrorMessages.Length)]));
                }
            }
        }
    }

    bool isNode(string input, out Node node)
    {
        node = null;

        var activeNodes = this.GetComponentsInChildren<Node>();

        // check active nodes
        for (int i = 0; i < activeNodes.Length; i++)
        {
            var synonymsLength = activeNodes[i].synonyms.Length;
            for (int j = 0; j < synonymsLength; j++)
            {
                if (input == activeNodes[i].synonyms[j].ToLower())
                {
                    node = activeNodes[i];
                    break;
                }
            }

            if (node == null)
            {
                if (input == "forward" || input == "forwards" || input == "f")
                {
                    node = currentNode.forwardNode;
                }
                if (input == "back" || input == "backward" || input == "backwards" || input == "b")
                {
                    node = currentNode.backNode;
                }
                if (input == "left" || input == "l")
                {
                    node = currentNode.leftNode;
                }
                if (input == "right" || input == "r")
                {
                    node = currentNode.rightNode;
                }
            }

            if (node != null)
            {
                break;
            }
        }


        return node != null;
    }

    bool isInteractable(string input, out Interactable interactable)
    {
        interactable = null;

        for (int i = 0; i < currentInteractables.Count; i++)
        {
            var synonymsLength = currentInteractables[i].synonyms.Length;
            for (int j = 0; j < synonymsLength; j++)
            {
                if (input.ToLower() == currentInteractables[i].synonyms[j].ToLower())
                {
                    interactable = currentInteractables[i];
                    break;
                }
            }

            if (interactable != null)
            {
                break;
            }
        }

        return interactable != null;
    }

    void startGame()
    {
        putInQueue(startingNode.arrivalLines);
    }

    void quitGame()
    {
        // uhhhh if this was a more complex game i guess i'd put an autosave thing here
        Application.Quit(0);
    }

    string[] getCondensedInput(string inputText)
    {
        var words = new List<string>(inputText.ToLower().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));

        string[] queryWordsToIgnore = new string[] { "the", "a", "an", "this", "that", "to", "at" };
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

        int wordIndex = 0;
        while (wordIndex < words.Count)
        {
            if (words.Count > 1 && ignoreThisWord(words[wordIndex]))
            {
                words.RemoveAt(wordIndex);
            }
            else
            {
                wordIndex++;
            }
        }

        //for (int i = 0; i < words.Count; i++)
        //{
        //    Debug.Log(words[i]);
        //}

        return words.ToArray();
    }

    void pushToDisplay(LineData lineData)
    {
        if (lineData.lineString != null && lineData.lineString != "")
        {
            GameObject newParagraph;
            RectTransform paragraphRect;
            TextMeshProUGUI paragraphTextMesh;

            if (paragraphContainers.Count < paragraphCapacity)
            {
                newParagraph = Instantiate(paragraphPrefab);
                newParagraph.SetActive(true);
                newParagraph.transform.SetParent(contentRect);
                newParagraph.transform.localPosition = Vector3.zero;

                paragraphRect = newParagraph.GetComponent<RectTransform>();
                paragraphRect.localScale = Vector3.one;
                paragraphRect.anchoredPosition = Vector3.zero;
                paragraphRect.offsetMin = Vector3.zero;
                paragraphRect.offsetMax = Vector3.zero;

                paragraphTextMesh = newParagraph.GetComponent<TextMeshProUGUI>();
                paragraphTextMesh.text = lineData.lineString;
                paragraphTextMesh.fontSize = fontSize;

                paragraphContainers.Add(paragraphRect);
            }
            else
            {
                newParagraph = contentRect.GetChild(0).gameObject;
                paragraphRect = newParagraph.GetComponent<RectTransform>();

                paragraphTextMesh = newParagraph.GetComponent<TextMeshProUGUI>();
                paragraphTextMesh.text = lineData.lineString;
                paragraphTextMesh.fontSize = fontSize;

                newParagraph.transform.SetAsLastSibling();
            }

            Canvas.ForceUpdateCanvases();
            Vector2 slideStartPos = contentRect.anchoredPosition;
            slideStartPos.y -= paragraphRect.sizeDelta.y;
            contentRect.anchoredPosition = slideStartPos;

            slideStartTime = Time.timeSinceLevelLoad;
        }

        if (lineData.commands != null)
        {
            processLineDataCommands(lineData.commands);
        }

        if (lineQueue.Count == 0 && activeState == GameState.TakeInput)
        {
            inputField.textComponent.color = Color.cyan;
            inputPlaceholder.text = "Enter text here...";
        }
    }

    void putInQueue(LineData[] lineData)
    {
        int len = lineData.Length;

        // print first paragraph
        pushToDisplay(lineData[0]);
        for (int i = 1; i < len; i++)
        {
            lineQueue.Enqueue(lineData[i]);
        }
    }

    void handleInteractable(string[] verbs, Interactable interactable)
    {
        int answerKey = interactable.tryGetInteractionAnswerKey(verbs);

        if (answerKey != -1)
        {
            var lineData = interactable.tryGetInteractionAnswer(answerKey);

            if (lineData != null)
            {
                pushToDisplay(lineData);
            }
        }
        else
        {
            pushToDisplay(new LineData(commandErrorMessages[Random.Range(0, commandErrorMessages.Length)]));
        }
    }

    void processLineDataCommands(string[] commands)
    {
        // remember! one command is: methodname param1 param2
        for (int i = 0; i < commands.Length; i++)
        {
            var words = commands[i].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            var methodName = words[0];
            string[] parameters = new string[words.Length - 1];
            for (int j = 1; j < words.Length; j++)
            {
                parameters[j - 1] = words[j];
            }

            switch (methodName)
            {
                case "giveDialogueChoice":
                    giveDialogueChoice(parameters);
                    break;

                case "addToInventory":
                    addToInventory(parameters);
                    break;

                case "handleStickUse":
                    handleStickUse();
                    break;

                case "handleKeyUse":
                    handleKeyUse();
                    break;

                case "restartGame":
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
                    break;

                case "fadeIn":
                    fadeIn();
                    break;

                case "fadeOut":
                    fadeOut();
                    break;

                case "startGame":
                    startGame();
                    break;

                case "strangerAnimation":
                    startStrangerAnimation();
                    break;

                case "quitGame":
                    quitGame();
                    break;

                default:
                    Debug.LogWarning("processLineDataCommands(" + methodName + "):  No valid method found in code...");
                    break;
            }
        }
    }

    float itemPickupStart = -Mathf.Infinity;
    Vector3 pickupLocation;
    void addToInventory(string[] parameters)
    {
        bool valid = false;
        if (parameters.Length == 1)
        {
            var item = GameObject.Find(parameters[0]);

            if (currentInteractables.Contains(item.GetComponent<Interactable>()) && !inventory.Contains(item))
            {
                pushToDisplay(new LineData("You take the " + item.name));
                inventory.Add(item);

                //var itemIndex = inventory.Count - 1;
                item.transform.SetParent(mainCamera.transform);
                pickupLocation = item.transform.position;

                var newSlot = Instantiate(slotPrefab);
                newSlot.transform.SetParent(inventoryPanel);
                newSlot.transform.localScale = Vector3.zero;
                newSlot.GetComponentInChildren<Image>().sprite = item.GetComponent<Interactable>().inventoryImage;
                inventorySlots.Add(newSlot.GetComponent<RectTransform>());

                itemPickupStart = Time.time;
                valid = true;
            }
        }

        if (!valid)
        {
            pushToDisplay(new LineData(commandErrorMessages[Random.Range(0, commandErrorMessages.Length)]));
        }
    }

    void removeFromInventory(string itemName)
    {
        var item = GameObject.Find(itemName);
        var itemIndex = inventory.IndexOf(item);

        inventory.Remove(item);

        var slot = inventorySlots[itemIndex];
        inventorySlots.RemoveAt(itemIndex);
        Destroy(slot.gameObject);
    }

    bool inventoryContainsItem(string itemName) // case sensitive!!
    {
        bool contains = false;
        for (int i = 0; i < inventory.Count; i++)
        {
            var item = inventory[i].GetComponent<Interactable>();
            var synonymLength = item.synonyms.Length;
            for (int j = 0; j < synonymLength; j++)
            {
                if (item.synonyms[j] == itemName)
                {
                    contains = true;
                    break;
                }
            }

            if (contains)
            {
                break;
            }
        }

        return contains;
    }

    void handleStickUse()
    {
        bool torchPresent = isInteractable("Torch", out var torchItem);
        bool holdingStick = inventoryContainsItem("Stick");
        if (holdingStick && torchPresent)
        {
            torchLight.gameObject.SetActive(true);

            addToInventory(new string[] { "Torch" });
            removeFromInventory("Stick");
        }
        else if (!holdingStick)
        {
            pushToDisplay(new LineData("You should pick it up first."));
        }
        else
        {
            pushToDisplay(new LineData(commandErrorMessages[Random.Range(0, commandErrorMessages.Length)]));
        }
    }

    void handleKeyUse()
    {
        if (currentNode.name == "Door")
        {
            pushToDisplay(new LineData("Nope. The key doesn't fit at all. Figures."));
        }
        else if(inventoryContainsItem("Torch") && inventoryContainsItem("Key"))
        {
            removeFromInventory("Key");
            exitDoor.play = true;
            currentNode.forwardNode = exitNode;
            exitNode.gameObject.SetActive(true);
            updateArrowKeys(currentNode);

        }
        else
        {
            pushToDisplay(new LineData("But where? It's too dark to see anything."));
        }
    }

    public ChoiceData[] choiceData;
    ChoiceData activeChoiceData;
    List<GameObject> activeChoiceOptions = new List<GameObject>();
    void giveDialogueChoice(string[] parameters)
    {
        var choiceId = parameters[0]; // we only need one

        for (int i = 0; i < choiceData.Length; i++)
        {
            if (choiceId == choiceData[i].choiceId)
            {
                activeChoiceData = choiceData[i];
                break;
            }
        }

        if (activeChoiceData.choiceId != null)
        {
            if (activeChoiceData.choiceOption1 != null && activeChoiceData.choiceOption1 != "")
            {
                var option = Instantiate(choicePrefab);

                var newChoice = option.GetComponentInChildren<DialogueParagraph>();
                newChoice.textMeshBase.text = activeChoiceData.choiceOption1;
                newChoice.textMeshSpoken.text = activeChoiceData.choiceOption1;
                newChoice.inputField = inputField;

                option.transform.SetParent(contentRect);
                option.GetComponent<RectTransform>().localScale = Vector3.one;

                activeChoiceOptions.Add(option);
            }
            if (activeChoiceData.choiceOption2 != null && activeChoiceData.choiceOption2 != "")
            {
                var option = Instantiate(choicePrefab);

                var newChoice = option.GetComponentInChildren<DialogueParagraph>();
                newChoice.textMeshBase.text = activeChoiceData.choiceOption2;
                newChoice.textMeshSpoken.text = activeChoiceData.choiceOption2;
                newChoice.inputField = inputField;

                option.transform.SetParent(contentRect);
                option.GetComponent<RectTransform>().localScale = Vector3.one;

                activeChoiceOptions.Add(option);
            }
            if (activeChoiceData.choiceOption3 != null && activeChoiceData.choiceOption3 != "")
            {
                var option = Instantiate(choicePrefab);

                var newChoice = option.GetComponentInChildren<DialogueParagraph>();
                newChoice.textMeshBase.text = activeChoiceData.choiceOption3;
                newChoice.textMeshSpoken.text = activeChoiceData.choiceOption3;
                newChoice.inputField = inputField;

                option.transform.SetParent(contentRect);
                option.GetComponent<RectTransform>().localScale = Vector3.one;

                activeChoiceOptions.Add(option);
            }

            changeState(GameState.DialogueChoice);
        }
    }

    bool isDialogueInputValid(string inputText, out LineData[] resultLines)
    {
        bool valid = false;
        resultLines = null;

        if (activeChoiceData.choiceOption1 != null && inputText.ToLower() == activeChoiceData.choiceOption1.ToLower())
        {
            valid = true;
            resultLines = activeChoiceData.choiceResult1;
        }
        else if (activeChoiceData.choiceOption2 != null && inputText.ToLower() == activeChoiceData.choiceOption2.ToLower())
        {
            valid = true;
            resultLines = activeChoiceData.choiceResult2;
        }
        else if (activeChoiceData.choiceOption3 != null && inputText.ToLower() == activeChoiceData.choiceOption3.ToLower())
        {
            valid = true;
            resultLines = activeChoiceData.choiceResult3;
        }

        return valid;
    }

    void startStrangerAnimation()
    {
        stranger.play = true;
    }

    public string[] commandErrorMessages;
}

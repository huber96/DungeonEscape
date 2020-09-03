using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameState { TakeInput, Move, DialogueChoice }

public partial class Game : MonoBehaviour
{
    public GameState activeState;

    [Header("Basic settings")]
    public Node startingNode;

    public Camera mainCamera;

    [Header("----Navigation")]
    [Space(5)]
    public UnityEngine.AI.NavMeshAgent agent;
    public float defaultMovementSpeed = 5f;
    public float retreadModifier = 3.5f;
    float activeMovementSpeed = 5f;

    Node currentNode;
    Node targetNode;

    float camOffsetX = 0;
    float camOffsetY = 0;
    //float cameraFadeStartTime = -Mathf.Infinity;

    [Header("----Options")]
    [Space(10)]
    public int masterVolume = 100;
    public AudioSource audioSource;
    public int fontSize = 20;


    [Header("----Input")]
    public RectTransform contentRect;
    public GameObject paragraphPrefab;
    public GameObject choicePrefab;

    List<RectTransform> paragraphContainers = new List<RectTransform>();
    int paragraphCapacity = 20;

    float paragraphSlideDuration = 3;
    float slideStartTime = -Mathf.Infinity;

    [Space(10)]
    public TMP_InputField inputField;
    public TextMeshProUGUI inputPlaceholder;
    public TextMeshProUGUI inputDisabledMarker; // container for dot animation

    [Space(10)]
    public Color commandColor = Color.cyan;
    public Color playerDialogueColor = Color.yellow;
    public Color npcDialogueColor = Color.magenta;

    Queue<LineData> lineQueue = new Queue<LineData>();
    Queue<string> contentQueue = new Queue<string>();
    Queue<GameObject> logQueue = new Queue<GameObject>(10);
    public float slidePosY = 185;

    //InputCommandHistory commandHistory = new InputCommandHistory(10);
    public List<Interactable> currentInteractables = new List<Interactable>();
    public Quaternion targetRot;

    [Header("----Inventory")]
    [Space(10)]
    public List<GameObject> inventory = new List<GameObject>();
    [Space(10)]
    public GameObject slotPrefab;
    public Transform inventoryPanel;
    public List<RectTransform> inventorySlots = new List<RectTransform>();

    [Header("----Level Specific")]
    public Light torchLight;
    public Node exitNode;
    public PathInteractable exitDoor;
    public PathInteractable stranger;

    void Awake()
    {
        mainCamera = Camera.main;
        currentNode = startingNode;
        agent.transform.position = startingNode.worldPos;
    }

    void Start()
    {
        currentNode.visited = true;

        //if (activeState == GameState.Startup)
        //{
        //    putInQueue(startupText);
        //    fadeTo(Color.black, 0, 4);
        //}

        updateCurrentInteractables(currentNode);

        inputField.ActivateInputField();

        slidePosY = contentRect.anchoredPosition.y;

        DataManager.loadFiles();

        updateArrowKeys(currentNode);
        fadeArrowKeys(1);

        audioSource.gameObject.SetActive(true);

        changeState(GameState.TakeInput);

        putInQueue(startingLines);

        exitNode.gameObject.SetActive(false);
    }

    void Update()
    {
        if (activeState == GameState.TakeInput)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                inputField.ActivateInputField();
            }

            if (lineQueue.Count == 0)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    inputField.ActivateInputField();
                    processInputText(inputField.text);
                    inputField.text = "";
                }
            }
            else
            {
                inputField.text = "";

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    inputField.ActivateInputField();
                    pushToDisplay(lineQueue.Dequeue());
                }
                else if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                {
                    inputField.ActivateInputField();
                    while (lineQueue.Count != 0)
                    {
                        pushToDisplay(lineQueue.Dequeue());
                    }
                }
            }
        }
        else if (activeState == GameState.Move)
        {
            // dotdot anim for movement
            switch ((int)((Time.timeSinceLevelLoad * 2f) % 3))
            {
                case 0:
                    inputDisabledMarker.text = ".";
                    break;
                case 1:
                    inputDisabledMarker.text = "..";
                    break;
                default:
                    inputDisabledMarker.text = "...";
                    break;
            }

            moveByNavMesh();
        }
        else if (activeState == GameState.DialogueChoice)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                inputField.ActivateInputField();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (inputField.text != "" && isDialogueInputValid(inputField.text, out var lineData))
                {
                    inputField.ActivateInputField();

                    pushToDisplay(new LineData("<color=#" + ColorUtility.ToHtmlStringRGBA(commandColor) + ">" + inputField.text + "</color>"));
                    putInQueue(lineData);

                    inputField.text = "";

                    for (int i = 0; i < activeChoiceOptions.Count; i++)
                    {
                        Destroy(activeChoiceOptions[i]);
                    }
                    activeChoiceOptions.Clear();

                    Canvas.ForceUpdateCanvases();

                    changeState(GameState.TakeInput);
                }
                else
                {
                    inputField.ActivateInputField();
                }
            }
        }
    }

    void LateUpdate()
    {
        // rotate camera independent of state

        if (agent.remainingDistance <= 4f)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * 1.23f);
        }

        camOffsetX = Mathf.Cos(Time.timeSinceLevelLoad * 0.85f);
        camOffsetY = Mathf.Sin(Time.timeSinceLevelLoad * 0.5f) * 5f;
        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x + camOffsetX / 50000, mainCamera.transform.position.y + camOffsetY / 50000, mainCamera.transform.position.z);

        //if (Time.timeSinceLevelLoad - overlayFadeStartTime <= fadeDuration)
        //{
        //    float alpha;
        //    if (fadeOverlayGoal == 1)
        //    {
        //        alpha = Mathf.SmoothStep(0, 1, (Time.timeSinceLevelLoad - overlayFadeStartTime) / fadeDuration);
        //    }
        //    else
        //    {
        //        alpha = Mathf.SmoothStep(1, 0, (Time.timeSinceLevelLoad - overlayFadeStartTime) / fadeDuration);
        //    }
        //    fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.g, fadeOverlay.color.b, alpha);
        //}

        var timeSinceLevelLoad = Time.timeSinceLevelLoad;

        if (timeSinceLevelLoad - slideStartTime <= paragraphSlideDuration)
        {
            contentRect.anchoredPosition = 
                Vector2.Lerp(contentRect.anchoredPosition, new Vector3(contentRect.anchoredPosition.x, slidePosY), (timeSinceLevelLoad - slideStartTime) / paragraphSlideDuration);
        }

        if (timeSinceLevelLoad - arrowFadeStartTime <= arrowFadeDuration)
        {
            arrowKeyContainer.alpha = Mathf.SmoothStep(arrowKeyContainer.alpha, arrowKeysTargetAlpha, (timeSinceLevelLoad - arrowFadeStartTime) / arrowFadeDuration);
        }


        var pickupTime = timeSinceLevelLoad - itemPickupStart;
        if (pickupTime <= 0.75f)
        {
            var item = inventory[inventory.Count - 1];
            if (Time.timeSinceLevelLoad - itemPickupStart <= 0.25f)
            {
                item.transform.position = 
                    Vector3.Lerp(pickupLocation, mainCamera.transform.position - mainCamera.transform.up + mainCamera.transform.right, pickupTime / 0.25f);
            }
            else
            {
                var slot = inventorySlots[inventory.Count - 1];
                slot.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, (pickupTime - 0.25f) / 0.5f);

                var rend = item.GetComponent<MeshRenderer>();
                if (rend != null && rend.enabled)
                {
                    rend.enabled = false;
                    slot.gameObject.SetActive(true);
                }
            }
        }

        if (Time.timeSinceLevelLoad - fadeInStartTime <= 2)
        {
            blackScreen.color = new Color(0, 0, 0, 1 - (Time.timeSinceLevelLoad - fadeInStartTime) / 2);
        }

        if (Time.timeSinceLevelLoad - fadeOutStartTime <= 3)
        {
            blackScreen.color = new Color(0, 0, 0, (Time.timeSinceLevelLoad - fadeOutStartTime) / 3);
        }


    }

    void moveByNavMesh()
    {
        if (targetNode.visited)
        {
            activeMovementSpeed = defaultMovementSpeed * retreadModifier;
        }
        else
        {
            activeMovementSpeed = defaultMovementSpeed;
        }


        if (agent.remainingDistance <= 0.5f)
        {
            currentNode = targetNode;
            targetNode = null;

            updateCurrentInteractables(currentNode);

            if (currentNode.visited || currentNode.arrivalLines.Length == 0)
            {
                putInQueue(currentNode.visitedLines);
            }
            else
            {
                currentNode.visited = true;

                putInQueue(currentNode.arrivalLines);
            }

            updateArrowKeys(currentNode);
            fadeArrowKeys(1);

            // if we don't check then it might skip GameState.DialogueChoice when it gets pushed on exit move
            if (activeState == GameState.Move)
            {
                changeState(GameState.TakeInput);
            }
        }
    }

    void setNavAgentDestination(Vector3 pos)
    {
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.destination = hit.position;
        }
    }

    void updateCurrentInteractables(Node node)
    {
        currentInteractables.Clear();
        node.GetComponentsInChildren(currentInteractables);
        for (int i = 0; i < inventory.Count; i++)
        {
            currentInteractables.Add(inventory[i].GetComponent<Interactable>());
        }
    }

    void changeState(GameState targetState)
    {
        // enable inputField if coming out of Move state
        if (activeState == GameState.Move)
        {
            inputDisabledMarker.gameObject.SetActive(false);
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
        }
        else if (activeState == GameState.DialogueChoice)
        {
            activeChoiceData = null;
        }


        activeState = targetState;


        // disable inputField if going into Move state
        if (targetState == GameState.Move)
        {
            inputDisabledMarker.gameObject.SetActive(true);
            inputField.text = "";
            inputField.gameObject.SetActive(false);

            fadeArrowKeys(0);
        }
        else if (targetState == GameState.TakeInput)
        {
            if (lineQueue.Count == 0)
            {
                inputField.textComponent.color = Color.cyan;
                inputPlaceholder.text = "Enter text here...";
            }
            else
            {
                inputField.textComponent.color = Color.white;
                inputPlaceholder.text = "Enter to continue...";
            }
        }
        else if (targetState == GameState.DialogueChoice)
        {
            inputField.textComponent.color = Color.cyan;
            inputPlaceholder.text = "Enter text here...";

            lineQueue.Clear();
        }
    }
}

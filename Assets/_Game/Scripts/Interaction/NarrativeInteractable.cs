using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Yarn.Unity;

[RequireComponent(typeof(Collider))]
public class NarrativeInteractable : MonoBehaviour
{
    [SerializeField] private string yarnNodeName;
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject promptObject;
    [SerializeField] private GameObject lockedPromptObject;
    [SerializeField] private DoorAnimationBridge doorAnimation;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Key fallbackInteractKey = Key.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool hidePromptOnStart = true;
    [SerializeField] private bool isLocked;
    [SerializeField] private UnityEvent onInteract;

    private bool playerInside;

    public bool IsLocked => isLocked;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        }

        if (hidePromptOnStart)
        {
            SetPromptVisible(false);
            SetLockedPromptVisible(false);
        }
    }

    private void OnEnable()
    {
        interactAction?.action?.Enable();
    }

    private void OnDisable()
    {
        interactAction?.action?.Disable();
        SetPromptVisible(false);
        SetLockedPromptVisible(false);
        playerInside = false;
    }

    private void Update()
    {
        if (!playerInside || !WasInteractPressed())
        {
            return;
        }

        TryInteract();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInside = true;
        RefreshPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInside = false;
        SetPromptVisible(false);
        SetLockedPromptVisible(false);
    }

    public void TryInteract()
    {
        if (isLocked || dialogueRunner == null || dialogueRunner.IsDialogueRunning || string.IsNullOrWhiteSpace(yarnNodeName))
        {
            return;
        }

        onInteract?.Invoke();
        doorAnimation?.PlayOpen();
        SetPromptVisible(false);
        _ = dialogueRunner.StartDialogue(yarnNodeName);
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        doorAnimation?.SetLocked(locked);
        RefreshPrompt();
    }

    public void SetYarnNodeName(string nodeName)
    {
        yarnNodeName = nodeName;
    }

    public void SetDialogueRunner(DialogueRunner runner)
    {
        dialogueRunner = runner;
    }

    public void SetPromptObject(GameObject target)
    {
        promptObject = target;
        RefreshPrompt();
    }

    public void SetPlayerTag(string tagName)
    {
        playerTag = tagName;
    }

    public void SetLockedPromptObject(GameObject target)
    {
        lockedPromptObject = target;
        RefreshPrompt();
    }

    public void SetDoorAnimationBridge(DoorAnimationBridge animationBridge)
    {
        doorAnimation = animationBridge;
        doorAnimation?.SetLocked(isLocked);
    }

    private bool WasInteractPressed()
    {
        if (interactAction != null && interactAction.action != null)
        {
            return interactAction.action.WasPressedThisFrame();
        }

        return Keyboard.current != null && Keyboard.current[fallbackInteractKey].wasPressedThisFrame;
    }

    private bool IsPlayer(Component other)
    {
        return string.IsNullOrWhiteSpace(playerTag) || other.CompareTag(playerTag);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptObject != null)
        {
            promptObject.SetActive(visible);
        }
    }

    private void SetLockedPromptVisible(bool visible)
    {
        if (lockedPromptObject != null)
        {
            lockedPromptObject.SetActive(visible);
        }
    }

    private void RefreshPrompt()
    {
        if (!playerInside)
        {
            SetPromptVisible(false);
            SetLockedPromptVisible(false);
            return;
        }

        SetPromptVisible(!isLocked);
        SetLockedPromptVisible(isLocked);
    }
}

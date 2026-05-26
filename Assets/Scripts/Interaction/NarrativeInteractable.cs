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
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Key fallbackInteractKey = Key.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool hidePromptOnStart = true;
    [SerializeField] private UnityEvent onInteract;

    private bool playerInside;

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
        SetPromptVisible(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInside = false;
        SetPromptVisible(false);
    }

    public void TryInteract()
    {
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning || string.IsNullOrWhiteSpace(yarnNodeName))
        {
            return;
        }

        onInteract?.Invoke();
        SetPromptVisible(false);
        _ = dialogueRunner.StartDialogue(yarnNodeName);
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
}

using UnityEngine;
using Yarn.Unity;

public class DialoguePlayerControlLock : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool unlockCursorDuringDialogue = true;
    [SerializeField] private bool lockCursorWhenDialogueEnds = true;

    private bool lastDialogueState;

    private void Awake()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        }

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }
    }

    private void OnEnable()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart?.AddListener(OnDialogueStarted);
            dialogueRunner.onDialogueComplete?.AddListener(OnDialogueCompleted);
        }
    }

    private void OnDisable()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart?.RemoveListener(OnDialogueStarted);
            dialogueRunner.onDialogueComplete?.RemoveListener(OnDialogueCompleted);
        }
    }

    private void Start()
    {
        lastDialogueState = dialogueRunner != null && dialogueRunner.IsDialogueRunning;
        ApplyDialogueState(lastDialogueState);
    }

    private void Update()
    {
        if (dialogueRunner == null)
        {
            return;
        }

        bool isRunning = dialogueRunner.IsDialogueRunning;
        if (isRunning == lastDialogueState)
        {
            return;
        }

        lastDialogueState = isRunning;
        ApplyDialogueState(isRunning);
    }

    private void OnDialogueStarted()
    {
        lastDialogueState = true;
        ApplyDialogueState(true);
    }

    private void OnDialogueCompleted()
    {
        lastDialogueState = false;
        ApplyDialogueState(false);
    }

    private void ApplyDialogueState(bool dialogueRunning)
    {
        if (playerController != null)
        {
            playerController.SetControlEnabled(!dialogueRunning);
        }

        if (dialogueRunning && unlockCursorDuringDialogue)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!dialogueRunning && lockCursorWhenDialogueEnds)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueInputController : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private Button continueButton;
    [SerializeField] private Transform optionsPresenter;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (!GameSceneNames.IsMainGame(scene.name))
        {
            return;
        }

        if (FindAnyObjectByType<DialogueInputController>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("DialogueInputController");
        controllerObject.AddComponent<DialogueInputController>();
    }

    private void Awake()
    {
        EnsureReferences();
    }

    private void Update()
    {
        EnsureReferences();

        if (dialogueRunner == null || !dialogueRunner.IsDialogueRunning)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Keyboard keyboard = Keyboard.current;
        bool keyboardContinue = keyboard != null
            && (keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame
                || keyboard.spaceKey.wasPressedThisFrame);

        if (keyboardContinue && TrySubmitSelectedOption())
        {
            return;
        }

        if (!CanContinue())
        {
            return;
        }

        Mouse mouse = Mouse.current;
        bool mouseContinue = mouse != null && mouse.leftButton.wasPressedThisFrame && !AreOptionsVisible();

        if (keyboardContinue || mouseContinue)
        {
            continueButton.onClick.Invoke();
        }
    }

    private void EnsureReferences()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        }

        if (continueButton == null)
        {
            continueButton = FindContinueButton();
        }

        if (optionsPresenter == null)
        {
            optionsPresenter = FindTransform("Options Presenter");
        }
    }

    private bool CanContinue()
    {
        return continueButton != null
            && continueButton.gameObject.activeInHierarchy
            && continueButton.interactable;
    }

    private bool AreOptionsVisible()
    {
        return optionsPresenter != null && optionsPresenter.gameObject.activeInHierarchy;
    }

    private bool TrySubmitSelectedOption()
    {
        if (!AreOptionsVisible() || EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
        {
            return false;
        }

        Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (selectedButton == null || selectedButton == continueButton || !selectedButton.interactable)
        {
            return false;
        }

        selectedButton.onClick.Invoke();
        return true;
    }

    private static Button FindContinueButton()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button.name == "Continue Button")
            {
                return button;
            }
        }

        return null;
    }

    private static Transform FindTransform(string objectName)
    {
        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (candidate.name == objectName)
            {
                return candidate;
            }
        }

        return null;
    }
}

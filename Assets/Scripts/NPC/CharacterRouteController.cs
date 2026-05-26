using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// Las clases RouteActionType, RouteCondition, RouteAction y RouteStep
// estan definidas en RouteSharedTypes.cs

/// <summary>
/// Orquestador de rutas/acciones de personajes.
/// Se dispara por variables de Yarn Spinner y permite ejecutar
/// animaciones, movimiento, eventos e interacciones sin duplicar codigo.
/// </summary>
public class CharacterRouteController : MonoBehaviour
{
    [Header("Yarn Spinner")]
    [Tooltip("Variable booleana de Yarn que dispara esta ruta (ej: $tema_desbloqueado)")]
    [SerializeField] private string triggerYarnVariable;
    [SerializeField] private bool triggerExpectedValue = true;
    [SerializeField] private bool autoRunOnStart;

    [Header("Recorrido de acciones")]
    [SerializeField] private List<RouteStep> steps = new List<RouteStep>();

    [Header("Fallback configuracion rapida")]
    [SerializeField] private bool buildDefaultRouteIfEmpty = true;
    [SerializeField] private Transform defaultMoveTarget;
    [SerializeField] private Animator defaultAnimator;
    [SerializeField] private string defaultAnimatorTrigger = "Saludar";
    [SerializeField] private float defaultMoveDuration = 1.6f;

    private InMemoryVariableStorage yarnStorage;
    private bool isRunning;
    private readonly HashSet<string> executedStepIds = new HashSet<string>(StringComparer.Ordinal);

    private void Awake()
    {
        EnsureDefaultSetup();
        yarnStorage = FindAnyObjectByType<InMemoryVariableStorage>();
        if (yarnStorage == null)
            Debug.LogWarning("[CharacterRouteController] No se encontro InMemoryVariableStorage en la escena.", this);
    }

    private void Start()
    {
        if (autoRunOnStart) RunRouteIfPossible();
    }

    /// <summary>
    /// Llama este metodo desde un Yarn Command o desde otro script
    /// cuando quieras disparar la ruta manualmente.
    /// </summary>
    public void RunRouteIfPossible()
    {
        if (isRunning) return;
        StartCoroutine(RunRouteCoroutine());
    }

    /// <summary>
    /// Verifica la variable de Yarn y ejecuta la ruta si coincide.
    /// Llamar desde DialogueRunner via evento o comando de Yarn.
    /// </summary>
    public void CheckAndRun()
    {
        if (string.IsNullOrWhiteSpace(triggerYarnVariable)) { RunRouteIfPossible(); return; }
        bool current = GetYarnBool(triggerYarnVariable);
        if (current == triggerExpectedValue) RunRouteIfPossible();
    }

    private IEnumerator RunRouteCoroutine()
    {
        isRunning = true;

        foreach (RouteStep step in steps)
        {
            if (step == null) continue;
            if (step.runOnlyOnce && executedStepIds.Contains(step.id)) continue;
            if (!PassesConditions(step.conditions)) continue;

            if (step.delayBefore > 0f)
                yield return new WaitForSeconds(step.delayBefore);

            yield return ExecuteActions(step.actions);

            if (step.runOnlyOnce && !string.IsNullOrWhiteSpace(step.id))
                executedStepIds.Add(step.id);
        }

        isRunning = false;
    }

    private bool PassesConditions(List<RouteCondition> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;
        foreach (RouteCondition condition in conditions)
        {
            if (condition == null || string.IsNullOrWhiteSpace(condition.yarnVariable)) continue;
            if (GetYarnBool(condition.yarnVariable) != condition.expectedValue) return false;
        }
        return true;
    }

    private IEnumerator ExecuteActions(List<RouteAction> actions)
    {
        if (actions == null) yield break;

        foreach (RouteAction action in actions)
        {
            if (action == null) continue;

            switch (action.actionType)
            {
                case RouteActionType.SetGameObjectActive:
                    action.targetObject?.SetActive(action.boolValue);
                    break;

                case RouteActionType.SetAnimatorTrigger:
                    if (action.targetAnimator != null && !string.IsNullOrWhiteSpace(action.parameterName))
                        action.targetAnimator.SetTrigger(action.parameterName);
                    break;

                case RouteActionType.SetAnimatorBool:
                    if (action.targetAnimator != null && !string.IsNullOrWhiteSpace(action.parameterName))
                        action.targetAnimator.SetBool(action.parameterName, action.boolValue);
                    break;

                case RouteActionType.MoveTransformToPoint:
                    if (action.targetTransform != null && action.destinationPoint != null)
                        yield return MoveOverTime(action.targetTransform, action.destinationPoint, action.moveDuration);
                    break;

                case RouteActionType.SetYarnVariable:
                    if (!string.IsNullOrWhiteSpace(action.yarnVariableName))
                        yarnStorage?.SetValue(action.yarnVariableName, action.yarnBoolValue);
                    break;

                case RouteActionType.InvokeUnityEvent:
                    action.customEvent?.Invoke();
                    break;
            }
        }
    }

    private static IEnumerator MoveOverTime(Transform source, Transform destination, float duration)
    {
        float safeDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        Vector3 initialPosition = source.position;
        Quaternion initialRotation = source.rotation;

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            source.position = Vector3.Lerp(initialPosition, destination.position, t);
            source.rotation = Quaternion.Slerp(initialRotation, destination.rotation, t);
            yield return null;
        }

        source.position = destination.position;
        source.rotation = destination.rotation;
    }

    private bool GetYarnBool(string variableName)
    {
        if (yarnStorage == null) return false;
        // Asegura que tenga el prefijo $
        string key = variableName.StartsWith("$") ? variableName : "$" + variableName;
        yarnStorage.TryGetValue(key, out bool value);
        return value;
    }

    private void EnsureDefaultSetup()
    {
        if (!buildDefaultRouteIfEmpty || steps.Count > 0) return;

        if (string.IsNullOrWhiteSpace(triggerYarnVariable))
            triggerYarnVariable = "$tema_siguiente_desbloqueado";

        RouteStep step = new RouteStep { id = "default_route_step", runOnlyOnce = true };

        if (defaultMoveTarget == null)
        {
            GameObject autoTarget = GameObject.Find("AsistenteRutaDestino");
            if (autoTarget != null) defaultMoveTarget = autoTarget.transform;
        }

        if (defaultAnimator != null && !string.IsNullOrWhiteSpace(defaultAnimatorTrigger))
            step.actions.Add(new RouteAction
            {
                actionType = RouteActionType.SetAnimatorTrigger,
                targetAnimator = defaultAnimator,
                parameterName = defaultAnimatorTrigger
            });

        if (defaultMoveTarget != null)
            step.actions.Add(new RouteAction
            {
                actionType = RouteActionType.MoveTransformToPoint,
                targetTransform = transform,
                destinationPoint = defaultMoveTarget,
                moveDuration = defaultMoveDuration
            });

        if (step.actions.Count > 0) steps.Add(step);
    }
}

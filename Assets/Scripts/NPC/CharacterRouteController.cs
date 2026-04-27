using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Las clases RouteActionType, RouteCondition, RouteAction y RouteStep
// estan definidas en RouteSharedTypes.cs

/// <summary>
/// Orquestador de rutas/acciones de personajes.
/// Se dispara por flags del sistema de dialogo y permite ejecutar
/// animaciones, movimiento, eventos e interacciones sin duplicar codigo.
/// </summary>
public class CharacterRouteController : MonoBehaviour
{
    [Header("Trigger por decision")]
    [SerializeField] private bool autoRunOnStart;
    [SerializeField] private string triggerFlagKey;
    [SerializeField] private bool triggerFlagValue = true;

    [Header("Recorrido de acciones")]
    [SerializeField] private List<RouteStep> steps = new List<RouteStep>();

    [Header("Fallback de configuracion rapida")]
    [SerializeField] private bool buildDefaultRouteIfEmpty = true;
    [SerializeField] private Transform defaultMoveTarget;
    [SerializeField] private Animator defaultAnimator;
    [SerializeField] private string defaultAnimatorTrigger = "Saludar";
    [SerializeField] private float defaultMoveDuration = 1.6f;

    private bool isSubscribed;
    private bool isRunning;
    private readonly HashSet<string> executedStepIds = new HashSet<string>(StringComparer.Ordinal);

    private void Awake()
    {
        EnsureDefaultSetup();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Update()
    {
        if (!isSubscribed) TrySubscribe();
    }

    private void Start()
    {
        if (autoRunOnStart) RunRouteIfPossible();
    }

    private void OnDisable()
    {
        if (!isSubscribed || PlayerDecisionState.Instance == null) return;
        PlayerDecisionState.Instance.OnFlagChanged -= HandleFlagChanged;
        isSubscribed = false;
    }

    public void RunRouteIfPossible()
    {
        if (isRunning) return;
        StartCoroutine(RunRouteCoroutine());
    }

    private void TrySubscribe()
    {
        if (isSubscribed || PlayerDecisionState.Instance == null) return;
        PlayerDecisionState.Instance.OnFlagChanged += HandleFlagChanged;
        isSubscribed = true;
    }

    private void HandleFlagChanged(string key, bool value)
    {
        if (string.IsNullOrWhiteSpace(triggerFlagKey)) return;
        if (!string.Equals(key, triggerFlagKey, StringComparison.Ordinal)) return;
        if (value == triggerFlagValue) RunRouteIfPossible();
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

        PlayerDecisionState state = PlayerDecisionState.Instance;
        foreach (RouteCondition condition in conditions)
        {
            if (condition == null || string.IsNullOrWhiteSpace(condition.key)) continue;
            bool current = state != null && state.GetFlag(condition.key);
            if (current != condition.expectedValue) return false;
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

                case RouteActionType.SetDecisionFlag:
                    if (!string.IsNullOrWhiteSpace(action.flagKey))
                        PlayerDecisionState.Instance?.SetFlag(action.flagKey, action.flagValue);
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

    private void EnsureDefaultSetup()
    {
        if (!buildDefaultRouteIfEmpty || steps.Count > 0) return;

        if (string.IsNullOrWhiteSpace(triggerFlagKey))
        {
            triggerFlagKey = "tema_siguiente_desbloqueado";
            triggerFlagValue = true;
        }

        RouteStep step = new RouteStep { id = "default_route_step", runOnlyOnce = true };

        if (defaultMoveTarget == null)
        {
            GameObject autoTarget = GameObject.Find("AsistenteRutaDestino");
            if (autoTarget != null) defaultMoveTarget = autoTarget.transform;
        }

        if (defaultAnimator != null && !string.IsNullOrWhiteSpace(defaultAnimatorTrigger))
        {
            step.actions.Add(new RouteAction
            {
                actionType = RouteActionType.SetAnimatorTrigger,
                targetAnimator = defaultAnimator,
                parameterName = defaultAnimatorTrigger
            });
        }

        if (defaultMoveTarget != null)
        {
            step.actions.Add(new RouteAction
            {
                actionType = RouteActionType.MoveTransformToPoint,
                targetTransform = transform,
                destinationPoint = defaultMoveTarget,
                moveDuration = defaultMoveDuration
            });
        }

        if (step.actions.Count > 0) steps.Add(step);
    }
}

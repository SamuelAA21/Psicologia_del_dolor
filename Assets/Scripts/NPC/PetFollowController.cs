using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// Las clases RouteActionType, RouteCondition, RouteAction y RouteStep
// estan definidas en RouteSharedTypes.cs

public enum PetState { Idle, Following, Sitting, RunningRoute }

[RequireComponent(typeof(NavMeshAgent))]
public class PetFollowController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator petAnimator;

    [Header("Seguimiento")]
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float followStartDistance = 3f;
    [SerializeField] private float runDistance = 6f;
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float idleBeforeSitTime = 5f;

    [Header("Parametros del Animator")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string sittingParam = "IsSitting";
    [SerializeField] private string greetTrigger = "Greet";

    [Header("Trigger por decision")]
    [SerializeField] private bool autoRunOnStart;
    [SerializeField] private string triggerFlagKey;
    [SerializeField] private bool triggerFlagValue = true;

    [Header("Recorrido de acciones")]
    [SerializeField] private List<RouteStep> steps = new List<RouteStep>();

    [Header("Fallback configuracion rapida")]
    [SerializeField] private bool buildDefaultRouteIfEmpty = true;
    [SerializeField] private Transform defaultMoveTarget;
    [SerializeField] private Animator defaultAnimator;
    [SerializeField] private string defaultAnimatorTrigger = "Saludar";
    [SerializeField] private float defaultMoveDuration = 1.6f;

    private NavMeshAgent agent;
    private PetState currentState = PetState.Idle;
    private bool isSubscribed;
    private bool isRunningRoute;
    private float idleTimer;
    private readonly HashSet<string> executedStepIds = new HashSet<string>(StringComparer.Ordinal);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ConfigureAgent();
        EnsureDefaultSetup();

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
            else Debug.LogWarning("[PetFollowController] No se encontro un GameObject con tag 'Player'.", this);
        }
    }

    private void OnEnable() => TrySubscribe();
    private void Start() { if (autoRunOnStart) StartCoroutine(RunRouteCoroutine()); }
    private void Update() { if (!isSubscribed) TrySubscribe(); if (!isRunningRoute) UpdateFollowBehavior(); }

    private void OnDisable()
    {
        if (isSubscribed && PlayerDecisionState.Instance != null)
        {
            PlayerDecisionState.Instance.OnFlagChanged -= HandleFlagChanged;
            isSubscribed = false;
        }
    }

    private void UpdateFollowBehavior()
    {
        if (playerTarget == null) return;
        float distance = Vector3.Distance(transform.position, playerTarget.position);
        switch (currentState)
        {
            case PetState.Idle:
            case PetState.Sitting: HandleIdleOrSitting(distance); break;
            case PetState.Following: HandleFollowing(distance); break;
        }
    }

    private void HandleIdleOrSitting(float distance)
    {
        if (distance > followStartDistance) { SetState(PetState.Following); idleTimer = 0f; return; }
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleBeforeSitTime && currentState != PetState.Sitting) SetState(PetState.Sitting);
    }

    private void HandleFollowing(float distance)
    {
        if (distance <= stopDistance)
        {
            agent.ResetPath();
            SetState(PetState.Idle);
            idleTimer = 0f;
            if (distance <= stopDistance * 1.2f && !string.IsNullOrWhiteSpace(greetTrigger))
                petAnimator?.SetTrigger(greetTrigger);
            return;
        }
        if (agent.isOnNavMesh) agent.SetDestination(playerTarget.position);
        agent.speed = distance > runDistance ? runSpeed : walkSpeed;
        SetAnimatorFloat(speedParam, agent.velocity.magnitude);
    }

    private void SetState(PetState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        switch (newState)
        {
            case PetState.Idle:
                agent.isStopped = true; SetAnimatorFloat(speedParam, 0f); SetAnimatorBool(sittingParam, false); break;
            case PetState.Following:
                agent.isStopped = false; SetAnimatorBool(sittingParam, false); break;
            case PetState.Sitting:
                agent.isStopped = true; SetAnimatorFloat(speedParam, 0f); SetAnimatorBool(sittingParam, true); break;
            case PetState.RunningRoute:
                agent.isStopped = true; break;
        }
    }

    public void RunRouteIfPossible() { if (!isRunningRoute) StartCoroutine(RunRouteCoroutine()); }

    private IEnumerator RunRouteCoroutine()
    {
        isRunningRoute = true;
        SetState(PetState.RunningRoute);
        foreach (RouteStep step in steps)
        {
            if (step == null) continue;
            if (step.runOnlyOnce && executedStepIds.Contains(step.id)) continue;
            if (!PassesConditions(step.conditions)) continue;
            if (step.delayBefore > 0f) yield return new WaitForSeconds(step.delayBefore);
            yield return ExecuteActions(step.actions);
            if (step.runOnlyOnce && !string.IsNullOrWhiteSpace(step.id)) executedStepIds.Add(step.id);
        }
        isRunningRoute = false;
        SetState(PetState.Idle);
    }

    private bool PassesConditions(List<RouteCondition> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;
        PlayerDecisionState state = PlayerDecisionState.Instance;
        foreach (RouteCondition condition in conditions)
        {
            if (condition == null || string.IsNullOrWhiteSpace(condition.key)) continue;
            if ((state != null && state.GetFlag(condition.key)) != condition.expectedValue) return false;
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
                    action.targetObject?.SetActive(action.boolValue); break;
                case RouteActionType.SetAnimatorTrigger:
                    if (action.targetAnimator != null && !string.IsNullOrWhiteSpace(action.parameterName))
                        action.targetAnimator.SetTrigger(action.parameterName); break;
                case RouteActionType.SetAnimatorBool:
                    if (action.targetAnimator != null && !string.IsNullOrWhiteSpace(action.parameterName))
                        action.targetAnimator.SetBool(action.parameterName, action.boolValue); break;
                case RouteActionType.MoveTransformToPoint:
                    if (action.destinationPoint != null && agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        agent.speed = walkSpeed;
                        agent.SetDestination(action.destinationPoint.position);
                        float timeout = action.moveDuration > 0 ? action.moveDuration * 4f : 10f;
                        float elapsed = 0f;
                        while (elapsed < timeout)
                        {
                            elapsed += Time.deltaTime;
                            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) break;
                            SetAnimatorFloat(speedParam, agent.velocity.magnitude);
                            yield return null;
                        }
                        agent.isStopped = true;
                        SetAnimatorFloat(speedParam, 0f);
                    }
                    break;
                case RouteActionType.SetDecisionFlag:
                    if (!string.IsNullOrWhiteSpace(action.flagKey))
                        PlayerDecisionState.Instance?.SetFlag(action.flagKey, action.flagValue); break;
                case RouteActionType.InvokeUnityEvent:
                    action.customEvent?.Invoke(); break;
            }
        }
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

    private void SetAnimatorFloat(string param, float value)
    { if (petAnimator != null && !string.IsNullOrWhiteSpace(param)) petAnimator.SetFloat(param, value); }

    private void SetAnimatorBool(string param, bool value)
    { if (petAnimator != null && !string.IsNullOrWhiteSpace(param)) petAnimator.SetBool(param, value); }

    private void ConfigureAgent()
    {
        agent.stoppingDistance = stopDistance;
        agent.speed = walkSpeed;
        agent.angularSpeed = 360f;
        agent.acceleration = 12f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    private void EnsureDefaultSetup()
    {
        if (!buildDefaultRouteIfEmpty || steps.Count > 0) return;
        if (string.IsNullOrWhiteSpace(triggerFlagKey)) { triggerFlagKey = "tema_siguiente_desbloqueado"; triggerFlagValue = true; }
        RouteStep step = new RouteStep { id = "default_route_step", runOnlyOnce = true };
        if (defaultMoveTarget == null) { GameObject g = GameObject.Find("AsistenteRutaDestino"); if (g != null) defaultMoveTarget = g.transform; }
        if (defaultAnimator != null && !string.IsNullOrWhiteSpace(defaultAnimatorTrigger))
            step.actions.Add(new RouteAction { actionType = RouteActionType.SetAnimatorTrigger, targetAnimator = defaultAnimator, parameterName = defaultAnimatorTrigger });
        if (defaultMoveTarget != null)
            step.actions.Add(new RouteAction { actionType = RouteActionType.MoveTransformToPoint, targetTransform = transform, destinationPoint = defaultMoveTarget, moveDuration = defaultMoveDuration });
        if (step.actions.Count > 0) steps.Add(step);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;  Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, followStartDistance);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, runDistance);
    }
#endif
}

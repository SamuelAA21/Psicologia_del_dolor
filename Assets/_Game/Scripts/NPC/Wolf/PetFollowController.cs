using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Yarn.Unity;

// Las clases RouteActionType, RouteCondition, RouteAction y RouteStep
// estan definidas en RouteSharedTypes.cs

public enum PetState { Idle, Following, Sitting, RunningRoute }

/// <summary>
/// Controlador de mascota NPC con seguimiento inteligente via NavMesh.
/// Integrado con Yarn Spinner 3.x para reaccionar a variables de dialogo.
/// Requiere: NavMeshAgent + NavMesh bakeado en la escena.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PetFollowController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El Transform del jugador. Si esta vacio se busca por tag 'Player'.")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator petAnimator;

    [Header("Seguimiento")]
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float followStartDistance = 1.8f;
    [SerializeField] private float runDistance = 6f;
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float idleBeforeSitTime = 5f;
    [SerializeField] private bool useNavMeshForFollow;
    [SerializeField] private bool directFollowFallback = true;
    [SerializeField] private float navMeshSampleRadius = 4f;
    [SerializeField] private float directRotationSpeed = 540f;

    [Header("Parametros del Animator")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string sittingParam = "Sitting";
    [SerializeField] private string isWalkingParam = "IsWalking";
    [SerializeField] private string greetTrigger = "";

    [Header("Yarn Spinner")]
    [Tooltip("Variable booleana de Yarn que dispara una ruta (ej: $mascota_activa)")]
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

    private NavMeshAgent agent;
    private InMemoryVariableStorage yarnStorage;
    private PetState currentState = PetState.Idle;
    private bool isRunningRoute;
    private bool navMeshWarningShown;
    private float idleTimer;
    private readonly HashSet<string> executedStepIds = new HashSet<string>(StringComparer.Ordinal);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ConfigureAgent();
        EnsureDefaultSetup();

        yarnStorage = FindAnyObjectByType<InMemoryVariableStorage>();
        if (yarnStorage == null)
            Debug.LogWarning("[PetFollowController] No se encontro InMemoryVariableStorage en la escena.", this);

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
            else
            {
                PlayerController playerController = FindAnyObjectByType<PlayerController>();
                if (playerController != null) playerTarget = playerController.transform;
                else Debug.LogWarning("[PetFollowController] No se encontro jugador por tag 'Player' ni por PlayerController.", this);
            }
        }

        TryPlaceAgentOnNavMesh();
    }

    private void Start()
    {
        if (autoRunOnStart) StartCoroutine(RunRouteCoroutine());
    }

    private void Update()
    {
        if (!isRunningRoute) UpdateFollowBehavior();
    }

    // ── Seguimiento ──────────────────────────────

    private void UpdateFollowBehavior()
    {
        if (playerTarget == null) return;
        float distance = Vector3.Distance(transform.position, playerTarget.position);

        switch (currentState)
        {
            case PetState.Idle:
            case PetState.Sitting:
                HandleIdleOrSitting(distance); break;
            case PetState.Following:
                HandleFollowing(distance); break;
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
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            SetState(PetState.Idle);
            idleTimer = 0f;
            if (!string.IsNullOrWhiteSpace(greetTrigger))
                petAnimator?.SetTrigger(greetTrigger);
            return;
        }

        agent.speed = distance > runDistance ? runSpeed : walkSpeed;
        float movementSpeed = MoveTowardsPlayer(agent.speed);
        petAnimator?.SetBool(isWalkingParam, true);
        SetAnimatorFloat(speedParam, movementSpeed);
    }

    private void SetState(PetState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case PetState.Idle:
                StopAgentIfPossible();
                petAnimator?.SetBool(isWalkingParam, false);
                petAnimator.speed = 1f;
                SetAnimatorFloat(speedParam, 0f);
                SetAnimatorBool(sittingParam, false);
                break;
            case PetState.Following:
                if (useNavMeshForFollow && agent.isOnNavMesh) agent.isStopped = false;
                SetAnimatorBool(sittingParam, false);
                break;
            case PetState.Sitting:
                StopAgentIfPossible();
                petAnimator?.SetBool(isWalkingParam, false);
                petAnimator.speed = 1f;
                SetAnimatorFloat(speedParam, 0f);
                SetAnimatorBool(sittingParam, true);
                break;
            case PetState.RunningRoute:
                StopAgentIfPossible();
                break;
        }
    }

    // ── Sistema de rutas ─────────────────────────

    public void RunRouteIfPossible() { if (!isRunningRoute) StartCoroutine(RunRouteCoroutine()); }

    /// <summary>
    /// Verifica la variable de Yarn y ejecuta la ruta si coincide.
    /// Llamar desde DialogueRunner via evento o comando de Yarn.
    /// </summary>
    public void CheckAndRun()
    {
        if (string.IsNullOrWhiteSpace(triggerYarnVariable)) { RunRouteIfPossible(); return; }
        if (GetYarnBool(triggerYarnVariable) == triggerExpectedValue) RunRouteIfPossible();
    }

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
                case RouteActionType.SetYarnVariable:
                    if (!string.IsNullOrWhiteSpace(action.yarnVariableName))
                        yarnStorage?.SetValue(action.yarnVariableName, action.yarnBoolValue);
                    break;
                case RouteActionType.InvokeUnityEvent:
                    action.customEvent?.Invoke(); break;
            }
        }
    }

    // ── Yarn helpers ─────────────────────────────

    private bool GetYarnBool(string variableName)
    {
        if (yarnStorage == null) return false;
        string key = variableName.StartsWith("$") ? variableName : "$" + variableName;
        yarnStorage.TryGetValue(key, out bool value);
        return value;
    }

    // ── Animator helpers ─────────────────────────

    private void SetAnimatorFloat(string param, float value)
    {
        if (petAnimator != null && !string.IsNullOrWhiteSpace(param))
        {
            petAnimator.SetFloat(param, value);
            petAnimator.speed = Mathf.Clamp(value / walkSpeed, 0.8f, 2.5f);
        }
    }

    private void SetAnimatorBool(string param, float value)
    { if (petAnimator != null && !string.IsNullOrWhiteSpace(param)) petAnimator.SetFloat(param, value); }

    private void SetAnimatorBool(string param, bool value)
    { if (petAnimator != null && !string.IsNullOrWhiteSpace(param)) petAnimator.SetBool(param, value); }

    // ── NavMesh config ───────────────────────────

    private void ConfigureAgent()
    {
        agent.stoppingDistance = stopDistance;
        agent.speed = walkSpeed;
        agent.angularSpeed = 360f;
        agent.acceleration = 12f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.updatePosition = useNavMeshForFollow;
        agent.updateRotation = useNavMeshForFollow;
    }

    private void TryPlaceAgentOnNavMesh()
    {
        if (agent == null || agent.isOnNavMesh)
        {
            return;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return;
        }

        if (useNavMeshForFollow && !navMeshWarningShown)
        {
            navMeshWarningShown = true;
            Debug.LogWarning("[PetFollowController] El lobo no esta sobre un NavMesh cercano. Rehornea el NavMesh o coloca al lobo sobre una zona walkable.", this);
        }
    }

    private float MoveTowardsPlayer(float speed)
    {
        if (playerTarget == null)
        {
            return 0f;
        }

        TryPlaceAgentOnNavMesh();

        if (useNavMeshForFollow)
        {
            if (agent == null || !agent.isOnNavMesh)
            {
                return 0f;
            }

            bool destinationAccepted;
            if (NavMesh.SamplePosition(playerTarget.position, out NavMeshHit playerHit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                destinationAccepted = agent.SetDestination(playerHit.position);
            }
            else
            {
                destinationAccepted = agent.SetDestination(playerTarget.position);
            }

            if (destinationAccepted)
            {
                return agent.velocity.magnitude;
            }

            if (!navMeshWarningShown)
            {
                navMeshWarningShown = true;
                Debug.LogWarning("[PetFollowController] NavMeshAgent no acepto destino hacia el jugador. Revisa que el NavMesh este bakeado y conectado.", this);
            }

            return 0f;
        }

        if (!directFollowFallback)
        {
            return 0f;
        }

        Vector3 targetPosition = playerTarget.position;
        targetPosition.y = transform.position.y;
        Vector3 toTarget = targetPosition - transform.position;
        if (toTarget.sqrMagnitude <= stopDistance * stopDistance)
        {
            return 0f;
        }

        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        Vector3 movement = nextPosition - transform.position;
        if (useNavMeshForFollow && agent != null && agent.isOnNavMesh)
        {
            agent.Move(movement);
        }
        else
        {
            transform.position = nextPosition;
        }

        Vector3 flatDirection = toTarget;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, directRotationSpeed * Time.deltaTime);
        }

        return speed;
    }

    private void StopAgentIfPossible()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    private void EnsureDefaultSetup()
    {
        if (!buildDefaultRouteIfEmpty || steps.Count > 0) return;
        if (string.IsNullOrWhiteSpace(triggerYarnVariable)) triggerYarnVariable = "$mascota_activa";
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

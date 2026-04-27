using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tipos y clases compartidas por CharacterRouteController y PetFollowController.
/// Mantener este archivo en la misma carpeta que ambos scripts.
/// </summary>
public enum RouteActionType
{
    SetGameObjectActive,
    SetAnimatorTrigger,
    SetAnimatorBool,
    MoveTransformToPoint,
    SetDecisionFlag,
    InvokeUnityEvent
}

[Serializable]
public class RouteCondition
{
    public string key;
    public bool expectedValue = true;
}

[Serializable]
public class RouteAction
{
    public RouteActionType actionType;
    public GameObject targetObject;
    public Animator targetAnimator;
    public string parameterName;
    public bool boolValue = true;
    public Transform targetTransform;
    public Transform destinationPoint;
    public float moveDuration = 1.2f;
    public string flagKey;
    public bool flagValue = true;
    public UnityEvent customEvent;
}

[Serializable]
public class RouteStep
{
    public string id = "step";
    public float delayBefore;
    public bool runOnlyOnce = true;
    public List<RouteCondition> conditions = new List<RouteCondition>();
    public List<RouteAction> actions = new List<RouteAction>();
}

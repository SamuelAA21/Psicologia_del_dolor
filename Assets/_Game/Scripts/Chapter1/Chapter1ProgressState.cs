using UnityEngine;

public enum Chapter1MiniGameResult
{
    None,
    Completed,
    Failed
}

public static class Chapter1ProgressState
{
    private static Chapter1MiniGameResult pendingMiniGameResult = Chapter1MiniGameResult.None;
    private static bool pendingDoor1BreathingChallenge;
    private static Vector3 savedPlayerPosition;
    private static Quaternion savedPlayerRotation;
    private static bool hasSavedPlayerTransform;

    public static bool HasCompass { get; private set; }
    public static bool BreathingCompleted { get; private set; }
    public static bool UnderstandApplyCompleted { get; private set; }
    public static bool PendingDoor1BreathingChallenge => pendingDoor1BreathingChallenge;

    public static void MarkCompassObtained()
    {
        HasCompass = true;
    }

    public static void ReportBreathingCompleted()
    {
        BreathingCompleted = true;
        pendingMiniGameResult = Chapter1MiniGameResult.Completed;
    }

    public static void ReportBreathingFailed()
    {
        pendingMiniGameResult = Chapter1MiniGameResult.Failed;
    }

    public static void ReportUnderstandApplyCompleted()
    {
        UnderstandApplyCompleted = true;
    }

    public static void BeginDoor1BreathingChallenge()
    {
        pendingDoor1BreathingChallenge = true;
        pendingMiniGameResult = Chapter1MiniGameResult.None;
    }

    public static void SavePlayerTransform(Vector3 position, Quaternion rotation)
    {
        savedPlayerPosition = position;
        savedPlayerRotation = rotation;
        hasSavedPlayerTransform = true;
    }

    public static bool TryConsumeSavedPlayerTransform(out Vector3 position, out Quaternion rotation)
    {
        position = savedPlayerPosition;
        rotation = savedPlayerRotation;

        if (!hasSavedPlayerTransform)
        {
            return false;
        }

        hasSavedPlayerTransform = false;
        return true;
    }

    public static bool TryGetSavedPlayerTransform(out Vector3 position, out Quaternion rotation)
    {
        position = savedPlayerPosition;
        rotation = savedPlayerRotation;
        return hasSavedPlayerTransform;
    }

    public static void ClearSavedPlayerTransform()
    {
        hasSavedPlayerTransform = false;
    }

    public static Chapter1MiniGameResult ConsumePendingMiniGameResult()
    {
        Chapter1MiniGameResult result = pendingMiniGameResult;
        pendingMiniGameResult = Chapter1MiniGameResult.None;
        return result;
    }

    public static bool ConsumePendingDoor1BreathingChallenge()
    {
        bool pending = pendingDoor1BreathingChallenge;
        pendingDoor1BreathingChallenge = false;
        return pending;
    }
}

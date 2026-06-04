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

    public static bool HasCompass { get; private set; }
    public static bool BreathingCompleted { get; private set; }
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

    public static void BeginDoor1BreathingChallenge()
    {
        pendingDoor1BreathingChallenge = true;
        pendingMiniGameResult = Chapter1MiniGameResult.None;
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

public enum Chapter1MiniGameResult
{
    None,
    Completed,
    Failed
}

public static class Chapter1ProgressState
{
    private static Chapter1MiniGameResult pendingMiniGameResult = Chapter1MiniGameResult.None;

    public static bool HasCompass { get; private set; }
    public static bool BreathingCompleted { get; private set; }

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

    public static Chapter1MiniGameResult ConsumePendingMiniGameResult()
    {
        Chapter1MiniGameResult result = pendingMiniGameResult;
        pendingMiniGameResult = Chapter1MiniGameResult.None;
        return result;
    }
}

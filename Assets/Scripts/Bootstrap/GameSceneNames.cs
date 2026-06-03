public static class GameSceneNames
{
    public const string Bootstrap = "Bootstrap";
    public const string MainMenu = "Interfaz";
    public const string MainGame = "SampleScene";
    public const string BreathingMiniGame = "FirstMiniGame";

    public static bool IsBootstrap(string sceneName)
    {
        return string.Equals(sceneName, Bootstrap, System.StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsMainMenu(string sceneName)
    {
        return string.Equals(sceneName, MainMenu, System.StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsMainGame(string sceneName)
    {
        return string.Equals(sceneName, MainGame, System.StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsBreathingMiniGame(string sceneName)
    {
        return string.Equals(sceneName, BreathingMiniGame, System.StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsGameplayScene(string sceneName)
    {
        return !IsBootstrap(sceneName) && !IsMainMenu(sceneName);
    }
}

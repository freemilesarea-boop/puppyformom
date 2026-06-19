namespace PuppyForMom.Core
{
    /// <summary>
    /// Central list of scene names so we never type magic strings.
    /// Order here matches the Build Settings order: Boot -> MainMenu -> Gameplay -> Ending.
    /// </summary>
    public static class SceneNames
    {
        public const string Boot = "Boot";
        public const string MainMenu = "MainMenu";
        public const string Gameplay = "Gameplay";
        public const string Ending = "Ending";
    }
}

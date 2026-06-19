namespace PuppyForMom.Core
{
    /// <summary>
    /// High level state of a play session, driven by <see cref="GameManager"/>.
    /// </summary>
    public enum GameState
    {
        Boot,
        MainMenu,
        Ready,      // Gameplay loaded, waiting for first tap
        Playing,
        Paused,
        GameOver,
        Cutscene,   // Story beat (e.g. 5000m) pausing the run
        Ending
    }
}

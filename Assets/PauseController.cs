using UnityEngine;

public class PauseController : MonoBehaviour
{
    private static bool _isPaused = false;

    public static void SetPause(bool pause)
    {
        _isPaused = pause;
    }

    public static bool IsPaused => _isPaused;

}

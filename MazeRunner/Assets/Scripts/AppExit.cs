using UnityEngine;

// Application.Quit() only works in a built game. In the editor we have to
// flip EditorApplication.isPlaying instead.
public static class AppExit
{
    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandSurvivor.UI
{
    public class ExitButton : MonoBehaviour
    {
        public void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            Debug.Log("[ExitButton] Exiting play mode");
#else
            Application.Quit();
            Debug.Log("[ExitButton] Quitting application");
#endif
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromFirstSceneButton
{
    static PlayFromFirstSceneButton()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Edit/Play From First Scene %#&p")]
    private static void PlayFromFirstScene()
    {
        // Don't allow starting play mode if already playing
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Already in play mode!");
            return;
        }

        // Check if there are any scenes in the build settings
        if (EditorBuildSettings.scenes.Length == 0)
        {
            Debug.LogWarning("No scenes in Build Settings! Add scenes to Build Settings first.");
            return;
        }

        // Get the first enabled scene
        EditorBuildSettingsScene firstScene = null;
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                firstScene = scene;
                break;
            }
        }

        if (firstScene == null)
        {
            Debug.LogWarning("No enabled scenes in Build Settings!");
            return;
        }

        // Save the current scene if it has been modified
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        // Load the first scene
        EditorSceneManager.OpenScene(firstScene.path);
        
        // Start play mode
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Optional: You can add logic here to restore the previous scene after exiting play mode
        // if desired
    }
}
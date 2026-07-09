using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AutoSave
{
    private static readonly float saveInterval = 60;

    private static double lastSaveTime;
    static AutoSave()
    {
        EditorApplication.update += Update;
    }
    private static void Update()
    {
        if (!Application.isPlaying && EditorApplication.timeSinceStartup - lastSaveTime > saveInterval)
        {
            SaveProject();
            lastSaveTime = EditorApplication.timeSinceStartup;
        }
    }
    private static void SaveProject()
    {
        //Debug.Log("Auto-Saving project...");
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
    }
}


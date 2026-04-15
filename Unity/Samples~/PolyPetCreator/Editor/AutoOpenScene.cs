using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AutoOpenScene
{
    const string CreatorSceneName = "PolyPetCreator";
    const string FarmSceneName = "PolyPetFarm";
    const string PrefKey = "PolyPetCreator_SceneOpened";

    static AutoOpenScene()
    {
        if (SessionState.GetBool(PrefKey, false))
            return;

        SessionState.SetBool(PrefKey, true);

        var activeScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(activeScene.path))
            return;

        EditorApplication.delayCall += () =>
        {
            var creatorScenePath = FindScenePath(CreatorSceneName);
            var farmScenePath = FindScenePath(FarmSceneName);
            EnsureScenesInBuildSettings(creatorScenePath, farmScenePath);

            if (!string.IsNullOrEmpty(creatorScenePath)
                && AssetDatabase.LoadAssetAtPath<SceneAsset>(creatorScenePath) != null)
            {
                EditorSceneManager.OpenScene(creatorScenePath);
            }
        };
    }

    private static void EnsureScenesInBuildSettings(params string[] scenePaths)
    {
        var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var changed = false;

        foreach (var scenePath in scenePaths)
        {
            if (string.IsNullOrEmpty(scenePath))
                continue;

            if (buildScenes.Exists(scene => scene.path == scenePath))
                continue;

            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            changed = true;
        }

        if (changed)
            EditorBuildSettings.scenes = buildScenes.ToArray();
    }

    private static string FindScenePath(string sceneName)
    {
        foreach (var guid in AssetDatabase.FindAssets($"t:Scene {sceneName}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
                return path;
        }

        return string.Empty;
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AutoOpenScene
{
    const string DefaultScenePath = "Assets/PolyPetCreator/PolyPetCreator.unity";
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
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DefaultScenePath) != null)
                EditorSceneManager.OpenScene(DefaultScenePath);
        };
    }
}

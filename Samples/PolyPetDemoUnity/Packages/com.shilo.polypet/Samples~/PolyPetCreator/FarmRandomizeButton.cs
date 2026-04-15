using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class FarmRandomizeButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(OnRandomize);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnRandomize);
    }

    private void OnRandomize()
    {
        foreach (var avatar in FindObjectsByType<PolyPetAvatar>(FindObjectsSortMode.None))
        {
            avatar.RandomizeSeed();
            avatar.RandomizeNameSeed();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class FarmRandomizeButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button == null)
            _button = GetComponentInChildren<Button>(true);
    }

    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnRandomize);
            _button.onClick.AddListener(OnRandomize);
        }
    }

    private void OnDisable()
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

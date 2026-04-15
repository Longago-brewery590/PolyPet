using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SceneToggleButton : MonoBehaviour
{
    [SerializeField] private string _creatorSceneName = "PolyPetCreator";
    [SerializeField] private string _farmSceneName = "PolyPetFarm";
    [SerializeField] private Sprite _farmIcon;
    [SerializeField] private Sprite _creatorIcon;
    [SerializeField] private Image _iconImage;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(OnToggle);

        UpdateIcon();
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnToggle);
    }

    private void OnToggle()
    {
        var current = SceneManager.GetActiveScene().name;
        if (current == _creatorSceneName)
            SceneManager.LoadScene(_farmSceneName);
        else
            SceneManager.LoadScene(_creatorSceneName);
    }

    private void UpdateIcon()
    {
        if (_iconImage == null)
            return;

        var current = SceneManager.GetActiveScene().name;
        if (current == _creatorSceneName)
            _iconImage.sprite = _farmIcon;
        else
            _iconImage.sprite = _creatorIcon;
    }
}

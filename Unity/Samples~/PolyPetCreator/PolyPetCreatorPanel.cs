using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PolyPetCreatorPanel : MonoBehaviour
{
    [SerializeField] private PolyPetAvatar _avatar;
    [SerializeField] private TMP_InputField _nameSeedInput;
    [SerializeField] private TMP_InputField _seedInput;
    [SerializeField] private Button _nameSeedRerollButton;
    [SerializeField] private Button _seedRerollButton;

    private bool _isRefreshing;

    private void OnEnable()
    {
        BindAvatar();
        BindControls();
        RefreshFromAvatar();
    }

    private void OnDisable()
    {
        UnbindControls();
        UnbindAvatar();
    }

    private void OnDestroy()
    {
        UnbindControls();
        UnbindAvatar();
    }

    private void BindAvatar()
    {
        if (_avatar != null)
        {
            _avatar.AddSeedChangedListener(HandleAvatarSeedChanged);
            _avatar.AddNameSeedChangedListener(HandleAvatarNameSeedChanged);
        }
    }

    private void UnbindAvatar()
    {
        if (_avatar != null)
        {
            _avatar.RemoveSeedChangedListener(HandleAvatarSeedChanged);
            _avatar.RemoveNameSeedChangedListener(HandleAvatarNameSeedChanged);
        }
    }

    private void BindControls()
    {
        if (_seedInput != null)
            _seedInput.onEndEdit.AddListener(HandleSeedEdited);

        if (_nameSeedInput != null)
            _nameSeedInput.onEndEdit.AddListener(HandleNameSeedEdited);

        if (_seedRerollButton != null)
            _seedRerollButton.onClick.AddListener(HandleSeedRerollClicked);

        if (_nameSeedRerollButton != null)
            _nameSeedRerollButton.onClick.AddListener(HandleNameSeedRerollClicked);
    }

    private void UnbindControls()
    {
        if (_seedInput != null)
            _seedInput.onEndEdit.RemoveListener(HandleSeedEdited);

        if (_nameSeedInput != null)
            _nameSeedInput.onEndEdit.RemoveListener(HandleNameSeedEdited);

        if (_seedRerollButton != null)
            _seedRerollButton.onClick.RemoveListener(HandleSeedRerollClicked);

        if (_nameSeedRerollButton != null)
            _nameSeedRerollButton.onClick.RemoveListener(HandleNameSeedRerollClicked);
    }

    private void HandleAvatarSeedChanged(PolyPetAvatar avatar, NullableInt seed)
    {
        RefreshSeedField();
    }

    private void HandleAvatarNameSeedChanged(PolyPetAvatar avatar, NullableInt nameSeed)
    {
        RefreshNameSeedField();
    }

    private void HandleSeedEdited(string text)
    {
        if (_isRefreshing || _avatar == null)
            return;

        if (TryParseNullableInt(text, out var seed))
        {
            if (_avatar.Seed != seed)
                _avatar.Seed = seed;

            return;
        }

        RefreshSeedField();
    }

    private void HandleNameSeedEdited(string text)
    {
        if (_isRefreshing || _avatar == null)
            return;

        if (TryParseNullableInt(text, out var nameSeed))
        {
            if (_avatar.NameSeed != nameSeed)
                _avatar.NameSeed = nameSeed;

            return;
        }

        RefreshNameSeedField();
    }

    private void HandleSeedRerollClicked()
    {
        if (_avatar != null)
            _avatar.RandomizeSeed();
    }

    private void HandleNameSeedRerollClicked()
    {
        if (_avatar != null)
            _avatar.RandomizeNameSeed();
    }

    private void RefreshFromAvatar()
    {
        _isRefreshing = true;
        try
        {
            RefreshSeedField();
            RefreshNameSeedField();
            SetControlsInteractable(_avatar != null);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void RefreshSeedField()
    {
        SetInputValue(_seedInput, _avatar != null ? _avatar.Seed : null);
    }

    private void RefreshNameSeedField()
    {
        SetInputValue(_nameSeedInput, _avatar != null ? _avatar.NameSeed : null);
    }

    private void SetInputValue(TMP_InputField inputField, int? value)
    {
        if (inputField == null)
            return;

        var formatted = value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        if (inputField.text != formatted)
            inputField.SetTextWithoutNotify(formatted);
    }

    private void SetControlsInteractable(bool interactable)
    {
        if (_seedInput != null)
            _seedInput.interactable = interactable;

        if (_nameSeedInput != null)
            _nameSeedInput.interactable = interactable;

        if (_seedRerollButton != null)
            _seedRerollButton.interactable = interactable;

        if (_nameSeedRerollButton != null)
            _nameSeedRerollButton.interactable = interactable;
    }

    private static bool TryParseNullableInt(string text, out int? value)
    {
        var trimmed = text != null ? text.Trim() : string.Empty;
        if (trimmed.Length == 0)
        {
            value = null;
            return true;
        }

        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = null;
        return false;
    }
}

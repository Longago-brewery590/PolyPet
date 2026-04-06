using UnityEngine;
using TMPro;

public class PolyPetName : MonoBehaviour
{
    [SerializeField] public PolyPetAvatar Avatar;

    private TMP_Text _text;

    void Start()
    {
        _text = GetComponent<TMP_Text>();
        if (_text == null || Avatar == null)
        {
            if (_text != null) _text.text = "";
            return;
        }

        Avatar.NameSeedChanged += UpdateText;
        UpdateText();
    }

    private void UpdateText()
    {
        _text.text = Avatar.Data.Name ?? "";
    }

    void OnDestroy()
    {
        if (Avatar != null) Avatar.NameSeedChanged -= UpdateText;
    }
}

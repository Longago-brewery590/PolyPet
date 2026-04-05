using UnityEngine;
using TMPro;

public class PolyPetName : MonoBehaviour
{
    [SerializeField] public PolyPet Pet;

    private TMP_Text _text;

    void Start()
    {
        _text = GetComponent<TMP_Text>();
        Pet.NameSeedChanged += UpdateText;
        UpdateText();
    }

    private void UpdateText()
    {
        _text.text = Pet.Data.Name ?? "";
    }

    void OnDestroy()
    {
        if (Pet != null) Pet.NameSeedChanged -= UpdateText;
    }
}

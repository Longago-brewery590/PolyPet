using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PolyPetFarmPanel : MonoBehaviour
{
    [SerializeField] private RectTransform _grid;
    [SerializeField] private GameObject _cardTemplate;
    [SerializeField] private int _count = 40;

    private void Start()
    {
        if (_cardTemplate == null || _grid == null) return;

        _cardTemplate.SetActive(false);

        var rng = new System.Random();
        for (int i = 0; i < _count; i++)
        {
            var card = Instantiate(_cardTemplate, _grid);
            card.name = $"PetCard{i + 1}";
            card.SetActive(true);

            var avatar = card.GetComponentInChildren<PolyPetAvatar>();
            if (avatar != null)
            {
                avatar.Seed = rng.Next();
                avatar.NameSeed = rng.Next();
            }
        }
    }
}

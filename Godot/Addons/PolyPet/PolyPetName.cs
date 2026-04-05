using Godot;

[GlobalClass]
public partial class PolyPetName : Label
{
    [Export] public PolyPet Pet { get; set; } = null!;

    public override void _Ready()
    {
        Pet.NameSeedChanged += UpdateText;
        UpdateText();
    }

    public override void _ExitTree()
    {
        if (Pet != null) Pet.NameSeedChanged -= UpdateText;
    }

    private void UpdateText()
    {
        Text = Pet.Data.Name ?? "";
    }
}

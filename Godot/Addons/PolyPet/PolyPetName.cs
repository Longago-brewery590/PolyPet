using Godot;

[GlobalClass]
public partial class PolyPetName : Label
{
    [Export] public PolyPet Pet { get; set; } = null!;

    public override void _Ready()
    {
        if (Pet == null)
        {
            Text = "";
            return;
        }

        Pet.SeedChanged += UpdateText;
        Pet.NameSeedChanged += UpdateText;
        UpdateText();
    }

    public override void _ExitTree()
    {
        if (Pet == null) return;

        Pet.SeedChanged -= UpdateText;
        Pet.NameSeedChanged -= UpdateText;
    }

    private void UpdateText()
    {
        Text = Pet?.Data.Name ?? "";
    }
}

using Godot;

[GlobalClass]
public partial class PolyPetName : Label
{
    [Export] public PolyPetAvatar Avatar { get; set; } = null!;

    public override void _Ready()
    {
        if (Avatar == null)
        {
            Text = "";
            return;
        }

        Avatar.NameSeedChanged += UpdateText;
        UpdateText();
    }

    public override void _ExitTree()
    {
        if (Avatar == null) return;

        Avatar.NameSeedChanged -= UpdateText;
    }

    private void UpdateText()
    {
        Text = Avatar?.Data.Name ?? "";
    }
}

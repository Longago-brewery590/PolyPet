# PolyPet

Acute-ly polygon pet creator. Procedural 2D generation inspired by Tamagotchi and Peridot. Supports Unity 6.4+ and Godot .NET 4.6+.

## Install

### Unity

**Package Manager (Git URL):**

Add via Window > Package Manager > + > Add package from git URL:

```
https://github.com/Shilo/PolyPet.git?path=Unity
```

**Manual:** Download `PolyPet-Unity-x.y.z.zip` from [Releases](https://github.com/Shilo/PolyPet/releases), extract it into your project's `Packages/com.shilo.polypet/` folder.

### Godot

Download `PolyPet-Godot-x.y.z.zip` from [Releases](https://github.com/Shilo/PolyPet/releases), extract to your project root so `addons/PolyPet/` is created.

## Usage

### Quick Start

Add a `PolyPet` node (Godot) or component (Unity) to your scene. Set a seed in the inspector or leave defaults for a deterministic pet on start.

### API

```csharp
// Properties (setters trigger regeneration)
pet.Seed = 42;           // Regenerates pet
pet.NameSeed = 99;       // Regenerates name

// Randomize
pet.RandomizeSeed();     // Random pet
pet.RandomizeNameSeed(); // Random name

// Events
pet.SeedChanged += () => { };
pet.NameSeedChanged += () => { };

// Read
PolyPetData data = pet.Data;
string? name = pet.Data.Name;
```

### Inspector Settings

| Field | Type | Default | Description |
|---|---|---|---|
| Start Seed | int | 0 | Seed value used on Start |
| Start Name Seed | int | 0 | Name seed value used on Start |
| Start Seed Type | StartSeedType | Fixed | None / Fixed / Random |
| Start Name Seed Type | StartSeedType | Fixed | None / Fixed / Random |

### Display Name

Add a `PolyPetName` component (Label in Godot, requires TextMeshPro in Unity). Set the `Pet` reference. It auto-updates when the name changes.

## Architecture

```
Core/           Shared C# (.NET Standard 2.1) — generation, animation, naming
Godot/          Godot addon — Node2D renderer + Label display
Unity/          Unity package — MonoBehaviour renderer + TMP display
```

## License

MIT

# PolyPet

**Acute-ly** polygon pet creator. A seeded procedural 2D creature generator inspired by Tamagotchi, Peridot, and Geometry Wars, with a shared C# Core library and lightweight adapters for Unity 6.4+ and Godot .NET 4.6+.

## What It Includes

- Deterministic pet generation from an integer seed.
- Optional seeded cute-name generation.
- Shared animation math for idle bobbing and pet/click squish reactions.
- A Unity `MonoBehaviour` renderer plus `PolyPetName` TextMeshPro helper.
- A Godot `Node2D` renderer plus `PolyPetName` label helper, packaged with a minimal editor plugin so the C# nodes register in the "Create New Node" dialog.
- GitHub Actions workflows for syncing Core into engine adapter folders and creating release zips.

The Core generator also produces `BodyPattern` and `HeadPattern` metadata for custom renderers and future visual expansion.

## Requirements

- Unity 6.4+
- Godot .NET 4.6+
- .NET SDK 8.x for local Core development and tests

## Install

### Unity

Add via `Window > Package Manager > + > Add package from git URL`:

```text
https://github.com/Shilo/PolyPet.git?path=Unity
```

Or download `PolyPet-Unity-x.y.z.zip` from [Releases](https://github.com/Shilo/PolyPet/releases) and extract it into `Packages/com.shilo.polypet/`.

### Godot

Download `PolyPet-Godot-x.y.z.zip` from [Releases](https://github.com/Shilo/PolyPet/releases) and extract it into your project root so `addons/PolyPet/` is created.
For Godot .NET, build the project once so the C# addon assembly compiles, then enable `PolyPet` in `Project > Project Settings > Plugins`.

## Quick Start

### Unity

1. Add the package.
2. Create an empty GameObject and add the `PolyPetAvatar` component.
3. Optionally add a TextMeshPro text object and attach `PolyPetName`, then assign its `Avatar` reference.
4. Set `Start Seed` / `Start Name Seed` or switch either seed type to `Random`.

### Godot

1. Copy the addon into `addons/PolyPet/`.
2. Open the project in Godot .NET and build once so `PolyPetEditorPlugin.cs` can compile.
3. Enable `PolyPet` in `Project > Project Settings > Plugins`.
4. Add a `PolyPetAvatar` node from the "Create New Node" dialog.
5. Optionally add a `PolyPetName` label and assign its `Avatar` export to the `PolyPetAvatar` node.
6. Set `Start Seed` / `Start Name Seed` or switch either seed type to `Random`.

## Runtime API

```csharp
// Setters regenerate at runtime when given a value.
pet.Seed = 42;
pet.NameSeed = 99;

// Randomize either seed.
pet.RandomizeSeed();
pet.RandomizeNameSeed();

// Observe updates.
pet.SeedChanged += () => { };
pet.NameSeedChanged += () => { };

// Read the generated data.
PolyPetData data = pet.Data;
string? name = pet.Data.Name;
```

If no `NameSeed` is provided, `pet.Data.Name` remains `null`.

## Inspector Fields

| Field                | Type            | Default | Description                                            |
| -------------------- | --------------- | ------- | ------------------------------------------------------ |
| Start Seed           | `int`           | `0`     | Seed value used when `Start Seed Type` is `Fixed`.     |
| Start Name Seed      | `int`           | `0`     | Name seed used when `Start Name Seed Type` is `Fixed`. |
| Start Seed Type      | `StartSeedType` | `Fixed` | `None`, `Fixed`, or `Random`.                          |
| Start Name Seed Type | `StartSeedType` | `Fixed` | `None`, `Fixed`, or `Random`.                          |

## Repository Layout

```text
Unity/                    Unity package root
Unity/Runtime/            Unity runtime adapter
Godot/                    Godot package root
Godot/addons/PolyPet/     Godot plugin entrypoint plus runtime adapter
Core/                     Shared .NET Standard 2.1 generation library
Core.Tests/               xUnit coverage for generator, names, and animation
Samples/
  PolyPetDemoGodot/       Standalone Godot project that imports the addon
  PolyPetDemoUnity/       Standalone Unity project that imports the package
docs/                     Design documents and references
.impeccable.md            Design context and visual direction
.github/workflows/        Core sync and release automation
```

`Core/` is the source of truth. The engine-specific `Core/` mirrors are populated by CI and should not be edited manually.

## Development

Open [`PolyPet.sln`](./PolyPet.sln) if you want both the [`Core` library](./Core/Core.csproj) and [`Core.Tests` test project](./Core.Tests/Core.Tests.csproj) loaded together in Rider or Visual Studio.

### CLI

For the usual contributor verification flow, run [`verify.ps1`](./verify.ps1) from the repo root:

```powershell
.\verify.ps1
```

Or run the Core verification commands manually:

```powershell
dotnet test .\Core.Tests\Core.Tests.csproj
dotnet build .\Core\Core.csproj --configuration Release
```

### Visual Studio / Rider

This repository does not include a standalone executable app. `Core` is a class library and `Core.Tests` is a test project, so the normal workflow is build plus test rather than a game-style Play button.

1. Open [`PolyPet.sln`](./PolyPet.sln).
2. Build the solution with your IDE's build command, or build the [`Core` project](./Core/Core.csproj) directly from the solution explorer.
3. Open your IDE's unit test or test explorer window.
4. Run all tests in the solution, or run individual tests from [`Core.Tests`](./Core.Tests/Core.Tests.csproj) using the editor gutter or test explorer.

If you open an individual `.csproj` instead of the solution, the project can still build, but your IDE may not give you the same solution-level build and test experience.

### Sample Projects

The `Samples/` folder contains standalone engine projects for testing the addon/package:

- **`Samples/PolyPetDemoGodot/`** — Standalone Godot project with the addon included.
- **`Samples/PolyPetDemoUnity/`** — Standalone Unity project with the package included.

The addon and package folders inside the sample projects are **managed by CI** (copied from `Godot/` and `Unity/` by the sync-core and release workflows) and are gitignored. They are not present after a fresh clone — run the sync workflow or copy manually:

```bash
# Godot sample
cp -r Godot/addons/PolyPet Samples/PolyPetDemoGodot/addons/PolyPet

# Unity sample
cp -r Unity Samples/PolyPetDemoUnity/Packages/com.shilo.polypet
```

In Godot, remember to build the sample project once and enable the `PolyPet` plugin in `Project Settings > Plugins` before expecting `PolyPetAvatar` and `PolyPetName` to appear as global nodes.

## License

MIT

# PolyPet

**Acute-ly** polygon pet creator. A seeded procedural 2D creature generator inspired by Tamagotchi, Peridot, and Geometry Wars, with a shared C# Core library and lightweight adapters for Unity 6.4+ and Godot .NET 4.6+.

## What It Includes

- Deterministic pet generation from an integer seed.
- Optional seeded cute-name generation.
- Shared animation math for idle bobbing and pet/click squish reactions.
- Frame-based avatar fitting that uniformly scales each pet into a caller-provided rectangle without letting animated geometry escape the frame.
- Unity: `PolyPetAvatar` (`MonoBehaviour`) plus `PolyPetName` (`TextMeshPro`), with either scene-space `FrameSize` sizing or `RectTransform` sizing inside a Canvas.
- Godot: `PolyPetAvatar` (`Control`) plus `PolyPetName` (`Label`) in an addon the user enables in the editor.
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
Releases are cut from a fully synced release commit on `main`. The release workflow refreshes engine adapters and sample projects before tagging so release assets and repository state stay aligned.

### Godot

Download `PolyPet-Godot-x.y.z.zip` from [Releases](https://github.com/Shilo/PolyPet/releases) and extract it into your project root so `addons/PolyPet/` is created.
Build the project's C# solution once so Godot can compile the addon scripts, then enable `PolyPet` in `Project > Project Settings > Plugins`.

## Quick Start

### Unity

1. Add the package.
2. Import the **PolyPet Samples** from `Window > Package Manager > PolyPet > Samples > Import` to get the **Creator** scene (avatar display, seed editing, and randomization) and the **Farm** scene (scrollable grid of random pets).
3. Or build your own: create an empty GameObject and add the `PolyPetAvatar` component.
4. Size the avatar by either setting `FrameSize` on the component for scene-space use, or by placing it on a Canvas object with a `RectTransform` and sizing that rect directly.
5. Optionally add a TextMeshPro text object and attach `PolyPetName`, then assign its `Avatar` reference.
6. Set `Start Seed` / `Start Name Seed` or switch either seed type to `Random`.

### Godot

1. Copy the addon into `addons/PolyPet/`.
2. Build the project's C# solution once so `PolyPetAvatar` and `PolyPetName` are compiled and discoverable by the editor.
3. Enable `PolyPet` in `Project > Project Settings > Plugins`.
4. Add a `PolyPetAvatar` node from the "Create New Node" dialog.
5. Size the avatar by resizing the `Control` rect. The pet scales uniformly to fit inside that frame, including its animation envelope.
6. Optionally add a `PolyPetName` label and assign its `Avatar` export to the `PolyPetAvatar` node.
7. Set `Start Seed` / `Start Name Seed` or switch either seed type to `Random`.

## Runtime API

```csharp
// Setters regenerate at runtime when given a value.
pet.Seed = 42;
pet.NameSeed = 99;

// Randomize either seed.
pet.RandomizeSeed();
pet.RandomizeNameSeed();

// Size the pet's hard render frame.
pet.FrameSize = new Vector2(4f, 3f);

// Observe updates from C# in Unity with explicit callback types.
pet.AddSeedChangedListener((PolyPetAvatar avatar, NullableInt seed) =>
{
    int? seedValue = seed;
});
pet.AddNameSeedChangedListener((PolyPetAvatar avatar, NullableInt nameSeed) =>
{
    int? nameSeedValue = nameSeed;
});

// Read the generated data.
PolyPetData data = pet.Data;
string? name = pet.Data.Name;
```

If no `NameSeed` is provided, `pet.Data.Name` remains `null`.

In Unity, `SeedChanged` passes `(avatar, seed)` and `NameSeedChanged` passes `(avatar, nameSeed)`. They are serialized typed `UnityEvent`s for inspector wiring, and `AddSeedChangedListener` / `AddNameSeedChangedListener` expose named callback delegates for code subscriptions. The seed payload uses the serializable `NullableInt` wrapper so `null` and integer values are both represented exactly.
If a `RectTransform` is present under a Canvas, `PolyPetAvatar` uses that rect as its frame; otherwise it renders into the serialized `FrameSize` in scene space.

In Godot, `SeedChanged` passes `(avatar, seed)` and `NameSeedChanged` passes `(avatar, nameSeed)`. The payload is a `Variant` carrying either the integer seed or `null`, and the signals can be connected from the editor after rebuilding the project's C# solution once.
`PolyPetAvatar` inherits `Control`, so resizing the node's rect defines the pet's render frame.

## Inspector Fields

| Field                | Type            | Default   | Description                                                              |
| -------------------- | --------------- | --------- | ------------------------------------------------------------------------ |
| Start Seed           | `int`           | `0`       | Seed value used when `Start Seed Type` is `Fixed`.                       |
| Start Name Seed      | `int`           | `0`       | Name seed used when `Start Name Seed Type` is `Fixed`.                   |
| Start Seed Type      | `StartSeedType` | `Fixed`   | `None`, `Fixed`, or `Random`.                                            |
| Start Name Seed Type | `StartSeedType` | `Fixed`   | `None`, `Fixed`, or `Random`.                                            |
| Frame Size           | `Vector2`       | `(3, 3)`  | Unity scene-space frame when no `RectTransform` is driving the avatar.   |

## Repository Layout

```text
Unity/                    Unity package root
Unity/Runtime/            Unity runtime adapter
Godot/                    Godot package root
Godot/addons/PolyPet/     Godot addon and runtime adapter
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

For the usual contributor verification flow, run [`scripts/verify.ps1`](./scripts/verify.ps1) from the repo root:

```powershell
.\scripts\verify.ps1
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

If you make changes inside the sample-owned addon/package folders and want to copy them back into the main adapter folders, use:

```powershell
.\scripts\sync-godot-addon-from-sample.ps1
.\scripts\sync-unity-package-from-sample.ps1
```

In Godot, build the project's C# solution once, then enable the `PolyPet` plugin in `Project Settings > Plugins` before expecting `PolyPetAvatar` and `PolyPetName` to appear as global nodes.

## License

MIT

PolyPet - seeded procedural polygon pet generator inspired by Tamagotchi, Peridot, and Geometry Wars, with shared C# Core library and Godot/Unity adapters.
Design: cute resolution-independent 2D vector polygon creatures with flat fills, clean edges, kawaii proportions, silhouette-first; Tamagotchi retro simplicity, Peridot procedural variety and warm colors, Geometry Wars geometric clarity and shape-as-identity; bright/saturated palettes, no neon/muddy/dark tones.
- Details: .impeccable.md
Documentation: README.md docs/
Core source of truth: Core/
Core entry points: Core/PolyPetGenerator.cs Core/PolyPetNameGenerator.cs Core/PolyPetAnimation.cs Core/PolyPetData.cs
Unity adapter: Unity/Runtime/PolyPetAvatar.cs Unity/Runtime/PolyPetName.cs Unity/Runtime/Shilo.PolyPet.asmdef Unity/package.json
Godot adapter: Godot/addons/PolyPet/PolyPetAvatar.cs Godot/addons/PolyPet/PolyPetName.cs Godot/addons/PolyPet/plugin.cfg
Tests: Core.Tests/Core.Tests.csproj
Automation: .github/workflows/sync-core.yml .github/workflows/release.yml
Rules: keep Core engine-agnostic; PolyPetName is display-only; seed generation stays in PolyPet
Verify after code changes, run: .\verify.ps1 -NoPause
Limits: sample folders are scaffolding; no sample scenes or Godot/Unity editor compile harness here
PolyPet - seeded procedural polygon pet generator inspired by Tamagotchi, Peridot, and Geometry Wars, with shared C# Core library and Godot/Unity adapters.

Design: .impeccable.md
Documentation: README.md docs/
Core source of truth: Core/
Core entry points: Core/PolyPetGenerator.cs Core/PolyPetNameGenerator.cs Core/PolyPetAnimation.cs Core/PolyPetData.cs
Unity adapter: Unity/Runtime/PolyPet.cs Unity/Runtime/PolyPetName.cs Unity/Runtime/Shilo.PolyPet.asmdef Unity/package.json
Godot adapter: Godot/Addons/PolyPet/PolyPet.cs Godot/Addons/PolyPet/PolyPetName.cs
Tests: Core.Tests/Core.Tests.csproj
Automation: .github/workflows/sync-core.yml .github/workflows/release.yml
Rules: keep Core engine-agnostic; PolyPetName is display-only; seed generation stays in PolyPet
Verify after code changes: dotnet test .\Core.Tests\Core.Tests.csproj; dotnet build .\Core\Core.csproj --configuration Release
Limits: sample folders are scaffolding; no sample scenes or Godot/Unity editor compile harness here
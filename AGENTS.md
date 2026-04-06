# AGENTS.md

This repository contains PolyPet, a procedural polygon pet generator with a shared Core library and engine adapters for Godot and Unity.

## Source Of Truth

- `Core/` is the authoritative implementation for shared generation, naming, animation math, and data structures.
- Do not hand-edit mirrored Core copies under `Godot/Addons/PolyPet/Core/` or `Unity/Runtime/Core/`. Those are CI-managed sync targets.
- Keep Core engine-agnostic. No Godot or Unity types should leak into `Core/`.

## Key Project Areas

- `Core/`: shared .NET Standard 2.1 library
- `Core.Tests/`: xUnit tests for Core behavior
- `Godot/Addons/PolyPet/`: Godot 4.6+ adapter (`PolyPet`, `PolyPetName`)
- `Unity/Runtime/`: Unity 6.4+ adapter (`PolyPet`, `PolyPetName`, asmdef)
- `.github/workflows/`: Core sync and release automation

## Adapter Rules

- For Godot work, use official Godot 4.6 documentation and APIs.
- For Unity work, use official Unity 6.4 ScriptReference APIs.
- Preserve the current architecture: Core generates data, adapters render it and handle engine input/events.
- `PolyPetName` stays display-only. Seed generation logic belongs in `PolyPet`.

## Verification

Run these after Core or adapter changes:

```powershell
dotnet test .\Core.Tests\Core.Tests.csproj
dotnet build .\Core\Core.csproj --configuration Release
```

If work touches Godot or Unity adapters, note that this repo does not currently include an editor-driven compile/test harness in CI or local automation here, so call that out explicitly after verification.

## Repo Notes

- Unity package metadata lives in `Unity/package.json`.
- The Unity assembly definition is `Unity/Runtime/Shilo.PolyPet.asmdef`.
- Sample folders exist as scaffolding, but sample scenes are not currently checked in.
- Local planning docs under `docs/superpowers/` may exist in the workspace but are ignored project-local docs, not shipping assets.

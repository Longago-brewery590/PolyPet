# Sample Parity Audit

Date: 2026-04-15

## Scope

Compared these sample surfaces:

- `Unity/Samples~/PolyPetCreator`
- `Samples/PolyPetDemoUnity/Assets/PolyPetCreator`
- `Godot/addons/PolyPet/Samples`
- `Samples/PolyPetDemoGodot/addons/PolyPet/Samples`

## Parity Summary

Both engines now expose the same sample UX:

- `PolyPetCreator` scene with a large avatar preview, generated name, editable body/name seeds, per-seed reroll buttons, and a scene toggle.
- `PolyPetFarm` scene with a scrollable gallery of 40 pets, a global randomize button, and a scene toggle.
- Warm cream/orange rounded-card styling with icon-only navigation and matching sample iconography.

## Drift Found

Before this sync pass, the sample surfaces were visually close but had two behavior gaps:

- Unity farm randomize was wired through a parent helper object while the actual clickable `Button` lived on a child object, so the sample script never subscribed to clicks.
- Godot farm randomize searched only owned descendants, which skipped the runtime-instantiated pet cards added into the grid, so the button could miss the farm avatars entirely.

There is also one engine-specific implementation difference that remains intentional:

- Unity uses TMP input fields for seed editing, while Godot uses `SpinBox`. The interaction is equivalent, so this was left unchanged rather than forcing a less native control in either engine.

## Fixes Applied

- Unity `FarmRandomizeButton` now falls back to a child `Button` and binds/unbinds safely across enable state changes.
- Unity and Godot avatar rerolls now use a shared adapter-level RNG instead of creating a fresh `System.Random()` for every call, which keeps batch randomization responsive and visually varied.
- Godot farm randomize now searches recursive descendants with `owned = false`, so runtime-created pet cards participate in the bulk reroll.

## Verification Notes

After edits, run:

```powershell
.\scripts\verify.ps1 -NoPause
```

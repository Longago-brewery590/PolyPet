# Avatar Fit Frame Design

**Goal:** Make `PolyPetAvatar` render into an explicit rectangular frame in both Godot and Unity, preserving relative pet size differences while guaranteeing that no generated or animated geometry can leave the frame.

## Approved Rules

- Width and height define a hard frame, not a loose scale hint.
- Rendering must use a uniform fit-to-frame scale and must never stretch pets independently on X and Y.
- The fit must account for worst-case animation motion so idle bob and pet reactions never clip outside the frame.
- Pets may preserve relative size differences inside the shared canonical bounds space.
- Larger pets should reach edge-to-edge when their geometry warrants it; smaller pets may remain smaller when that is part of the generated output.
- Core remains the source of truth for pet-space bounds and fit inputs.

## Shared Bounds Contract

- Add a core utility that computes pet-space bounds from `PolyPetData`.
- The bounds calculation must include every rendered part: body, head, ears, eyes, mouth, limbs, and tail.
- The contract must also expose worst-case animated bounds by applying the maximum translation and scale envelope used by `PolyPetAnimation`.
- Adapters must consume the shared bounds contract instead of inventing separate per-engine extents logic.

## Fit Algorithm

For any requested frame:

1. Compute the pet's worst-case animated bounds in canonical pet space.
2. Compute a single uniform scale using `min(frameWidth / boundsWidth, frameHeight / boundsHeight)`.
3. Translate the pet so the scaled bounds are centered within the frame.
4. Render all geometry through that fitted transform.
5. Use the inverse of the same fitted transform for hit testing so interaction remains aligned after scaling and centering.

This algorithm defines the containment guarantee: no rendered point may land outside the frame.

## Godot Adapter

- Change `Godot/addons/PolyPet/PolyPetAvatar.cs` to inherit from `Control`.
- Use the control's `Size` as the frame dimensions.
- Draw the pet centered within the control using the shared bounds and fit transform.
- Continue to expose the existing seed APIs and redraw behavior.
- Input handling must translate from control-local coordinates back into pet space using the inverse fitted transform.

## Unity Adapter

- Keep `Unity/Runtime/PolyPetAvatar.cs` usable in general 2D scenes instead of making it UI-only.
- Add explicit frame sizing support to the component for non-UI scene usage.
- When a `RectTransform` is present, use its rect as the frame source.
- When a `RectTransform` is not present, use the component's configured frame width and height in local 2D space.
- Render the pet using the same shared fit algorithm in either mode.
- Pointer hit testing must use the inverse fitted transform so interactions remain correct after fitting.

## Documentation Updates

- Update `docs/procedural-pet-generation/design.md` to describe `PolyPetAvatar` as a frame-based renderer instead of a free-size renderer.
- Document that adapters must guarantee full containment for the animated pet, not just the static generated mesh.

## Verification

- Add core tests for geometry bounds across a seed range.
- Add core tests that worst-case animated bounds contain static geometry and remain finite.
- Add adapter contract tests that enforce the new Godot `Control` inheritance and Unity frame-sizing / `RectTransform` integration surface.
- Run `.\scripts\verify.ps1 -NoPause` after implementation.

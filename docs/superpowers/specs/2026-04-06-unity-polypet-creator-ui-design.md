# Unity PolyPet Creator UI Design

Date: 2026-04-06

## Goal

Create a Unity sample scene that mirrors the purpose of the Godot `PolyPetCreator` sample while adopting a better desktop-oriented layout. The new Unity version should use a full Canvas-based UI, place the pet display and controls side-by-side, and present the avatar inside a rounded framed panel with a warm toy-box look.

The Unity sample does not need to match the current Godot sample visually. Instead, Unity becomes the new visual reference, and Godot can later be updated to follow the same direction.

## Scope

In scope:

- Replace the current bare sample scene with a Canvas-based creator UI in the Unity sample project.
- Keep the same sample interactions:
  - Display the generated pet avatar.
  - Display the generated pet name.
  - Edit `NameSeed`.
  - Edit `Seed` for the body/avatar.
  - Randomize `NameSeed`.
  - Randomize `Seed`.
- Use a desktop-friendly two-column layout that fills the screen better than a centered single-column stack.
- Add a rounded frame-like background around the avatar area.
- Use a warm, soft toy-box visual direction with cream/orange surfaces.

Out of scope:

- Matching the current Godot visuals exactly.
- Reworking the underlying seed generation model.
- Adding extra sample features beyond the current creator interactions.
- Building a Unity editor test harness not already present in the repo.

## Layout

The sample scene will use a full-screen `Canvas` with a centered content region.

Primary structure:

- Root background layer spanning the full screen.
- Main content container centered on screen with generous padding.
- Two-column desktop layout:
  - Left column: large avatar display frame.
  - Right column: control panel card.

Column behavior:

- The left avatar column should be visually dominant and consume most of the width.
- The right control panel should remain compact and readable.
- The layout should still degrade acceptably on narrower aspect ratios by reducing spacing and allowing the control panel to remain usable without redesigning the entire scene.

Avatar area:

- Use a rounded rectangular frame panel with visible corner radius.
- Keep the avatar centered within that frame.
- Use a `RectTransform`-driven `PolyPetAvatar` so the sample exercises the intended Canvas/UI render path.
- Size the avatar frame to feel generous on desktop without forcing the pet to touch the edges.

Control panel:

- Separate rounded card from the avatar frame.
- Top: generated pet name, centered and visually prominent.
- Below: two control rows, one for `Name Seed` and one for `Body Seed`.
- Each row should include:
  - label,
  - editable numeric field,
  - reroll button.

The overall composition should preserve the same logical flow as the Godot sample while using side-by-side desktop presentation rather than the current vertical stack.

## Visual Direction

The scene should use a soft toy-box style consistent with PolyPet’s cute, warm direction.

Visual choices:

- Page background: light cream-to-peach gradient or similarly soft warm backdrop.
- Avatar frame: strongest visual emphasis, using a rounded panel with an inset border or framed-window feel.
- Control card: lighter warm surface separated from the page background with shadow or tonal contrast.
- Controls: simple, chunky, readable, and playful rather than minimalist or sterile.
- Colors: bright, warm, and friendly. Avoid dark, muddy, or neon-heavy treatment.

The sample should feel polished enough to demonstrate intended package usage, but it should still remain lightweight and maintainable.

## Components

Expected Unity scene components:

- `Canvas`
- `CanvasScaler`
- `GraphicRaycaster`
- Event system objects if Unity does not already provide them in-scene
- UI panels/images for background and cards
- `PolyPetAvatar` placed on a UI object with `RectTransform`
- name text object bound with `PolyPetName`
- numeric entry controls for the two seeds
- reroll buttons for each seed

Sample-only Unity scripts may be added if needed to keep the scene wiring clean. Likely responsibilities:

- Sync a numeric input field to `PolyPetAvatar.Seed`
- Sync a numeric input field to `PolyPetAvatar.NameSeed`
- Hook reroll buttons to `RandomizeSeed()` and `RandomizeNameSeed()`
- Reflect avatar seed changes back into the displayed controls

These scripts should remain sample/UI glue only and must not push engine-specific sample behavior back into `Core`.

## Data Flow

The scene’s data flow should remain simple and explicit:

1. `PolyPetAvatar` owns the active seed values and generated `PolyPetData`.
2. The avatar regenerates when `Seed` or `NameSeed` changes.
3. `PolyPetName` reads the avatar’s generated name and updates the UI label.
4. UI input controls push edited values into the avatar.
5. Reroll buttons call the avatar randomization methods.
6. Avatar change events push the latest seed values back into the input controls so the UI stays in sync after rerolls.

The sample should avoid hidden logic or duplicated seed state outside the avatar wherever possible.

## Asset Strategy

The sample should prefer lightweight built-in Unity UI assets and simple sample-owned assets over complex dependencies.

Recommended approach:

- Use standard Unity UI objects for layout and controls.
- Use simple sample-owned visual assets only if necessary to achieve the rounded framed-card look cleanly.
- Keep styling assets contained to the Unity sample path so they can later move into `Unity/Samples~`.

If built-in UI visuals are sufficient for the rounded frame/card treatment, prefer them. If they are not sufficient, add the smallest possible sample-owned art assets needed to get a convincing rounded frame result.

## Error Handling and Usability

The sample should prioritize resilience and ease of testing:

- Invalid or partial text entry in numeric fields should not permanently desynchronize the UI from the avatar.
- Seed changes triggered from code or reroll buttons must flow back into the visible controls.
- The scene should still present a valid UI if the avatar starts with random seeds.
- Missing references in the sample should be minimized by explicit serialized wiring in the scene rather than runtime discovery when practical.

The goal is a sample that is easy to inspect in the editor and hard to accidentally miswire.

## File Placement

During iteration, the working scene can live in the Unity sample project under `Samples/PolyPetDemoUnity/Assets/`.

Final intended package/sample placement:

- Unity sample scene and sample-owned assets/scripts move into `Unity/Samples~/PolyPetCreator/`
- The sample project can then be refreshed from the package source as part of the existing sample sync flow

The design should avoid depending on anything that cannot later live under `Unity/Samples~`.

## Testing Strategy

Verification should focus on the existing repo capabilities plus manual Unity validation.

Required verification:

- Build the Unity sample assembly or sample project C# targets from the repo.
- Run the repo verification script: `.\scripts\verify.ps1 -NoPause`
- Manually validate in Unity that:
  - the avatar appears inside the UI frame in Game view,
  - the layout reads correctly on a desktop-sized Game view,
  - the name updates,
  - both seed fields update the avatar,
  - both reroll buttons work,
  - rerolls also update the visible field values.

Because the repo does not include a dedicated Unity editor compile/test harness for scene behavior, final layout and interaction validation remains a manual Unity check.

## Implementation Notes

The implementation should keep the Unity adapter runtime focused on reusable avatar behavior and keep scene orchestration in sample files. If small adapter adjustments are needed to support the UI sample cleanly, they should remain generic and reusable, not sample-specific.

The preferred final result is a Unity sample scene that:

- demonstrates the intended UI-based `PolyPetAvatar` usage,
- feels purposefully designed for desktop,
- becomes the new visual/layout reference for a later Godot alignment pass.

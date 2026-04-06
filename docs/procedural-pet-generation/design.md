# Procedural Pet Generation Design

## Overview

PolyPet is a procedural 2D creature generator for Unity 6.4+ and Godot .NET 4.6+. The feature produces cute polygon-based pets from deterministic seeds, supports lightweight interaction and animation, and can optionally generate a matching pet name.

This document is the canonical design reference for the procedural pet generation feature. It is intended for both human contributors and AI agents working on the project.

## Goals

- Generate a wide variety of visually distinct pets from seeded input.
- Keep core generation logic engine-agnostic and deterministic.
- Support both Unity and Godot through thin engine adapters.
- Make pets feel alive through subtle idle motion and a simple petting reaction.
- Allow optional seeded name generation without making names required.

## Non-Goals

- Persistent pet progression, breeding, or evolution systems.
- Audio, networking, or save/load features.
- Editor tooling beyond what is needed to expose the runtime components.
- 3D rendering or simulation-heavy animation.

## Experience Pillars

### Cute and readable

Every generated pet should feel immediately legible as a character, not random noise. Shape choices, color relationships, and feature placement should favor charm over novelty for its own sake.

### Deterministic and portable

The same seed should produce equivalent pet data regardless of whether the consumer is Unity or Godot. Engine-specific code should render and animate the same underlying pet rather than inventing separate interpretations.

### Lightweight to use

Consumers should be able to drop in a pet component, provide or randomize seeds, and get a complete result without needing to manually assemble sub-parts.

## High-Level Architecture

The system is split into two layers:

- Core: pure C# logic that defines pet data, deterministic generation, naming, and animation math.
- Engine adapters: Unity and Godot wrappers that expose seeds, trigger regeneration, render the generated shapes, and handle simple interaction.

Core owns what a pet is. Engine adapters own how that pet is displayed and interacted with.

## Core Design

### Core responsibilities

The core layer is responsible for:

- Defining engine-agnostic value types for vectors, colors, shape parts, patterns, animation frames, and full pet data.
- Generating complete pet data from a pet seed.
- Generating a pronounceable name from an optional name seed.
- Producing animation offsets from time-based math rather than engine timers or stateful animation systems.

The core layer must not depend on Unity or Godot APIs.

### Pet composition

A generated pet is composed from the following conceptual parts:

- Body
- Head
- Eyes
- Mouth
- Ears
- Limbs
- Tail
- Body pattern
- Head pattern
- Palette colors
- Optional generated name

Each visible part is represented as a simple geometric description that can be rendered by either engine.

### Shape vocabulary

The visual language favors simple, stylized geometry:

- Base solids: triangle, square, pentagon, hexagon, octagon, circle
- Feature variants: pointed, rounded, floppy, star-like, dot-like, fan-like, or stub-like shapes depending on part type
- Surface treatments: solid fill, polkadots, stripes, and spots

The generator should bias decisions toward combinations that stay cute and readable. Not every combination needs equal probability.

### Color design

Each pet uses a small harmonious palette:

- Primary color for the main body mass
- Secondary color for contrast and supporting features
- Tertiary color for accents such as eyes or smaller details

Palettes should stay in a bright, playful range. High-contrast combinations are allowed, but the output should avoid muddy or hostile color relationships.

### Deterministic generation

Given the same pet seed, the generator must produce the same pet data every time. Name generation is also deterministic when a name seed is supplied.

The intended generation flow is:

1. Generate a palette.
2. Choose and size the body.
3. Choose and place the head.
4. Choose eye style and place the eyes symmetrically.
5. Generate a simple mouth.
6. Generate ear geometry based on ear style.
7. Generate either two or four limbs.
8. Generate a tail style, including the option for no tail.
9. Generate independent body and head patterns.
10. Optionally assign a generated name.

### Naming

Name generation should produce short, cute, pronounceable names assembled from a limited syllable pool. The output should feel toy-like and friendly rather than fantasy-lore-heavy or realistic.

Names are optional. A pet remains valid when no name seed is provided.

### Animation math

Animation is intentionally simple and data-driven:

- Idle: a gentle vertical bob over time.
- Being pet: a brief squash-and-stretch reaction with a quick recovery.

The core animation API should return transform-style offsets only. It should not own rendering, timers, or engine state.

## Engine Adapter Design

### Responsibilities

Each engine adapter is responsible for:

- Exposing seed configuration in the editor/inspector.
- Generating initial pet data from configured seeds.
- Regenerating pet or name data when seeds change.
- Rendering the generated geometry using native engine APIs.
- Applying animation offsets each frame.
- Detecting simple click or tap interaction to trigger the pet reaction.

### Seed behavior

Both adapters should support three start modes for both pet seed and name seed:

- None
- Fixed
- Random

The initial generation should be resolved in a single startup pass so the pet does not visibly flash through intermediate states.

### Adapter consistency

Unity and Godot implementations should expose the same conceptual runtime behavior:

- Setting the pet seed regenerates the full pet.
- Setting the name seed regenerates the displayed name without changing the pet identity.
- Randomization helpers are available for both pet and name seeds.
- The current generated data is accessible for display or debugging.

### Name display component

A separate display-oriented component should listen for pet/name updates and show the generated name when one exists. It should remain passive and avoid owning generation logic.

## Rendering Expectations

The rendering strategy should stay intentionally lightweight:

- Filled polygons and circles are the primary building blocks.
- Pattern rendering is layered on top of base shapes.
- Animation is applied through transform offsets rather than skeletal systems.
- The result should be visually appealing in both engines without requiring complex shaders or imported art assets.

Perfect pixel-for-pixel parity between engines is not required, but the same pet should clearly read as the same creature in both environments.

## Interaction Expectations

The pet supports a minimal interaction loop:

- Idle when untouched
- React when clicked or tapped
- Continue idling after the reaction settles

Interaction should feel responsive and pleasant, but it should not introduce gameplay systems or persistent pet state.

## Constraints

- Core code must remain engine-agnostic.
- Generated output must be deterministic from seeds.
- Runtime integration must support Unity 6.4+ and Godot .NET 4.6+.
- The feature should remain lightweight enough to embed in simple demos, UI scenes, or toy experiences.

## Success Criteria

The feature is successful when:

- A contributor can understand the system by reading this design and then locating the relevant runtime code.
- A consumer can instantiate a pet in either supported engine and get a complete generated character.
- Multiple seeds produce obviously distinct but consistently appealing pets.
- The same seed yields the same pet identity across runs and across engines.

## Future Extension Areas

These are plausible extensions, but they are not part of the current feature scope:

- Additional pattern families
- Expanded facial expression options
- More tail, ear, and limb silhouettes
- Batch or zoo-style pet presentation
- Export or shareable seed workflows

## Sample Projects

The repository includes standalone sample projects that are independently distributable:

- `Samples/PolyPetDemoGodot/` — A standalone Godot project with the PolyPet addon copied into `addons/PolyPet/`.
- `Samples/PolyPetDemoUnity/` — A standalone Unity project with the PolyPet package copied into `Packages/com.shilo.polypet/`.

The addon and package folders inside sample projects are managed by CI (sync-core and release workflows copy from the source `Godot/` and `Unity/` directories) and are gitignored. After a fresh clone, contributors must run the sync workflow or copy manually.

Each sample project should contain a demo scene with a `PolyPetAvatar` renderer centered on screen, a `PolyPetName` display above, and buttons to re-roll the pet seed and name seed.

## Out of Scope

- Sound effects
- Saving and loading pets
- Networking
- Breeding or evolution systems
- Full editor tooling suites
- 3D presentation

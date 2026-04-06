# PolyPet Release Workflow Design

Date: 2026-04-06

## Goal

Make releases deterministic so that the release tag, release commit on `main`, packaged engine artifacts, and future sample-project zips all represent the same fully synced repository state.

Specifically, a release must satisfy these rules:

- `main` must already contain synced `Unity/`, `Godot/`, and `Samples/` content before the release tag is created.
- The release tag must point at that fully synced release commit.
- Release assets must be built only after that synced release commit has been pushed successfully.

## Problem

The current workflow family spreads release state across multiple workflows:

- `sync-core.yml` copies `Core/` into engine adapters.
- `sync-godot-addon.yml` copies `Godot/` into the Godot sample project.
- `sync-unity-package.yml` copies `Unity/` into the Unity sample project.
- `release.yml` currently bumps versions and builds archives.

This creates two correctness problems:

1. A failed or skipped sync workflow can leave release inputs stale.
2. Even when all workflows succeed, the release tag can be created before downstream sync commits land on `origin/main`.

That makes the release tag and release assets potentially disagree with the final synced repo state.

## Recommended Approach

Make `release.yml` the single authority for release correctness.

The release workflow should perform all required sync work itself, produce one final release commit, push that exact commit to `origin/main`, then tag and package from that pushed commit.

The existing sync workflows remain useful for normal maintenance on `main`, but release correctness must not depend on them running successfully or finishing in a particular order.

## Why Not Orchestrate Other Workflows

Having `release.yml` dispatch `sync-core.yml` and wait for the follow-on sample sync workflows is possible, but it has avoidable flaws:

- It is harder to reliably associate downstream workflow runs with a specific release attempt.
- It introduces more moving parts and more race surfaces on `main`.
- The order becomes fragile once version bumps are part of release state.
- Tagging only after "all related workflows finished" still requires an additional fetch-and-validate step against `origin/main`.

This is more complex than necessary for a repo whose sync operations are just deterministic file copies.

## Release Workflow Design

`release.yml` should execute the full release pipeline in one job:

1. Check out `main` with full history.
2. Sync `Core/*.cs` into:
   - `Godot/addons/PolyPet/Core/`
   - `Unity/Runtime/Core/`
3. Sync engine deliverables into sample projects:
   - copy `Godot/addons/PolyPet/` into `Samples/PolyPetDemoGodot/addons/PolyPet/`
   - copy `Unity/` into `Samples/PolyPetDemoUnity/Packages/com.shilo.polypet/`
4. Bump release versions:
   - `Unity/package.json`
   - `Godot/addons/PolyPet/plugin.cfg`
5. Stage all synced and versioned release inputs together.
6. Create one release commit on `main`.
7. Rebase that commit onto the latest `origin/main`.
8. Push the release commit to `origin/main`.
9. Create the release tag from the pushed release commit.
10. Push the tag.
11. Build release zips from the already synced workspace.
12. Create the GitHub release and upload the assets.

## Commit and Tag Semantics

The release commit must include all release-relevant filesystem changes:

- `Godot/addons/PolyPet/**`
- `Unity/**`
- `Samples/PolyPetDemoGodot/addons/PolyPet/**`
- `Samples/PolyPetDemoUnity/Packages/com.shilo.polypet/**`

The tag must be created only after the release commit has been pushed successfully.

This ensures:

- browsing the tag shows the same synced content the release used,
- future sample zips can be built from the same tagged commit,
- there is a single canonical release commit instead of a release commit plus later sync commits.

## Concurrency and Safety

All repo-mutating workflows should share a common GitHub Actions `concurrency` group so only one such workflow runs at a time.

Recommended scope:

- `release.yml`
- `sync-core.yml`
- `sync-godot-addon.yml`
- `sync-unity-package.yml`

Recommended behavior:

- `cancel-in-progress: false`

This serializes writes to `main` instead of trying to cancel active workflows midway through a push-oriented job.

The rebase-before-push behavior should stay in place as a second line of defense.

## Role of Existing Sync Workflows

After this design is implemented:

- `sync-core.yml` remains the normal maintenance path when `Core/**` changes on `main`.
- `sync-godot-addon.yml` remains the normal maintenance path when `Godot/**` changes on `main`.
- `sync-unity-package.yml` remains the normal maintenance path when `Unity/**` changes on `main`.

However, release no longer depends on those workflows to establish correctness.

They become convenience maintenance automations, not release prerequisites.

## Packaging Behavior

For the current release artifacts:

- `PolyPet-Godot-x.y.z.zip` should be built from synced `Godot/addons/PolyPet/`.
- `PolyPet-Unity-x.y.z.zip` should be built from synced `Unity/`.

For future expansion:

- sample-project zip assets should also be built from the same release workspace after the synced release commit and tag exist.

This keeps package assets and sample assets aligned to the same released source state.

## Error Handling

Release should fail immediately if any of these steps fail:

- Core sync
- sample sync
- version bump
- commit
- rebase
- push to `main`
- tag creation or tag push
- zip creation
- GitHub release creation

No release asset should be created if the synced release commit has not been pushed.

If the push fails due to concurrent remote movement, the workflow should stop rather than tag a local-only commit.

## Testing and Verification

Implementation verification should cover:

1. Static validation:
   - workflow YAML stays valid
   - paths use lowercase `addons`
2. Behavioral verification:
   - release workflow stages `Samples/` changes as part of the release commit
   - tag creation happens after push, not before
   - release assets are created after the synced release commit exists on `origin/main`
3. Regression protection:
   - normal sync workflows still function for non-release maintenance
   - release no longer assumes prior sync workflows already ran

## Implementation Notes

To keep logic maintainable, the sync copy commands should eventually be centralized in shared scripts so release and maintenance workflows call the same operations.

That is an implementation cleanup, not a design dependency.

The important architectural decision is simpler:

- one release workflow owns release correctness,
- one pushed commit represents the release source state,
- one tag points to that exact pushed commit,
- all release assets are built afterward from that state.

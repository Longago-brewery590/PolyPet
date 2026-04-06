#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
workspace="$(mktemp -d)"
trap 'rm -rf "$workspace"' EXIT

mkdir -p \
  "$workspace/success/Core" \
  "$workspace/success/Godot/addons/PolyPet/Core" \
  "$workspace/success/Godot/addons/PolyPet/Samples" \
  "$workspace/success/Unity/Runtime/Core" \
  "$workspace/success/Samples/PolyPetDemoGodot/addons" \
  "$workspace/success/Samples/PolyPetDemoUnity/Packages" \
  "$workspace/success/scripts" \
  "$workspace/failure/Core" \
  "$workspace/failure/Godot/addons/PolyPet/Core" \
  "$workspace/failure/Godot/addons/PolyPet/Samples" \
  "$workspace/failure/Unity/Runtime/Core" \
  "$workspace/failure/Samples/PolyPetDemoGodot/addons/PolyPet" \
  "$workspace/failure/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet" \
  "$workspace/failure/scripts" \
  "$workspace/outside"

cp "$repo_root"/scripts/sync-core-to-adapters.sh "$workspace/success/scripts/"
cp "$repo_root"/scripts/sync-godot-sample.sh "$workspace/success/scripts/"
cp "$repo_root"/scripts/sync-unity-sample.sh "$workspace/success/scripts/"
cp "$repo_root"/scripts/sync-core-to-adapters.sh "$workspace/failure/scripts/"
cp "$repo_root"/scripts/sync-godot-sample.sh "$workspace/failure/scripts/"
cp "$repo_root"/scripts/sync-unity-sample.sh "$workspace/failure/scripts/"

cat > "$workspace/success/Core/PolyPetAnimation.cs" <<'EOF'
namespace Core;
public sealed class PolyPetAnimation {}
EOF

cat > "$workspace/success/Core/PolyPetData.cs" <<'EOF'
namespace Core;
public sealed class PolyPetData {}
EOF

cat > "$workspace/success/Core/PolyPetGenerator.cs" <<'EOF'
namespace Core;
public sealed class PolyPetGenerator {}
EOF

cat > "$workspace/success/Core/PolyPetNameGenerator.cs" <<'EOF'
namespace Core;
public sealed class PolyPetNameGenerator {}
EOF

cat > "$workspace/success/Godot/addons/PolyPet/PolyPetAvatar.cs" <<'EOF'
// godot avatar marker
EOF

cat > "$workspace/success/Godot/addons/PolyPet/PolyPetName.cs" <<'EOF'
// godot name marker
EOF

cat > "$workspace/success/Godot/addons/PolyPet/plugin.cfg" <<'EOF'
[plugin]
name="PolyPet"
description="Test"
author="Shilo"
version="9.9.9"
script=""
EOF

cat > "$workspace/success/Unity/package.json" <<'EOF'
{ "name": "com.shilo.polypet", "version": "9.9.9" }
EOF

cat > "$workspace/success/Core/Core.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk"></Project>
EOF

(
  cd "$workspace/outside"
  bash "$workspace/success/scripts/sync-core-to-adapters.sh"
  bash "$workspace/success/scripts/sync-godot-sample.sh"
  bash "$workspace/success/scripts/sync-unity-sample.sh"
)

test -f "$workspace/success/Godot/addons/PolyPet/Core/PolyPetGenerator.cs"
test -f "$workspace/success/Unity/Runtime/Core/PolyPetGenerator.cs"
test -f "$workspace/success/Samples/PolyPetDemoGodot/addons/PolyPet/plugin.cfg"
test -f "$workspace/success/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet/package.json"
test ! -f "$workspace/success/Godot/addons/PolyPet/Core/Core.csproj"
test ! -f "$workspace/success/Unity/Runtime/Core/Core.csproj"

cmp "$workspace/success/Godot/addons/PolyPet/plugin.cfg" "$workspace/success/Samples/PolyPetDemoGodot/addons/PolyPet/plugin.cfg"
cmp "$workspace/success/Unity/package.json" "$workspace/success/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet/package.json"

cat > "$workspace/failure/Godot/addons/PolyPet/Core/keep.txt" <<'EOF'
keep me
EOF

cat > "$workspace/failure/Unity/Runtime/Core/keep.txt" <<'EOF'
keep me
EOF

if bash "$workspace/failure/scripts/sync-core-to-adapters.sh"; then
  exit 1
fi

test -f "$workspace/failure/Godot/addons/PolyPet/Core/keep.txt"
test -f "$workspace/failure/Unity/Runtime/Core/keep.txt"

cat > "$workspace/failure/Samples/PolyPetDemoGodot/addons/PolyPet/keep.txt" <<'EOF'
keep me
EOF

if bash "$workspace/failure/scripts/sync-godot-sample.sh"; then
  exit 1
fi

test -f "$workspace/failure/Samples/PolyPetDemoGodot/addons/PolyPet/keep.txt"

cat > "$workspace/failure/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet/keep.txt" <<'EOF'
keep me
EOF

if bash "$workspace/failure/scripts/sync-unity-sample.sh"; then
  exit 1
fi

test -f "$workspace/failure/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet/keep.txt"

echo "sync script smoke test passed"

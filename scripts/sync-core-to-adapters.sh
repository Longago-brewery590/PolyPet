#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="${1:-$script_dir/..}"
repo_root="$(cd "$repo_root" && pwd)"

source_dir="$repo_root/Core"
godot_dest="$repo_root/Godot/addons/PolyPet/Core"
unity_dest="$repo_root/Unity/Runtime/Core"

shopt -s nullglob
core_files=("$source_dir"/*.cs)
if [[ ! -d "$source_dir" || ${#core_files[@]} -eq 0 ]]; then
  exit 1
fi

staging="$(mktemp -d)"
trap 'rm -rf "$staging"' EXIT

mkdir -p "$staging/Godot/addons/PolyPet" "$staging/Unity/Runtime"
cp -R "$source_dir" "$staging/Godot/addons/PolyPet/Core"
cp -R "$source_dir" "$staging/Unity/Runtime/Core"

mkdir -p "$(dirname "$godot_dest")" "$(dirname "$unity_dest")"
rm -rf "$godot_dest" "$unity_dest"
mv "$staging/Godot/addons/PolyPet/Core" "$godot_dest"
mv "$staging/Unity/Runtime/Core" "$unity_dest"

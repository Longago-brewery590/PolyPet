#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="${1:-$script_dir/..}"
repo_root="$(cd "$repo_root" && pwd)"

source_dir="$repo_root/Godot/addons/PolyPet"
dest="$repo_root/Samples/PolyPetDemoGodot/addons/PolyPet"

if [[ ! -d "$source_dir" || ! -f "$source_dir/plugin.cfg" ]]; then
  exit 1
fi

staging="$(mktemp -d)"
trap 'rm -rf "$staging"' EXIT

mkdir -p "$staging/Samples/PolyPetDemoGodot/addons"
cp -R "$source_dir" "$staging/Samples/PolyPetDemoGodot/addons/PolyPet"

mkdir -p "$(dirname "$dest")"
rm -rf "$dest"
mv "$staging/Samples/PolyPetDemoGodot/addons/PolyPet" "$dest"

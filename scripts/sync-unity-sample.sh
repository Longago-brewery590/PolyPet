#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="${1:-$script_dir/..}"
repo_root="$(cd "$repo_root" && pwd)"

source_dir="$repo_root/Unity"
dest="$repo_root/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet"

if [[ ! -d "$source_dir" || ! -f "$source_dir/package.json" ]]; then
  exit 1
fi

staging="$(mktemp -d)"
trap 'rm -rf "$staging"' EXIT

mkdir -p "$staging/Samples/PolyPetDemoUnity/Packages"
cp -R "$source_dir" "$staging/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet"

mkdir -p "$(dirname "$dest")"
rm -rf "$dest"
mv "$staging/Samples/PolyPetDemoUnity/Packages/com.shilo.polypet" "$dest"

# Copy package sample into the standalone project's Assets so the scene is
# available out-of-the-box without a Package Manager import step.
sample_source="$source_dir/Samples~/PolyPetCreator"
sample_dest="$repo_root/Samples/PolyPetDemoUnity/Assets/PolyPetCreator"
if [[ -d "$sample_source" ]]; then
  rm -rf "$sample_dest"
  mkdir -p "$sample_dest"
  cp -R "$sample_source"/. "$sample_dest"
fi

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

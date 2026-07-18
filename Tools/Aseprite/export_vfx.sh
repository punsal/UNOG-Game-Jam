#!/usr/bin/env bash
# Runs the Last Light VFX Aseprite pipeline: templates -> validate -> export.
# Usage: Tools/Aseprite/export_vfx.sh [path-to-aseprite]
set -euo pipefail

DIR="$(cd "$(dirname "$0")" && pwd)"
ASEPRITE="${1:-}"

if [ -z "$ASEPRITE" ]; then
  for candidate in \
    "$(command -v aseprite || true)" \
    "/Applications/Aseprite.app/Contents/MacOS/aseprite" \
    "$HOME/Library/Application Support/Steam/steamapps/common/Aseprite/Aseprite.app/Contents/MacOS/aseprite"; do
    if [ -n "$candidate" ] && [ -x "$candidate" ]; then
      ASEPRITE="$candidate"
      break
    fi
  done
fi

if [ -z "$ASEPRITE" ]; then
  echo "ERROR: aseprite binary not found. Pass its path as the first argument." >&2
  exit 1
fi

echo "Using aseprite: $ASEPRITE"
"$ASEPRITE" -b --script "$DIR/create_vfx_templates.lua"
"$ASEPRITE" -b --script "$DIR/validate_vfx_palette.lua"
"$ASEPRITE" -b --script "$DIR/export_vfx.lua"
echo "VFX export pipeline complete. Re-import in Unity to pick up new sheets."

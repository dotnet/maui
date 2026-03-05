#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — Run All Demos
# Master script that runs each demo in sequence
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — Full Demo Suite                ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""
echo "This will run all non-destructive demos in sequence."
echo "Press Ctrl+C at any time to stop."
echo ""

run_demo() {
  local script="$1"
  local name="$2"
  echo ""
  echo "┌──────────────────────────────────────────────────────────────┐"
  echo "│  Running: $name"
  echo "└──────────────────────────────────────────────────────────────┘"
  echo ""
  bash "$SCRIPT_DIR/$script"
  echo ""
  echo "Press Enter to continue to next demo..."
  read -r
}

run_demo "demo-overview.sh"           "Overview (version, doctor, devices)"
run_demo "demo-android-sdk.sh"        "Android SDK Management"
run_demo "demo-android-emulator.sh"   "Android Emulator Management"
run_demo "demo-json-scripting.sh"     "JSON Output & Scripting"

if [[ "$(uname)" == "Darwin" ]]; then
  run_demo "demo-apple.sh"           "Apple Platform (macOS)"
fi

echo ""
echo "╔══════════════════════════════════════════════════════════════╗"
echo "║                    All demos complete! 🎉                   ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""
echo "Interactive demos to try manually:"
echo "  maui android install              # Interactive SDK setup wizard"
echo "  maui android emulator create      # Pick image → device → name"
echo "  maui android emulator start       # Pick and boot an emulator"
echo "  maui doctor --fix                 # Auto-fix with progress"

#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — Android Emulator Management Demo
# Shows emulator listing, create, start, stop, delete
# =============================================================================
set -e

DEMO_AVD_NAME="maui-demo-avd"

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — Emulator Demo                  ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# 1. List existing emulators
echo "━━━ 1. List Emulators ━━━"
maui android emulator list
echo ""

# 2. Create dry-run (non-interactive with explicit flags)
echo "━━━ 2. Create Emulator (dry-run) ━━━"
maui android emulator create "$DEMO_AVD_NAME" \
  --package "system-images;android-35;google_apis;arm64-v8a" \
  --device "pixel_6" \
  --dry-run
echo ""

# 3. Create for real (uncomment to run)
echo "━━━ 3. Create Emulator ━━━"
echo "  [skipped — uncomment below to create]"
# maui android emulator create "$DEMO_AVD_NAME" \
#   --package "system-images;android-35;google_apis;arm64-v8a" \
#   --device "pixel_6" --force
echo ""

# 4. Start (uncomment to run — requires AVD to exist)
echo "━━━ 4. Start Emulator ━━━"
echo "  [skipped — uncomment below to start]"
# maui android emulator start "$DEMO_AVD_NAME" --wait
echo ""

# 5. Stop (uncomment to run — requires running emulator)
echo "━━━ 5. Stop Emulator ━━━"
echo "  [skipped — uncomment below to stop]"
# maui android emulator stop "$DEMO_AVD_NAME"
echo ""

# 6. Delete (uncomment to run)
echo "━━━ 6. Delete Emulator ━━━"
echo "  [skipped — uncomment below to delete]"
# maui android emulator delete "$DEMO_AVD_NAME"
echo ""

# 7. JSON output
echo "━━━ 7. Emulator List (JSON) ━━━"
maui android emulator list --json
echo ""

echo "✅ Emulator demo complete!"
echo ""
echo "💡 To run the full lifecycle interactively:"
echo "   maui android emulator create my-avd     # interactive prompts"
echo "   maui android emulator start my-avd --wait"
echo "   maui android emulator stop my-avd"
echo "   maui android emulator delete my-avd"

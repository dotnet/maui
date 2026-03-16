#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — Android Emulator Management Demo
# Shows emulator listing, interactive create, interactive start, stop, delete
# =============================================================================
set -e

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — Emulator Demo                  ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# 1. List existing emulators
echo "━━━ 1. List Emulators ━━━"
maui android emulator list
echo ""

# 2. Create dry-run (non-interactive with explicit flags)
echo "━━━ 2. Create Emulator (dry-run, explicit flags) ━━━"
maui android emulator create "maui-demo-avd" \
  --package "system-images;android-35;google_apis;arm64-v8a" \
  --device "pixel_6" \
  --dry-run
echo ""

# 3. JSON output
echo "━━━ 3. Emulator List (JSON) ━━━"
maui android emulator list --json
echo ""

echo "✅ Emulator demo complete!"
echo ""
echo "💡 Interactive commands to try:"
echo ""
echo "  Create — walks you through image → device → name:"
echo "    maui android emulator create"
echo ""
echo "  Start — pick from your existing emulators:"
echo "    maui android emulator start"
echo ""
echo "  Full lifecycle with explicit names:"
echo "    maui android emulator create my-avd --package '...' --device pixel_9"
echo "    maui android emulator start my-avd --wait"
echo "    maui android emulator stop my-avd"
echo "    maui android emulator delete my-avd"

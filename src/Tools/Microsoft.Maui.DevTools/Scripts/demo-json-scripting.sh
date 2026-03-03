#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — JSON Output & Scripting Demo
# Shows how to use --json flag for automation and piping
# =============================================================================
set -e

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — JSON Scripting Demo            ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# Every command supports --json for machine-readable output

# 1. Version as JSON
echo "━━━ 1. Version (JSON) ━━━"
maui version --json
echo ""

# 2. Doctor report as JSON — pipe to jq
echo "━━━ 2. Doctor Report → jq ━━━"
if command -v jq &>/dev/null; then
  echo "  Failed checks:"
  maui doctor --json | jq '[.checks[] | select(.status != "ok")] | length' 2>/dev/null || echo "  (no failures)"
else
  echo "  [install jq for this demo: brew install jq]"
fi
echo ""

# 3. Device list — extract running device IDs
echo "━━━ 3. Running Device IDs ━━━"
if command -v jq &>/dev/null; then
  maui device list --json | jq -r '.devices[]? | select(.state == "booted" or .state == "connected") | .id' 2>/dev/null || echo "  (no running devices)"
else
  maui device list --json
fi
echo ""

# 4. SDK packages — extract installed package paths
echo "━━━ 4. Installed SDK Package Paths ━━━"
if command -v jq &>/dev/null; then
  maui android sdk list --json | jq -r '.[].path' 2>/dev/null || echo "  (none)"
else
  maui android sdk list --json
fi
echo ""

# 5. Emulators as JSON
echo "━━━ 5. Emulator List (JSON) ━━━"
maui android emulator list --json
echo ""

# 6. Conditional scripting example
echo "━━━ 6. Scripting Example: Check if Android SDK ready ━━━"
echo '  if maui android sdk check --json | jq -e ".status == \"ok\"" > /dev/null 2>&1; then'
echo '    echo "SDK ready!"'
echo '  else'
echo '    maui android install --ci --accept-licenses'
echo '  fi'
echo ""

echo "✅ JSON scripting demo complete!"
echo ""
echo "💡 All commands support --json. Combine with jq for powerful automation."

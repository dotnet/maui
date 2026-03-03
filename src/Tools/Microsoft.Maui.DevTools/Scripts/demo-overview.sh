#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — Quick Overview Demo
# Shows version, doctor checks, and device listing
# =============================================================================
set -e

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — Overview Demo                  ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# 1. Version
echo "━━━ 1. Version ━━━"
maui version
echo ""

# 2. Doctor — system health check with spinner
echo "━━━ 2. Doctor (system health check) ━━━"
maui doctor || true
echo ""

# 3. Doctor — JSON output for scripting
echo "━━━ 3. Doctor (JSON output) ━━━"
maui doctor --json 2>/dev/null | head -20 || true
echo "  ... (truncated)"
echo ""

# 4. Device list — all connected devices/emulators
echo "━━━ 4. Device List ━━━"
maui device list
echo ""

# 5. Device list — JSON for scripting
echo "━━━ 5. Device List (JSON) ━━━"
maui device list --json | head -10
echo "  ... (truncated)"
echo ""

echo "✅ Overview demo complete!"

#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — Apple Platform Demo (macOS only)
# Shows Xcode, runtimes, and simulator management
# =============================================================================
set -e

if [[ "$(uname)" != "Darwin" ]]; then
  echo "❌ This demo requires macOS"
  exit 1
fi

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — Apple Platform Demo            ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# 1. Xcode installations
echo "━━━ 1. Xcode Installations ━━━"
maui apple xcode list
echo ""

# 2. Installed runtimes
echo "━━━ 2. iOS Runtimes ━━━"
maui apple runtime list
echo ""

# 3. Available simulators
echo "━━━ 3. Simulators ━━━"
maui apple simulator list
echo ""

# 4. JSON output for automation
echo "━━━ 4. Simulators (JSON) ━━━"
maui apple simulator list --json | head -30
echo "  ... (truncated)"
echo ""

echo "✅ Apple platform demo complete!"

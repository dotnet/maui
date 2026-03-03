#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — Android SDK Management Demo
# Shows JDK check, SDK listing, and install dry-run
# =============================================================================
set -e

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — Android SDK Demo               ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# 1. JDK check
echo "━━━ 1. JDK Status ━━━"
maui android jdk check
echo ""

# 2. SDK check
echo "━━━ 2. SDK Status ━━━"
maui android sdk check
echo ""

# 3. Installed packages (grouped by category/major version)
echo "━━━ 3. Installed SDK Packages ━━━"
maui android sdk list
echo ""

# 4. Available packages (grouped view)
echo "━━━ 4. Available SDK Packages ━━━"
maui android sdk list --available
echo ""

# 5. Both installed and available
echo "━━━ 5. All Packages ━━━"
maui android sdk list --all
echo ""

# 6. Install dry-run — shows what would happen without doing it
echo "━━━ 6. Install (dry-run) ━━━"
maui android install --dry-run
echo ""

# 7. Install specific packages dry-run
echo "━━━ 7. Install Specific Packages (dry-run) ━━━"
maui android install --packages "platforms;android-36,build-tools;36.0.0" --dry-run
echo ""

# 8. SDK list as JSON (for scripting/automation)
echo "━━━ 8. SDK List (JSON) ━━━"
maui android sdk list --json | head -20
echo "  ... (truncated)"
echo ""

echo "✅ Android SDK demo complete!"

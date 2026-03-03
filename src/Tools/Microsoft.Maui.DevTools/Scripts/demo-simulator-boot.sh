#!/bin/bash
# =============================================================================
# MAUI DevTools CLI — Simulator Boot Demo
# Boots an iOS simulator, waits for it to launch, then shuts it down
# =============================================================================
set -e

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║          MAUI DevTools CLI — Simulator Boot Demo            ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# Pick a simulator: default to iPhone Xs on latest iOS, or allow override
UDID="${1:-}"
SIM_NAME=""

if [[ -z "$UDID" ]]; then
  echo "━━━ Finding a simulator to boot ━━━"
  echo ""

  # Find a shutdown iPhone simulator (prefer iPhone Xs or iPhone 17 Pro)
  UDID=$(xcrun simctl list devices available --json | python3 -c "
import json, sys
data = json.load(sys.stdin)
candidates = []
for runtime, devices in data['devices'].items():
    if 'iOS' not in runtime:
        continue
    for d in devices:
        if d['state'] == 'Shutdown' and 'iPhone' in d['name']:
            # Extract iOS version for sorting
            ver = runtime.split('iOS-')[-1].replace('-', '.') if 'iOS-' in runtime else '0'
            priority = 0
            if 'iPhone Xs' in d['name']: priority = 3
            elif 'iPhone 17 Pro' in d['name'] and 'Max' not in d['name']: priority = 2
            elif 'iPhone 11 Pro' in d['name']: priority = 1
            candidates.append((priority, ver, d['udid'], d['name']))

candidates.sort(key=lambda x: (x[0], x[1]), reverse=True)
if candidates:
    print(candidates[0][2])
" 2>/dev/null)

  if [[ -z "$UDID" ]]; then
    echo "❌ No shutdown iPhone simulators found."
    echo "   Create one with: maui apple simulator create \"Test iPhone\" com.apple.CoreSimulator.SimDeviceType.iPhone-17-Pro com.apple.CoreSimulator.SimRuntime.iOS-26-2"
    exit 1
  fi
fi

# Get simulator name
SIM_NAME=$(xcrun simctl list devices --json | python3 -c "
import json, sys
data = json.load(sys.stdin)
for runtime, devices in data['devices'].items():
    for d in devices:
        if d['udid'] == '$UDID':
            print(d['name'])
            sys.exit(0)
print('Unknown')
" 2>/dev/null)

echo "  Simulator: $SIM_NAME"
echo "  UDID:      $UDID"
echo ""

# 1. Show current state
echo "━━━ 1. Before: Simulator State ━━━"
xcrun simctl list devices | grep "$UDID" || true
echo ""

# 2. Boot with spinner
echo "━━━ 2. Booting Simulator ━━━"
maui apple simulator start "$UDID"
echo ""

# 3. Wait for it to be visible in Simulator.app
echo "━━━ 3. Simulator is booted ━━━"
xcrun simctl list devices | grep "$UDID" || true
echo ""

# 4. Pause to see it running
echo "  Simulator is running! Waiting 5 seconds..."
sleep 5

# 5. Shut it down
echo ""
echo "━━━ 4. Shutting Down Simulator ━━━"
maui apple simulator stop "$UDID"
echo ""

# 6. Confirm shutdown
echo "━━━ 5. After: Simulator State ━━━"
xcrun simctl list devices | grep "$UDID" || true
echo ""

echo "✅ Simulator boot demo complete!"
echo ""
echo "💡 Usage:"
echo "   bash demo-simulator-boot.sh                  # auto-picks a simulator"
echo "   bash demo-simulator-boot.sh <UDID>           # specific simulator"

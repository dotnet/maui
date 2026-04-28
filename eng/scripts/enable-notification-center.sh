#!/bin/sh

# Re-enable macOS notifications after UI tests complete.
# Counterpart to disable-notification-center.sh.

export PATH=/usr/bin:/bin:/usr/sbin:/sbin

set +e

echo "=== Re-enabling macOS notifications ==="

# Disable Do Not Disturb
defaults delete com.apple.ncprefs dnd_prefs 2>/dev/null && echo "Do Not Disturb disabled" || echo "Do Not Disturb: already disabled or not supported"

echo "=== Notifications re-enabled ==="
exit 0
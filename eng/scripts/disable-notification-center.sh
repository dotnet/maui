#!/bin/sh

# Suppress macOS system dialogs and notifications that interfere with UI tests.
# This script runs before MacCatalyst UI tests in CI to prevent system-level
# windows (e.g., Apple Sign-In, Setup Assistant) from blocking app interaction
# and appearing in screenshots.

export PATH=/usr/bin:/bin:/usr/sbin:/sbin

set +e  # Don't exit on individual command failures

echo "=== Dismissing macOS system dialogs ==="

# 1. Kill Setup Assistant (shows "Sign In to Apple Account" dialog)
if pgrep -x "Setup Assistant" > /dev/null 2>&1; then
  echo "Found Setup Assistant running, terminating..."
  killall "Setup Assistant" 2>/dev/null
  echo "Setup Assistant terminated"
else
  echo "Setup Assistant not running"
fi

# 2. Prevent Setup Assistant from reappearing
defaults write com.apple.SetupAssistant DidSeeCloudSetup -bool true 2>/dev/null
defaults write com.apple.SetupAssistant LastSeenCloudProductVersion -string "15.0" 2>/dev/null
echo "Setup Assistant preferences updated"

# 3. Enable Do Not Disturb to suppress notification banners
#    Uses defaults write (works without elevated permissions, unlike launchctl unload)
defaults write com.apple.ncprefs dnd_prefs -data 62706C6973743030D60102030405060708080A08085B646E644D6972726F7265645F100F646E64446973706C6179536C6565705F101E72657065617465644661636574696D6543616C6C73427265616B73444E445E646E64446973706C61794C6F636B5F10136661636574696D6543616E427265616B444E4409080808 2>/dev/null && echo "Do Not Disturb enabled" || echo "Do Not Disturb: defaults write not supported on this version"

# 4. Also try killing any other system dialogs that might interfere
for proc in "UserNotificationCenter" "CoreServicesUIAgent"; do
  if pgrep -x "$proc" > /dev/null 2>&1; then
    echo "Terminating $proc..."
    killall "$proc" 2>/dev/null
  fi
done

echo "=== System dialog suppression complete ==="

# 5. Force system-wide Light appearance for consistent UI test screenshots.
#    macOS 26 (Tahoe) CI VMs default to dark mode, which causes screenshot
#    mismatches against light-mode baseline images.
echo "=== Forcing Light appearance ==="
defaults write -g AppleInterfaceStyle -string Light 2>/dev/null
defaults delete -g AppleInterfaceStyle 2>/dev/null && echo "System appearance set to Light" || echo "Already using Light appearance"

exit 0
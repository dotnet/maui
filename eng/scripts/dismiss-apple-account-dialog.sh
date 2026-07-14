#!/bin/sh

# Dismiss the macOS "Sign in to your Apple Account" Setup Assistant modal that
# can appear on CI mac agents and block MacCatalyst UI automation.
#
# Symptom this fixes: on the shared mac pool the Setup Assistant iCloud pane
# ("Sign In to Your Apple Account") is presented full-screen on top of the app
# under test. Appium's mac2 driver then cannot see any of the app's elements, so
# EVERY UI test fails identically with "System.TimeoutException: Timed out
# waiting for element" (observed: 391/391 CollectionView catalyst tests failed,
# each tear-down screenshot showing the sign-in pane covering the app). Because
# each failure burns the ~15s WaitForElement timeout, the category also blows
# the per-category time budget and produces no TRX.
#
# We (a) kill the presenting process to dismiss any modal already on screen and
# (b) set the Setup Assistant "seen" flags for the console user so it does not
# reappear during the run. Everything is best-effort — this script must never
# fail the build.

export PATH=/usr/bin:/bin:/usr/sbin:/sbin

currentUser=$( echo "show State:/Users/ConsoleUser" | scutil | awk '/Name :/ { print $3 }' )

if [ -z "$currentUser" ] || [ "$currentUser" = "loginwindow" ]; then
  echo "No console user logged in — no Apple Account dialog to dismiss."
  exit 0
fi

uid=$(id -u "$currentUser")

runAsUser() {
  launchctl asuser "$uid" sudo -u "$currentUser" "$@"
}

echo "Dismissing Setup Assistant / Apple Account sign-in modal for console user '$currentUser'..."

# (b) Prevent recurrence FIRST: mark the Setup Assistant panes as already seen so
# that when the current instance is killed the system does not immediately
# re-present it.
for key in DidSeeCloudSetup DidSeeSiriSetup DidSeePrivacy DidSeeTrueTone \
           DidSeeAppearanceSetup DidSeeSyncSetup2 DidSeeAppleIDSetup; do
  runAsUser defaults write com.apple.SetupAssistant "$key" -bool TRUE 2>/dev/null || true
done
runAsUser defaults write com.apple.SetupAssistant GestureMovieSeen none 2>/dev/null || true
sudo defaults write /var/db/com.apple.SetupAssistant LastSeenCloudProductVersion "$(sw_vers -productVersion 2>/dev/null)" 2>/dev/null || true
sudo defaults write /var/db/com.apple.SetupAssistant LastSeenBuddyBuildVersion "$(sw_vers -buildVersion 2>/dev/null)" 2>/dev/null || true

# (a) Dismiss any modal already on screen by killing its presenter.
runAsUser killall "Setup Assistant" 2>/dev/null || true
sudo killall "Setup Assistant" 2>/dev/null || true
# accountsd re-checks Apple Account state; restarting it clears a stuck prompt
# and it will re-read the "seen" flags above on relaunch. Harmless (auto-restarts).
runAsUser killall accountsd 2>/dev/null || true

echo "Apple Account dialog dismissal complete (best-effort)."
exit 0

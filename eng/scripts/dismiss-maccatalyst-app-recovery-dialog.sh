#!/bin/sh

# Prevent and dismiss the macOS app-restoration alert that can appear after the
# MacCatalyst HostApp is force-killed:
#
#   "The last time you opened Controls.TestCases.HostApp, it unexpectedly quit
#    while reopening windows. Do you want to try to reopen its windows again?"
#
# The alert owns the HostApp process but exposes none of the test page's
# accessibility elements. Once present, every later Appium fixture and category
# fails with the same WaitForElement timeout even though the app reports state 4
# (running in the foreground). Everything here is best-effort and narrowly
# scoped to the UI-test HostApp.

export PATH=/usr/bin:/bin:/usr/sbin:/sbin

scriptDir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
. "$scriptDir/run-as-console-user.sh"

bundleId="com.microsoft.maui.uitests"
processName="Controls.TestCases.HostApp"
processPattern='(^|/)Controls[.]TestCases[.]HostApp($| )'
currentUser=$(echo "show State:/Users/ConsoleUser" | scutil | awk '/Name :/ { print $3 }')

if [ -z "$currentUser" ] || [ "$currentUser" = "loginwindow" ]; then
  echo "No console user logged in — no MacCatalyst recovery dialog to dismiss."
  exit 0
fi

uid=$(id -u "$currentUser")
userHome=$(dscl . -read "/Users/$currentUser" NFSHomeDirectory 2>/dev/null | awk -F': ' '/NFSHomeDirectory:/ { print $2; exit }')

echo "Preparing MacCatalyst HostApp recovery state for console user '$currentUser'..."

# Disable AppKit state restoration before touching the current process. This is
# what prevents a future SIGKILL/timeout recovery from presenting the alert
# again inside the same long-running category.
run_as_console_user "$currentUser" "$uid" defaults write -g ApplePersistenceIgnoreState -bool true 2>/dev/null || true
run_as_console_user "$currentUser" "$uid" defaults write -g NSQuitAlwaysKeepsWindows -bool false 2>/dev/null || true
run_as_console_user "$currentUser" "$uid" defaults write "$bundleId" ApplePersistenceIgnoreState -bool true 2>/dev/null || true
run_as_console_user "$currentUser" "$uid" defaults write "$bundleId" NSQuitAlwaysKeepsWindows -bool false 2>/dev/null || true

# Dismiss an alert that is already visible. Use both apostrophe spellings
# exposed by different macOS accessibility versions, then fall back to the
# second button only on a window whose text identifies the recovery alert.
run_as_console_user "$currentUser" "$uid" osascript <<'APPLESCRIPT' >/dev/null 2>&1 || true
with timeout of 5 seconds
	tell application "System Events"
		if exists process "Controls.TestCases.HostApp" then
			tell process "Controls.TestCases.HostApp"
				repeat with appWindow in windows
					set isRecoveryAlert to false
					repeat with labelElement in static texts of appWindow
						try
							if (value of labelElement as text) contains "unexpectedly quit" then
								set isRecoveryAlert to true
								exit repeat
							end if
						end try
					end repeat

					if isRecoveryAlert then
						if exists button "Don't Reopen" of appWindow then
							click button "Don't Reopen" of appWindow
						else if exists button "Don’t Reopen" of appWindow then
							click button "Don’t Reopen" of appWindow
						else if (count of buttons of appWindow) is greater than or equal to 2 then
							click button 2 of appWindow
						end if
						exit repeat
					end if
				end repeat
			end tell
		end if
	end tell
end timeout
APPLESCRIPT

# The app should not be running before Appium creates or reuses its session.
# A graceful TERM also closes a recovery alert when UI scripting is unavailable.
processIds=$(run_as_console_user "$currentUser" "$uid" pgrep -f "$processPattern" 2>/dev/null || true)
for processId in $processIds; do
  case "$processId" in
    *[!0-9]*|'') continue ;;
  esac
  processCommand=$(run_as_console_user "$currentUser" "$uid" ps -p "$processId" -o command= 2>/dev/null || true)
  case "$processCommand" in
    "$processName"|*/"$processName"|*/"$processName"\ *)
      run_as_console_user "$currentUser" "$uid" kill "$processId" 2>/dev/null || true
      ;;
  esac
done
sleep 1

# Remove only this app's exact saved-state directories. The standard location
# covers ordinary apps; the container location covers sandboxed Catalyst apps.
if [ -n "$userHome" ]; then
  for savedState in \
    "$userHome/Library/Saved Application State/$bundleId.savedState" \
    "$userHome/Library/Containers/$bundleId/Data/Library/Saved Application State/$bundleId.savedState"; do
    if [ -e "$savedState" ]; then
      run_as_console_user "$currentUser" "$uid" find "$savedState" -depth -delete 2>/dev/null || true
    fi
  done
fi

echo "MacCatalyst HostApp recovery preparation complete (best-effort)."
exit 0

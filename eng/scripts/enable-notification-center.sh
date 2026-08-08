#!/bin/sh

export PATH=/usr/bin:/bin:/usr/sbin:/sbin

scriptDir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
. "$scriptDir/run-as-console-user.sh"

currentUser=$(echo "show State:/Users/ConsoleUser" | scutil | awk '/Name :/ { print $3 }')

if [ -z "$currentUser" ] || [ "$currentUser" = "loginwindow" ]; then
  echo "No console user logged in — Notification Center enable is not needed."
  exit 0
fi

uid=$(id -u "$currentUser")
servicePlist="/System/Library/LaunchAgents/com.apple.notificationcenterui.plist"
serviceDomain="gui/$uid"

if [ ! -r "$servicePlist" ]; then
  echo "##vso[task.logissue type=warning]Could not read the Notification Center launch agent plist; continuing without changing host state."
  exit 0
fi

serviceLabel=$(/usr/libexec/PlistBuddy -c 'Print :Label' "$servicePlist" 2>/dev/null)
serviceLabelStatus=$?

if [ "$serviceLabelStatus" -ne 0 ] || [ -z "$serviceLabel" ]; then
  echo "##vso[task.logissue type=warning]Could not resolve the Notification Center launch agent identity; continuing without changing host state."
  exit 0
fi

serviceTarget="$serviceDomain/$serviceLabel"
diagnosticLog=$(mktemp "${TMPDIR:-/tmp}/maui-notification-center-enable.XXXXXX")
if [ -z "$diagnosticLog" ]; then
  echo "##vso[task.logissue type=warning]Could not create a Notification Center diagnostics file; continuing without changing host state."
  exit 0
fi
trap 'rm -f "$diagnosticLog"' EXIT HUP INT TERM

is_service_disabled() {
  printf '%s\n' "$1" | awk -v label="$serviceLabel" '
    index($0, "\"" label "\"") && ($NF == "disabled" || $NF == "true") { found = 1 }
    END { exit found ? 0 : 1 }
  '
}

if ! run_as_console_user "$currentUser" "$uid" launchctl enable "$serviceTarget" >"$diagnosticLog" 2>&1; then
  echo "##vso[task.logissue type=warning]Could not re-enable the Notification Center launch agent for '$currentUser'."
  sed 's/^/  /' "$diagnosticLog"
  exit 0
fi

# Bootstrap is harmless when launchd already has the job; verification below is
# based on launchd's persisted disabled state rather than this command's output.
run_as_console_user "$currentUser" "$uid" launchctl bootstrap "$serviceDomain" "$servicePlist" >>"$diagnosticLog" 2>&1 || true

disabledState=$(run_as_console_user "$currentUser" "$uid" launchctl print-disabled "$serviceDomain" 2>>"$diagnosticLog")
disabledStateStatus=$?
run_as_console_user "$currentUser" "$uid" launchctl print "$serviceTarget" >>"$diagnosticLog" 2>&1
serviceStateStatus=$?

if [ "$disabledStateStatus" -ne 0 ] ||
    is_service_disabled "$disabledState" ||
    [ "$serviceStateStatus" -ne 0 ]; then
  echo "##vso[task.logissue type=warning]Could not verify that Notification Center was re-enabled for '$currentUser' after cleanup."
  sed 's/^/  /' "$diagnosticLog"
else
  echo "Notification Center enabled for '$currentUser' (verified)."
fi

exit 0

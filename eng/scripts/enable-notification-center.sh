#!/bin/sh

export PATH=/usr/bin:/bin:/usr/sbin:/sbin

scriptDir=$(CDPATH= cd "$(dirname "$0")" && pwd)
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
serviceProgram=$(/usr/libexec/PlistBuddy -c 'Print :Program' "$servicePlist" 2>/dev/null)
serviceProgramStatus=$?
if [ "$serviceProgramStatus" -ne 0 ] || [ -z "$serviceProgram" ]; then
  serviceProgram=$(/usr/libexec/PlistBuddy -c 'Print :ProgramArguments:0' "$servicePlist" 2>/dev/null)
  serviceProgramStatus=$?
fi
serviceProcess=$(basename "$serviceProgram")

if [ "$serviceLabelStatus" -ne 0 ] ||
    [ "$serviceProgramStatus" -ne 0 ] ||
    [ -z "$serviceLabel" ] ||
    [ -z "$serviceProcess" ]; then
  echo "##vso[task.logissue type=warning]Could not resolve the Notification Center launch agent identity; continuing without changing host state."
  exit 0
fi

serviceTarget="$serviceDomain/$serviceLabel"
diagnosticLog=$(mktemp "${TMPDIR:-/tmp}/maui-notification-center-enable.XXXXXX" 2>/dev/null)
diagnosticLogStatus=$?
if [ "$diagnosticLogStatus" -ne 0 ] ||
    [ -z "$diagnosticLog" ] ||
    [ ! -f "$diagnosticLog" ]; then
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

process_is_suspended() {
  state=$(/bin/ps -o state= -p "$1" 2>>"$diagnosticLog" | tr -d '[:space:]')
  case "$state" in
    T*) return 0 ;;
    *) return 1 ;;
  esac
}

# Always resume an exact NotificationCenter process before launchd recovery.
# This also repairs the host when a prior disable step used the SIP fallback.
runningPids=$(/usr/bin/pgrep -u "$uid" -x "$serviceProcess" 2>>"$diagnosticLog")
processCheckStatus=$?
if [ "$processCheckStatus" -le 1 ]; then
  for pid in $runningPids; do
    case "$pid" in
      *[!0-9]*|'') continue ;;
    esac
    run_as_console_user "$currentUser" "$uid" kill -CONT "$pid" >>"$diagnosticLog" 2>&1 || true
  done
fi

run_as_console_user "$currentUser" "$uid" launchctl enable "$serviceTarget" >>"$diagnosticLog" 2>&1 || true

# Bootstrap is harmless when launchd already has the job; verification below is
# based on launchd's persisted disabled state rather than this command's output.
run_as_console_user "$currentUser" "$uid" launchctl bootstrap "$serviceDomain" "$servicePlist" >>"$diagnosticLog" 2>&1 || true

disabledState=$(run_as_console_user "$currentUser" "$uid" launchctl print-disabled "$serviceDomain" 2>>"$diagnosticLog")
disabledStateStatus=$?
run_as_console_user "$currentUser" "$uid" launchctl print "$serviceTarget" >>"$diagnosticLog" 2>&1
serviceStateStatus=$?
runningPids=$(/usr/bin/pgrep -u "$uid" -x "$serviceProcess" 2>>"$diagnosticLog")
processCheckStatus=$?
stoppedPids=
if [ "$processCheckStatus" -le 1 ]; then
  for pid in $runningPids; do
    case "$pid" in
      *[!0-9]*|'') continue ;;
    esac
    if process_is_suspended "$pid"; then
      stoppedPids="${stoppedPids}${stoppedPids:+ }$pid"
    fi
  done
fi

if [ "$disabledStateStatus" -ne 0 ] ||
    is_service_disabled "$disabledState" ||
    [ "$serviceStateStatus" -ne 0 ] ||
    [ "$processCheckStatus" -gt 1 ] ||
    [ -n "$stoppedPids" ]; then
  echo "##vso[task.logissue type=warning]Could not verify that Notification Center was re-enabled for '$currentUser' after cleanup."
  {
    printf 'launchctl print-disabled status: %s\n' "$disabledStateStatus"
    printf 'launchctl service status: %s\n' "$serviceStateStatus"
    printf 'pgrep status: %s\n' "$processCheckStatus"
    printf 'matching process IDs: %s\n' "${runningPids:-none}"
    printf 'still-suspended process IDs: %s\n' "${stoppedPids:-none}"
  } >>"$diagnosticLog"
  sed 's/^/  /' "$diagnosticLog"
else
  echo "Notification Center enabled for '$currentUser' (verified)."
fi

exit 0

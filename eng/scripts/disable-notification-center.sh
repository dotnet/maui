#!/bin/sh

export PATH=/usr/bin:/bin:/usr/sbin:/sbin

scriptDir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
. "$scriptDir/run-as-console-user.sh"

currentUser=$(echo "show State:/Users/ConsoleUser" | scutil | awk '/Name :/ { print $3 }')

if [ -z "$currentUser" ] || [ "$currentUser" = "loginwindow" ]; then
  echo "No console user logged in — Notification Center disable is not needed."
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
diagnosticLog=$(mktemp "${TMPDIR:-/tmp}/maui-notification-center-disable.XXXXXX")
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

processes_are_suspended() {
  [ -n "$1" ] || return 1

  for pid in $1; do
    state=$(/bin/ps -o state= -p "$pid" 2>>"$diagnosticLog" | tr -d '[:space:]')
    case "$state" in
      T*) ;;
      *) return 1 ;;
    esac
  done

  return 0
}

append_verification_details() {
  disabledLine=$(printf '%s\n' "$disabledState" | awk -v label="$serviceLabel" 'index($0, "\"" label "\"") { print; exit }')
  {
    printf 'launchctl print-disabled status: %s\n' "$disabledStateStatus"
    printf 'launchctl state for %s: %s\n' "$serviceLabel" "${disabledLine:-not reported}"
    printf 'pgrep status: %s\n' "$processCheckStatus"
    printf 'matching process IDs: %s\n' "${runningPids:-none}"
  } >>"$diagnosticLog"
}

# Repair a process left suspended by an interrupted earlier run before applying
# this run's disable sequence. SIGCONT is harmless for an active process.
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

if ! run_as_console_user "$currentUser" "$uid" launchctl disable "$serviceTarget" >"$diagnosticLog" 2>&1; then
  echo "##vso[task.logissue type=warning]Could not disable the Notification Center launch agent for '$currentUser'."
  sed 's/^/  /' "$diagnosticLog"
  exit 0
fi

# Legacy plist unloading is deprecated and can print an I/O error while
# returning success. Disable the service in the user's GUI domain, then try to
# remove or terminate the exact running instance before using the SIP fallback.
run_as_console_user "$currentUser" "$uid" launchctl bootout "$serviceTarget" >>"$diagnosticLog" 2>&1 || true

attempt=0
while [ "$attempt" -lt 5 ]; do
  runningPids=$(/usr/bin/pgrep -u "$uid" -x "$serviceProcess" 2>>"$diagnosticLog")
  processCheckStatus=$?
  if [ "$processCheckStatus" -gt 1 ] || [ -z "$runningPids" ]; then
    break
  fi

  for pid in $runningPids; do
    case "$pid" in
      *[!0-9]*|'') continue ;;
    esac
    run_as_console_user "$currentUser" "$uid" kill "$pid" >>"$diagnosticLog" 2>&1 || true
  done

  attempt=$((attempt + 1))
  sleep 1
done

disabledState=$(run_as_console_user "$currentUser" "$uid" launchctl print-disabled "$serviceDomain" 2>>"$diagnosticLog")
disabledStateStatus=$?
runningPids=$(/usr/bin/pgrep -u "$uid" -x "$serviceProcess" 2>>"$diagnosticLog")
processCheckStatus=$?

if [ "$disabledStateStatus" -eq 0 ] &&
    [ "$processCheckStatus" -le 1 ] &&
    is_service_disabled "$disabledState" &&
    [ -z "$runningPids" ]; then
  echo "Notification Center disabled for '$currentUser' (verified)."
  exit 0
fi

# SIP can prevent bootout of Apple's protected launch agent even after
# launchctl disable succeeds. Suspend only the exact remaining process instead;
# launchd still sees it as alive, so it cannot immediately respawn.
attempt=0
suspendedPids=
while [ "$attempt" -lt 5 ]; do
  runningPids=$(/usr/bin/pgrep -u "$uid" -x "$serviceProcess" 2>>"$diagnosticLog")
  processCheckStatus=$?
  if [ "$processCheckStatus" -gt 1 ]; then
    break
  fi
  if [ -z "$runningPids" ]; then
    disabledState=$(run_as_console_user "$currentUser" "$uid" launchctl print-disabled "$serviceDomain" 2>>"$diagnosticLog")
    disabledStateStatus=$?
    if [ "$disabledStateStatus" -eq 0 ] && is_service_disabled "$disabledState"; then
      echo "Notification Center disabled for '$currentUser' (verified)."
      exit 0
    fi
    break
  fi

  for pid in $runningPids; do
    case "$pid" in
      *[!0-9]*|'') continue ;;
    esac
    run_as_console_user "$currentUser" "$uid" kill -STOP "$pid" >>"$diagnosticLog" 2>&1 || true
  done

  sleep 1
  runningPids=$(/usr/bin/pgrep -u "$uid" -x "$serviceProcess" 2>>"$diagnosticLog")
  processCheckStatus=$?
  if [ "$processCheckStatus" -le 1 ] && processes_are_suspended "$runningPids"; then
    verifiedPids=$runningPids
    sleep 1
    runningPids=$(/usr/bin/pgrep -u "$uid" -x "$serviceProcess" 2>>"$diagnosticLog")
    processCheckStatus=$?
    if [ "$processCheckStatus" -le 1 ] &&
        [ "$runningPids" = "$verifiedPids" ] &&
        processes_are_suspended "$runningPids"; then
      suspendedPids=$runningPids
      break
    fi
  fi

  attempt=$((attempt + 1))
done

if [ -n "$suspendedPids" ]; then
  echo "Notification Center suspended for '$currentUser' (verified SIP fallback; PIDs: $suspendedPids)."
else
  append_verification_details
  echo "##vso[task.logissue type=warning]Could not verify that Notification Center stopped for '$currentUser'; Catalyst UI tests may be obstructed."
  sed 's/^/  /' "$diagnosticLog"
fi

exit 0

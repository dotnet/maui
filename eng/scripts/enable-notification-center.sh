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

if run_as_console_user "$currentUser" "$uid" launchctl load -w /System/Library/LaunchAgents/com.apple.notificationcenterui.plist; then
  echo "Notification Center enabled for '$currentUser'."
else
  echo "Could not re-enable Notification Center for '$currentUser' — continuing."
fi

exit 0

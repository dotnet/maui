#!/bin/sh

run_as_console_user() {
  target_user=$1
  target_uid=$2
  shift 2

  caller_uid=$(id -u)
  if [ "$caller_uid" = "$target_uid" ]; then
    "$@"
  elif [ "$caller_uid" = "0" ]; then
    launchctl asuser "$target_uid" sudo -n -u "$target_user" "$@"
  else
    sudo -n launchctl asuser "$target_uid" sudo -n -u "$target_user" "$@"
  fi
}

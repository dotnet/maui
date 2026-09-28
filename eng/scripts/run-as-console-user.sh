#!/bin/sh

run_as_console_user() {
  if [ "$#" -lt 3 ]; then
    echo "run_as_console_user requires a user, uid, and command." >&2
    return 64
  fi

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

#!/usr/bin/env bash

set -euo pipefail

driver="${HELIX_CORRELATION_PAYLOAD:?HELIX_CORRELATION_PAYLOAD is required for Helix device performance execution.}/eng/scripts/Run-DevicePerformanceComparison.ps1"

platform=""
previous=""
for argument in "$@"; do
  if [[ "$previous" == "-Platform" ]]; then
    platform="$argument"
    break
  fi
  previous="$argument"
done

require_environment() {
  local name="$1"
  if [[ -z "${!name:-}" ]]; then
    echo "$name is required for Helix device performance execution." >&2
    exit 1
  fi

  allowed_environment+=("$name=${!name}")
}

add_environment_if_set() {
  local name="$1"
  if [[ -n "${!name:-}" ]]; then
    allowed_environment+=("$name=${!name}")
  fi
}

get_console_user() {
  echo "show State:/Users/ConsoleUser" |
    scutil |
    awk '/Name :/ { print $3; exit }'
}

get_console_user_home() {
  local username="$1"
  local home

  if ! home="$(
    dscl . -read "/Users/$username" NFSHomeDirectory 2>/dev/null |
      awk '/NFSHomeDirectory:/ { sub(/^[[:space:]]*NFSHomeDirectory:[[:space:]]*/, ""); print; exit }'
  )" || [[ "$home" != /* ]]; then
    echo "Unable to resolve the home directory for console user '$username'." >&2
    return 1
  fi

  printf '%s\n' "$home"
}

run_as_console_user() {
  local console_user uid console_home
  console_user="$(get_console_user)"

  if [[ -z "$console_user" || "$console_user" == "loginwindow" || "$console_user" == "root" ]]; then
    echo "A non-root console user is required for $platform device performance tests; scutil reported '${console_user:-<none>}'." >&2
    exit 1
  fi

  if ! uid="$(id -u "$console_user" 2>/dev/null)" || [[ ! "$uid" =~ ^[0-9]+$ || "$uid" == "0" ]]; then
    echo "Unable to resolve a non-root uid for console user '$console_user'." >&2
    exit 1
  fi

  console_home="$(get_console_user_home "$console_user")"

  local -a allowed_environment=(
    "PATH=${PATH:-/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin}"
    "HOME=$console_home"
    "USER=$console_user"
    "LOGNAME=$console_user"
    "SHELL=/bin/bash"
  )

  require_environment "XHARNESS_CLI_PATH"
  require_environment "HELIX_CORRELATION_ID"
  require_environment "HELIX_WORKITEM_FRIENDLYNAME"

  add_environment_if_set "DOTNET_ROOT"
  add_environment_if_set "DOTNET_INSTALL_DIR"
  add_environment_if_set "DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR"
  add_environment_if_set "DOTNET_MULTILEVEL_LOOKUP"
  add_environment_if_set "DOTNET_ROLL_FORWARD"
  add_environment_if_set "DOTNET_SKIP_FIRST_TIME_EXPERIENCE"
  add_environment_if_set "DOTNET_CLI_TELEMETRY_OPTOUT"
  add_environment_if_set "DOTNET_NOLOGO"
  add_environment_if_set "POWERSHELL_TELEMETRY_OPTOUT"

  exec sudo -n launchctl asuser "$uid" sudo -n -u "$console_user" \
    env -i "${allowed_environment[@]}" \
    pwsh "$driver" "$@"
}

if [[ "$platform" == "ios" || "$platform" == "maccatalyst" ]]; then
  run_as_console_user "$@"
fi

exec pwsh "$driver" "$@"

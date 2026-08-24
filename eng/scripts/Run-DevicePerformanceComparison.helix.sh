#!/usr/bin/env bash

set -euo pipefail

if [[ "${1:-}" == "--child" ]]; then
  environment_file="$2"
  shift 2

  # launchctl starts with a clean environment. Restore the Helix/XHarness variables
  # captured by the parent process before executing the PowerShell driver.
  # shellcheck disable=SC1090
  source "$environment_file"
  exec pwsh "$HELIX_CORRELATION_PAYLOAD/eng/scripts/Run-DevicePerformanceComparison.ps1" "$@"
fi

platform=""
previous=""
for argument in "$@"; do
  if [[ "$previous" == "-Platform" ]]; then
    platform="$argument"
    break
  fi
  previous="$argument"
done

if [[ "$platform" == "ios" || "$platform" == "maccatalyst" ]]; then
  environment_file="$HELIX_WORKITEM_ROOT/device-performance-environment.sh"
  export -p > "$environment_file"

  uid="$(id -u)"
  username="$(id -un)"
  exec sudo launchctl asuser "$uid" sudo -u "$username" \
    bash "$0" --child "$environment_file" "$@"
fi

exec pwsh "$HELIX_CORRELATION_PAYLOAD/eng/scripts/Run-DevicePerformanceComparison.ps1" "$@"

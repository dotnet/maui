#!/usr/bin/env bash
# One-time (idempotent) WSL/Linux setup for building and testing the MAUI GTK backend.
#   - installs a .NET 10 SDK into ~/.dotnet (NOT into the repo's .dotnet, which holds the Windows SDK when provisioned by
#     build.cmd; global.json lists ".dotnet" first and then "$host$", so under Linux the host SDK on PATH is used)
#   - checks the native packages the GTK tests need (does not install them; prints the apt line)
# Usage: eng/gtk/wsl-setup.sh [channel]   (default channel 10.0)
set -euo pipefail
channel="${1:-10.0}"
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

if "$DOTNET_ROOT/dotnet" --list-sdks 2>/dev/null | grep -q "^${channel%%.*}\."; then
  echo "SDK already installed in $DOTNET_ROOT:"; "$DOTNET_ROOT/dotnet" --list-sdks
else
  tmp="$(mktemp -d)"
  trap 'rm -rf "$tmp"' EXIT
  if command -v curl >/dev/null; then curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$tmp/dotnet-install.sh"
  else wget -qO "$tmp/dotnet-install.sh" https://dot.net/v1/dotnet-install.sh; fi
  bash "$tmp/dotnet-install.sh" --channel "$channel" --install-dir "$DOTNET_ROOT"
fi

missing=()
for bin in xvfb-run dbus-run-session; do command -v "$bin" >/dev/null || missing+=("$bin"); done
for lib in libgtk-4.so.1 libadwaita-1.so.0; do
  ldconfig -p 2>/dev/null | grep -q "$lib" || missing+=("$lib")
done
if [ "${#missing[@]}" -gt 0 ]; then
  echo "Missing: ${missing[*]}"
  echo "Debian 13 / Ubuntu 24.04+: sudo apt install -y xvfb dbus-x11 libgtk-4-1 libadwaita-1-0 libwebkitgtk-6.0-4 fonts-dejavu-core"
  exit 1
fi
echo "OK: add to PATH:  export DOTNET_ROOT=$DOTNET_ROOT PATH=$DOTNET_ROOT:\$PATH"

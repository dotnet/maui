#!/usr/bin/env bash
# Runs test projects headless on Linux/WSL: Xvfb virtual display, a private D-Bus session, GDK_BACKEND=x11.
# Usage: eng/gtk/wsl-test.sh <test.csproj> [<test.csproj> ...] [-- extra dotnet-test args, e.g. --filter "FullyQualifiedName~Foo"]
#   MAUI_GTK_NO_XVFB=1  skip xvfb/dbus (use the real display, e.g. WSLg)
#   MAUI_GTK_FRAMEWORK  target framework to run (default net10.0)
# Output goes to artifacts-wsl/ (see Directory.Build.props), so Windows builds of the same tree are not disturbed.
set -uo pipefail
repo="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export GDK_BACKEND="${GDK_BACKEND:-x11}"
export NO_AT_BRIDGE=1 GSK_RENDERER="${GSK_RENDERER:-cairo}" LIBGL_ALWAYS_SOFTWARE=1

projects=(); extra=()
while [ $# -gt 0 ]; do
  if [ "$1" = "--" ]; then shift; extra=("$@"); break; fi
  projects+=("$1"); shift
done
if [ "${#projects[@]}" -eq 0 ]; then
  sed -n '2,6p' "${BASH_SOURCE[0]}"; exit 2
fi
command -v dotnet >/dev/null || { echo "dotnet not found; run eng/gtk/wsl-setup.sh"; exit 3; }

cd "$repo"
rc=0
for p in "${projects[@]}"; do
  cmd=(dotnet test "$p" -f "${MAUI_GTK_FRAMEWORK:-net10.0}" --nologo "${extra[@]}")
  echo "== ${cmd[*]}"
  if [ -n "${MAUI_GTK_NO_XVFB:-}" ]; then
    "${cmd[@]}" || rc=1
  else
    dbus-run-session -- xvfb-run -a --server-args="-screen 0 1280x1024x24" "${cmd[@]}" || rc=1
  fi
done
exit $rc

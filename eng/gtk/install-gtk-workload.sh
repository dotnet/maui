#!/usr/bin/env bash
# Installs the GtkSharp `gtk` workload (target framework net10.0-gtk4.22) into a user-local .NET SDK on Linux/WSL by copying
# the files that GtkSharp's `dotnet cake build.cake --BuildTarget=InstallPackedWorkload` wrote into another SDK
# (a plain copy: manifest, packs, install marker; see GtkSharp Docs/workload-install.md).
# Usage: eng/gtk/install-gtk-workload.sh [<source SDK dir>]
#   source SDK dir   default: the first /mnt/c/Users/*/.dotnet-gtk (the private SDK of the Windows recipe)
#   DOTNET_ROOT      target SDK, default ~/.dotnet (its feature band, e.g. 10.0.400 for 10.0.403, gets the manifest)
# Without a source: build the workload on Linux with GtkSharp (DOTNET_ROOT=~/.dotnet dotnet cake build.cake --BuildTarget=InstallWorkload).
set -euo pipefail
dst="${DOTNET_ROOT:-$HOME/.dotnet}"
src="${1:-}"
if [ -z "$src" ]; then
  for candidate in /mnt/c/Users/*/.dotnet-gtk; do [ -d "$candidate/packs/GtkSharp4.Runtime" ] && { src="$candidate"; break; }; done
fi
[ -d "$src/packs/GtkSharp4.Runtime" ] || { echo "no GtkSharp workload found in '${src:-<none>}'"; sed -n 2,9p "${BASH_SOURCE[0]}"; exit 2; }

version="$("$dst/dotnet" --version)"           # 10.0.403
band="${version%.*}.$(( ${version##*.} / 100 * 100 ))"     # 10.0.400
manifest="$(ls -d "$src"/sdk-manifests/*/GtkSharp4.NET.Sdk.Gtk | head -1)"

mkdir -p "$dst/sdk-manifests/$band" "$dst/packs" "$dst/metadata/workloads/$band/InstalledWorkloads"
rm -rf "$dst/sdk-manifests/$band/GtkSharp4.NET.Sdk.Gtk"
cp -r "$manifest" "$dst/sdk-manifests/$band/"
for pack in GtkSharp4.Ref GtkSharp4.Runtime GtkSharp4.Sdk; do
  rm -rf "$dst/packs/$pack"; cp -r "$src/packs/$pack" "$dst/packs/"
done
: > "$dst/metadata/workloads/$band/InstalledWorkloads/gtk"
"$dst/dotnet" workload list

#!/usr/bin/env bash
# Purges the forked packages from the Linux/WSL NuGet cache so the next restore unpacks the nupkg now in the local feed.
# pack-local.ps1 (Windows) purges only the WINDOWS cache; run this under WSL/Linux after each repack, because a
# repack of an already restored revision would otherwise keep serving the old extracted folder.
# Usage: eng/gtk/refresh-gtk-packages.sh [--all]
#   default: for each package id with a -gtk.* nupkg in the feed (Packages/), removes every cached -gtk.* version of it
#            (the feed holds only the newest, so the next restore re-extracts it).
#   --all  : removes every cached -gtk.* version of every Microsoft.Maui*/GtkSharp-family id found in the cache.
set -euo pipefail
repo="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
feed="$repo/Packages"
cache="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
[ -d "$cache" ] || { echo "no NuGet cache at $cache"; exit 0; }
removed=0
if [ "${1:-}" = "--all" ]; then
  for d in "$cache"/microsoft.maui*/ ; do
    for v in "$d"*-gtk.*/ ; do [ -d "$v" ] && { rm -rf "$v"; echo "removed $v"; removed=$((removed+1)); }; done
  done
else
  [ -d "$feed" ] || { echo "no feed at $feed"; exit 1; }
  # id.version from the feed's *-gtk.* nupkgs (ignores .symbols/.snupkg)
  while IFS= read -r f; do
    name="$(basename "$f" .nupkg)"
    case "$name" in *.symbols) continue;; esac
    if [[ "$name" =~ ^(.+)\.([0-9]+\.[0-9]+\.[0-9]+-gtk\.[0-9A-Za-z.]+)$ ]]; then
      id="${BASH_REMATCH[1],,}"
      for v in "$cache/$id"/*-gtk.*/ ; do
        [ -d "$v" ] || continue
        rm -rf "$v"; echo "removed $v"; removed=$((removed+1))
      done
    fi
  done < <(find -L "$feed" -maxdepth 1 -name '*-gtk.*.nupkg')
fi
echo "refresh-gtk-packages: removed $removed cached folder(s) from $cache"

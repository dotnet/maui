#!/usr/bin/env bash
# CI for the Linux/GTK 4 backend (not Azure DevOps): Microsoft.Maui-linux.slnf, headless tests. GtkSharp4 restores from nuget.org.
#
#   eng/gtk/ci-linux.sh                run on this machine (steps below)
#   eng/gtk/ci-linux.sh --containers   re-run it in docker/podman containers: ubuntu:24.04 (GTK 4.14, the floor) and
#                                      debian:forky (GTK 4.22); override with MAUI_GTK_CI_IMAGES="img1 img2"
#
# Steps (each can be skipped with CI_SKIP_<NAME>=1):
#   SDK       .NET 10 SDK into ~/.dotnet (eng/gtk/wsl-setup.sh)
#   BUILD     dotnet build Microsoft.Maui-linux.slnf (mobile/Windows TFMs disabled by Directory.Build.Override.props)
#   TEST      the GTK unit tests under Xvfb + a private D-Bus session, GDK_BACKEND=x11, software rendering
#   SAMPLE    the sample app headless (--smoke)
# Environment: MAUI_GTK_FATAL_CRITICALS=1 (G_DEBUG=fatal-criticals:
# a GTK critical aborts the test run, e.g. "Allocating size ... without calling gtk_widget_measure").
set -euo pipefail
repo="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo"

# ------------------------------------------------------------------------------------------------ containers
if [ "${1:-}" = "--containers" ]; then
  engine="$(command -v docker || command -v podman || true)"
  [ -n "$engine" ] || { echo "ci-linux: docker or podman is required for --containers"; exit 3; }
  rc=0
  for image in ${MAUI_GTK_CI_IMAGES:-ubuntu:24.04 debian:forky}; do
    echo "=== $image"
    # The container needs the tools to download the SDK and the native GTK 4 stack the tests load.
    "$engine" run --rm -v "$repo:/src" -w /src -e WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1 \
      "$image" bash -c '
        set -e
        export DEBIAN_FRONTEND=noninteractive
        apt-get update -qq
        apt-get install -y -qq --no-install-recommends ca-certificates curl git xvfb dbus-x11 \
          libgtk-4-1 libadwaita-1-0 libwebkitgtk-6.0-4 fonts-dejavu-core libicu-dev libssl-dev zlib1g > /dev/null
        bash eng/gtk/ci-linux.sh' || rc=1
  done
  exit $rc
fi

step() { local var="CI_SKIP_$1"; [ -z "${!var:-}" ]; }
say() { echo; echo "=== $*"; }

# ------------------------------------------------------------------------------------------------ SDK
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
if step SDK; then
  say ".NET SDK"
  bash eng/gtk/wsl-setup.sh
fi

# Without the mobile/Windows workloads only the platform-less TFMs build (see src/Gtk/README.md, "Building").
if [ ! -f Directory.Build.Override.props ]; then
  cat > Directory.Build.Override.props << 'XML'
<Project>
  <PropertyGroup>
    <IncludeAndroidTargetFrameworks>false</IncludeAndroidTargetFrameworks>
    <IncludeIosTargetFrameworks>false</IncludeIosTargetFrameworks>
    <IncludeMacCatalystTargetFrameworks>false</IncludeMacCatalystTargetFrameworks>
    <IncludeMacOSTargetFrameworks>false</IncludeMacOSTargetFrameworks>
    <IncludeTizenTargetFrameworks>false</IncludeTizenTargetFrameworks>
    <IncludeWindowsTargetFrameworks>false</IncludeWindowsTargetFrameworks>
  </PropertyGroup>
</Project>
XML
fi

# ------------------------------------------------------------------------------------------------ build
if step BUILD; then
  say "build Microsoft.Maui-linux.slnf"
  dotnet build Microsoft.Maui-linux.slnf --nologo
fi

# ------------------------------------------------------------------------------------------------ tests
if step TEST; then
  say "GTK unit tests (headless)"
  [ -z "${MAUI_GTK_FATAL_CRITICALS:-}" ] || export G_DEBUG=fatal-criticals
  bash eng/gtk/wsl-test.sh src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests
  bash eng/gtk/wsl-test.sh src/Core/tests/UnitTests/Core.UnitTests.csproj -- --filter "FullyQualifiedName~PlatformViewBackendTests"
  bash eng/gtk/wsl-test.sh src/SingleProject/Resizetizer/test/UnitTests/Resizetizer.UnitTests.csproj
fi

# ------------------------------------------------------------------------------------------------ sample
if step SAMPLE; then
  say "sample app (--smoke)"
  # The sample is a net10.0-gtk4.22 app: it needs the gtk workload, which the dotnet-install SDK does not have.
  if dotnet workload list 2> /dev/null | grep -q '^gtk'; then
    export GDK_BACKEND=x11 NO_AT_BRIDGE=1 GSK_RENDERER=cairo LIBGL_ALWAYS_SOFTWARE=1
    dbus-run-session -- xvfb-run -a --server-args="-screen 0 1280x1024x24" \
      dotnet run --project src/Gtk/samples/Maui.Gtk.Sample -- --smoke
  else
    echo "gtk workload not installed: sample skipped (docs: GtkSharp Docs/workload-install.md)"
  fi
fi

say "done"

#!/usr/bin/env bash

set -ue

source="${BASH_SOURCE[0]}"

# resolve $source until the file is no longer a symlink
while [[ -h "$source" ]]; do
  scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
  source="$(readlink "$source")"
  # if $source was a relative symlink, we need to resolve it relative to the path where the
  # symlink file was located
  [[ $source != /* ]] && source="$scriptroot/$source"
done
scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"

usage()
{
  echo "Common settings:"
  echo "  --arch (-a)                     Target platform: x86, x64, arm or arm64."
  echo "                                  [Default: Your machine's architecture.]"
  echo "  --binaryLog (-bl)               Output binary log."
  echo "  --configuration (-c)            Build configuration: Debug or Release."
  echo "                                  [Default: Debug]"
  echo "  --help (-h)                     Print help and exit."
  echo "  --os                            Target operating system: windows, linux, or osx."
  echo "                                  [Default: Your machine's OS.]"
  echo "  --verbosity (-v)                MSBuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]."
  echo "                                  [Default: Minimal]"
  echo ""

  echo "Actions (defaults to --restore --build):"
  echo "  --build (-b)               Build all source projects."
  echo "                             This assumes --restore has been run already."
  echo "  --clean                    Clean the solution."
  echo "  --pack                     Package build outputs into NuGet packages."
  echo "  --publish                  Publish artifacts (e.g. symbols)."
  echo "                             This assumes --build has been run already."
  echo "  --rebuild                  Rebuild all source projects."
  echo "  --restore (-r)             Restore dependencies."
  echo "  --sign                     Sign build outputs."
  echo "  --test (-t)                Incrementally builds and runs tests."
  echo "                             Use in conjunction with --testnobuild to only run tests."
  echo "  --testCoverage             Run unit tests and capture code coverage information."
  echo ""

  echo "Libraries settings:"
  echo "  --testnobuild              Skip building tests when invoking -test."
  echo ""

  echo "Command line arguments starting with '/p:' are passed through to MSBuild."
  echo "Arguments can also be passed in with a single hyphen."
  echo ""
}

arguments=''
extraargs=''
testCoverage=false

# Check if an action is passed in
declare -a actions=("b" "build" "r" "restore" "rebuild" "testnobuild" "sign" "publish" "clean")
actInt=($(comm -12 <(printf '%s\n' "${actions[@]/#/-}" | sort) <(printf '%s\n' "${@/#--/-}" | sort)))

while [[ $# > 0 ]]; do
  opt="$(echo "${1/#--/-}" | tr "[:upper:]" "[:lower:]")"

  case "$opt" in
     -help|-h|-\?|/?)
      usage
      exit 0
      ;;

     -arch|-a)
      if [ -z ${2+x} ]; then
        echo "No architecture supplied. See help (--help) for supported architectures." 1>&2
        exit 1
      fi
      passedArch="$(echo "$2" | tr "[:upper:]" "[:lower:]")"
      case "$passedArch" in
        x64|x86|arm|arm64)
          arch=$passedArch
          ;;
        *)
          echo "Unsupported target architecture '$2'."
          echo "The allowed values are x86, x64, arm, arm64."
          exit 1
          ;;
      esac
      arguments="$arguments /p:TargetArchitecture=$arch"
      shift 2
      ;;

     -configuration|-c)
      if [ -z ${2+x} ]; then
        echo "No configuration supplied. See help (--help) for supported configurations." 1>&2
        exit 1
      fi
      passedConfig="$(echo "$2" | tr "[:upper:]" "[:lower:]")"
      case "$passedConfig" in
        debug|release)
          val="$(tr '[:lower:]' '[:upper:]' <<< ${passedConfig:0:1})${passedConfig:1}"
          ;;
        *)
          echo "Unsupported target configuration '$2'."
          echo "The allowed values are Debug and Release."
          exit 1
          ;;
      esac
      arguments="$arguments -configuration $val"
      shift 2
      ;;

     -os)
      if [ -z ${2+x} ]; then
        echo "No target operating system supplied. See help (--help) for supported target operating systems." 1>&2
        exit 1
      fi
      passedOS="$(echo "$2" | tr "[:upper:]" "[:lower:]")"
      case "$passedOS" in
        windows)
          os="windows" ;;
        linux)
          os="linux" ;;
        osx)
          os="osx" ;;
        *)
          echo "Unsupported target OS '$2'."
          echo "Try 'build.sh --help' for values supported by '--os'."
          exit 1
          ;;
      esac
      arguments="$arguments /p:TargetOS=$os"
      shift 2
      ;;

     -testnobuild)
      arguments="$arguments /p:TestNoBuild=true"
      shift 1
      ;;

    -testcoverage)
      testCoverage=true
      ;;

     *)
      extraargs="$extraargs $1"
      shift 1
      ;;
  esac
done

if [ ${#actInt[@]} -eq 0 ]; then
    arguments="-restore -build $arguments"
fi

if [[ "${TreatWarningsAsErrors:-}" == "false" ]]; then
    arguments="$arguments -warnAsError 0"
fi

arguments="$arguments $extraargs"

# Remove stale workload manifest band directories before MSBuild starts.
# The SDK ships with manifests for multiple preview bands (e.g. preview.1 and preview.4).
# If MSBuild sees the older band's manifests, the workload SDK resolver will use them
# (referencing old pack versions like net11.0_26.2 instead of net11.0_26.4), causing
# NETSDK1178 errors. This must happen before MSBuild starts since the resolver caches
# its state at evaluation time.
repo_root="$( cd -P "$( dirname "$scriptroot" )" && pwd )"
dotnet_dir="$repo_root/.dotnet"
if [[ -d "$dotnet_dir/sdk-manifests" ]]; then
  sdk_version=$(python3 -c "import json; print(json.load(open('$repo_root/global.json'))['tools']['dotnet'])" 2>/dev/null || true)
  if [[ -n "$sdk_version" ]]; then
    # Extract version band: e.g. "11.0.100-preview.4.26224.122" -> "11.0.100-preview.4"
    version_band=$(python3 -c "
import re, sys
v = '$sdk_version'
m = re.match(r'(\d+\.\d+\.)(\d)(\d{2})(.*)', v)
patch = m.group(2) + '00'
pre = re.match(r'^(-(?:preview|rc|alpha)\.\d+)', m.group(4))
band = m.group(1) + patch + (pre.group(1) if pre else '')
print(band)
" 2>/dev/null || true)
    for band_dir in "$dotnet_dir/sdk-manifests"/*/; do
      band_name=$(basename "$band_dir")
      if [[ "$band_name" != "$version_band" && "$band_name" != "." ]]; then
        echo "Removing stale workload manifest band: $band_name (current band: $version_band)"
        rm -rf "$band_dir"
      fi
    done
  fi
fi

"$scriptroot/common/build.sh" $arguments


# Perform code coverage as the last operation, this enables the following scenarios:
#   .\build.sh --restore --build --c Release --testCoverage
if [[ "$testCoverage" == true ]]; then
  # Install required toolset
  . "$DIR/common/tools.sh"
  InitializeDotNetCli true > /dev/null

  repoRoot=$(realpath $DIR/../)
  testResultPath="$repoRoot/artifacts/TestResults/$configuration"

  # Run tests and collect code coverage
  $repoRoot/.dotnet/dotnet 'dotnet-coverage' collect --settings $repoRoot/eng/CodeCoverage.config --output $testResultPath/local.cobertura.xml "$repoRoot/build.sh --test --configuration $configuration"

  # Generate the code coverage report and open it in the browser
  $repoRoot/.dotnet/dotnet reportgenerator -reports:$testResultPath/*.cobertura.xml -targetdir:$testResultPath/CoverageResultsHtml -reporttypes:HtmlInline_AzurePipelines
  echo ""
  echo -e "\e[32mCode coverage results:\e[0m $testResultPath/CoverageResultsHtml/index.html"
  echo ""
fi
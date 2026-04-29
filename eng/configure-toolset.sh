# We can't use already installed dotnet cli since we need to install additional workloads.
# We could potentially try to find an existing installation that has all the required workloads,
# but it's unlikely one will be available.

useInstalledDotNetCli="false"

# Working around issue https://github.com/dotnet/arcade/issues/7327
# DisableNativeToolsetInstalls=true

# Wrap InitializeToolset to remove stale workload manifest band directories
# after the SDK is installed but before MSBuild evaluates any projects.
#
# The SDK tarball bundles manifests from older preview bands (e.g. preview.1)
# in sdk-manifests/. The SDK resolver imports WorkloadManifest.targets from
# ALL band directories during evaluation, causing NETSDK1178 errors for old
# pack versions (e.g. Microsoft.iOS.Sdk.net11.0_26.2).
_original_initialize_toolset=$(declare -f InitializeToolset)
eval "${_original_initialize_toolset/InitializeToolset/_OriginalInitializeToolset}"

function InitializeToolset {
  _OriginalInitializeToolset

  local dotnet_dir="$repo_root/.dotnet"
  local manifests_dir="$dotnet_dir/sdk-manifests"
  if [[ -d "$manifests_dir" ]]; then
    local sdk_version
    sdk_version=$(jq -r '.tools.dotnet' "$repo_root/global.json")
    local version_band
    version_band=$(echo "$sdk_version" | sed -E 's/^([0-9]+\.[0-9]+\.[0-9])[0-9]*/\100/')
    local preview_suffix
    preview_suffix=$(echo "$sdk_version" | grep -oE '\-(preview|rc|alpha)\.[0-9]+' || true)
    local current_band="${version_band}${preview_suffix}"

    for band_dir in "$manifests_dir"/*/; do
      local band_name
      band_name=$(basename "$band_dir")
      if [[ "$band_name" != "$current_band" ]]; then
        echo "Removing stale workload manifest band: $band_name"
        rm -rf "$band_dir"
      fi
    done
  fi
}
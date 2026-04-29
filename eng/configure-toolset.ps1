# We can't use already installed dotnet cli since we need to install additional workloads.
# We could potentially try to find an existing installation that has all the required workloads,
# but it's unlikely one will be available.

$script:useInstalledDotNetCli = $false

# Wrap InitializeToolset to remove stale workload manifest band directories
# after the SDK is installed but before MSBuild evaluates any projects.
#
# The SDK tarball bundles manifests from older preview bands (e.g. preview.1)
# in sdk-manifests/. The SDK resolver imports WorkloadManifest.targets from
# ALL band directories during evaluation, causing NETSDK1178 errors for old
# pack versions (e.g. Microsoft.iOS.Sdk.net11.0_26.2).
$script:_OriginalInitializeToolset = ${function:InitializeToolset}
function InitializeToolset {
  & $script:_OriginalInitializeToolset

  $dotnetDir = Join-Path $RepoRoot '.dotnet'
  $manifestsDir = Join-Path $dotnetDir 'sdk-manifests'
  if (Test-Path $manifestsDir) {
    $sdkVersion = $GlobalJson.tools.dotnet
    $versionBand = [System.Text.RegularExpressions.Regex]::Match($sdkVersion, '^\d+\.\d+\.\d').Value + '00'
    $previewSuffix = [System.Text.RegularExpressions.Regex]::Match($sdkVersion, '\-(preview|rc|alpha)\.\d+').Value
    $currentBand = "${versionBand}${previewSuffix}"

    Get-ChildItem -Path $manifestsDir -Directory |
      Where-Object { $_.Name -ne $currentBand } |
      ForEach-Object {
        Write-Host "Removing stale workload manifest band: $($_.Name)"
        Remove-Item -Recurse -Force $_.FullName
      }
  }
}
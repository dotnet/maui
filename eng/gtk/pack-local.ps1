<#
.SYNOPSIS
  Packs the forked MAUI packages (version 10.0.130-gtk.<yyMMdd stamp>.<rev>) and publishes them to the local feed.

.DESCRIPTION
  For every project below it runs `dotnet pack` with the Arcade properties that produce the forked version
  (OfficialBuild + OfficialBuildId + PreReleaseVersionLabel=gtk; see src/Gtk/README.md) and
  -p:MauiGtkLocalFeedDir, which makes eng/gtk/LocalFeed.targets copy the nupkg into the feed, delete older -gtk
  nupkgs of the same id and purge their folders from the NuGet cache.
  Projects that do not exist (yet) are skipped. Only the TFMs enabled by the (gitignored) Directory.Build.Override.props
  are packed: on a machine without the mobile workloads that is netstandard + net10.0 only.
  Run `eng/gtk/refresh-gtk-packages.sh` under WSL afterwards to purge the WSL NuGet cache.

.PARAMETER FeedDir        Feed folder. Default: <repo>/Packages (junction to the shared feed).
.PARAMETER Configuration  Build configuration. Default Debug.
.PARAMETER Revision       Build-of-the-day number (the trailing .N of the version). Default: next free one for today in the feed.
.PARAMETER Only           Pack only projects whose path contains one of these substrings (e.g. -Only Core,Graphics).
.PARAMETER NoRestore      Pass --no-restore.
.PARAMETER WhatIf         List the projects and the version, pack nothing.

.EXAMPLE
  pwsh eng/gtk/pack-local.ps1 -Only src/Core/src/Core.csproj,src/Graphics/src/Graphics/
#>
[CmdletBinding()]
param(
    [string]$FeedDir,
    [string]$Configuration = 'Debug',
    [int]$Revision = 0,
    [string[]]$Only,
    [switch]$NoRestore,
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
if (-not $FeedDir) { $FeedDir = Join-Path $repo 'Packages' }
if (-not (Test-Path $FeedDir)) { throw "Feed folder '$FeedDir' does not exist (junction Packages -> D:\Starb\Packages?)." }
$FeedDir = (Resolve-Path $FeedDir).Path

# Arcade SHORT_DATE := yy*1000 + mm*50 + dd (Version.BeforeCommonTargets.targets); the version is <label>.<SHORT_DATE>.<rev>.
$now = Get-Date
$stamp = ($now.Year % 100) * 1000 + $now.Month * 50 + $now.Day
if ($Revision -le 0) {
    $max = 0
    Get-ChildItem $FeedDir -Filter "Microsoft.Maui*-gtk.$stamp.*.nupkg" -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.Name -match "-gtk\.$stamp\.(\d+)(\.symbols)?\.nupkg$") { $max = [Math]::Max($max, [int]$Matches[1]) }
    }
    $Revision = $max + 1
}
$buildId = '{0:yyyyMMdd}.{1}' -f $now, $Revision
$version = "10.0.130-gtk.$stamp.$Revision"

# Dependency order: a package is packed before the ones that reference it.
$ordered = @(
    'src/Graphics/src/Graphics/Graphics.csproj'
    'src/Graphics/src/Graphics.Cairo/Graphics.Cairo.csproj'
    'src/Essentials/src/Essentials.csproj'
    'src/Core/src/Core.csproj'
    'src/Controls/src/Build.Tasks/Controls.Build.Tasks.csproj'
    'src/Controls/src/Xaml/Controls.Xaml.csproj'
    'src/Controls/src/Core/Controls.Core.csproj'
    'src/Controls/src/SourceGen/Controls.SourceGen.csproj'
    'src/Controls/src/BindingSourceGen/Controls.BindingSourceGen.csproj'
    'src/Controls/src/NuGet/Controls.NuGet.csproj'
    'src/SingleProject/Resizetizer/src/Resizetizer.csproj'
    'src/Core/maps/src/Maps.csproj'
    'src/Controls/Maps/src/Controls.Maps.csproj'
    'src/BlazorWebView/src/Maui/Microsoft.AspNetCore.Components.WebView.Maui.csproj'
)
$gtkRoot = Join-Path $repo 'src/Gtk/src'
$gtk = @()
if (Test-Path $gtkRoot) {
    # Core first (others reference it), then the rest alphabetically.
    $gtk = Get-ChildItem $gtkRoot -Recurse -Filter *.csproj | Sort-Object { if ($_.Name -eq 'Microsoft.Maui.Gtk.csproj') { '0' } else { '1' + $_.FullName } } |
        ForEach-Object { $_.FullName.Substring($repo.Length + 1).Replace('\', '/') }
}
$projects = @($ordered + $gtk)
if ($Only) { $Only = @($Only | ForEach-Object { $_ -split "," } | Where-Object { $_ }); $projects = @($projects | Where-Object { $p = $_; $Only | Where-Object { $p.Contains($_.Replace('\', '/')) } }) }

if ($Only -and $PSBoundParameters.ContainsKey('Revision') -eq $false) { Write-Warning 'Partial pack (-Only) gets a NEW revision: the packed projects reference their dependencies at that revision, so pack the dependencies too (or the whole set) or pass -Revision of the existing set.' }
Write-Host "Version $version (OfficialBuildId $buildId), feed $FeedDir, configuration $Configuration"
$done = @(); $skipped = @(); $failed = @()
foreach ($p in $projects) {
    $full = Join-Path $repo $p
    if (-not (Test-Path $full)) { $skipped += $p; Write-Host "skip (missing): $p"; continue }
    Write-Host "== pack $p"
    if ($WhatIf) { continue }
    $packArgs = @('pack', $full, '-c', $Configuration,
        '-p:OfficialBuild=true', "-p:OfficialBuildId=$buildId", '-p:PreReleaseVersionLabel=gtk',
        "-p:MauiGtkLocalFeedDir=$FeedDir", '-nologo', '-v:minimal')
    if ($NoRestore) { $packArgs += '--no-restore' }
    & dotnet @packArgs
    if ($LASTEXITCODE -ne 0) { $failed += $p; Write-Warning "pack failed: $p" } else { $done += $p }
}
Write-Host "`nPacked: $($done.Count), skipped (missing): $($skipped.Count), failed: $($failed.Count)"
$failed | ForEach-Object { Write-Host "  FAILED $_" }
if ($failed.Count -gt 0) { exit 1 }

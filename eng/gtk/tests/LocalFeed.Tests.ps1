<#
  Self-test of eng/gtk/LocalFeed.targets on a throw-away project (no MAUI build needed). Checks that after Pack:
  the new nupkg is in the feed, older -gtk nupkgs of the same id are gone, a sibling id (<id>.Extra) is kept, the old and
  new cache folders are purged, other cache folders survive, and a non-gtk version is refused.
  Usage: pwsh eng/gtk/tests/LocalFeed.Tests.ps1      (exit code 0 = all passed)
#>
$ErrorActionPreference = 'Stop'
$targets = (Resolve-Path (Join-Path $PSScriptRoot '..' 'LocalFeed.targets')).Path
$root = Join-Path ([IO.Path]::GetTempPath()) ("localfeed-test-" + [guid]::NewGuid().ToString('N'))
$feed = Join-Path $root 'feed'; $cache = Join-Path $root 'cache'; $proj = Join-Path $root 'proj'
New-Item -ItemType Directory $feed, $cache, $proj | Out-Null
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackageId>Fake.Gtk.Pkg</PackageId>
    <Version>`$(FakeVersion)</Version>
    <IsPackable>true</IsPackable>
    <PackageOutputPath>`$(MSBuildProjectDirectory)/out</PackageOutputPath>
    <MauiGtkNuGetCacheDir>$cache</MauiGtkNuGetCacheDir>
  </PropertyGroup>
  <Import Project="$targets" />
</Project>
"@ | Set-Content (Join-Path $proj 'Fake.csproj')
$fail = 0
function Check($name, $cond) { if ($cond) { Write-Host "ok   $name" } else { Write-Host "FAIL $name"; $script:fail++ } }
function Pack($v) { & dotnet pack (Join-Path $proj 'Fake.csproj') -p:FakeVersion=$v "-p:MauiGtkLocalFeedDir=$feed" -nologo -v:q 2>&1 | Out-Null; $LASTEXITCODE }

# seed: old versions of the id, a sibling id, an unrelated package; cache folders for old, new and other
'Fake.Gtk.Pkg.10.0.130-gtk.1.1.nupkg', 'Fake.Gtk.Pkg.10.0.130-gtk.1.1.symbols.nupkg', 'Fake.Gtk.Pkg.Extra.10.0.130-gtk.1.1.nupkg', 'Other.1.0.0.nupkg' |
    ForEach-Object { New-Item -ItemType File (Join-Path $feed $_) | Out-Null }
foreach ($d in 'fake.gtk.pkg/10.0.130-gtk.1.1', 'fake.gtk.pkg/10.0.130-gtk.1.2', 'fake.gtk.pkg/9.9.9', 'other/1.0.0') { New-Item -ItemType Directory (Join-Path $cache $d) | Out-Null }

Check 'pack gtk version succeeds' ((Pack '10.0.130-gtk.1.2') -eq 0)
$names = (Get-ChildItem $feed).Name
Check 'new nupkg copied'            ($names -contains 'Fake.Gtk.Pkg.10.0.130-gtk.1.2.nupkg')
Check 'old nupkg removed'           ($names -notcontains 'Fake.Gtk.Pkg.10.0.130-gtk.1.1.nupkg')
Check 'old symbols nupkg removed'   ($names -notcontains 'Fake.Gtk.Pkg.10.0.130-gtk.1.1.symbols.nupkg')
Check 'sibling id kept'             ($names -contains 'Fake.Gtk.Pkg.Extra.10.0.130-gtk.1.1.nupkg')
Check 'unrelated package kept'      ($names -contains 'Other.1.0.0.nupkg')
Check 'old cache folder removed'    (-not (Test-Path (Join-Path $cache 'fake.gtk.pkg/10.0.130-gtk.1.1')))
Check 'new cache folder removed'    (-not (Test-Path (Join-Path $cache 'fake.gtk.pkg/10.0.130-gtk.1.2')))
Check 'other cache version kept'    (Test-Path (Join-Path $cache 'fake.gtk.pkg/9.9.9'))
Check 'other cache id kept'         (Test-Path (Join-Path $cache 'other/1.0.0'))
Check 'stable version refused'      ((Pack '10.0.130') -ne 0)
Check 'stable nupkg not in feed'    (-not (Test-Path (Join-Path $feed 'Fake.Gtk.Pkg.10.0.130.nupkg')))

Remove-Item $root -Recurse -Force
if ($fail) { Write-Host "$fail check(s) failed"; exit 1 } else { Write-Host 'all checks passed' }

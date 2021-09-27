Param (
    [string] $MSBuild = "$env:MSBUILD_EXE"
)

$ErrorActionPreference = "Stop"

$msbuildRoot = Split-Path -Parent $MSBuild

$files = @(
    'System.Text.Json.dll',
    'System.Text.Encodings.Web.dll',
    'System.Threading.Tasks.Extensions.dll'
)

$urls = @{
    'https://globalcdn.nuget.org/packages/system.text.json.5.0.0.nupkg' = 'lib\net461\System.Text.Json.dll';
    'https://globalcdn.nuget.org/packages/system.text.encodings.web.5.0.0.nupkg' = 'lib\netstandard2.0\System.Text.Encodings.Web.dll';
    'https://globalcdn.nuget.org/packages/system.threading.tasks.extensions.4.5.4.nupkg' = 'lib\netstandard2.0\System.Threading.Tasks.Extensions.dll';
}

# backup
foreach ($file in $files) {
    $p = Join-Path $msbuildRoot $file
    if (!(Test-Path $p-old)) {
        Move-Item $p $p-old
    }
}

foreach ($url in $urls.GetEnumerator()) {
    $nupkg = Split-Path -Leaf $url.Key
    $dst = Join-Path $env:TEMP $nupkg
    $extract = "$dst-out"

    # clean
    Remove-Item -Force -Recurse -ErrorAction Ignore $dst
    Remove-Item -Force -Recurse -ErrorAction Ignore $extract

    # downlaod and extract
    Invoke-WebRequest -Uri $url.Key -OutFile $dst
    Expand-Archive -Path $dst -DestinationPath $extract

    # copy
    $dll = Split-Path -Leaf $url.Value
    $src = Join-Path $extract $url.Value
    $dst = Join-Path $msbuildRoot $dll
    Copy-Item -Path $src -Destination $dst
}

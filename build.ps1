$ErrorActionPreference = "Stop"

# Make sure that we have something on non-bots
if (!$env:BUILD_NUMBER) {
    $env:BUILD_NUMBER = "0"
}

# Find MSBuild on this machine
if ($IsMacOS) {
    $msbuild = "msbuild"
} else {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    $msbuild = join-path $msbuild 'MSBuild\15.0\Bin\MSBuild.exe'
}

Write-Output "Using MSBuild from: $msbuild"

# Build the projects
& $msbuild "./Xamarin.Essentials.sln" /t:"Restore;Build" /p:Configuration=Release
if ($lastexitcode -ne 0) { exit $lastexitcode; }

# Create the stable NuGet package
& $msbuild "./Xamarin.Essentials/Xamarin.Essentials.csproj" /t:Pack /p:Configuration=Release /p:VersionSuffix=".$env:BUILD_NUMBER"
if ($lastexitcode -ne 0) { exit $lastexitcode; }

# Create the beta NuGet package
& $msbuild "./Xamarin.Essentials/Xamarin.Essentials.csproj" /t:Pack /p:Configuration=Release /p:VersionSuffix=".$env:BUILD_NUMBER-beta"
if ($lastexitcode -ne 0) { exit $lastexitcode; }

# Copy everything into the output folder
Copy-Item "./Xamarin.Essentials/bin/Release" "./Output" -Recurse -Force

exit $lastexitcode;

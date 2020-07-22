$ErrorActionPreference = "Stop"

$cibuild = "false"

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
    $msbuild = join-path $msbuild 'MSBuild\Current\Bin\MSBuild.exe'
    $cibuild = "true"
}

Write-Output "Using MSBuild from: $msbuild"

# Build the projects
& $msbuild "./Xamarin.Essentials.sln" /restore /t:Build /p:Configuration=Release /p:ContinuousIntegrationBuild=$ciBuild /p:Deterministic=false
if ($lastexitcode -ne 0) { exit $lastexitcode; }

# Create the stable NuGet package
& $msbuild "./Xamarin.Essentials/Xamarin.Essentials.csproj" /t:Pack /p:Configuration=Release /p:ContinuousIntegrationBuild=$cibuild /p:Deterministic=false /p:VersionSuffix=".$env:BUILD_NUMBER"
if ($lastexitcode -ne 0) { exit $lastexitcode; }

# Create the beta NuGet package
& $msbuild "./Xamarin.Essentials/Xamarin.Essentials.csproj" /t:Pack /p:Configuration=Release /p:ContinuousIntegrationBuild=$cibuild /p:Deterministic=false /p:VersionSuffix=".$env:BUILD_NUMBER-beta"
if ($lastexitcode -ne 0) { exit $lastexitcode; }

# Copy everything into the output folder
Copy-Item "./Xamarin.Essentials/bin/Release" "./Output" -Recurse -Force

exit $lastexitcode;

#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "windows")]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [string]$ScenarioId,

    [Parameter(Mandatory = $true)]
    [string]$DevFlowFeed,

    [Parameter(Mandatory = $true)]
    [string]$DevFlowSourceRoot,

    [Parameter(Mandatory = $true)]
    [string]$LogDirectory,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

function Invoke-SanitizedDotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$LogName
    )

    New-Item -ItemType Directory -Force -Path $LogDirectory | Out-Null
    $rawLog = Join-Path $LogDirectory "$LogName.raw.log"
    $safeLog = Join-Path $LogDirectory "$LogName.log"

    & dotnet @Arguments 2>&1 |
        Tee-Object -FilePath $rawLog |
        ForEach-Object {
            $line = [string]$_
            $safe = $line.Replace("`r", "") -replace '(?i)##vso\[[^\]]*\]', '[redacted-pipeline-command]'
            $safe | Add-Content -LiteralPath $safeLog -Encoding UTF8
            Write-Host $safe
        }
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "dotnet $($Arguments[0]) failed with exit code $exitCode. See $safeLog."
    }
}

$targetFramework = if ($Platform -eq "android") {
    "net10.0-android"
}
else {
    "net10.0-windows10.0.19041.0"
}

Invoke-SanitizedDotNet @(
    "build",
    "Microsoft.Maui.BuildTasks.slnf",
    "-c", $Configuration,
    "--nologo",
    "--verbosity:minimal",
    "-m:1",
    "-p:IncludeIosTargetFrameworks=false",
    "-p:IncludeAndroidTargetFrameworks=false",
    "-p:IncludeMacCatalystTargetFrameworks=false",
    "-p:IncludeWindowsTargetFrameworks=false",
    "-p:IncludeTizenTargetFrameworks=false",
    "-p:TreatWarningsAsErrors=false"
) "build-tasks"

$appCommand = if ($Platform -eq "windows") { "publish" } else { "build" }
$appArguments = @(
    $appCommand,
    "src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj",
    "-f", $targetFramework,
    "-c", $Configuration,
    "--nologo",
    "--verbosity:minimal",
    "-m:1",
    "-p:EnableMauiUiEvidence=true",
    "-p:MauiUiEvidenceScenario=$ScenarioId",
    "-p:MauiUiEvidenceDevFlowEnabled=true",
    "-p:MauiUiEvidenceDevFlowSourceRoot=$DevFlowSourceRoot",
    "-p:MauiUiEvidenceMauiRoot=$([IO.Path]::GetFullPath((Get-Location).Path))",
    "-p:MauiDevFlowPort=9223",
    "-p:MauiDevFlowSessionId=uievidence",
    "-p:RestoreAdditionalProjectSources=$DevFlowFeed",
    "-p:TreatWarningsAsErrors=false"
)
if ($Platform -eq "windows") {
    $appArguments += @(
        "-p:RuntimeIdentifierOverride=win-x64",
        "-p:SelfContained=true",
        "-p:WindowsPackageType=None",
        "-p:_MauiDeviceTestUnpackaged=true",
        "-p:ExtraDefineConstants=UNPACKAGED"
    )
}
Invoke-SanitizedDotNet $appArguments "host-app"

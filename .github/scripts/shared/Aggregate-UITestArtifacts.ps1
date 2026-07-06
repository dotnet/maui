<#
.SYNOPSIS
  Download AzDO build artifacts from a ci-copilot-uitests child build,
  parse all TRX files, and merge them into per-category aggregates the
  Review-PR.ps1 STEP 3 renderer expects.

.DESCRIPTION
  The child pipeline (eng/pipelines/ci-copilot-uitests.yml) publishes
  one drop-* artifact per matrix job (one job per detected category per
  platform) via PublishBuildArtifacts@1 in ui-tests-steps.yml. Each
  artifact contains the TRX file from `dotnet test --logger trx`.

  This script:
    1. Lists artifacts on the build (filtered to drop-* + ui-tests-samples).
    2. Downloads them into a temp dir.
    3. Walks all .trx files.
    4. Calls Get-TrxResults from Review-PR.ps1 (sourced via -ScriptDir)
       to parse each one.
    5. Merges results by category. The category for each TRX is derived
       from the artifact name (drop-<stage>-<job>-<attempt> where job
       contains the CATEGORYGROUP matrix variable).

  Returns a hashtable keyed by category name. Each value matches the
  shape returned by Get-TrxResults so the existing renderer in
  Review-PR.ps1 just needs the per-category dict.

.PARAMETER BuildId
  AzDO build ID returned by Wait-CopilotUITests.

.PARAMETER OutputDir
  Where to download artifacts. Defaults to a temp folder.

.PARAMETER ScriptDir
  Path to .github/scripts (so we can dot-source Get-TrxResults from
  Review-PR.ps1). Defaults to the parent of this script.
#>
param(
    [Parameter(Mandatory=$true)]
    [int]$BuildId,

    [string]$OutputDir = "",

    [string]$ScriptDir = "",

    [string]$Org = "https://devdiv.visualstudio.com",
    [string]$Project = "DevDiv"
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ScriptDir)) {
    # shared/Aggregate-UITestArtifacts.ps1 lives in .github/scripts/shared,
    # Get-TrxResults lives one level up in .github/scripts/Review-PR.ps1.
    $ScriptDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
}
$reviewScript = Join-Path $ScriptDir "Review-PR.ps1"
if (-not (Test-Path $reviewScript)) {
    throw "Review-PR.ps1 not found at '$reviewScript' — needed for Get-TrxResults"
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path ([System.IO.Path]::GetTempPath()) "copilot-uitests-$BuildId"
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# --------- Source Get-TrxResults ---------
$trxHelperPath = Join-Path $PSScriptRoot "Get-TrxResults.ps1"
if (Test-Path $trxHelperPath) {
    . $trxHelperPath
} else {
    throw "Get-TrxResults.ps1 not found at $trxHelperPath"
}

# Map artifact name → matrix category. Job names look like:
#   android_ui_tests_controls_30_<CATEGORYGROUP>
#   ios_ui_tests_mono_controls_latest_<CATEGORYGROUP>
#   winui_ui_tests_controls_<CATEGORYGROUP>
#   mac_ui_tests_controls_<CATEGORYGROUP>
function Get-CategoryFromArtifactName {
    param([string]$ArtifactName)

    # Pattern: drop-<stagename>-<jobname>-<attempt>
    $stagePrefixes = @(
        'android_ui_tests', 'android_ui_tests_coreclr', 'android_ui_tests_material3',
        'ios_ui_tests_mono', 'ios_ui_tests_mono_cv1', 'ios_ui_tests_mono_carv1',
        'ios_ui_tests_nativeaot',
        'winui_ui_tests', 'mac_ui_tests'
    )

    $name = $ArtifactName -replace '^drop-', '' -replace '-\d+$', ''

    foreach ($sp in $stagePrefixes | Sort-Object Length -Descending) {
        if ($name -match "^${sp}-(.+)$") {
            return $Matches[1].Trim()
        }
    }
    return $name
}

# Walk a pre-populated OutputDir: find all .trx files (one per matrix
# job's drop-* artifact) and merge by category. Pure function — no az
# calls — so it can be tested with synthetic fixtures.
function Get-AggregatedTrxFromDirectory {
    param([string]$RootDir)

    $byCategory = @{}
    if (-not (Test-Path $RootDir)) {
        return $byCategory
    }
    $trxFiles = @(Get-ChildItem -Path $RootDir -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)
    Write-Host "  Found $($trxFiles.Count) TRX file(s) under $RootDir" -ForegroundColor Gray

    foreach ($trx in $trxFiles) {
        $trxResult = Get-TrxResults -TrxPath $trx.FullName
        if (-not $trxResult) { continue }

        $relative = $trx.FullName.Substring($RootDir.Length).TrimStart('/','\')
        $artName = $relative.Split([System.IO.Path]::DirectorySeparatorChar)[0]
        $category = Get-CategoryFromArtifactName -ArtifactName $artName

        if (-not $byCategory.ContainsKey($category)) {
            $byCategory[$category] = @{
                Total                      = 0
                Passed                     = 0
                Failed                     = 0
                Skipped                    = 0
                Results                    = @()
                TrxPaths                   = @()
                ArtifactName               = $artName
                SetupFailure               = $false
                SetupFailureCount          = 0
                SetupFailureMessage        = ''
                SetupFailureStack          = ''
                SetupFailureSignatureCount = 0
            }
        }
        $cur = $byCategory[$category]
        $cur.Total   += [int]$trxResult.Total
        $cur.Passed  += [int]$trxResult.Passed
        $cur.Failed  += [int]$trxResult.Failed
        $cur.Skipped += [int]$trxResult.Skipped
        $cur.Results = @($cur.Results) + @($trxResult.Results)
        $cur.TrxPaths = @($cur.TrxPaths) + @($trx.FullName)
        $byCategory[$category] = $cur
    }

    foreach ($category in @($byCategory.Keys)) {
        $cur = $byCategory[$category]
        $failedResults = @($cur.Results | Where-Object { $_.status -eq 'Failed' })
        if ($failedResults.Count -eq 0) {
            continue
        }

        $setupFailures = @($failedResults | Where-Object {
            $errorText = [string]($_.error)
            $stackText = [string]($_.stack)
            $errorText -match '^\s*OneTimeSetUp:' -or
                $errorText -match 'Timed out waiting for Go To Test button to appear' -or
                $stackText -match '(_GalleryUITest\.FixtureSetup|\bFixtureSetup\b|UITestBase\.(OneTimeSetup|TestSetup))'
        })

        if ($setupFailures.Count -ne $failedResults.Count) {
            continue
        }

        $signatures = @{}
        foreach ($failure in $setupFailures) {
            $errorText = ([string]($failure.error) -replace '\s+', ' ').Trim()
            $stackText = ([string]($failure.stack) -replace '\s+', ' ').Trim()
            $signatures["$errorText|$stackText"] = $true
        }

        $sample = $setupFailures | Select-Object -First 1
        $cur.SetupFailure = $true
        $cur.SetupFailureCount = $setupFailures.Count
        $cur.SetupFailureMessage = ([string]($sample.error)).Trim()
        $cur.SetupFailureStack = ([string]($sample.stack)).Trim()
        $cur.SetupFailureSignatureCount = $signatures.Count
        $byCategory[$category] = $cur
    }

    return $byCategory
}

# --------- List artifacts on the build ---------
Write-Host "Aggregate-UITestArtifacts: listing artifacts for build #$BuildId" -ForegroundColor Cyan
$artifactsRaw = az pipelines runs artifact list `
    --org $Org --project $Project --run-id $BuildId -o json 2>$null
if ($LASTEXITCODE -ne 0 -or -not $artifactsRaw) {
    Write-Host "  ⚠️  Failed to list artifacts; falling back to walking $OutputDir directly" -ForegroundColor Yellow
    return Get-AggregatedTrxFromDirectory -RootDir $OutputDir
}
$artifacts = $artifactsRaw | ConvertFrom-Json

# Match drop-* (one per platform job) — that's where ui-tests-steps.yml's
# PublishBuildArtifacts@1 lands. Skip CopilotLogs / BuildLogs / etc.
# Also accept legacy names like "<stage>-<job> (attempt N)" which the
# template's PublishBuildArtifacts step uses by default.
$dropArtifacts = @($artifacts | Where-Object {
    $_.name -match '^drop-' -or
    $_.name -match '^ui-tests-samples' -or
    $_.name -match '\(attempt \d+\)$'
})
Write-Host "  Found $($dropArtifacts.Count) drop/test artifact(s) on build #$BuildId" -ForegroundColor Gray

if ($dropArtifacts.Count -eq 0) {
    Write-Host "  ⚠️  No drop-* artifacts — child build may not have reached test execution stage" -ForegroundColor Yellow
    return @{}
}

# --------- Download each artifact ---------
foreach ($art in $dropArtifacts) {
    $artDir = Join-Path $OutputDir $art.name
    if (Test-Path $artDir) { continue }   # already downloaded
    Write-Host "  ⬇  $($art.name)" -ForegroundColor DarkGray
    az pipelines runs artifact download `
        --org $Org --project $Project --run-id $BuildId `
        --artifact-name $art.name --path $artDir 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "    ⚠  download failed for $($art.name)" -ForegroundColor Yellow
    }
}

# --------- Walk all .trx files ---------
$byCategory = Get-AggregatedTrxFromDirectory -RootDir $OutputDir

Write-Host "Aggregate-UITestArtifacts: aggregated $($byCategory.Count) category bucket(s)" -ForegroundColor Cyan
foreach ($k in $byCategory.Keys | Sort-Object) {
    $b = $byCategory[$k]
    Write-Host "  ${k}: total=$($b.Total) passed=$($b.Passed) failed=$($b.Failed) skipped=$($b.Skipped) (from $($b.TrxPaths.Count) TRX file(s))" -ForegroundColor Gray
}

return $byCategory

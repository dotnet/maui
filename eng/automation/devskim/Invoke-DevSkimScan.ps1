#Requires -Version 7.0
<#
.SYNOPSIS
Scans the src and eng trees with DevSkim and fails if it reports a banned API.

.DESCRIPTION
Runs DevSkim over each source tree with every rule at every severity, ignoring inline suppressions and
excluding only the globs in devskim-options.json. DevSkim exits with 0 even when it reports findings, so the
SARIF reports are evaluated here: any result tagged API.DangerousAPI.BannedFunction fails the script, at any
severity. Findings from other rules stay in the reports for triage.

A clean result has to mean something, so the script also fails when
  - DevSkim isn't the version this script was validated with,
  - the options file is missing (DevSkim would silently scan everything),
  - DevSkim doesn't report the banned strcpy calls in the canary files that the script writes,
  - DevSkim analyzed no files in a source tree, or
  - a report is missing or malformed.
#>
[CmdletBinding()]
param (
    # The devskim executable from the pinned Microsoft.CST.DevSkim.CLI package.
    [Parameter(Mandatory)]
    [string] $DevSkim,

    # Receives a SARIF report and a log for each scan. Keep it outside the scanned trees.
    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [string] $RepositoryRoot = (Join-Path $PSScriptRoot '../../..'),

    [string[]] $SourceTrees = @('src', 'eng'),

    [string] $OptionsFile = (Join-Path $PSScriptRoot 'devskim-options.json')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'DevSkimReport.psm1') -Force

# The DevSkim version whose command line, SARIF layout and log summary this script relies on. When the pinned
# version in stage-devskim.yml changes, check all three against the new version and update this value.
$validatedVersion = '1.0.100'

$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$OutputDirectory = (New-Item -ItemType Directory -Path $OutputDirectory -Force).FullName

$version = & $DevSkim --version | Select-Object -First 1
if ($version -notmatch "^devskim $([regex]::Escape($validatedVersion))(\+|$)") {
    throw "This script was validated with DevSkim $validatedVersion, but '$DevSkim' reports '$version'."
}

# DevSkim ignores an options file that doesn't exist and then scans everything, third-party code included.
$options = Get-Content -LiteralPath $OptionsFile -Raw | ConvertFrom-Json -AsHashtable
if ($options -isnot [System.Collections.IDictionary] -or
    $options['Globs'] -isnot [System.Collections.IList] -or
    $options['Globs'].Count -eq 0) {
    throw "'$OptionsFile' doesn't define the Globs to exclude."
}

function Invoke-DevSkim {
    param ([string] $Name, [string] $Source, [string] $BasePath, [string[]] $ExtraArguments)

    $sarif = Join-Path $OutputDirectory "$Name.sarif"
    $log = Join-Path $OutputDirectory "$Name.log"
    # The Debug log ends with the number of files analyzed.
    & $DevSkim analyze --source-code $Source --base-path $BasePath --output-file $sarif --file-format sarif `
        --severity 'Critical,Important,Moderate,BestPractice,ManualReview' --confidence 'High,Medium,Low' `
        --disable-supression --disable-parallel --console-verbosity Debug @ExtraArguments 2>&1 |
        Tee-Object -FilePath $log | Write-Host
    if ($LASTEXITCODE -ne 0) {
        throw "DevSkim failed with exit code $LASTEXITCODE while scanning '$Source'. See '$log'."
    }

    [pscustomobject]@{ Sarif = $sarif; Log = $log }
}

function Test-DevSkimDetection {
    $directory = Join-Path $OutputDirectory 'canary'
    $null = New-Item -ItemType Directory -Path $directory -Force
    $files = 'canary.c', 'canary.cpp', 'canary.m'
    foreach ($file in $files) {
        Set-Content -LiteralPath (Join-Path $directory $file) -Value "void Canary(char *destination, const char *source)`n{`n    strcpy(destination, source);`n}"
    }

    $scan = Invoke-DevSkim -Name 'canary' -Source $directory -BasePath $directory -ExtraArguments '--ignore-globs', '**/.git/**'
    $detected = @(Get-DevSkimBannedFindings -Path $scan.Sarif | ForEach-Object Uri)
    $missed = $files | Where-Object { $_ -notin $detected }
    if ($missed) {
        throw "DevSkim didn't report the banned strcpy call in $($missed -join ', '). Its banned-function rules don't work, so a clean scan would mean nothing."
    }
    Write-Host "DevSkim reports the banned strcpy call in all $($files.Count) canary files."
}

Test-DevSkimDetection

$findings = @()
foreach ($tree in $SourceTrees) {
    $source = Join-Path $RepositoryRoot $tree
    if (-not (Test-Path -LiteralPath $source -PathType Container)) {
        throw "The source tree '$tree' doesn't exist in '$RepositoryRoot'."
    }

    $scan = Invoke-DevSkim -Name ($tree -replace '[\\/]', '-') -Source $source -BasePath $RepositoryRoot -ExtraArguments '--options-json', $OptionsFile
    $analyzed = Get-DevSkimAnalyzedFileCount -LogPath $scan.Log
    if ($analyzed -eq 0) {
        throw "DevSkim analyzed no files in '$tree'. The globs in '$OptionsFile' match absolute paths, so they exclude everything when the checkout is below a directory named like one of them, such as 'tests'."
    }

    $treeFindings = @(Get-DevSkimBannedFindings -Path $scan.Sarif)
    Write-Host "${tree}: $analyzed files analyzed, $($treeFindings.Count) banned-function finding(s)."
    $findings += $treeFindings
}

foreach ($finding in $findings) {
    Write-Host "$($finding.Uri):$($finding.Line): $($finding.RuleId): $($finding.Message)"
}
if ($findings.Count -gt 0) {
    throw "DevSkim reported $($findings.Count) banned-function finding(s). All results are in '$OutputDirectory'."
}
Write-Host 'DevSkim found no banned functions.'

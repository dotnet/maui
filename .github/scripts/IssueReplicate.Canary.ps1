#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Input', 'Candidate')][string]$Mode,
    [Parameter(Mandatory)][ValidateSet('android-carousel', 'android-scrollview', 'ios-refresh', 'ios-shell-navigation',
        'android-webview-scroll', 'ios-modal-singleton', 'ios-collection-shrink')][string]$Scenario,
    [Parameter(Mandatory)][string]$DataRoot,
    [Parameter(Mandatory)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$root = Join-Path $DataRoot $Scenario
$utf8 = [Text.UTF8Encoding]::new($false, $true)

function Read-CanaryText {
    param([string]$Name, [int]$MaxBytes)

    $file = Get-Item -LiteralPath (Join-Path $root $Name) -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -lt 1 -or $file.Length -gt $MaxBytes -or
        $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Canary input must be a bounded regular file.'
    }
    return $utf8.GetString([IO.File]::ReadAllBytes($file.FullName))
}

$manifestText = Read-CanaryText -Name 'manifest.json' -MaxBytes 50000
$manifest = $manifestText | ConvertFrom-Json -Depth 6
if ($manifest.schemaVersion -ne 1 -or $manifest.issueNumber -lt 1 -or $manifest.commentId -lt 1 -or
    $manifest.platform -cnotin @('android', 'ios') -or
    $manifest.targetRef -cnotmatch '^(main|net[0-9]+\.0)$' -or
    $manifest.targetSha -cnotmatch '^[0-9a-f]{40}$' -or
    $manifest.sampleSha256 -cnotmatch '^[0-9a-f]{64}$' -or
    $manifest.sourceType -cnotin @('attachment', 'repository') -or
    ($manifest.sourceType -ceq 'repository' -and $manifest.sourceCommit -cnotmatch '^[0-9a-f]{40}$')) {
    throw 'The reviewed canary snapshot is not immutable.'
}
$source = Get-IssueReplicateSource -AuthorTexts @("[repro.zip]($($manifest.sourceUrl))")
if ($source.Type -cne $manifest.sourceType -or $source.Url -cne $manifest.sourceUrl) {
    throw 'The reviewed canary source is not an approved public GitHub repro.'
}
$name = 'manifest.json'
$text = $manifestText
if ($Mode -eq 'Candidate') {
    $candidate = @{
        kind = 'ui'
        files = @(
            @{
                path = "src/Controls/tests/TestCases.HostApp/Issues/Issue$($manifest.issueNumber).cs"
                content = Read-CanaryText -Name 'HostApp.cs' -MaxBytes 30000
            },
            @{
                path = "src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue$($manifest.issueNumber).cs"
                content = Read-CanaryText -Name 'Tests.cs' -MaxBytes 30000
            }
        )
    }
    Assert-IssueReplicateCandidate -Candidate $candidate -IssueNumber $manifest.issueNumber `
        -Platform $manifest.platform | Out-Null
    $name = 'candidate.json'
    $text = $candidate | ConvertTo-Json -Depth 6
    if ($utf8.GetByteCount($text) -gt 80000) { throw 'The reviewed canary candidate is oversized.' }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$destination = Join-Path $OutputDirectory $name
if (Test-Path -LiteralPath $destination) { throw 'Canary preparation cannot overwrite existing job data.' }
[IO.File]::WriteAllText($destination, $text, $utf8)
Write-Host "Prepared reviewed $Scenario $Mode data without model invocation."

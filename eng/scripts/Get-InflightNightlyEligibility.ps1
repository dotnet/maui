[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [string] $SourceBranch = $env:BUILD_SOURCEBRANCH
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($SourceBranch -cne 'refs/heads/inflight/current' -and
    $SourceBranch -cnotmatch '^refs/heads/inflight/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*$') {
    throw "Nightly selection requires a legacy or versioned inflight source ref."
}

$state = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -StatePath $StatePath -ResolveStateOnly |
    ConvertFrom-Json
if ($state.repository -cne 'https://github.com/dotnet/maui.git') {
    throw "Official nightly selection requires the canonical dotnet/maui release state."
}

[pscustomobject][ordered]@{
    stateSha256 = $state.stateSha256
    mode = $state.nightlyMode
    activeBranch = "refs/heads/$($state.nightlyBranch)"
    sourceBranch = $SourceBranch
    shouldBuild = $SourceBranch -ceq "refs/heads/$($state.nightlyBranch)"
} | ConvertTo-Json

#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$manifest = Get-Content -Raw -LiteralPath (Join-Path $InputDirectory 'manifest.json') | ConvertFrom-Json
$zip = Join-Path $InputDirectory 'sample.zip'
if ($manifest.schemaVersion -ne 1 -or
    (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant() -cne $manifest.sampleSha256) {
    throw 'The sample does not match the issue snapshot.'
}
$sampleDir = Join-Path $OutputDirectory 'sample'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
Assert-IssueReplicateZip -Path $zip -ExtractTo $sampleDir
$projects = @(Get-ChildItem -LiteralPath $sampleDir -Filter *.csproj -File -Recurse |
    Where-Object { $_.FullName -notmatch '[/\\](obj|bin)[/\\]' })
if ($projects.Count -ne 1) { throw 'The sample needs exactly one buildable .csproj.' }
$project = $projects[0]
$projectXml = Get-Content -LiteralPath $project.FullName -Raw
$tfm = [regex]::Match($projectXml, "net[0-9]+\.[0-9]+-$($manifest.platform)\b").Value
if (-not $tfm) { throw "The sample project does not target $($manifest.platform)." }

$log = Join-Path $OutputDirectory 'sample-build.log'
$redacted = [System.Collections.Generic.List[string]]::new()
& dotnet build $project.FullName -c Debug -f $tfm --nologo --verbosity quiet 2>&1 |
    ForEach-Object {
        $line = $_.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', ''
        if ($redacted.Count -lt 3000) { $redacted.Add($line) }
        Write-Host $line
    }
$exitCode = $LASTEXITCODE
$redacted | Set-Content -LiteralPath $log -Encoding utf8
@{
    sampleSha256 = $manifest.sampleSha256
    targetSha = $manifest.targetSha
    buildSucceeded = ($exitCode -eq 0)
    sampleProject = $project.Name
    targetFramework = $tfm
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'sample-result.json') -Encoding utf8
if ($exitCode -ne 0) { throw 'The author sample did not build; reproduction is inconclusive.' }

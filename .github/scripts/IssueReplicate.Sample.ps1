#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$NuGetConfigPath = '',
    [scriptblock]$OnCompleted,
    [switch]$CoreLoaded
)

$ErrorActionPreference = 'Stop'
if (-not $CoreLoaded) { . (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1') }
$manifest = Get-Content -Raw -LiteralPath (Join-Path $InputDirectory 'manifest.json') | ConvertFrom-Json
$zip = Join-Path $InputDirectory 'sample.zip'
if ($manifest.schemaVersion -ne 1 -or
    (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant() -cne $manifest.sampleSha256) {
    throw 'The sample does not match the issue snapshot.'
}
$sampleDir = Join-Path $OutputDirectory 'sample'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
Assert-IssueReplicateZip -Path $zip -ExtractTo $sampleDir
$selection = Get-IssueReplicateSampleProject -Directory $sampleDir -Platform $manifest.platform
$project = $selection.Project
$tfm = $selection.TargetFramework

$log = Join-Path $OutputDirectory 'sample-build.log'
$redacted = [System.Collections.Generic.List[string]]::new()
$errors = [System.Collections.Generic.List[string]]::new()
$logBytes = 0
$outputTruncated = $false
$buildArguments = @('build', $project.FullName, '-c', 'Debug', '-f', $tfm, "-p:TargetFrameworks=$tfm",
    '--nologo', '--verbosity', 'quiet')
if ($NuGetConfigPath) {
    $config = Get-Item -LiteralPath $NuGetConfigPath -ErrorAction Stop
    if ($config.PSIsContainer -or $config.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'The sample restore configuration must be a regular file.'
    }
    $buildArguments += "-p:RestoreConfigFile=$($config.FullName)"
    $buildArguments += '-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json'
}
Push-Location $project.DirectoryName
try {
    & dotnet @buildArguments 2>&1 |
        ForEach-Object {
            $line = $_.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', ''
            if ($line.Length -gt 8192) {
                $length = if ([char]::IsHighSurrogate($line[8191])) { 8191 } else { 8192 }
                $line = $line.Substring(0, $length)
                if (-not $outputTruncated) { Write-Warning 'Author build output was truncated to its fixed line/output bounds.' }
                $outputTruncated = $true
            }
            if ($errors.Count -lt 3 -and $line -match '\berror(?:\s+[A-Z]+[0-9]+)?\s*:') {
                $errors.Add($line)
            }
            $bytes = [Text.Encoding]::UTF8.GetByteCount($line) + 1
            if ($redacted.Count -lt 3000 -and $logBytes + $bytes -le 2MB) {
                $redacted.Add($line)
                $logBytes += $bytes
                Write-Host $line
            }
            else {
                if (-not $outputTruncated) { Write-Warning 'Author build output was truncated to its fixed line/output bounds.' }
                $outputTruncated = $true
            }
        }
    $exitCode = $LASTEXITCODE
}
finally { Pop-Location }
$redacted | Set-Content -LiteralPath $log -Encoding utf8
$diagnostic = $errors -join "`n"
if ($diagnostic.Length -gt 2048) {
    $length = if ([char]::IsHighSurrogate($diagnostic[2047])) { 2047 } else { 2048 }
    $diagnostic = $diagnostic.Substring(0, $length)
}
$result = @{
    sampleSha256    = $manifest.sampleSha256
    targetSha       = $manifest.targetSha
    buildSucceeded  = ($exitCode -eq 0)
    sampleProject   = $project.Name
    targetFramework = $tfm
    diagnostic      = $diagnostic
}
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'sample-result.json') -Encoding utf8
if ($OnCompleted) { & $OnCompleted $result }
if ($exitCode -ne 0) { throw 'The author sample did not build; reproduction is inconclusive.' }

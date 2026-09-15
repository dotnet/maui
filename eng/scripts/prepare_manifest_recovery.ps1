param (
  [Parameter(Mandatory)]
  [string] $SourceBuildId,
  [Parameter(Mandatory)]
  [string] $CommitHash,
  [string] $GhOwner = 'dotnet',
  [string] $GhRepo = 'maui',
  [bool] $PushWorkloadSet = $false,
  [bool] $PushNugetOrg = $true,
  [string] $IncludeFilters = 'skip',
  [string] $ExcludeFilters = 'skip',
  [string] $PackSkipFilters = 'skip',
  [Parameter(Mandatory)]
  [string] $PacksPath,
  [Parameter(Mandatory)]
  [string] $ManifestsPath,
  [Parameter(Mandatory)]
  [string] $OutputPath,
  [string] $CollectionUri = $env:SYSTEM_COLLECTIONURI,
  [string] $ProjectId = $env:SYSTEM_TEAMPROJECTID,
  [string] $DefinitionId = $env:SYSTEM_DEFINITIONID,
  [string] $RepositoryId = $env:BUILD_REPOSITORY_ID,
  [string] $CurrentBuildId = $env:BUILD_BUILDID,
  [string] $AccessToken = $env:SYSTEM_ACCESSTOKEN
)

$ErrorActionPreference = 'Stop'

$sourceId = 0
if ($SourceBuildId -notmatch '^[1-9][0-9]*$' -or
    ![int]::TryParse($SourceBuildId, [ref] $sourceId)) {
  throw 'manifestSourceBuildId must be a positive Azure DevOps build ID.'
}
if ($SourceBuildId -eq $CurrentBuildId) {
  throw 'Manifest recovery cannot use the current run as its source.'
}
if ($CommitHash -notmatch '^[0-9a-fA-F]{40}$') {
  throw 'Manifest recovery requires the full original release commitHash.'
}
if ($GhOwner -cne 'dotnet' -or $GhRepo -cne 'maui') {
  throw 'Retained manifest recovery supports only dotnet/maui.'
}
if ($PushWorkloadSet -or !$PushNugetOrg -or $PackSkipFilters -ne 'skip') {
  throw 'Manifest recovery requires pushWorkloadSet=false, pushNugetOrg=true, and nugetAlreadyAttemptedPackFilters=skip.'
}
if ($CollectionUri -notmatch '^https://dev\.azure\.com/[a-zA-Z0-9-]+/?$' -or
    [string]::IsNullOrWhiteSpace($ProjectId) -or
    [string]::IsNullOrWhiteSpace($DefinitionId) -or
    [string]::IsNullOrWhiteSpace($RepositoryId) -or
    [string]::IsNullOrWhiteSpace($CurrentBuildId) -or
    [string]::IsNullOrWhiteSpace($AccessToken)) {
  throw 'Manifest recovery requires the authenticated Azure DevOps build context.'
}

$buildUri = "$($CollectionUri.TrimEnd('/'))/$([Uri]::EscapeDataString($ProjectId))/_apis/build/builds/$sourceId"
$headers = @{ Authorization = "Bearer $AccessToken" }
$source = Invoke-RestMethod -Uri "${buildUri}?api-version=7.1" -Headers $headers -TimeoutSec 60 -MaximumRedirection 0
if ($source.id -ne $sourceId -or $source.project.id -ne $ProjectId -or
    $source.definition.id -ne $DefinitionId -or $source.repository.id -ne $RepositoryId -or
    $source.repository.type -ne 'TfsGit') {
  throw "The source must belong to this project, release pipeline, and internal repository. Received build '$($source.id)', project '$($source.project.id)', definition '$($source.definition.id)', repository '$($source.repository.id)' ($($source.repository.type))."
}
if ($source.status -ne 'completed' -or $source.reason -eq 'pullRequest' -or
    $source.sourceBranch -notlike 'refs/heads/*' -or
    $source.sourceVersion -notmatch '^[0-9a-fA-F]{40}$') {
  throw 'The source must be a completed, non-PR release run.'
}
$sourceParameters = $source.templateParameters
if (!$sourceParameters -or $sourceParameters.ghOwner -cne $GhOwner -or
    $sourceParameters.ghRepo -cne $GhRepo -or $sourceParameters.commitHash -ine $CommitHash) {
  throw 'The source release repository or commitHash does not match the requested release.'
}
if ($sourceParameters.nugetIncludeFilters -cne $IncludeFilters -or
    $sourceParameters.nugetExcludeFilters -cne $ExcludeFilters) {
  throw 'Use the original nugetIncludeFilters and nugetExcludeFilters; recovery cannot change the retained selection.'
}

$timeline = Invoke-RestMethod -Uri "$buildUri/timeline?api-version=7.1" -Headers $headers -TimeoutSec 60 -MaximumRedirection 0
$preparation = @($timeline.records | Where-Object {
  $_.type -eq 'Job' -and $_.identifier -eq 'prepare_release.prepare_release.__default'
})
if ($preparation.Count -ne 1 -or $preparation[0].result -notin @('succeeded', 'succeededWithIssues')) {
  throw 'The source run has no successful original release preparation.'
}
$gather = @($timeline.records | Where-Object {
  $_.parentId -eq $preparation[0].id -and
  $_.name -eq 'Resolve BAR build and prepare packages' -and $_.result -eq 'succeeded'
})
if ($gather.Count -ne 1) {
  throw 'The source did not successfully gather and classify its release packages.'
}

$artifacts = Invoke-RestMethod -Uri "$buildUri/artifacts?api-version=7.1" -Headers $headers -TimeoutSec 60 -MaximumRedirection 0
foreach ($name in @('MauiPacksForNuGet', 'MauiManifestsForNuGet')) {
  $artifact = @($artifacts.value | Where-Object { $_.name -ceq $name -and $_.resource.type -eq 'PipelineArtifact' })
  if ($artifact.Count -ne 1) {
    throw "The source run has no retained pipeline artifact '$name'."
  }
}

# Only execute the helper from this checkout, never a script from a recovered artifact.
$helper = Join-Path $PSScriptRoot 'nuget_release_packages.ps1'
& $helper -Action ValidateManifests -PackagesPath $ManifestsPath
$packs = @(Get-Content -LiteralPath (Join-Path $PacksPath 'expected-packages.json') -Raw | ConvertFrom-Json)
if (@($packs | Where-Object { $_.id -like '*Manifest*' }).Count -gt 0) {
  throw 'The retained pack inventory must not contain workload manifests.'
}
& $helper -Action Verify -PackagesPath $PacksPath -MaxAttempts 1

$manifests = @(Get-Content -LiteralPath (Join-Path $ManifestsPath 'expected-packages.json') -Raw | ConvertFrom-Json)
$manifestAudit = @($manifests | ForEach-Object {
  [ordered]@{
    id = $_.id
    version = $_.version
    fileName = $_.fileName
    sha256 = (Get-FileHash -LiteralPath (Join-Path $ManifestsPath $_.fileName) -Algorithm SHA256).Hash
  }
})
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
Copy-Item -LiteralPath $helper -Destination $OutputPath
$helperHash = (Get-FileHash -LiteralPath (Join-Path $OutputPath 'nuget_release_packages.ps1') -Algorithm SHA256).Hash
[ordered]@{
  sourceBuildId = $sourceId
  sourcePipelineCommit = $source.sourceVersion
  repository = "https://github.com/$GhOwner/$GhRepo"
  releaseCommit = $CommitHash
  includeFilters = $IncludeFilters
  excludeFilters = $ExcludeFilters
  verifiedPacks = @($packs | Sort-Object id, version)
  manifests = $manifestAudit
  packageStatusScriptHash = $helperHash
} | ConvertTo-Json -Depth 6 |
  Set-Content -LiteralPath (Join-Path $OutputPath 'recovery-audit.json') -Encoding utf8
Write-Host "Validated $($manifests.Count) retained manifests from release run $sourceId; verified $($packs.Count) existing packs. No packs will be published."
Write-Host "##vso[task.setvariable variable=PackageStatusScriptHash;isOutput=true]$helperHash"

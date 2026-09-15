#Requires -Modules Pester

Describe 'prepare_manifest_recovery.ps1' {
  BeforeEach {
    $scriptPath = Join-Path $PSScriptRoot 'prepare_manifest_recovery.ps1'
    $root = Join-Path $TestDrive ([guid]::NewGuid().ToString())
    $packsPath = Join-Path $root 'packs'
    $manifestsPath = Join-Path $root 'manifests'
    $outputPath = Join-Path $root 'output'
    New-Item -ItemType Directory -Path $packsPath, $manifestsPath | Out-Null
    $pack = @{ id = 'Microsoft.Maui.Sdk'; version = '10.0.101'; normalizedVersion = '10.0.101'; fileName = 'Microsoft.Maui.Sdk.10.0.101.nupkg' }
    $manifest = @{ id = 'Microsoft.NET.Sdk.Maui.Manifest-10.0.100'; version = '10.0.101'; normalizedVersion = '10.0.101'; fileName = 'Microsoft.NET.Sdk.Maui.Manifest-10.0.100.10.0.101.nupkg' }
    ConvertTo-Json -InputObject @($pack) | Set-Content (Join-Path $packsPath 'expected-packages.json')
    ConvertTo-Json -InputObject @($manifest) | Set-Content (Join-Path $manifestsPath 'expected-packages.json')
    Set-Content (Join-Path $manifestsPath 'nuget_release_packages.ps1') "throw 'Recovered scripts must never execute'"
    $archive = [System.IO.Compression.ZipFile]::Open(
      (Join-Path $manifestsPath $manifest.fileName), [System.IO.Compression.ZipArchiveMode]::Create)
    try {
      $writer = [System.IO.StreamWriter]::new($archive.CreateEntry('manifest.nuspec').Open())
      try {
        $writer.Write("<package><metadata><id>$($manifest.id)</id><version>$($manifest.version)</version></metadata></package>")
      }
      finally {
        $writer.Dispose()
      }
    }
    finally {
      $archive.Dispose()
    }
    $arguments = @{
      SourceBuildId = '42'
      CommitHash = '1234567890123456789012345678901234567890'
      PacksPath = $packsPath
      ManifestsPath = $manifestsPath
      OutputPath = $outputPath
      CollectionUri = 'https://dev.azure.com/example/'
      ProjectId = 'project'
      DefinitionId = '1445'
      RepositoryId = 'repository'
      CurrentBuildId = '43'
      AccessToken = 'test-token'
    }
    $sourceResponse = @{
      id = 42
      project = @{ id = 'project' }
      definition = @{ id = 1445 }
      repository = @{ id = 'repository'; type = 'TfsGit' }
      status = 'completed'
      result = 'partiallySucceeded'
      reason = 'manual'
      sourceBranch = 'refs/heads/release/test'
      sourceVersion = 'abcdefabcdefabcdefabcdefabcdefabcdefabcd'
      templateParameters = @{
        ghOwner = 'dotnet'
        ghRepo = 'maui'
        commitHash = $arguments.CommitHash
        nugetIncludeFilters = 'skip'
        nugetExcludeFilters = 'skip'
      }
    }
    $timelineResponse = @{ records = @(
      @{ id = 'prepare-job'; type = 'Job'; identifier = 'prepare_release.prepare_release.__default'; result = 'succeeded' }
      @{ parentId = 'prepare-job'; name = 'Resolve BAR build and prepare packages'; result = 'succeeded' }
    ) }
    $artifactsResponse = @{ value = @(
      @{ name = 'MauiPacksForNuGet'; resource = @{ type = 'PipelineArtifact' } }
      @{ name = 'MauiManifestsForNuGet'; resource = @{ type = 'PipelineArtifact' } }
    ) }
    Mock Invoke-RestMethod {
      if ($Uri -like '*/timeline?*') { return $timelineResponse }
      if ($Uri -like '*/artifacts?*') { return $artifactsResponse }
      if ($Uri -eq 'https://dev.azure.com/example/project/_apis/build/builds/42?api-version=7.1') {
        return $sourceResponse
      }
      throw "Unexpected request: $Uri"
    }
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 200 } }
  }

  It 'reuses the exact manifests, verifies packs without their nupkgs, and stages only trusted tooling' {
    $originalHash = (Get-FileHash (Join-Path $manifestsPath $manifest.fileName)).Hash
    $output = & $scriptPath @arguments 6>&1

    $audit = Get-Content (Join-Path $outputPath 'recovery-audit.json') -Raw | ConvertFrom-Json
    $audit.sourceBuildId | Should -Be 42
    $audit.releaseCommit | Should -Be $arguments.CommitHash
    $audit.verifiedPacks.Count | Should -Be 1
    $audit.manifests.Count | Should -Be 1
    $audit.manifests[0].sha256 | Should -Be $originalHash
    (Get-FileHash (Join-Path $manifestsPath $manifest.fileName)).Hash | Should -Be $originalHash
    (Get-FileHash (Join-Path $outputPath 'nuget_release_packages.ps1')).Hash |
      Should -Be (Get-FileHash (Join-Path $PSScriptRoot 'nuget_release_packages.ps1')).Hash
    @(Get-ChildItem $outputPath -Filter '*.nupkg').Count | Should -Be 0
    $output -join "`n" | Should -Match 'PackageStatusScriptHash;isOutput=true'
    Should -Invoke Invoke-WebRequest -Times 1 -Exactly -ParameterFilter {
      $Method -eq 'Head' -and $Uri -like 'https://api.nuget.org/*microsoft.maui.sdk/10.0.101/*'
    }
  }

  It 'rejects invalid source build ID <Value>' -ForEach @(
    @{ Value = '0' }, @{ Value = '-1' }, @{ Value = '1.5' },
    @{ Value = '42/other' }, @{ Value = '2147483648' }
  ) {
    $arguments.SourceBuildId = $Value
    { & $scriptPath @arguments } | Should -Throw '*positive Azure DevOps build ID*'
    Should -Invoke Invoke-RestMethod -Times 0
  }

  It 'accepts an overall failed release when preparation succeeded' {
    $sourceResponse.result = 'failed'
    { & $scriptPath @arguments } | Should -Not -Throw
    Test-Path (Join-Path $outputPath 'recovery-audit.json') | Should -BeTrue
  }

  It 'rejects the current run as its source' {
    $arguments.CurrentBuildId = '42'
    { & $scriptPath @arguments } | Should -Throw '*current run*'
  }

  It 'requires the original full release commit' {
    $arguments.CommitHash = '1234567'
    { & $scriptPath @arguments } | Should -Throw '*full original release commitHash*'
  }

  It 'rejects non-MAUI recovery' {
    $arguments.GhRepo = 'android-libraries'
    { & $scriptPath @arguments } | Should -Throw '*only dotnet/maui*'
  }

  It 'rejects conflicting operation <Name>' -ForEach @(
    @{ Name = 'PushWorkloadSet'; Value = $true }
    @{ Name = 'PushNugetOrg'; Value = $false }
    @{ Name = 'PackSkipFilters'; Value = '*' }
  ) {
    $arguments[$Name] = $Value
    { & $scriptPath @arguments } | Should -Throw '*requires pushWorkloadSet=false*'
    Should -Invoke Invoke-WebRequest -Times 0
  }

  It 'does not send credentials outside Azure DevOps' {
    $arguments.CollectionUri = 'https://example.com/'
    { & $scriptPath @arguments } | Should -Throw '*authenticated Azure DevOps build context*'
    Should -Invoke Invoke-RestMethod -Times 0
  }

  It 'requires the authenticated build context field <Name>' -ForEach @(
    @{ Name = 'ProjectId' }, @{ Name = 'DefinitionId' }, @{ Name = 'RepositoryId' },
    @{ Name = 'CurrentBuildId' }, @{ Name = 'AccessToken' }
  ) {
    $arguments[$Name] = ''
    { & $scriptPath @arguments } | Should -Throw '*authenticated Azure DevOps build context*'
    Should -Invoke Invoke-RestMethod -Times 0
  }

  It 'rejects a source from a different <Resource>' -ForEach @(
    @{ Resource = 'project' }, @{ Resource = 'definition' }, @{ Resource = 'repository' }
  ) {
    $sourceResponse[$Resource].id = 'different'
    { & $scriptPath @arguments } | Should -Throw '*this project, release pipeline, and internal repository*'
  }

  It 'rejects a source outside the internal git repository' {
    $sourceResponse.repository.type = 'GitHub'
    { & $scriptPath @arguments } | Should -Throw '*internal repository*'
  }

  It 'rejects an unfinished source run' {
    $sourceResponse.status = 'inProgress'
    { & $scriptPath @arguments } | Should -Throw '*completed, non-PR*'
  }

  It 'rejects a PR source run' {
    $sourceResponse.reason = 'pullRequest'
    { & $scriptPath @arguments } | Should -Throw '*completed, non-PR*'
  }

  It 'rejects an invalid source ref or pipeline commit <Name>' -ForEach @(
    @{ Name = 'sourceBranch'; Value = 'refs/pull/1/merge' }
    @{ Name = 'sourceBranch'; Value = '' }
    @{ Name = 'sourceVersion'; Value = '' }
    @{ Name = 'sourceVersion'; Value = '1234567' }
  ) {
    $sourceResponse[$Name] = $Value
    { & $scriptPath @arguments } | Should -Throw '*completed, non-PR*'
    Test-Path $outputPath | Should -BeFalse
    Should -Invoke Invoke-WebRequest -Times 0
  }

  It 'rejects a source with mismatched <Parameter>' -ForEach @(
    @{ Parameter = 'commitHash' }, @{ Parameter = 'ghRepo' }, @{ Parameter = 'ghOwner' }
  ) {
    $sourceResponse.templateParameters[$Parameter] = 'different'
    { & $scriptPath @arguments } | Should -Throw '*does not match the requested release*'
  }

  It 'requires the original <Parameter>' -ForEach @(
    @{ Parameter = 'nugetIncludeFilters' }, @{ Parameter = 'nugetExcludeFilters' }
  ) {
    $sourceResponse.templateParameters[$Parameter] = '*Special*'
    { & $scriptPath @arguments } | Should -Throw '*original nugetIncludeFilters and nugetExcludeFilters*'
  }

  It 'rejects failed preparation even when the overall source run completed' {
    $timelineResponse.records[0].result = 'failed'
    { & $scriptPath @arguments } | Should -Throw '*no successful original release preparation*'
  }

  It 'rejects a failed gather step' {
    $timelineResponse.records[1].result = 'failed'
    { & $scriptPath @arguments } | Should -Throw '*successfully gather and classify*'
  }

  It 'rejects missing retained artifacts' {
    $artifactsResponse.value = @($artifactsResponse.value[0])
    { & $scriptPath @arguments } | Should -Throw "*no retained pipeline artifact 'MauiManifestsForNuGet'*"
  }

  It 'rejects non-pipeline artifacts' {
    $artifactsResponse.value[1].resource.type = 'Container'
    { & $scriptPath @arguments } | Should -Throw '*no retained pipeline artifact*'
  }

  It 'fails closed when source validation is inaccessible' {
    Mock Invoke-RestMethod { throw 'Access denied' }
    { & $scriptPath @arguments } | Should -Throw '*Access denied*'
    Test-Path $outputPath | Should -BeFalse
  }

  It 'withholds publishing inputs when a pack is not available on NuGet.org' {
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 404 } }
    { & $scriptPath @arguments } | Should -Throw '*Microsoft.Maui.Sdk 10.0.101*'
    Test-Path $outputPath | Should -BeFalse
  }

  It 'rejects a manifest in the pack inventory' {
    ConvertTo-Json -InputObject @($manifest) | Set-Content (Join-Path $packsPath 'expected-packages.json')
    { & $scriptPath @arguments } | Should -Throw '*pack inventory must not contain workload manifests*'
  }

  It 'rejects an empty original pack inventory' {
    '[]' | Set-Content (Join-Path $packsPath 'expected-packages.json')
    { & $scriptPath @arguments } | Should -Throw '*contains no packages*'
    Test-Path $outputPath | Should -BeFalse
  }
}

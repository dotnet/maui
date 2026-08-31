#Requires -Modules Pester

Describe 'ci-official-release.yml' {
  BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot 'ci-official-release.yml'
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
    $nonWorkloadTemplate = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/non-workload-publish.yml') -Raw
  }

  It 'gathers NuGet package assets without downloading symbol blobs' {
    $assignmentPattern = '(?m)^\s*' +
      [regex]::Escape('$packageAssetFilter') +
      " = '([^']+)'\s*`$"
    $filterMatch = [regex]::Match(
      $pipeline,
      $assignmentPattern)

    $filterMatch.Success | Should -BeTrue
    $packageAssetFilter = $filterMatch.Groups[1].Value
    'Microsoft.Maui.Controls.Foldable' | Should -Match $packageAssetFilter
    'assets/symbols/dotnet-maui/Microsoft.Maui.Controls.Foldable.symbols.nupkg' |
      Should -Not -Match $packageAssetFilter
  }

  It 'keeps workload gathering fail-fast and validates non-workload misses' {
    $gatherCommands = [regex]::Matches($pipeline, '(?m)^\s*& \$darc @gatherArguments.*$')

    $gatherCommands.Count | Should -Be 1
    $pipeline | Should -Match ([regex]::Escape("'--asset-filter', `$packageAssetFilter"))
    $pipeline | Should -Match '(?s)if \(!\$releaseWorkload\) \{\s*\$gatherArguments \+= ''--continue-on-error'''
    $pipeline | Should -Match '(?s)& \$darc @gatherArguments.*?if \(\$gatherFailed -and \$releaseWorkload\) \{\s*throw'
    $pipeline | Should -Match 'missing selected packages must already exist on NuGet\.org'
  }

  It 'rejects duplicate package file names before copying release artifacts' {
    $duplicateCheck = [regex]::Match(
      $pipeline,
      '(?s)\$fileNameDuplicates = @\(\$identities \| Group-Object fileName.*?' +
      'throw "[^"]*duplicate package file names.*?' +
      '\$packageSet\.Packages \| Copy-Item')

    $duplicateCheck.Success | Should -BeTrue
  }

  It 'infers workload releases from the workload repositories' {
    $pipeline | Should -Not -Match '(?m)^- name: releaseWorkload$'
    @([regex]::Matches($pipeline, "in\(parameters\.ghRepo, 'android', 'macios', 'maui'\)")).Count |
      Should -Be 1
    $pipeline | Should -Match '(?s)- name: isWorkload.*?value: \$\{\{ and'
    $pipeline | Should -Match '\$releaseWorkload = \[bool\]::Parse\(\$env:IS_WORKLOAD\)'
    $pipeline | Should -Match '--skip-assets-publishing'
    $nonWorkloadTemplate | Should -Not -Match 'skip-assets-publishing|add-build-to-channel'
  }

  It 'requires commit resolution to return one BAR build for the repository' {
    $pipeline | Should -Match '--repo \$sourceRepository --commit'
    $pipeline | Should -Match 'Expected exactly one BAR build'
    # Repository-specific channel policy was deliberately removed; identity is
    # established by repository and commit alone.
    $pipeline | Should -Not -Match 'requiredChannel(Name|Id)|build\.channels|5172'
  }

  It 'enables dotnet/skiasharp as a non-workload repository' {
    $allowed = [regex]::Match(
      $pipeline,
      '(?s)\$allowedRepositories = @\((?<entries>.*?)\r?\n\s*\)').Groups['entries'].Value
    $allowed | Should -Match 'dotnet/skiasharp'

    # skiasharp must not be inferred as a workload repository.
    $isWorkload = [regex]::Match(
      $pipeline,
      "(?s)- name: isWorkload.*?value: \\\$\{\{(?<expr>.*?)\}\}").Groups['expr'].Value
    $isWorkload | Should -Not -Match 'skiasharp'
  }

  It 'resolves a build by BAR ID when the GitHub URL is unavailable' {
    $pipeline | Should -Match '(?m)^- name: barBuildId$'
    $pipeline | Should -Match 'get-build --ci --id \$barBuildId --extended'
    $pipeline | Should -Match 'BAR_BUILD_ID: \$\{\{ parameters\.barBuildId \}\}'
    # A build found by ID was never matched against the request, so the commit
    # must still be verified.
    $pipeline | Should -Match 'not the requested'
    # Darc reports failures on stdout; losing them makes the run undiagnosable.
    $pipeline | Should -Match 'Darc failed to resolve the BAR build: \$\(\$buildJson \| Out-String\)'
  }

  It 'falls back to the AzDO mirror convention when BAR has no GitHub URL' {
    # Anchored: a substring match would still pass if the field were renamed.
    $pipeline | Should -Match '(?m)\$buildRepository = \$build\.gitHubRepository\s*$'
    $pipeline | Should -Match '\[string\]::IsNullOrWhiteSpace\(\$buildRepository\)'
    $pipeline | Should -Match "\`$separator = \`$azdoName\.IndexOf\('-'\)"
    $pipeline | Should -Match 'mirror convention'
    # The verified identity must come from the fallback, never the raw field.
    $pipeline | Should -Match '\[Uri\] \$buildRepository'
    $pipeline | Should -Not -Match '\[Uri\] \$build\.gitHubRepository'
  }

  It 'publishes a generic audit before approval and supports dry runs' {
    $pipeline | Should -Match 'artifactName: NuGetReleaseAudit'
    $nonWorkloadTemplate | Should -Match '(?s)dependsOn: prepare_release.*ManualValidation@0'
    $pipeline | Should -Match "(?s)eq\(parameters\.pushPackages, true\).*non-workload-publish\.yml"
    $nonWorkloadTemplate | Should -Match '(?s)-Action FilterExisting.*?1ES\.PublishNuget@1'
    $nonWorkloadTemplate | Should -Match "eq\(variables\['NuGetPackagesToPublish'\], 'true'\)"
    $nonWorkloadTemplate | Should -Match '1ES\.PublishNuget@1'
    $nonWorkloadTemplate | Should -Match "publishFeedCredentials: 'nuget\.org \(dotnetframework\)'"
    $pipeline | Should -Not -Match '(?m)^- name: nugetPublishServiceConnection$'
    $pipeline | Should -Not -Match '(?m)^- name: nugetAlreadyAttemptedNonWorkloadFilters$'
  }

  It 'uses the original repository text inputs and rejects workload manifests on the inferred non-workload path' {
    $pipeline | Should -Match '(?s)- name: ghRepo.*?default: maui'
    $pipeline | Should -Match '(?s)- name: ghOwner.*?default: dotnet'
    $pipeline | Should -Not -Match '(?s)- name: ghRepo.*?values:'
    $pipeline | Should -Match 'Repository.*is not enabled for this release pipeline'
    $pipeline | Should -Match 'https://github\.com/dotnet/android'
    $pipeline | Should -Match 'https://github\.com/dotnet/macios'
    $pipeline | Should -Match 'non-workload release cannot contain workload manifest'
    $pipeline | Should -Match '(?s)Name = ''NuGet packages''.*?Packages = \$selectedPackages.*?Identities = \$selectedIdentities'
    $pipeline | Should -Match '(?s)-Action FilterExisting.*?selectedPackages.*?stagedPackages'
    $pipeline | Should -Match 'belongs to ''\$repository'', not ''\$sourceRepository'''
    $pipeline | Should -Match '(?s)\$barPackageAssets = @\(\$build\.assets.*?\$selectedBarAssets = @\(\$barPackageAssets'
    $pipeline | Should -Match 'Gathered packages are absent from the selected BAR assets'
  }

  It 'supports package selection and rejects workload recovery filters for non-workload releases' {
    $pipeline | Should -Match '(?s)\$selectedPackages = @\(\$allPackages \| Where-Object \{.*?' +
      'Test-AnyFilter \$_.Name \$includeFilters.*?' +
      'Test-AnyFilter \$_.Name \$excludeFilters.*?' +
      'Package filtering selected no non-workload NuGet packages'
    $pipeline | Should -Match '(?s)else \{.*?\$unsupportedFilters = @\(@\(.*?' +
      'nugetAlreadyAttemptedPackFilters.*?nugetAlreadyAttemptedManifestFilters.*?' +
      '\) \| Where-Object \{ \$_.Values.Count -gt 0 \}\).*?' +
      'Non-workload releases do not support workload recovery filters'
    $pipeline | Should -Match 'NUGET_ALREADY_ATTEMPTED_PACK_FILTERS: \$\{\{ parameters\.nugetAlreadyAttemptedPackFilters \}\}'
    $pipeline | Should -Match 'NUGET_ALREADY_ATTEMPTED_MANIFEST_FILTERS: \$\{\{ parameters\.nugetAlreadyAttemptedManifestFilters \}\}'
  }
}

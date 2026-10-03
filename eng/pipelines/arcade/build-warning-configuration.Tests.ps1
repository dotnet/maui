#Requires -Modules Pester

Describe 'Official build warning configuration' {
  BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    $workloads = [xml](Get-Content -LiteralPath (Join-Path $repoRoot 'src/Workload/workloads.csproj') -Raw)
    $pack = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'stage-pack.yml') -Raw
    $apiScan = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'stage-api-scan.yml') -Raw
    $android = Get-Content -LiteralPath (Join-Path $repoRoot 'src/Core/AndroidNative/maui/build.gradle') -Raw
  }

  It 'uses the no-targets SDK for workload orchestration instead of compiling an empty assembly' {
    $sdkImports = @($workloads.Project.Import | Where-Object { $_.Sdk })

    $sdkImports.Count | Should -Be 2
    @($sdkImports | Where-Object { $_.Sdk -ne 'Microsoft.Build.NoTargets' }).Count | Should -Be 0
    @($sdkImports.Project) | Should -Contain 'Sdk.props'
    @($sdkImports.Project) | Should -Contain 'Sdk.targets'
  }

  It 'preserves MSI generation and the manifest hooks around signing' {
    $generateMsis = $workloads.Project.Target | Where-Object { $_.Name -eq '_GenerateAndSignMsis' }
    $generateManifests = $workloads.Project.Target | Where-Object { $_.Name -eq '_GenerateManifestsAndMsiNuGets' }

    $generateMsis.BeforeTargets | Should -Be 'SignFiles'
    $generateMsis.DependsOnTargets.Split(';') | Should -Contain '_EnsureWorkloadMsiGenProps'
    $generateManifests.AfterTargets | Should -Be 'SignFiles'
    $workloads.Project.PropertyGroup.BuildDependsOn -join '' | Should -Match '_GenerateAndSignMsis;'
  }

  It 'uses matching supported Java source and bytecode levels' {
    $android | Should -Match '(?m)^\s*sourceCompatibility = JavaVersion\.VERSION_17\s*$'
    $android | Should -Match '(?m)^\s*targetCompatibility = JavaVersion\.VERSION_17\s*$'
    $android | Should -Not -Match '-Xlint:-options'
  }

  It 'avoids deprecated Gradle property assignment syntax' {
    $android | Should -Not -Match '(?m)^\s*(compileSdk|namespace|minSdk|targetSdk)\s+[^=\s]'
  }

  It 'classifies only the APIScan analysis artifact as non-production' {
    $analysisArtifact = [regex]::Match(
      $pack,
      '(?m)^(?<indent> +)targetPath: \$\(Agent\.TempDirectory\)/APIScanFiles\r?\n' +
      '(?<metadata>(?:\k<indent>[^\r\n]+\r?\n)+)')

    $analysisArtifact.Success | Should -BeTrue
    $analysisArtifact.Groups['metadata'].Value | Should -Match 'artifactName: APIScanFiles'
    $analysisArtifact.Groups['metadata'].Value | Should -Match '(?m)^\s+isProduction: false\s*$'
    @([regex]::Matches($pack, '(?m)^\s+isProduction: false\s*$')).Count | Should -Be 1
    $pack | Should -Match '-restore -pack -sign'
    $pack | Should -Match '-restore -build -sign'
  }

  It 'keeps API analysis enabled without treating it as a production release' {
    $apiScan | Should -Match '(?s)templateContext:\s+type: releaseJob\s+(?:#[^\r\n]*\s+)?isProduction: false'
    $apiScan | Should -Match 'input: pipelineArtifact\s+artifactName: APIScanFiles'
    $apiScan | Should -Match 'task: APIScan@2'
    $apiScan | Should -Match 'task: PublishSecurityAnalysisLogs@3'
  }
}

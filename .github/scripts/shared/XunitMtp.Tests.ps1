BeforeAll {
    . (Join-Path $PSScriptRoot 'XunitMtp.ps1')
}

Describe 'Target-checkout managed test protocol' {
    It 'preserves VSTest filters for default and explicitly retained VSTest targets' -TestCases @(
        @{ Settings = '{"tools":{"dotnet":"10.0.100"}}' }
        @{ Settings = '{"test":{"runner":"VSTest"}}' }
    ) {
        param($Settings)
        $Settings | Set-Content -LiteralPath (Join-Path $TestDrive 'global.json')
        $filter = 'FullyQualifiedName~Tests.One|Category=Two'
        @(Get-XunitTestArguments -RepoRoot $TestDrive -ProjectPath 'Tests With Spaces.csproj' -Filter $filter) |
            Should -Be @('test', 'Tests With Spaces.csproj', '--logger', 'trx', '--filter', $filter)
    }

    It 'selects MTP only when the target checkout enables it' {
        '{"test":{"runner":"Microsoft.Testing.Platform"}}' |
            Set-Content -LiteralPath (Join-Path $TestDrive 'global.json')
        @(Get-XunitTestArguments -RepoRoot $TestDrive -ProjectPath 'Tests.csproj' -Filter 'FullyQualifiedName~Tests.One') |
            Should -Be @('test', '--project', 'Tests.csproj', '--report-trx', '--filter-method', '*Tests.One*')
    }

    It 'fails explicit unknown runner configurations rather than silently falling back' {
        '{"test":{"runner":"Unknown"}}' | Set-Content -LiteralPath (Join-Path $TestDrive 'global.json')
        { Get-XunitTestArguments -RepoRoot $TestDrive -ProjectPath 'Tests.csproj' } |
            Should -Throw '*Unsupported test runner*'
    }

    It 'wires the target protocol into both managed gate paths and the clean-rebuild retry' -TestCases @(
        @{ Runner = 'VSTest'; Selector = '--filter' }
        @{ Runner = 'Microsoft.Testing.Platform'; Selector = '--filter-method' }
    ) {
        param($Runner, $Selector)
        @{ test = @{ runner = $Runner } } | ConvertTo-Json -Depth 3 |
            Set-Content -LiteralPath (Join-Path $TestDrive 'global.json')
        $path = Join-Path $PSScriptRoot '../../skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($path, [ref]$null, [ref]$null)
        $assignments = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                $node.Left.Extent.Text -eq '$testArgs' -and
                $node.Right.Extent.Text.Contains('Get-XunitTestArguments')
        }, $true))
        $assignments.Count | Should -Be 3
        $RepoRoot = $TestDrive
        $projectPath = $projFull = 'Managed Tests.csproj'
        $Filter = 'FullyQualifiedName~Tests.One'
        $retryEntry = @{ Filter = $Filter }
        $hostOnlyTargetFrameworkArgs = @('-p:TargetFramework=net10.0')
        foreach ($assignment in $assignments) {
            . ([scriptblock]::Create($assignment.Extent.Text))
            $testArgs | Should -Contain $Selector
            $testArgs | Should -Contain '-p:TargetFramework=net10.0'
            $testArgs | Should -Contain '-p:TreatWarningsAsErrors=false'
            if ($Runner -eq 'VSTest') {
                $testArgs | Should -Contain $Filter
                $testArgs | Should -Not -Contain '--project'
                $testArgs | Should -Not -Contain '--report-trx'
            }
        }
    }
}

Describe 'xUnit MTP filter translation' {
    It 'preserves an empty selection as the complete suite' {
        @(Get-XunitMtpFilterArguments '').Count | Should -Be 0
    }
    It 'supports a simple name without VSTest arguments' {
        @(Get-XunitMtpFilterArguments 'Issue123') | Should -Be @('--filter-method', '*Issue123*')
    }
    It 'preserves fully qualified method alternatives' {
        @(Get-XunitMtpFilterArguments 'FullyQualifiedName~Tests.One|FullyQualifiedName=Tests.Two.Method') |
            Should -Be @('--filter-method', '*Tests.One*', '--filter-method', 'Tests.Two.Method')
    }
    It 'preserves category alternatives' {
        @(Get-XunitMtpFilterArguments 'Category=One|Category=Two') |
            Should -Be @('--filter-trait', 'Category=One', '--filter-trait', 'Category=Two')
    }
    It 'shares faithful category and method exclusion handling with Cake' {
        @(Get-XunitMtpFilterArguments '(Category=One|Category=Two)&FullyQualifiedName!~Ignored') |
            Should -Be @('--filter-trait', 'Category=One', '--filter-trait', 'Category=Two', '--filter-not-method', '*Ignored*')
    }
    It 'preserves target-framework theory row inclusion and exclusion alongside a category' {
        @(Get-XunitMtpFilterArguments 'Category=Build&DisplayName~net10.0') |
            Should -Be @('--filter-trait', 'Category=Build', '--filter-display-name', '*net10.0*')
        @(Get-XunitMtpFilterArguments 'Category=Build&DisplayName!~net10.0') |
            Should -Be @('--filter-trait', 'Category=Build', '--filter-not-display-name', '*net10.0*')
    }
    It 'accepts native xUnit queries without translating their semantics' {
        @(Get-XunitMtpFilterArguments '/assembly/namespace/class/method') |
            Should -Be @('--filter-query', '/assembly/namespace/class/method')
    }
    It 'rejects filters that cannot be translated faithfully' {
        { Get-XunitMtpFilterArguments 'Category=One|FullyQualifiedName~Two' } | Should -Throw
        { Get-XunitMtpFilterArguments 'Category=One&Category=Two' } | Should -Throw
        { Get-XunitMtpFilterArguments "Category=One`nInjected" } | Should -Throw
    }
}

Describe 'real TRX outcomes' {
    It 'counts individual outcomes and treats unknown outcomes as errors' {
        $path = Join-Path $TestDrive 'results.trx'
        @'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results>
<UnitTestResult testName="Passed" outcome="Passed" />
<UnitTestResult testName="Failed" outcome="Failed" />
<UnitTestResult testName="Skipped" outcome="NotExecuted" />
<UnitTestResult testName="Aborted" outcome="Aborted" />
</Results></TestRun>
'@ | Set-Content -LiteralPath $path
        $summary = Read-XunitMtpResults $path
        $summary.Total | Should -Be 4
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 1
        $summary.Skipped | Should -Be 1
        $summary.Errors | Should -Be 1
    }

    Describe 'integration entrypoint arguments' {
        BeforeAll {
            $runnerPath = Join-Path $PSScriptRoot '../../skills/run-integration-tests/scripts/Run-IntegrationTests.ps1'
            $source = Get-Content -Raw -LiteralPath $runnerPath
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($source, [ref]$null, [ref]$null)
            $assignment = $ast.Find({
                param($node)
                $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                    $node.Left.Extent.Text -eq '$testArgs' -and
                    $node.Operator -eq 'Equals'
            }, $true)
            $nextCommand = $ast.FindAll({
                param($node)
                $node -is [System.Management.Automation.Language.CommandAst] -and
                    $node.GetCommandName() -eq 'Write-Host' -and
                    $node.Extent.StartOffset -gt $assignment.Extent.EndOffset
            }, $true) | Select-Object -First 1
            $script:BuildIntegrationArguments = [scriptblock]::Create(
                $source.Substring($assignment.Extent.StartOffset,
                    $nextCommand.Extent.StartOffset - $assignment.Extent.StartOffset))
            $testProjectPath = 'Integration Tests.csproj'
            $Configuration = 'Release'
            $resultsPath = Join-Path $TestDrive 'results'
            $TestClass = @()
            $Category = ''
            $ArtifactsDirectory = ''
        }

        It 'translates legacy fully qualified selectors at the integration entrypoint' -TestCases @(
            @{ Filter = 'FullyQualifiedName~BuildSample'; Expected = @('--filter-method', '*BuildSample*') }
            @{ Filter = 'FullyQualifiedName=Tests.BuildSample'; Expected = @('--filter-method', 'Tests.BuildSample') }
            @{ Filter = '/assembly/namespace/class/method'; Expected = @('--filter-query', '/assembly/namespace/class/method') }
        ) {
            param($Filter, $Expected)
            $TestFilter = $Filter
            . $script:BuildIntegrationArguments
            $testArgs[-2..-1] | Should -Be $Expected
        }

        It 'round trips artifact path separators through the real MSBuild property parser' {
            $TestFilter = ''
            $Category = 'Build'
            $ArtifactsDirectory = Join-Path $TestDrive 'validation,run;literal%2C'
            . $script:BuildIntegrationArguments
            $property = @($testArgs | Where-Object { $_ -like '-p:ArtifactsDir=*' })
            $property.Count | Should -Be 1
            $project = Join-Path $TestDrive 'Property.proj'
            '<Project />' | Set-Content -LiteralPath $project
            $actual = & dotnet msbuild $project -nologo @property -getProperty:ArtifactsDir
            $LASTEXITCODE | Should -Be 0
            "$actual".Trim() | Should -Be ([IO.Path]::GetFullPath($ArtifactsDirectory))
        }
    }
    It 'does not accept an empty or missing report as success' {
        $path = Join-Path $TestDrive 'empty.trx'
        '<TestRun><Results /></TestRun>' | Set-Content -LiteralPath $path
        { Read-XunitMtpResults $path } | Should -Throw
        { Read-XunitMtpResults (Join-Path $TestDrive 'missing.trx') } | Should -Throw
    }
    It 'rejects XML document type declarations' {
        $path = Join-Path $TestDrive 'unsafe.trx'
        '<!DOCTYPE TestRun [<!ENTITY x "data">]><TestRun><Results /></TestRun>' | Set-Content -LiteralPath $path
        { Read-XunitMtpResults $path } | Should -Throw
    }
}

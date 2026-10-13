#Requires -Modules Pester

BeforeAll {
    $repository = (Get-Item $PSScriptRoot).Parent.Parent.Parent.Parent.FullName
    $runner = Join-Path $PSScriptRoot 'Run-IntegrationTests.ps1'
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($runner, [ref]$null, [ref]$null)
    $buildStep = [scriptblock]::Create($ast.Find({
        param($node)
        $node -is [System.Management.Automation.Language.IfStatementAst] -and
        $node.Clauses[0].Item1.Extent.Text -eq '-not $SkipBuild'
    }, $true).Extent.Text)
}

Describe 'Integration runner build bootstrap' {
    BeforeEach {
        $RepoRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $RepoRoot 'eng\common') -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $repository 'build.cmd') -Destination $RepoRoot
        Copy-Item -LiteralPath (Join-Path $repository 'eng\build.ps1') -Destination (Join-Path $RepoRoot 'eng')
        $commonBuild = Join-Path $RepoRoot 'eng\common\build.ps1'
        $capture = Join-Path $RepoRoot 'bootstrap.json'
        $RunningOnWindows = $true
        $SkipBuild = $false
        $Configuration = 'Debug'
        $savedWarningsAsErrors = $env:TreatWarningsAsErrors
        $env:TreatWarningsAsErrors = $null
        @'
param(
    [switch]$restore,
    [switch]$pack,
    [string]$configuration,
    [bool]$warnAsError = $true
)
[pscustomobject]@{
    Restore = [bool]$restore
    Pack = [bool]$pack
    Configuration = $configuration
    WarningsAsErrors = $warnAsError
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $PSScriptRoot '..\..\bootstrap.json')
exit 0
'@ | Set-Content -LiteralPath $commonBuild
    }

    AfterEach {
        $env:TreatWarningsAsErrors = $savedWarningsAsErrors
    }

    It 'passes Boolean false through the native Windows bootstrap for <BuildConfiguration>' -Skip:(-not $IsWindows) -ForEach @(
        @{ BuildConfiguration = 'Debug' }
        @{ BuildConfiguration = 'Release' }
    ) {
        $Configuration = $BuildConfiguration
        & $buildStep
        Test-Path -LiteralPath $capture | Should -BeTrue
        $arguments = Get-Content -LiteralPath $capture -Raw | ConvertFrom-Json
        $arguments.WarningsAsErrors | Should -BeFalse
        $arguments.Configuration | Should -Be $Configuration
        $arguments.Restore | Should -BeTrue
        $arguments.Pack | Should -BeTrue
    }

    It 'propagates native bootstrap failures instead of proceeding to workload installation' -Skip:(-not $IsWindows) {
        (Get-Content -LiteralPath $commonBuild -Raw).Replace('exit 0', 'exit 19') |
            Set-Content -LiteralPath $commonBuild
        # Windows PowerShell -Command normalizes script failures to exit code 1.
        { & $buildStep } | Should -Throw '*Build and pack failed with exit code 1*'
    }

    It 'does not run a native bootstrap when SkipBuild is selected' -Skip:(-not $IsWindows) {
        $SkipBuild = $true
        & $buildStep
        Test-Path -LiteralPath $capture | Should -BeFalse
    }

    It 'preserves the POSIX false argument' {
        $platformBranch = $ast.Find({
            param($node)
            $node -is [System.Management.Automation.Language.IfStatementAst] -and
            $node.Clauses[0].Item1.Extent.Text -eq '$RunningOnWindows' -and
            $node.Extent.Text -match '\$buildArgs'
        }, $true)
        $assignment = $platformBranch.ElseClause.Find({
            param($node)
            $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $node.Left.Extent.Text -eq '$buildArgs'
        }, $true)
        $arguments = & ([scriptblock]::Create($assignment.Extent.Text + '; $buildArgs'))
        $arguments[-2..-1] | Should -Be @('-warnAsError', 'false')
    }
}

#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for Get-TestResultFromOutput build-error classification in
    verify-tests-fail.ps1.

    A test whose log shows a COMPILE error (any `error <ABBR><NNNN>` — MAUIX, CS, MSB,
    NETSDK, XA, NU, …) never produced a runnable test result, so the gate must classify it
    as a build error (-> INCONCLUSIVE), not as a failing test (-> FAILED). This guards the
    fix for the net11 Controls.Xaml.UnitTests MAUIX2017 baseline break, where an unrelated
    fixture (Bz40906.xaml) fails to compile and takes the whole assembly down.
.EXAMPLE
    Invoke-Pester ./Verify-TestsFail.Tests.ps1
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'verify-tests-fail.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $function = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Get-TestResultFromOutput'
    }, $true)
    if (-not $function) { throw "Function 'Get-TestResultFromOutput' not found" }
    Invoke-Expression $function.Extent.Text

    $autoDetectionFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Get-AutoDetectedTests'
    }, $true)
    if (-not $autoDetectionFunction) { throw "Function 'Get-AutoDetectedTests' not found" }
    Invoke-Expression $autoDetectionFunction.Extent.Text

    $invokeTestRunFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Invoke-TestRun'
    }, $true)
    if (-not $invokeTestRunFunction) { throw "Function 'Invoke-TestRun' not found" }
    $script:invokeTestRunText = $invokeTestRunFunction.Extent.Text

    $script:UnitTestProjectMap = @{
        "Controls.Core.UnitTests" = "src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj"
    }
    $script:DeviceTestProjectMap = @{
        "Controls.DeviceTests" = "Controls"
        "Core.DeviceTests" = "Core"
    }

    foreach ($helperName in @('Resolve-ExplicitTestProject', 'Set-WithoutFixFileState', 'Set-WithFixFileState')) {
        $helper = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $helperName
        }, $true)
        if (-not $helper) { throw "Function '$helperName' not found" }
        Invoke-Expression $helper.Extent.Text
    }

    function Write-Log { param([string]$Message) }

    function New-LogFile {
        param([string]$Content)
        $f = Join-Path ([System.IO.Path]::GetTempPath()) ("verifylog-" + [Guid]::NewGuid().ToString('N') + ".log")
        $Content | Set-Content -LiteralPath $f -Encoding UTF8
        return $f
    }

}

Describe 'Invoke-TestRun — host-only target frameworks' {
    It 'applies the shared platform exclusions to unit and XAML unit tests' {
        ([regex]::Matches($script:invokeTestRunText, '\+\s*\$hostOnlyTargetFrameworkArgs')).Count | Should -Be 2
        foreach ($property in @(
            'IncludeAndroidTargetFrameworks',
            'IncludeIosTargetFrameworks',
            'IncludeMacCatalystTargetFrameworks',
            'IncludeWindowsTargetFrameworks',
            'IncludeTizenTargetFrameworks'
        )) {
            $script:invokeTestRunText | Should -Match "-p:$property=false"
        }
    }
}

Describe 'Get-TestResultFromOutput — build error classification' {
    It 'flags the net11 Xaml.UnitTests MAUIX2017 baseline break as a build error (not a test failure)' {
        $log = New-LogFile @"
  Controls.Xaml -> /a/b/Microsoft.Maui.Controls.Xaml.dll
/s/src/Controls/tests/Xaml.UnitTests/Issues/Bz40906.xaml(6,4): error MAUIX2017: Property 'ContentPage.Content' is being set multiple times. Only the last value will be used. [/s/src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj]
Build FAILED.
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.BuildError | Should -BeTrue
        $r.Passed | Should -BeFalse
        $r.Error | Should -Match 'MAUIX2017'
        Remove-Item -LiteralPath $log -Force
    }

    It 'flags CS / MSB / NETSDK / XA compile errors as build errors' {
        foreach ($err in @(
            "error CS0234: The type or namespace name 'CodeAnalysis' does not exist",
            "error MSB3073: The command exited with code 1",
            "error NETSDK1005: Assets file doesn't have a target for 'net11.0-android'",
            "error XA4210: missing UsesLibrary"
        )) {
            $log = New-LogFile "Build FAILED.`n$err"
            (Get-TestResultFromOutput -LogFile $log).BuildError | Should -BeTrue
            Remove-Item -LiteralPath $log -Force
        }
    }

    It 'does NOT flag a clean passing run as a build error' {
        $log = New-LogFile "Build succeeded.`n    0 Error(s)`n  Passed:  57`n  Failed:  0"
        $r = Get-TestResultFromOutput -LogFile $log
        $r.BuildError | Should -Not -BeTrue
        $r.Passed | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'does NOT flag a warning-only build as a build error' {
        $log = New-LogFile @"
Gh2517.xaml(6,13): warning MAUIG2045: Binding: Property "MissingProperty" not found
Build succeeded.
    0 Error(s)
  Passed:  10
  Failed:  0
"@
        (Get-TestResultFromOutput -LogFile $log).BuildError | Should -Not -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'still reports a genuine ran-and-failed test as a failure, not a build error' {
        $log = New-LogFile "Build succeeded.`n    0 Error(s)`n  Passed:  3`n  Failed:  2"
        $r = Get-TestResultFromOutput -LogFile $log
        $r.BuildError | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Get-AutoDetectedTests — frozen worktree isolation' {
    It 'prefers the immutable explicit-base local diff over PR metadata' {
        $repo = Join-Path ([System.IO.Path]::GetTempPath()) ("verifyrepo-" + [Guid]::NewGuid().ToString('N'))
        $detector = Join-Path $repo 'detect.ps1'
        try {
            New-Item -ItemType Directory -Path $repo | Out-Null
            git -C $repo init --quiet
            'base' | Set-Content -LiteralPath (Join-Path $repo 'README.md')
            git -C $repo add README.md
            git -C $repo -c user.name='Vally Test' -c user.email='vally-test@example.invalid' commit --quiet -m base
            $base = git -C $repo rev-parse HEAD

            $testPath = 'src/Controls/tests/Core.UnitTests/VallyFixtureTests.cs'
            New-Item -ItemType Directory -Path (Split-Path (Join-Path $repo $testPath)) -Force | Out-Null
            'fixture' | Set-Content -LiteralPath (Join-Path $repo $testPath)
            git -C $repo add $testPath
            git -C $repo -c user.name='Vally Test' -c user.email='vally-test@example.invalid' commit --quiet -m fixture

            @(
                'param([string]$PRNumber, [string[]]$ChangedFiles)'
                '[pscustomobject]@{'
                '    PRNumber = $PRNumber'
                '    ChangedFiles = @($ChangedFiles)'
                '}'
            ) | Set-Content -LiteralPath $detector

            $script:PRNumber = '33134'
            $script:ExplicitBaseBranch = $base
            $script:DetectTestsScript = $detector
            Push-Location $repo
            try {
                $result = Get-AutoDetectedTests -MergeBase $base
            } finally {
                Pop-Location
            }

            $result.PRNumber | Should -BeNullOrEmpty
            @($result.ChangedFiles) | Should -Contain $testPath
        } finally {
            Remove-Item -LiteralPath $repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'Full verification file-state transitions' {
    It 'removes and restores a fix composed entirely of new committed files' {
        $repo = Join-Path ([System.IO.Path]::GetTempPath()) ("verifyrepo-" + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $repo | Out-Null

        try {
            Push-Location $repo
            git init --quiet
            git config user.email "verify-tests@example.invalid"
            git config user.name "Verify Tests"
            "baseline" | Set-Content baseline.txt
            git add -- baseline.txt
            git commit --quiet -m "baseline"
            $mergeBase = (git rev-parse HEAD).Trim()

            "new product implementation" | Set-Content new-fix.cs
            git add -- new-fix.cs
            git commit --quiet -m "add fix"

            Set-WithoutFixFileState -RepoRoot $repo -MergeBase $mergeBase -NewFiles @('new-fix.cs')
            Test-Path (Join-Path $repo 'new-fix.cs') | Should -BeFalse

            Set-WithFixFileState -RepoRoot $repo -NewFiles @('new-fix.cs')
            Test-Path (Join-Path $repo 'new-fix.cs') | Should -BeTrue
            Get-Content (Join-Path $repo 'new-fix.cs') | Should -Be 'new product implementation'
            git status --porcelain | Should -BeNullOrEmpty
        } finally {
            Pop-Location
            Remove-Item -LiteralPath $repo -Recurse -Force
        }
    }
}

Describe 'Get-AutoDetectedTests — ordinary PR metadata' {
    It 'uses PR metadata when the explicit base is a branch name' {
        $detector = Join-Path ([System.IO.Path]::GetTempPath()) ("detect-" + [Guid]::NewGuid().ToString('N') + ".ps1")
        try {
            @(
                'param([string]$PRNumber, [string[]]$ChangedFiles)'
                '[pscustomobject]@{'
                '    PRNumber = $PRNumber'
                '    ChangedFiles = @($ChangedFiles)'
                '}'
            ) | Set-Content -LiteralPath $detector

            $script:PRNumber = '33134'
            $script:ExplicitBaseBranch = 'main'
            $script:DetectTestsScript = $detector

            $result = Get-AutoDetectedTests -MergeBase ('a' * 40)

            $result.PRNumber | Should -Be '33134'
            @($result.ChangedFiles) | Should -BeNullOrEmpty
        } finally {
            Remove-Item -LiteralPath $detector -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'Explicit test project resolution' {
    It 'resolves a known unit test project key' {
        $result = Resolve-ExplicitTestProject -TestType UnitTest `
            -TestProject Controls.Core.UnitTests -RepoRoot $TestDrive
        $result.Project | Should -Be 'Controls.Core.UnitTests'
        $result.ProjectPath | Should -Be 'src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj'
    }

    It 'resolves a repo-relative unit test project path' {
        $projectPath = 'tests/Custom.UnitTests.csproj'
        $fullPath = Join-Path $TestDrive $projectPath
        New-Item -ItemType Directory -Path (Split-Path $fullPath) -Force | Out-Null
        '<Project />' | Set-Content $fullPath

        $result = Resolve-ExplicitTestProject -TestType UnitTest `
            -TestProject $projectPath -RepoRoot $TestDrive
        $result.Project | Should -Be 'Custom.UnitTests'
        $result.ProjectPath | Should -Be $projectPath
    }

    It 'requires an explicit project for unit and device tests' {
        { Resolve-ExplicitTestProject -TestType UnitTest -RepoRoot $TestDrive } |
            Should -Throw '*requires -TestProject*'
        { Resolve-ExplicitTestProject -TestType DeviceTest -RepoRoot $TestDrive } |
            Should -Throw '*requires -TestProject*'
    }

    It 'resolves a non-Controls device test project without defaulting' {
        $result = Resolve-ExplicitTestProject -TestType DeviceTest `
            -TestProject Core -RepoRoot $TestDrive
        $result.Project | Should -Be 'Core'
        $result.ProjectPath | Should -BeNullOrEmpty
    }
}

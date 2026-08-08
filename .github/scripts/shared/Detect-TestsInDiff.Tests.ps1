#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Detect-TestsInDiff.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $platformFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Test-DeviceTestFileAppliesToPlatform'
    }, $true)
    if (-not $platformFunction) {
        throw "Function 'Test-DeviceTestFileAppliesToPlatform' not found"
    }
    Invoke-Expression $platformFunction.Extent.Text
}

Describe 'Detect-TestsInDiff device-test filtering' {
    It 'sets class and category filters when a device-test diff has no added method signatures' {
        $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')
        $scriptPath = Join-Path $PSScriptRoot 'Detect-TestsInDiff.ps1'
        $changedFile = 'src/Core/tests/DeviceTests/Handlers/Entry/EntryHandlerTests.cs'

        Push-Location $repoRoot
        try {
            $tests = @(& $scriptPath -ChangedFiles $changedFile)
        } finally {
            Pop-Location
        }

        $test = $tests | Where-Object { $_.Type -eq 'DeviceTest' } | Select-Object -First 1
        $test.Filter | Should -Be 'Category=Entry'
        $test.ClassFilter | Should -Be 'Microsoft.Maui.DeviceTests.EntryHandlerTests'
        $test.Methods | Should -BeNullOrEmpty
    }
}

Describe 'Detect-TestsInDiff platform-specific methods' {
    It 'includes iOS partials for iOS and Mac Catalyst only' {
        $path = 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.iOS.cs'
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform ios | Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform catalyst | Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform android | Should -BeFalse
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform windows | Should -BeFalse
    }

    It 'keeps Mac Catalyst partials exclusive to Mac Catalyst' {
        $path = 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.MacCatalyst.cs'
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform maccatalyst | Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform ios | Should -BeFalse
    }

    It 'matches Android and Windows partial files only on their target' {
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.Android.cs' `
            -TargetPlatform android |
            Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.Android.cs' `
            -TargetPlatform windows |
            Should -BeFalse
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.Windows.cs' `
            -TargetPlatform windows |
            Should -BeTrue
    }

    It 'keeps shared files and applies platform directory conventions' {
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.cs' `
            -TargetPlatform windows |
            Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Platforms/Android/FooTests.cs' `
            -TargetPlatform windows |
            Should -BeFalse
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Platforms/iOS/FooTests.cs' `
            -TargetPlatform maccatalyst |
            Should -BeTrue
    }
}
Describe 'Detect-TestsInDiff PR files cache' {
    It 'gates the fetch on a fetch-attempted sentinel, not on the (possibly empty) cache' {
        $scriptContent = Get-Content (Join-Path $PSScriptRoot 'Detect-TestsInDiff.ps1') -Raw

        # `-not @()` is $true, so guarding on the cache alone re-runs `gh api` for every
        # device-test group after a failed or empty fetch.
        $scriptContent | Should -Match '\$PRNumber -and -not \$script:_prFilesFetchAttempted'
        $scriptContent | Should -Not -Match '\$PRNumber -and -not \$script:_cachedPRFiles'
    }
}

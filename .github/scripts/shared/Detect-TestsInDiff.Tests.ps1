#Requires -Modules Pester

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

#Requires -Modules Pester

Describe 'Windows device-test result merging' {
    BeforeAll {
        $scriptPath = Join-Path $PSScriptRoot 'Merge-DeviceTestResults.ps1'
        $powershellPath = (Get-Process -Id $PID).Path
        $passingXml = '<assemblies><assembly name="Passing" total="1" passed="1" failed="0" skipped="0"><collection><test name="Passing" result="Pass" /></collection></assembly></assemblies>'
    }

    BeforeEach {
        $resultsDirectory = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        New-Item -Path $resultsDirectory -ItemType Directory | Out-Null
        Set-Content -LiteralPath (Join-Path $resultsDirectory 'TestResults-Passing.xml') -Value $passingXml
    }

    It 'merges passing assemblies without counting a previous merged file twice' {
        Set-Content -LiteralPath (Join-Path $resultsDirectory 'testResults.xml') -Value $passingXml
        $output = & $powershellPath -NoProfile -File $scriptPath -ResultsDirectory $resultsDirectory 2>&1
        $LASTEXITCODE | Should -Be 0

        [xml] $merged = Get-Content -LiteralPath (Join-Path $resultsDirectory 'testResults.xml') -Raw
        $merged.SelectNodes('/assemblies/assembly').Count | Should -Be 1
        $merged.assemblies.assembly.passed | Should -Be '1'
    }

    It 'fails rather than hiding <Name> while preserving valid results' -TestCases @(
        @{ Name = 'an empty category file'; Xml = '' }
        @{ Name = 'truncated XML'; Xml = '<assemblies><assembly' }
        @{ Name = 'a missing assembly'; Xml = '<assemblies />' }
        @{ Name = 'a missing count'; Xml = '<assemblies><assembly total="1" passed="1" skipped="0" /></assemblies>' }
        @{ Name = 'inconsistent counts'; Xml = '<assemblies><assembly total="2" passed="1" failed="0" skipped="0" /></assemblies>' }
        @{ Name = 'missing test cases'; Xml = '<assemblies><assembly total="1" passed="1" failed="0" skipped="0" /></assemblies>' }
        @{ Name = 'incorrect outcome counts'; Xml = '<assemblies><assembly total="1" passed="1" failed="0" skipped="0"><collection><test result="Skip" /></collection></assembly></assemblies>' }
        @{ Name = 'a negative error count'; Xml = '<assemblies><assembly total="0" passed="0" failed="0" skipped="0" errors="-1" /></assemblies>' }
        @{ Name = 'a DTD'; Xml = '<!DOCTYPE assemblies [<!ENTITY example "value">]><assemblies />' }
    ) {
        param($Name, $Xml)
        Set-Content -LiteralPath (Join-Path $resultsDirectory 'TestResults-Broken.xml') -Value $Xml

        $output = & $powershellPath -NoProfile -File $scriptPath -ResultsDirectory $resultsDirectory 2>&1
        $LASTEXITCODE | Should -Be 1
        ($output -join "`n") | Should -Match 'ERROR: Failed to parse TestResults-Broken.xml'

        [xml] $merged = Get-Content -LiteralPath (Join-Path $resultsDirectory 'testResults.xml') -Raw
        $merged.SelectNodes('/assemblies/assembly').Count | Should -Be 1
        $merged.assemblies.assembly.name | Should -Be 'Passing'
    }

    It 'fails for <Name>' -TestCases @(
        @{ Name = 'test failures'; Xml = '<assembly total="1" passed="0" failed="1" skipped="0"><collection><test result="Fail" /></collection></assembly>' }
        @{ Name = 'assembly error counts'; Xml = '<assembly total="0" passed="0" failed="0" skipped="0" errors="1" />' }
        @{ Name = 'assembly error nodes'; Xml = '<assembly total="0" passed="0" failed="0" skipped="0"><errors><error /></errors></assembly>' }
    ) {
        param($Name, $Xml)
        Set-Content -LiteralPath (Join-Path $resultsDirectory 'TestResults-Failing.xml') -Value "<assemblies>$Xml</assemblies>"

        $output = & $powershellPath -NoProfile -File $scriptPath -ResultsDirectory $resultsDirectory 2>&1
        $LASTEXITCODE | Should -Be 1
        ($output -join "`n") | Should -Match 'ERROR: Test failures or assembly errors'
    }

    It 'fails when no category results exist' {
        Remove-Item -LiteralPath (Join-Path $resultsDirectory 'TestResults-Passing.xml')
        $output = & $powershellPath -NoProfile -File $scriptPath -ResultsDirectory $resultsDirectory 2>&1
        $LASTEXITCODE | Should -Be 1
        ($output -join "`n") | Should -Match 'No device-test result files found'
    }

    It 'accepts an empty discovered category and existing skipped tests' {
        Set-Content -LiteralPath (Join-Path $resultsDirectory 'TestResults-Empty.xml') -Value '<assemblies><assembly total="0" passed="0" failed="0" skipped="0" /></assemblies>'
        Set-Content -LiteralPath (Join-Path $resultsDirectory 'TestResults-Skipped.xml') -Value '<assemblies><assembly total="1" passed="0" failed="0" skipped="1"><collection><test result="Skip" /></collection></assembly></assemblies>'

        $output = & $powershellPath -NoProfile -File $scriptPath -ResultsDirectory $resultsDirectory 2>&1
        $LASTEXITCODE | Should -Be 0
        [xml] $merged = Get-Content -LiteralPath (Join-Path $resultsDirectory 'testResults.xml') -Raw
        $merged.SelectNodes('/assemblies/assembly').Count | Should -Be 3
    }
}

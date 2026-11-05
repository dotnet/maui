BeforeAll {
    . (Join-Path $PSScriptRoot 'XunitMtp.ps1')
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

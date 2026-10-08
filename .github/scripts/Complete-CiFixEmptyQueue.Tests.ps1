#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe 'Complete-CiFixEmptyQueue' {
    BeforeEach {
        $script:scriptPath = Join-Path $PSScriptRoot 'Complete-CiFixEmptyQueue.ps1'
        $caseRoot = Join-Path $TestDrive ([Guid]::NewGuid().ToString('N'))
        $script:candidatesPath = Join-Path $caseRoot 'prefetch.json'
        $script:outputDirectory = Join-Path $caseRoot 'agent'
        $script:expectationDirectory = Join-Path $caseRoot 'expectations'
        New-Item -ItemType Directory -Force -Path $caseRoot | Out-Null
    }

    It 'persists an authoritative empty queue and registers one noop' {
        @{
            schemaVersion = 2
            repository = 'dotnet/maui'
            issueEvidence = @{
                authoritative = $true
                exactLabel = 'ci-scan'
                scopedIssueNumber = $null
                truncated = $false
                count = 0
                totalMatched = 0
                issues = @()
            }
            candidates = @()
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $script:candidatesPath

        $result = & $script:scriptPath `
            -CandidatesPath $script:candidatesPath `
            -ExpectedIssueLabel ci-scan `
            -OutputDirectory $script:outputDirectory `
            -ExpectationDirectory $script:expectationDirectory

        $result.emptyQueue | Should -BeTrue
        (Get-Item -LiteralPath (Join-Path $script:outputDirectory 'coverage.txt')).Length |
            Should -Be 0
        (Get-Content -Raw -LiteralPath (Join-Path $script:outputDirectory 'summary.md')) |
            Should -BeExactly "| issue | branch | attempt | outcome | reason |`n|---|---|---|---|---|`n"
        $expectations = @(Get-ChildItem -LiteralPath $script:expectationDirectory -Filter '*.json')
        $expectations.Count | Should -Be 1
        $expectation = Get-Content -Raw -LiteralPath $expectations[0].FullName | ConvertFrom-Json
        $expectation.type | Should -BeExactly 'noop'
        $expectation.pullRequestNumber | Should -BeNullOrEmpty
    }

    It 'rejects non-empty or incomplete queue evidence without registering noop' -ForEach @(
        @{
            Name = 'non-empty issues'
            IssueEvidence = @{
                authoritative = $true; exactLabel = 'ci-scan'; scopedIssueNumber = $null
                truncated = $false; count = 1; totalMatched = 1; issues = @(@{ issueNumber = 1 })
            }
            Candidates = @()
        }
        @{
            Name = 'truncated evidence'
            IssueEvidence = @{
                authoritative = $true; exactLabel = 'ci-scan'; scopedIssueNumber = $null
                truncated = $true; count = 0; totalMatched = 0; issues = @()
            }
            Candidates = @()
        }
        @{
            Name = 'watch candidate'
            IssueEvidence = @{
                authoritative = $true; exactLabel = 'ci-scan'; scopedIssueNumber = $null
                truncated = $false; count = 0; totalMatched = 0; issues = @()
            }
            Candidates = @(@{ prNumber = 42 })
        }
        @{
            Name = 'missing candidates inventory'
            IssueEvidence = @{
                authoritative = $true; exactLabel = 'ci-scan'; scopedIssueNumber = $null
                truncated = $false; count = 0; totalMatched = 0; issues = @()
            }
            Candidates = $null
            OmitCandidates = $true
        }
        @{
            Name = 'null issues inventory'
            IssueEvidence = @{
                authoritative = $true; exactLabel = 'ci-scan'; scopedIssueNumber = $null
                truncated = $false; count = 0; totalMatched = 0; issues = $null
            }
            Candidates = @()
        }
        @{
            Name = 'null candidates inventory'
            IssueEvidence = @{
                authoritative = $true; exactLabel = 'ci-scan'; scopedIssueNumber = $null
                truncated = $false; count = 0; totalMatched = 0; issues = @()
            }
            Candidates = $null
        }
    ) {
        $snapshot = @{
            schemaVersion = 2
            repository = 'dotnet/maui'
            issueEvidence = $IssueEvidence
        }
        if (-not $OmitCandidates) {
            $snapshot.candidates = $Candidates
        }
        $snapshot | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $script:candidatesPath

        {
            & $script:scriptPath `
                -CandidatesPath $script:candidatesPath `
                -ExpectedIssueLabel ci-scan `
                -OutputDirectory $script:outputDirectory `
                -ExpectationDirectory $script:expectationDirectory
        } | Should -Throw

        Test-Path -LiteralPath $script:expectationDirectory | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $script:outputDirectory 'coverage.txt') | Should -BeFalse
    }
}

Describe 'CI-fixer unattended bootstrap contracts' {
    It 'uses trusted empty-queue completion and bounded detection ripgrep preparation in both twins' -ForEach @(
        @{ Workflow = 'ci-status-fix'; Label = 'ci-scan' }
        @{ Workflow = 'ci-status-fix-net11'; Label = 'ci-scan-net11' }
    ) {
        $workflowRoot = Join-Path (Split-Path $PSScriptRoot) 'workflows'
        $source = Get-Content -Raw -LiteralPath (Join-Path $workflowRoot "$Workflow.md")
        $lock = Get-Content -Raw -LiteralPath (Join-Path $workflowRoot "$Workflow.lock.yml")

        $source | Should -Match '(?ms)^  detection:\r?\n    pre-steps:\r?\n      - name: Prepare bounded ripgrep dependency'
        $source | Should -Match "timeout --signal=TERM --kill-after=15s 270s bash <<'RIPGREP_PREP'"
        $source | Should -Match 'DEBIAN_FRONTEND=noninteractive'
        $source | Should -Match 'Acquire::https::Timeout=30'
        $source | Should -Match 'DPkg::Lock::Timeout=60'
        $source | Should -Match 'rg --version'

        $prepareIndex = $lock.IndexOf('- name: Prepare bounded ripgrep dependency')
        $installerIndex = $lock.IndexOf('- name: Install ripgrep', $prepareIndex + 1)
        $prepareIndex | Should -BeGreaterThan -1
        $installerIndex | Should -BeGreaterThan $prepareIndex
        $lock.Substring($prepareIndex, $installerIndex - $prepareIndex) |
            Should -Match "timeout --signal=TERM --kill-after=15s 270s bash <<'RIPGREP_PREP'"

        $source | Should -Match ([regex]::Escape(
                ".github/scripts/Complete-CiFixEmptyQueue.ps1 ``"))
        $source | Should -Match "-ExpectedIssueLabel $Label"
        $source | Should -Match 'call the `safeoutputs` `noop` tool exactly\s+once'
        $source | Should -Not -Match '(?ms)empty queue.*?touch /tmp/gh-aw/agent'
        $lock | Should -Match ([regex]::Escape(
                "{{#runtime-import .github/workflows/$Workflow.md}}"))
    }
}

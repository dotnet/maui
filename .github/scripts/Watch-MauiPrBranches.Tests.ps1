#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Watch-MauiPrBranches.ps1')
    $script:ConfiguredBranches = @($script:BranchCiBranches)

    function New-TestBuild {
        param([long]$Id = 21, [string]$Result = 'failed', [string]$Branch = 'main')
        return @{
            id = $Id
            definition = @{ id = 302; name = 'maui-pr' }
            repository = @{ id = 'dotnet/maui'; type = 'GitHub' }
            sourceBranch = "refs/heads/$Branch"
            sourceVersion = 'a' * 40
            status = 'completed'
            result = $Result
            reason = 'individualCI'
        }
    }

    function New-TestIssue {
        param([long]$Id = 20, [string]$Branch = 'main', [string]$State = 'open', [int]$Number = 700)
        return @{
            number = $Number
            state = $State
            user = @{ login = 'github-actions[bot]' }
            labels = @(@{ name = 'ci-branch-health' })
            body = Format-BranchCiObservation -Branch $Branch -Build (New-TestBuild -Id $Id -Branch $Branch) -NewIssue
        }
    }

    function New-TestComment {
        param([long]$Id = 21, [string]$Result = 'failed', [string]$Author = 'github-actions[bot]')
        return @{
            id = $Id
            user = @{ login = $Author }
            body = Format-BranchCiObservation -Branch main -Build (New-TestBuild -Id $Id -Result $Result)
        }
    }

    function gh { throw 'Live GitHub access is forbidden in tests.' }
}

Describe 'Branch monitor transport' {
    BeforeEach {
        Mock Invoke-WebRequest { throw 'Live AzDO access is forbidden in tests.' }
    }

    It 'flattens every page, including singleton and empty pages' {
        Mock gh { $global:LASTEXITCODE = 0; '[[{"number":1}],[],[{"number":2}]]' }
        $items = @(Invoke-BranchCiGitHub -Endpoint 'repos/dotnet/maui/issues' -Paginate)
        $items.number | Should -Be @(1, 2)
        Should -Invoke gh -Times 1 -ParameterFilter { $args -contains '--paginate' -and $args -contains '--slurp' }
    }

    It 'preserves an empty listing' {
        Mock gh { $global:LASTEXITCODE = 0; '[[]]' }
        @(Invoke-BranchCiGitHub -Endpoint 'repos/dotnet/maui/issues' -Paginate).Count | Should -Be 0
    }

    It 'rejects a null response rather than treating a branch as absent' {
        Mock gh { $global:LASTEXITCODE = 0; 'null' }
        { Invoke-BranchCiGitHub -Endpoint 'repos/dotnet/maui/git/matching-refs/heads/main' } | Should -Throw '*Empty GitHub response*'
    }

    It 'rejects failed GitHub calls even if partial JSON was returned' {
        Mock gh { $global:LASTEXITCODE = 1; '[[]]' }
        { Invoke-BranchCiGitHub -Endpoint 'repos/dotnet/maui/issues' -Paginate } | Should -Throw '*GitHub GET*failed*'
    }

    It 'rejects a malformed pagination response: <Json>' -ForEach @(
        @{ Json = 'null' }, @{ Json = '{"message":"error"}' }, @{ Json = '[{"number":1}]' }
    ) {
        Mock gh { $global:LASTEXITCODE = 0; $Json }
        { Invoke-BranchCiGitHub -Endpoint 'repos/dotnet/maui/issues' -Paginate } | Should -Throw
    }
}

Describe 'AzDO build selection' {
    BeforeEach {
        $script:ApiBuild = New-TestBuild
        Mock Invoke-WebRequest { @{ Content = (@{ value = @($script:ApiBuild) } | ConvertTo-Json -Depth 10) } }
    }

    It 'requests only completed conclusive branch builds, newest queued first' {
        $script:ApiBuild.sourceBranch = 'refs/heads/inflight/current'
        (Get-BranchCiLatestBuild 'inflight/current').id | Should -Be 21
        Should -Invoke Invoke-WebRequest -Times 1 -ParameterFilter {
            $Uri -match 'definitions=302&repositoryId=dotnet%2Fmaui&repositoryType=GitHub' -and
            $Uri -match 'branchName=refs%2Fheads%2Finflight%2Fcurrent' -and
            $Uri -match 'statusFilter=completed&resultFilter=succeeded,failed,partiallySucceeded' -and
            $Uri -match 'queryOrder=queueTimeDescending&\$top=1' -and
            $TimeoutSec -eq 60
        }
    }

    It 'accepts a <Result> observation' -ForEach @(
        @{ Result = 'failed' }, @{ Result = 'partiallySucceeded' }, @{ Result = 'succeeded' }
    ) {
        $script:ApiBuild.result = $Result
        (Get-BranchCiLatestBuild main).result | Should -Be $Result
    }

    It 'rejects an unexpected <Field>' -ForEach @(
        @{ Field = 'sourceBranch'; Value = 'refs/pull/123/merge' }
        @{ Field = 'repository'; Value = @{ id = 'someone/maui'; type = 'GitHub' } }
        @{ Field = 'definition'; Value = @{ id = 313; name = 'maui-pr-uitests' } }
        @{ Field = 'reason'; Value = 'pullRequest' }
        @{ Field = 'result'; Value = 'canceled' }
        @{ Field = 'status'; Value = 'inProgress' }
        @{ Field = 'id'; Value = '21' }
        @{ Field = 'sourceVersion'; Value = '@someone [click](https://example.com)' }
    ) {
        $script:ApiBuild[$Field] = $Value
        { Get-BranchCiLatestBuild main } | Should -Throw '*unexpected build*'
    }

    It 'reports missing builds as unknown, not green' {
        Mock Invoke-WebRequest { @{ Content = '{"value":[]}' } }
        { Get-BranchCiLatestBuild main } | Should -Throw '*CI health is unknown*'
    }

    It 'rejects malformed or unexpectedly broad build listings: <Json>' -ForEach @(
        @{ Json = '<html>Sign in</html>' }
        @{ Json = '{"value":null}' }
        @{ Json = '{"value":{}}' }
        @{ Json = '{"value":[{},{}]}' }
    ) {
        Mock Invoke-WebRequest { @{ Content = $Json } }
        { Get-BranchCiLatestBuild main } | Should -Throw
    }

    It 'does not hide an AzDO timeout' {
        Mock Invoke-WebRequest { throw 'AzDO timed out' }
        { Get-BranchCiLatestBuild main } | Should -Throw '*timed out*'
    }
}

Describe 'Branch outage lifecycle' {
    BeforeEach {
        $script:BranchCiBranches = @('main')
        $script:Issues = @()
        $script:Comments = @()
        $script:Build = New-TestBuild
        $script:MissingBranches = @()
        $script:Writes = [System.Collections.Generic.List[object]]::new()
        $script:SavedSummaryPath = $env:GITHUB_STEP_SUMMARY
        $env:GITHUB_STEP_SUMMARY = Join-Path $TestDrive 'summary.md'
        Mock Invoke-WebRequest { throw 'Live AzDO access is forbidden in tests.' }
        Mock Get-BranchCiLatestBuild { $script:Build }
        Mock Invoke-BranchCiGitHub {
            param($Endpoint, $Method = 'GET', $Body, $Paginate)
            if ($Method -ne 'GET') {
                $script:Writes.Add(@{ Endpoint = $Endpoint; Method = $Method; Body = $Body })
                return @{ number = 701 }
            }
            if ($Endpoint -match '/issues\?') { return $script:Issues }
            if ($Endpoint -match '/comments\?') { return $script:Comments }
            if ($Endpoint -match '/labels\?') { return @() }
            if ($Endpoint -match '/git/matching-refs/heads/(.+)$') {
                if ($Matches[1] -cin $script:MissingBranches) { return @() }
                return @(@{ ref = "refs/heads/$($Matches[1])" })
            }
            throw "Unexpected GitHub read: $Endpoint"
        }
    }

    AfterEach {
        $env:GITHUB_STEP_SUMMARY = $script:SavedSummaryPath
    }

    It 'does not write by default, even when an issue would be created' {
        Invoke-BranchCiMonitor
        $script:Writes.Count | Should -Be 0
        Get-Content $env:GITHUB_STEP_SUMMARY -Raw | Should -Match 'Dry run: Create outage issue'
    }

    It 'creates one labeled issue and pings kubaflo for <Result>' -ForEach @(
        @{ Result = 'failed' }, @{ Result = 'partiallySucceeded' }
    ) {
        $script:Build.result = $Result
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 2
        $script:Writes[0].Endpoint | Should -Be 'repos/dotnet/maui/labels'
        $script:Writes[1].Body.title | Should -Be '[maui-pr] main is failing'
        $script:Writes[1].Body.labels | Should -Be @('ci-branch-health')
        $script:Writes[1].Body.body | Should -Match '@kubaflo'
        $script:Writes[1].Body.body | Should -Match 'buildId=21'
        $script:Writes[1].Body.body | Should -Match "<!-- maui-pr-branch-build:21;result:$Result -->"
    }

    It 'does not recreate or recolor an existing label' {
        Mock Invoke-BranchCiGitHub { @(@{ name = 'ci-branch-health' }) } -ParameterFilter { $Endpoint -match '/labels\?' }
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 1
        $script:Writes[0].Endpoint | Should -Be 'repos/dotnet/maui/issues'
    }

    It 'does nothing when green with no outage' {
        $script:Build.result = 'succeeded'
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 0
    }

    It 'does not repeat a previously reported build on an <State> issue' -ForEach @(
        @{ State = 'open' }, @{ State = 'closed' }
    ) {
        $script:Issues = @(New-TestIssue -Id 21 -State $State)
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 0
    }

    It 'appends a new failed build without overwriting human edits or re-pinging' {
        $script:Issues = @(New-TestIssue)
        $script:Issues[0].body += "`nHuman investigation notes."
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 1
        $script:Writes[0].Endpoint | Should -Be 'repos/dotnet/maui/issues/700/comments'
        $script:Writes[0].Body.body | Should -Not -Match '@kubaflo'
        $script:Issues[0].body | Should -Match 'Human investigation notes'
    }

    It 'deduplicates build observations in paginated bot comments' {
        $script:Issues = @(New-TestIssue)
        $script:Comments = @(New-TestComment)
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 0
        Should -Invoke Invoke-BranchCiGitHub -Times 1 -ParameterFilter { $Endpoint -match '/comments\?' -and $Paginate }
        Should -Invoke Invoke-BranchCiGitHub -Times 1 -ParameterFilter { $Endpoint -match '/issues\?state=all' -and $Paginate }
    }

    It 'does not trust build markers posted by humans' {
        $script:Issues = @(New-TestIssue)
        $script:Comments = @(New-TestComment -Id 999 -Author someone)
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 1
    }

    It 'closes an owned issue on recovery, including a retry of the same build' -ForEach @(
        @{ RecoveredId = 20 }, @{ RecoveredId = 21 }
    ) {
        $script:Issues = @(New-TestIssue)
        $script:Build = New-TestBuild -Id $RecoveredId -Result succeeded
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 2
        $script:Writes[0].Method | Should -Be 'POST'
        $script:Writes[0].Body.body | Should -Match 'succeeded'
        $script:Writes[1].Method | Should -Be 'PATCH'
        $script:Writes[1].Endpoint | Should -Be 'repos/dotnet/maui/issues/700'
        $script:Writes[1].Body.state | Should -Be 'closed'
    }

    It 'does not close anything in a recovery dry run' {
        $script:Issues = @(New-TestIssue)
        $script:Build.result = 'succeeded'
        Invoke-BranchCiMonitor
        $script:Writes.Count | Should -Be 0
    }

    It 'retries a failed closure without duplicating the recovery comment' {
        $script:Issues = @(New-TestIssue)
        $script:Comments = @(New-TestComment -Result succeeded)
        $script:Build.result = 'succeeded'
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 1
        $script:Writes[0].Method | Should -Be 'PATCH'
    }

    It 'starts a new outage after a recovered issue' {
        $script:Issues = @(New-TestIssue -State closed)
        $script:Comments = @(New-TestComment -Id 20 -Result succeeded)
        Invoke-BranchCiMonitor -Apply
        $script:Writes[-1].Endpoint | Should -Be 'repos/dotnet/maui/issues'
    }

    It 'refuses stale build data rather than closing or reopening issues' -ForEach @(
        @{ Result = 'succeeded' }, @{ Result = 'failed' }
    ) {
        $script:Issues = @(New-TestIssue -Id 30)
        $script:Build.result = $Result
        { Invoke-BranchCiMonitor -Apply } | Should -Throw '*predates the history*'
        $script:Writes.Count | Should -Be 0
    }

    It 'never modifies a <Kind> issue' -ForEach @(
        @{ Kind = 'human-authored' }
        @{ Kind = 'pull-request' }
        @{ Kind = 'different-branch' }
        @{ Kind = 'unlabeled' }
        @{ Kind = 'unmarked' }
    ) {
        $issue = New-TestIssue
        switch ($Kind) {
            'human-authored' { $issue.user.login = 'someone' }
            'pull-request' { $issue.pull_request = @{} }
            'different-branch' { $issue = New-TestIssue -Branch net11.0 }
            'unlabeled' { $issue.labels = @() }
            'unmarked' { $issue.body = 'Not owned by the monitor.' }
        }
        $script:Issues = @($issue)
        $script:Build.result = 'succeeded'
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 0
    }

    It 'fails closed for multiple open outage issues' {
        $script:Issues = @((New-TestIssue), (New-TestIssue -Number 702))
        { Invoke-BranchCiMonitor -Apply } | Should -Throw '*Multiple open monitor issues*'
        $script:Writes.Count | Should -Be 0
    }

    It 'fails closed for corrupt monitor state' {
        $script:Issues = @(New-TestIssue)
        $script:Issues[0].body = Get-BranchCiMarker main
        { Invoke-BranchCiMonitor -Apply } | Should -Throw '*no valid build marker*'
        $script:Writes.Count | Should -Be 0
    }

    It 'does not mutate anything when the issue index cannot be read' {
        Mock Invoke-BranchCiGitHub { throw 'GitHub unavailable' } -ParameterFilter { $Endpoint -match '/issues\?' }
        { Invoke-BranchCiMonitor -Apply } | Should -Throw '*GitHub unavailable*'
        $script:Writes.Count | Should -Be 0
    }

    It 'does not treat API failures as green, but still monitors independent branches' {
        $script:BranchCiBranches = @('main', 'inflight/current')
        Mock Get-BranchCiLatestBuild { throw 'AzDO unavailable' } -ParameterFilter { $Branch -eq 'main' }
        { Invoke-BranchCiMonitor -Apply } | Should -Throw '*main: AzDO unavailable*'
        $script:Writes[-1].Body.title | Should -Be '[maui-pr] inflight/current is failing'
        Get-Content $env:GITHUB_STEP_SUMMARY -Raw | Should -Match 'Unknown / operation failed'
    }

    It 'surfaces failed writes and does not close an issue if its recovery comment failed' {
        $script:Issues = @(New-TestIssue)
        $script:Build.result = 'succeeded'
        Mock Invoke-BranchCiGitHub { throw 'Comment failed' } -ParameterFilter { $Method -eq 'POST' }
        { Invoke-BranchCiMonitor -Apply } | Should -Throw '*Comment failed*'
        Should -Invoke Invoke-BranchCiGitHub -Times 0 -ParameterFilter { $Method -eq 'PATCH' }
    }

    It 'skips absent branches without treating them as green or changing their issues' {
        $script:BranchCiBranches = @('inflight/candidate', 'net12.0')
        $script:MissingBranches = @('inflight/candidate', 'net12.0')
        $script:Issues = @(New-TestIssue -Branch 'inflight/candidate')
        Invoke-BranchCiMonitor -Apply
        $script:Writes.Count | Should -Be 0
        Get-Content $env:GITHUB_STEP_SUMMARY -Raw | Should -Match 'inflight/candidate \| Branch does not exist \| Skipped'
        Get-Content $env:GITHUB_STEP_SUMMARY -Raw | Should -Match 'net12.0 \| Branch does not exist \| Skipped'
        Should -Invoke Get-BranchCiLatestBuild -Times 0
    }

    It 'starts checking a previously absent branch as soon as it exists' {
        $script:BranchCiBranches = @('net12.0')
        $script:MissingBranches = @('net12.0')
        Invoke-BranchCiMonitor -Apply
        $script:MissingBranches = @()
        Invoke-BranchCiMonitor -Apply
        $script:Writes[-1].Body.title | Should -Be '[maui-pr] net12.0 is failing'
        Should -Invoke Get-BranchCiLatestBuild -Times 1 -ParameterFilter { $Branch -eq 'net12.0' }
    }
}

Describe 'Branch monitor workflow contract' {
    It 'monitors precisely the requested branches' {
        $script:ConfiguredBranches | Should -Be @('inflight/current', 'inflight/candidate', 'main', 'net11.0', 'net12.0')
    }

    It 'schedules every six hours and runs only trusted code with narrowly scoped credentials' {
        $workflow = Get-Content (Join-Path $PSScriptRoot '../workflows/maui-pr-branch-monitor.yml') -Raw
        $workflow | Should -Match "cron: '17 \*/6 \* \* \*'"
        $workflow | Should -Match 'workflow_dispatch:'
        $workflow | Should -Match '(?s)dry_run:.*?default: true'
        $workflow | Should -Match 'needs: test'
        $workflow | Should -Match 'cancel-in-progress: false'
        $workflow | Should -Match 'MAUI_PR_BRANCH_MONITOR_DISABLED'
        $workflow | Should -Not -Match 'pull_request_target|secrets\.|COPILOT'
        ([regex]::Matches($workflow, 'ref: \$\{\{ github.event.repository.default_branch \}\}')).Count | Should -Be 1
        $workflow | Should -Match 'ref: \$\{\{ needs.test.outputs.source_sha \}\}'
        $workflow | Should -Match 'source_sha: \$\{\{ steps.tests.outputs.source_sha \}\}'
        $workflow | Should -Match '"source_sha=\$sha" >> \$env:GITHUB_OUTPUT'
        ([regex]::Matches($workflow, 'persist-credentials: false')).Count | Should -Be 2
        ([regex]::Matches($workflow, 'GH_TOKEN:')).Count | Should -Be 1
        $gate = Get-Content (Join-Path $PSScriptRoot '../workflows/powershell-script-tests.yml') -Raw
        $gate | Should -Match 'maui-pr-branch-monitor.yml'
    }
}

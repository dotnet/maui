#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Hermetic behavioral tests for /issue trace-regression.
.EXAMPLE
    Import-Module Pester -RequiredVersion 5.9.0
    $configuration = New-PesterConfiguration
    $configuration.Run.Path = './.github/scripts/IssueRegression.Tests.ps1'
    $configuration.Run.PassThru = $true
    $configuration.TestDrive.Enabled = $false
    $configuration.TestRegistry.Enabled = $false
    $configuration.Output.Verbosity = 'Detailed'
    Invoke-Pester -Configuration $configuration
.NOTES
    All GitHub calls are mocked. Fixtures live under the working directory and
    are removed after the run; neither TestDrive nor a real network is needed.
#>

BeforeAll {
    function gh { throw 'The real GitHub CLI must never run in this suite.' }

    . (Join-Path $PSScriptRoot 'Get-IssueRegressionContext.ps1')

    $script:GoodSha = 'a' * 40
    $script:BadSha = 'b' * 40
    $script:TagSha = 'c' * 40
    $script:FixtureRoot = Join-Path (Get-Location).Path ".issue-regression-tests-$([guid]::NewGuid().ToString('N'))"
    $null = New-Item -ItemType Directory -Path $script:FixtureRoot
    $script:SavedEnvironment = @{}
    foreach ($name in @('GITHUB_OUTPUT', 'GITHUB_ACTOR', 'GITHUB_TRIGGERING_ACTOR')) {
        $script:SavedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
    }

    function New-TestEvent {
        param(
            [AllowNull()][AllowEmptyString()][string]$Body = '/issue trace-regression',
            [AllowNull()][AllowEmptyString()][string]$Login = 'command-author'
        )

        [pscustomobject]@{
            action = 'created'
            repository = [pscustomobject]@{ full_name = 'dotnet/maui' }
            sender = [pscustomobject]@{ login = 'event-sender'; type = 'User' }
            issue = [pscustomobject]@{ number = 12345 }
            comment = [pscustomobject]@{
                body = $Body
                node_id = 'IC_authorized_command'
                user = [pscustomobject]@{ login = $Login; type = 'User' }
                author_association = 'MEMBER'
            }
        }
    }

    function New-TestIssue {
        param(
            $Number = 12345,
            [AllowNull()][AllowEmptyString()][string]$Body = 'A regression report without version fields.',
            [int]$CommentCount = 0
        )

        [pscustomobject]@{
            number = $Number
            html_url = "https://github.com/dotnet/maui/issues/$Number"
            title = 'The control stopped working'
            body = $Body
            user = [pscustomobject]@{ login = 'issue-author' }
            updated_at = '2026-09-19T12:00:00Z'
            labels = @([pscustomobject]@{ name = 't/bug' }, [pscustomobject]@{ name = 'platform/ios' })
            comments = $CommentCount
        }
    }

    function New-TestComment {
        param([int]$Number)

        [pscustomobject]@{
            html_url = "https://github.com/dotnet/maui/issues/12345#issuecomment-$Number"
            user = [pscustomobject]@{ login = "commenter-$Number"; type = 'User' }
            author_association = 'CONTRIBUTOR'
            created_at = '2026-09-19T12:01:00Z'
            updated_at = '2026-09-19T12:02:00Z'
            body = "Comment $Number"
        }
    }

    function Set-TestResponse {
        param([string]$Endpoint, [AllowNull()]$Value)
        $script:Responses[$Endpoint] = ConvertTo-Json -InputObject $Value -Depth 30 -Compress
    }

    function Set-TestTags {
        param([string]$Version, [AllowNull()][string]$Sha)
        foreach ($tag in @($Version, "v$Version")) {
            $script:Responses["repos/dotnet/maui/git/ref/tags/$tag"] = $null
        }
        if ($Sha) {
            Set-TestResponse "repos/dotnet/maui/git/ref/tags/$Version" @{
                object = @{ type = 'commit'; sha = $Sha }
            }
        }
    }

    function New-TestComparison {
        param(
            [string]$Status = 'ahead',
            [string]$MergeBase = $script:GoodSha,
            [int]$TotalCommits = 1,
            [int]$CommitCount = 1,
            [int]$FileCount = 1
        )

        [pscustomobject]@{
            html_url = 'https://github.com/dotnet/maui/compare/reported-good...reported-bad'
            status = $Status
            merge_base_commit = [pscustomobject]@{ sha = $MergeBase }
            ahead_by = if ($Status -eq 'ahead') { $TotalCommits } else { 0 }
            behind_by = if ($Status -in @('behind', 'diverged')) { $TotalCommits } else { 0 }
            total_commits = $TotalCommits
            commits = @(
                for ($i = 0; $i -lt $CommitCount; $i++) {
                    [pscustomobject]@{
                        sha = $script:BadSha
                        html_url = "https://github.com/dotnet/maui/commit/$($script:BadSha)"
                        commit = [pscustomobject]@{ message = "Commit $i`r`n`r`nAdditional details" }
                    }
                }
            )
            files = @(
                for ($i = 0; $i -lt $FileCount; $i++) {
                    [pscustomobject]@{
                        filename = "src/Control$i.cs"
                        previous_filename = "src/OldControl$i.cs"
                        status = 'renamed'
                    }
                }
            )
        }
    }

    function Set-TestResolvedBoundaries {
        Set-TestTags '10.0.10' $script:GoodSha
        Set-TestTags '10.0.20' $script:BadSha
        $script:ComparisonEndpoint = "repos/dotnet/maui/compare/$($script:GoodSha)...$($script:BadSha)?per_page=100&page=1"
        Set-TestResponse $script:ComparisonEndpoint (New-TestComparison)
    }

    function New-TestVersionedIssue {
        New-TestIssue -Body "### Last version that worked well`n`n10.0.10`n`n### Version with bug`n`n10.0.20"
    }

    function Assert-TriggerStopped {
        Get-Content -LiteralPath $env:GITHUB_OUTPUT | Should -BeExactly 'should_run=false'
        Test-Path -LiteralPath $script:ContextPath | Should -BeFalse
        @($script:GhCalls | Where-Object Endpoint -EQ 'graphql').Count | Should -Be 0
    }
}

Describe 'Issue regression automation' {
    BeforeEach {
        $script:Responses = @{}
        $script:Failures = @{}
        $script:GhCalls = [System.Collections.Generic.List[object]]::new()
        $script:UnexpectedCalls = [System.Collections.Generic.List[string]]::new()

        Mock gh { throw 'The real GitHub CLI must never run in this suite.' }
        Mock Invoke-WebRequest { throw 'HTTP requests must never run in this suite.' }
        Mock Invoke-RestMethod { throw 'HTTP requests must never run in this suite.' }
        Mock Invoke-GhCommandWithRetry {
            param($Arguments, $Description, $AllowNotFound, $AllowFailure, $RequireOutput)

            $endpoint = $Arguments[1]
            $script:GhCalls.Add([pscustomobject]@{
                    Arguments = @($Arguments)
                    Endpoint = $endpoint
                    Description = $Description
                    AllowNotFound = [bool]$AllowNotFound
                    AllowFailure = [bool]$AllowFailure
                    RequireOutput = [bool]$RequireOutput
                })
            if ($script:Failures.ContainsKey($endpoint)) {
                # The shared wrapper suppresses failures only for an explicit AllowFailure.
                if ($AllowFailure) { return $null }
                throw $script:Failures[$endpoint]
            }
            if ($script:Responses.ContainsKey($endpoint)) {
                return $script:Responses[$endpoint]
            }
            $script:UnexpectedCalls.Add(($Arguments -join ' '))
            throw "Unexpected mocked GitHub request: $($Arguments -join ' ')"
        }
    }

    AfterEach {
        $script:UnexpectedCalls | Should -BeNullOrEmpty
        Should -Invoke gh -Times 0 -Exactly
        Should -Invoke Invoke-WebRequest -Times 0 -Exactly
        Should -Invoke Invoke-RestMethod -Times 0 -Exactly
    }

    AfterAll {
        foreach ($name in $script:SavedEnvironment.Keys) {
            [Environment]::SetEnvironmentVariable($name, $script:SavedEnvironment[$name])
        }
        if ($script:FixtureRoot -and (Test-Path -LiteralPath $script:FixtureRoot)) {
            Remove-Item -LiteralPath $script:FixtureRoot -Recurse -Force
        }
    }

    Describe 'Get-IssueRegressionRequest' {
        It 'accepts a standalone command with <Name>' -ForEach @(
            @{ Name = 'one space'; Body = '/issue trace-regression' }
            @{ Name = 'multiple spaces'; Body = '/issue   trace-regression' }
            @{ Name = 'trailing horizontal whitespace'; Body = "/issue trace-regression`t  " }
            @{ Name = 'trailing CRLF'; Body = "/issue trace-regression`r`n" }
            @{ Name = 'trailing blank lines'; Body = "/issue trace-regression`n`n" }
            @{ Name = 'mixed trailing spaces, tabs, and CRLF'; Body = "/issue trace-regression `t`r`n`t " }
        ) {
            $result = Get-IssueRegressionRequest (New-TestEvent -Body $Body)

            $result.issueNumber | Should -Be 12345
            $result.issueNumber | Should -BeOfType ([int])
            $result.requester | Should -BeExactly 'command-author'
            $result.commentNodeId | Should -BeExactly 'IC_authorized_command'
            $script:GhCalls.Count | Should -Be 0
        }

        It 'rejects <Name>' -ForEach @(
            @{ Name = 'an empty body'; Body = '' }
            @{ Name = 'a null body'; Body = $null }
            @{ Name = 'a leading space'; Body = ' /issue trace-regression' }
            @{ Name = 'a leading newline'; Body = "`n/issue trace-regression" }
            @{ Name = 'a tab separator'; Body = "/issue`ttrace-regression" }
            @{ Name = 'a space followed by a tab separator'; Body = "/issue `ttrace-regression" }
            @{ Name = 'a tab followed by a space separator'; Body = "/issue`t trace-regression" }
            @{ Name = 'a carriage return separator'; Body = "/issue`rtrace-regression" }
            @{ Name = 'a newline between command words'; Body = "/issue`ntrace-regression" }
            @{ Name = 'a nonbreaking space separator'; Body = "/issue$([char]0xa0)trace-regression" }
            @{ Name = 'an uppercase command'; Body = '/Issue trace-regression' }
            @{ Name = 'an uppercase subcommand'; Body = '/issue Trace-Regression' }
            @{ Name = 'the former regression command'; Body = '/issue regression' }
            @{ Name = 'the former bisect command'; Body = '/issue bisect' }
            @{ Name = 'a review command'; Body = '/review trace-regression' }
            @{ Name = 'a misspelled subcommand'; Body = '/issue trace-regresion' }
            @{ Name = 'an underscore subcommand'; Body = '/issue trace_regression' }
            @{ Name = 'no subcommand'; Body = '/issue' }
            @{ Name = 'an issue argument'; Body = '/issue trace-regression 12345' }
            @{ Name = 'a version argument'; Body = '/issue trace-regression --good 10.0.10' }
            @{ Name = 'a second command'; Body = "/issue trace-regression`n/review" }
            @{ Name = 'a duplicate command'; Body = "/issue trace-regression`n/issue trace-regression" }
            @{ Name = 'a prose prefix'; Body = 'Please /issue trace-regression' }
            @{ Name = 'a prose suffix'; Body = '/issue trace-regression please' }
            @{ Name = 'a Markdown quote'; Body = '> /issue trace-regression' }
            @{ Name = 'a Markdown code span'; Body = '`/issue trace-regression`' }
            @{ Name = 'a Markdown code fence'; Body = "``````text`n/issue trace-regression`n``````" }
            @{ Name = 'an HTML suffix'; Body = '/issue trace-regression <!-- hidden -->' }
            @{ Name = 'a shell semicolon'; Body = '/issue trace-regression; echo injected' }
            @{ Name = 'a shell pipeline'; Body = '/issue trace-regression | echo injected' }
            @{ Name = 'a shell conditional'; Body = '/issue trace-regression && echo injected' }
            @{ Name = 'a shell substitution'; Body = '/issue trace-regression $(echo injected)' }
            @{ Name = 'a backtick substitution'; Body = '/issue trace-regression `echo injected`' }
            @{ Name = 'a workflow output injection'; Body = "/issue trace-regression`nshould_run=true" }
            @{ Name = 'a NUL suffix'; Body = "/issue trace-regression$([char]0)" }
        ) {
            Get-IssueRegressionRequest (New-TestEvent -Body $Body) | Should -BeNullOrEmpty
            $script:GhCalls.Count | Should -Be 0
        }

        It 'rejects event action <Action>' -ForEach @(
            @{ Action = 'edited' }, @{ Action = 'deleted' }, @{ Action = 'opened' },
            @{ Action = 'Created' }, @{ Action = '' }, @{ Action = $null }
        ) {
            $event = New-TestEvent
            $event.action = $Action
            Get-IssueRegressionRequest $event | Should -BeNullOrEmpty
        }

        It 'rejects repository <Repository>' -ForEach @(
            @{ Repository = 'someone/maui' }, @{ Repository = 'dotnet/docs-maui' },
            @{ Repository = 'DotNet/maui' }, @{ Repository = 'dotnet/MAUI' },
            @{ Repository = 'dotnet/maui/extra' }, @{ Repository = '' },
            @{ Repository = "dotnet/maui`n" }, @{ Repository = $null }
        ) {
            $event = New-TestEvent
            $event.repository.full_name = $Repository
            Get-IssueRegressionRequest $event | Should -BeNullOrEmpty
        }

        It 'requires the comment author to be an actual User, not <Type>' -ForEach @(
            @{ Type = 'Bot' }, @{ Type = 'Organization' }, @{ Type = 'Mannequin' },
            @{ Type = 'user' }, @{ Type = 'USER' }, @{ Type = '' }, @{ Type = $null }
        ) {
            $event = New-TestEvent
            $event.comment.user.type = $Type
            Get-IssueRegressionRequest $event | Should -BeNullOrEmpty
        }

        It 'accepts safe GitHub username <Login>' -ForEach @(
            @{ Login = 'a' }, @{ Login = 'A9-maintainer' }, @{ Login = '9user' },
            @{ Login = ('a' * 39) }
        ) {
            (Get-IssueRegressionRequest (New-TestEvent -Login $Login)).requester |
                Should -BeExactly $Login
        }

        It 'rejects an invalid or injected username: <Name>' -ForEach @(
            @{ Name = 'empty'; Login = '' }, @{ Name = 'null'; Login = $null }
            @{ Name = 'too long'; Login = ('a' * 40) }
            @{ Name = 'leading hyphen'; Login = '-user' }
            @{ Name = 'bot suffix'; Login = 'maui-bot[bot]' }
            @{ Name = 'space'; Login = 'command author' }
            @{ Name = 'underscore'; Login = 'command_author' }
            @{ Name = 'dot'; Login = 'command.author' }
            @{ Name = 'path traversal'; Login = '../other' }
            @{ Name = 'path separator'; Login = 'author/permission' }
            @{ Name = 'query'; Login = 'author?admin=true' }
            @{ Name = 'non-ASCII'; Login = 'autór' }
            @{ Name = 'newline'; Login = "author`nadmin" }
            @{ Name = 'semicolon'; Login = 'author;echo' }
            @{ Name = 'substitution'; Login = '$(echo author)' }
            @{ Name = 'quote'; Login = "author'" }
            @{ Name = 'at sign'; Login = '@author' }
        ) {
            Get-IssueRegressionRequest (New-TestEvent -Login $Login) | Should -BeNullOrEmpty
        }

        It 'rejects an invalid issue number: <Name>' -ForEach @(
            @{ Name = 'zero'; Number = 0 }, @{ Name = 'negative'; Number = -1 },
            @{ Name = 'null'; Number = $null }, @{ Name = 'empty'; Number = '' },
            @{ Name = 'fraction'; Number = 1.5 }, @{ Name = 'text'; Number = 'issue-12345' },
            @{ Name = 'overflow'; Number = '2147483648' },
            @{ Name = 'output injection'; Number = "12345`nshould_run=true" },
            @{ Name = 'shell substitution'; Number = '$(echo 12345)' },
            @{ Name = 'path injection'; Number = '12345/comments' }
        ) {
            $event = New-TestEvent
            $event.issue.number = $Number
            Get-IssueRegressionRequest $event | Should -BeNullOrEmpty
        }

        It 'normalizes a positive string ID to an integer' {
            $event = New-TestEvent
            $event.issue.number = '0012345'
            $result = Get-IssueRegressionRequest $event
            $result.issueNumber | Should -Be 12345
            $result.issueNumber | Should -BeOfType ([int])
        }

        It 'accepts the maximum positive supported issue ID' {
            $event = New-TestEvent
            $event.issue.number = [int]::MaxValue
            (Get-IssueRegressionRequest $event).issueNumber | Should -Be ([int]::MaxValue)
        }

        It 'rejects PR comments even when pull_request is an empty object' {
            $event = New-TestEvent
            $event.issue | Add-Member -NotePropertyName pull_request -NotePropertyValue ([pscustomobject]@{})
            Get-IssueRegressionRequest $event | Should -BeNullOrEmpty
        }

        It 'rejects incomplete webhook payloads: <Missing>' -ForEach @(
            @{ Missing = 'issue' }, @{ Missing = 'repository' }, @{ Missing = 'comment' }
        ) {
            $event = New-TestEvent
            $event.PSObject.Properties.Remove($Missing)
            Get-IssueRegressionRequest $event | Should -BeNullOrEmpty
        }

        It 'rejects a missing comment author even if the sender is a User' {
            $event = New-TestEvent
            $event.comment.PSObject.Properties.Remove('user')
            Get-IssueRegressionRequest $event | Should -BeNullOrEmpty
        }
    }

    Describe 'Resolve-RegressionVersion' {
        It 'does not invent a tag for <Name>' -ForEach @(
            @{ Name = 'an omitted answer'; Version = '' }
            @{ Name = 'a null answer'; Version = $null }
            @{ Name = 'the template placeholder'; Version = '_No response_' }
            @{ Name = 'an unknown version'; Version = 'Unknown' }
            @{ Name = 'a never-working version'; Version = 'Never worked' }
            @{ Name = 'a generic major version'; Version = '10' }
            @{ Name = 'a generic minor version'; Version = '10.0' }
            @{ Name = 'a wildcard'; Version = '10.0.x' }
            @{ Name = 'the generic template option'; Version = 'Other (please specify)' }
            @{ Name = 'latest'; Version = 'latest' }
            @{ Name = 'a workload description'; Version = '.NET MAUI 10.0.20' }
            @{ Name = 'a range'; Version = '10.0.10 - 10.0.20' }
            @{ Name = 'multiple versions'; Version = "10.0.10`n10.0.20" }
            @{ Name = 'a branch name'; Version = 'main' }
            @{ Name = 'a commit SHA'; Version = ('a' * 40) }
            @{ Name = 'a tag URL'; Version = 'https://github.com/dotnet/maui/releases/tag/10.0.20' }
            @{ Name = 'an uppercase tag prefix'; Version = 'V10.0.20' }
            @{ Name = 'a lowercase qualifier'; Version = '10.0.20 ga' }
            @{ Name = 'an incomplete qualifier'; Version = '10.0.20 SR' }
            @{ Name = 'a qualifier alone'; Version = 'SR10' }
            @{ Name = 'a trailing line'; Version = "10.0.20`n" }
            @{ Name = 'a leading space'; Version = ' 10.0.20' }
            @{ Name = 'a path'; Version = '10.0.20/../../main' }
            @{ Name = 'shell syntax'; Version = '10.0.20; echo injected' }
            @{ Name = 'a substitution'; Version = '$(echo 10.0.20)' }
        ) {
            $result = Resolve-RegressionVersion -Version $Version
            $result.reported | Should -BeExactly ([string]$Version)
            $result.status | Should -BeExactly 'unresolved'
            $result.refs | Should -BeNullOrEmpty
            $result.sha | Should -BeNullOrEmpty
            $script:GhCalls.Count | Should -Be 0
        }

        It 'leaves an exact version with no matching tags unresolved' {
            Set-TestTags '10.0.20' $null

            $result = Resolve-RegressionVersion '10.0.20'

            $result.status | Should -BeExactly 'unresolved'
            $result.refs | Should -BeNullOrEmpty
            $result.sha | Should -BeNullOrEmpty
            $script:GhCalls.Endpoint | Should -Be @(
                'repos/dotnet/maui/git/ref/tags/10.0.20',
                'repos/dotnet/maui/git/ref/tags/v10.0.20'
            )
            foreach ($call in $script:GhCalls) {
                $call.AllowNotFound | Should -BeTrue
                $call.RequireOutput | Should -BeTrue
                $call.AllowFailure | Should -BeFalse
            }
        }

        It 'resolves <Version> using only the exact and v-prefixed tag candidates' -ForEach @(
            @{ Version = '10.0.20'; Tag = '10.0.20' }
            @{ Version = 'v10.0.20'; Tag = '10.0.20' }
            @{ Version = '10.0.20 GA'; Tag = '10.0.20' }
            @{ Version = 'v10.0.20 GA'; Tag = '10.0.20' }
            @{ Version = '10.0.20 SR2'; Tag = '10.0.20' }
            @{ Version = '10.0.20 SR2.1'; Tag = '10.0.20' }
            @{ Version = "10.0.20`tSR2"; Tag = '10.0.20' }
            @{ Version = '11.0.0-preview.6.26302.1'; Tag = '11.0.0-preview.6.26302.1' }
            @{ Version = 'v11.0.0-rc.1.26401.2'; Tag = '11.0.0-rc.1.26401.2' }
        ) {
            Set-TestTags $Tag $script:BadSha

            $result = Resolve-RegressionVersion $Version

            $result.reported | Should -BeExactly $Version
            $result.status | Should -BeExactly 'resolved'
            $result.sha | Should -BeExactly $script:BadSha
            @($result.refs).Count | Should -Be 1
            $result.refs[0].tag | Should -BeExactly $Tag
            $script:GhCalls.Endpoint | Should -Be @(
                "repos/dotnet/maui/git/ref/tags/$Tag",
                "repos/dotnet/maui/git/ref/tags/v$Tag"
            )
        }

        It 'resolves a release that has only a v-prefixed tag' {
            Set-TestTags '10.0.20' $null
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/v10.0.20' @{
                object = @{ type = 'commit'; sha = $script:BadSha }
            }
            $result = Resolve-RegressionVersion '10.0.20'
            $result.status | Should -BeExactly 'resolved'
            $result.sha | Should -BeExactly $script:BadSha
            $result.refs[0].tag | Should -BeExactly 'v10.0.20'
        }

        It 'records both aliases when they resolve to the same commit' {
            Set-TestTags '10.0.20' $script:BadSha
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/v10.0.20' @{
                object = @{ type = 'commit'; sha = $script:BadSha }
            }
            $result = Resolve-RegressionVersion '10.0.20'
            $result.status | Should -BeExactly 'resolved'
            $result.sha | Should -BeExactly $script:BadSha
            $result.refs.tag | Should -Be @('10.0.20', 'v10.0.20')
        }

        It 'does not choose one of two conflicting tag SHAs' {
            Set-TestTags '10.0.20' $script:BadSha
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/v10.0.20' @{
                object = @{ type = 'commit'; sha = $script:GoodSha }
            }
            $result = Resolve-RegressionVersion '10.0.20'
            $result.status | Should -BeExactly 'ambiguous'
            $result.sha | Should -BeNullOrEmpty
            $result.refs.sha | Should -Be @($script:BadSha, $script:GoodSha)
        }

        It 'peels an annotated release tag to its commit' {
            Set-TestTags '10.0.20' $null
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/10.0.20' @{
                object = @{ type = 'tag'; sha = $script:TagSha }
            }
            Set-TestResponse "repos/dotnet/maui/git/tags/$($script:TagSha)" @{
                object = @{ type = 'commit'; sha = $script:BadSha }
            }
            $result = Resolve-RegressionVersion '10.0.20'
            $result.status | Should -BeExactly 'resolved'
            $result.sha | Should -BeExactly $script:BadSha
            $peel = @($script:GhCalls | Where-Object Endpoint -Like '*/git/tags/*')
            $peel.Count | Should -Be 1
            $peel[0].RequireOutput | Should -BeTrue
            $peel[0].AllowNotFound | Should -BeFalse
        }

        It 'bounds annotated tag traversal at five objects (<Depth> requested)' -ForEach @(
            @{ Depth = 5; Resolves = $true }, @{ Depth = 6; Resolves = $false }
        ) {
            Set-TestTags '10.0.20' $null
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/10.0.20' @{
                object = @{ type = 'tag'; sha = ('{0:x40}' -f 1) }
            }
            for ($i = 1; $i -le $Depth; $i++) {
                $object = if ($i -eq $Depth) {
                    @{ type = 'commit'; sha = $script:BadSha }
                } else {
                    @{ type = 'tag'; sha = ('{0:x40}' -f ($i + 1)) }
                }
                Set-TestResponse ("repos/dotnet/maui/git/tags/{0:x40}" -f $i) @{ object = $object }
            }

            if ($Resolves) {
                (Resolve-RegressionVersion '10.0.20').sha | Should -BeExactly $script:BadSha
            } else {
                { Resolve-RegressionVersion '10.0.20' } | Should -Throw '*did not resolve to a commit*'
            }
            @($script:GhCalls | Where-Object Endpoint -Like '*/git/tags/*').Count | Should -Be 5
        }

        It 'bounds a cyclic annotated tag without treating it as a commit' {
            Set-TestTags '10.0.20' $null
            $tag = @{ object = @{ type = 'tag'; sha = $script:TagSha } }
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/10.0.20' $tag
            Set-TestResponse "repos/dotnet/maui/git/tags/$($script:TagSha)" $tag
            { Resolve-RegressionVersion '10.0.20' } | Should -Throw '*did not resolve to a commit*'
            @($script:GhCalls | Where-Object Endpoint -Like '*/git/tags/*').Count | Should -Be 5
        }

        It 'rejects an invalid tag object: <Name>' -ForEach @(
            @{ Name = 'missing object'; Payload = '{}' }
            @{ Name = 'JSON null'; Payload = 'null' }
            @{ Name = 'invalid JSON'; Payload = '{broken' }
            @{ Name = 'tree'; Payload = ('{"object":{"type":"tree","sha":"' + ('a' * 40) + '"}}') }
            @{ Name = 'blob'; Payload = ('{"object":{"type":"blob","sha":"' + ('a' * 40) + '"}}') }
            @{ Name = 'missing SHA'; Payload = '{"object":{"type":"commit"}}' }
            @{ Name = 'short SHA'; Payload = '{"object":{"type":"commit","sha":"abc123"}}' }
            @{ Name = 'nonhex SHA'; Payload = ('{"object":{"type":"commit","sha":"' + ('g' * 40) + '"}}') }
            @{ Name = 'path in SHA'; Payload = '{"object":{"type":"commit","sha":"../../main"}}' }
            @{ Name = 'invalid annotated SHA'; Payload = '{"object":{"type":"tag","sha":"injected;echo"}}' }
        ) {
            $script:Responses['repos/dotnet/maui/git/ref/tags/10.0.20'] = $Payload
            { Resolve-RegressionVersion '10.0.20' } | Should -Throw
            $script:GhCalls.Count | Should -Be 1
        }

        It 'fails instead of trusting a partial resolution when <Name> fails' -ForEach @(
            @{ Name = 'the first lookup'; Endpoint = 'repos/dotnet/maui/git/ref/tags/10.0.20' }
            @{ Name = 'the other alias lookup'; Endpoint = 'repos/dotnet/maui/git/ref/tags/v10.0.20' }
        ) {
            Set-TestTags '10.0.20' $script:BadSha
            $script:Failures[$Endpoint] = 'HTTP 503: tag service unavailable'
            { Resolve-RegressionVersion '10.0.20' } | Should -Throw '*tag service unavailable*'
        }

        It 'fails if an annotated tag cannot be retrieved' {
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/10.0.20' @{
                object = @{ type = 'tag'; sha = $script:TagSha }
            }
            $script:Failures["repos/dotnet/maui/git/tags/$($script:TagSha)"] = 'HTTP 404: annotated tag unavailable'
            { Resolve-RegressionVersion '10.0.20' } | Should -Throw '*annotated tag unavailable*'
        }
    }

    Describe 'Get-IssueRegressionContext' {
        BeforeEach {
            Set-TestResponse 'repos/dotnet/maui/issues/12345/comments?per_page=100&page=1' @()
        }

        It 'rejects PRs before collecting any evidence' {
            $issue = New-TestIssue
            $issue | Add-Member -NotePropertyName pull_request -NotePropertyValue ([pscustomobject]@{})
            { Get-IssueRegressionContext $issue } | Should -Throw '*issue, not a pull request*'
            $script:GhCalls.Count | Should -Be 0
        }

        It 'rejects nonpositive issue IDs (<Number>)' -ForEach @(
            @{ Number = 0 }, @{ Number = -1 }
        ) {
            { Get-IssueRegressionContext (New-TestIssue -Number $Number) } | Should -Throw
            $script:GhCalls.Count | Should -Be 0
        }

        It 'preserves issue metadata and parses template headings without interpreting Markdown' {
            $body = @'
Issue preamble

### Description

Text with `code`, [a link](https://example.invalid), and Unicode: résumé.

### Version with bug

10.0.20 SR2.1

### Last version that worked well

10.0.10 GA

### Affected platforms

iOS, Android

### Version with bug

9.0.99
'@ -replace "`n", "`r`n"
            Set-TestResolvedBoundaries
            $issue = New-TestIssue -Body $body

            $context = Get-IssueRegressionContext $issue

            $context.schemaVersion | Should -Be 1
            $context.repository | Should -BeExactly 'dotnet/maui'
            { [DateTimeOffset]::Parse($context.capturedAt) } | Should -Not -Throw
            $context.issue.number | Should -Be 12345
            $context.issue.url | Should -BeExactly $issue.html_url
            $context.issue.author | Should -BeExactly 'issue-author'
            $context.issue.title | Should -BeExactly $issue.title
            $context.issue.body | Should -BeExactly $body
            $context.issue.updatedAt | Should -BeExactly $issue.updated_at
            $context.issue.labels | Should -Be @('t/bug', 'platform/ios')
            $context.issue.fields['Version with bug'] | Should -BeExactly '10.0.20 SR2.1'
            $context.issue.fields['Last version that worked well'] | Should -BeExactly '10.0.10 GA'
            $context.issue.fields['Affected platforms'] | Should -BeExactly 'iOS, Android'
            $context.issue.fields['Description'] | Should -Match 'résumé'
            $context.boundaries.reportedGood.reported | Should -BeExactly '10.0.10 GA'
            $context.boundaries.reportedBad.reported | Should -BeExactly '10.0.20 SR2.1'
            $context.gaps | Should -BeNullOrEmpty
        }

        It 'handles <Name> issue bodies without inventing template fields' -ForEach @(
            @{ Name = 'empty'; Body = '' }, @{ Name = 'null'; Body = $null },
            @{ Name = 'free-form'; Body = 'It worked before, but I do not know the version.' }
        ) {
            $context = Get-IssueRegressionContext (New-TestIssue -Body $Body)
            $context.issue.fields.Count | Should -Be 0
            $context.boundaries.reportedGood.status | Should -BeExactly 'unresolved'
            $context.boundaries.reportedBad.status | Should -BeExactly 'unresolved'
            $context.comparison | Should -BeNullOrEmpty
            @($script:GhCalls | Where-Object Endpoint -Like '*/git/*').Count | Should -Be 0
        }

        It 'keeps the last 100 of <Count> comments using no more than the last two pages' -ForEach @(
            @{ Count = 0 }, @{ Count = 1 }, @{ Count = 100 }, @{ Count = 101 },
            @{ Count = 199 }, @{ Count = 200 }, @{ Count = 201 }, @{ Count = 250 },
            @{ Count = 10000 }
        ) {
            $lastPage = [Math]::Max(1, [int][Math]::Ceiling($Count / 100.0))
            $firstPage = [Math]::Max(1, $lastPage - 1)
            $expectedEndpoints = @(
                for ($page = $firstPage; $page -le $lastPage; $page++) {
                    $endpoint = "repos/dotnet/maui/issues/12345/comments?per_page=100&page=$page"
                    $comments = @(
                        for ($i = (($page - 1) * 100 + 1); $i -le [Math]::Min($Count, $page * 100); $i++) {
                            New-TestComment $i
                        }
                    )
                    Set-TestResponse $endpoint $comments
                    $endpoint
                }
            )

            $context = Get-IssueRegressionContext (New-TestIssue -CommentCount $Count)

            @($context.comments).Count | Should -Be ([Math]::Min(100, $Count))
            $context.commentsTruncated | Should -Be ($Count -gt 100)
            $script:GhCalls.Endpoint | Should -Be $expectedEndpoints
            $script:GhCalls.Count | Should -BeLessOrEqual 2
            foreach ($call in $script:GhCalls) {
                $call.RequireOutput | Should -BeTrue
                $call.Arguments | Should -Not -Contain '--paginate'
            }
            if ($Count -gt 0) {
                $context.comments[0].body | Should -BeExactly "Comment $([Math]::Max(1, $Count - 99))"
                $context.comments[-1].body | Should -BeExactly "Comment $Count"
                $context.comments[-1].author | Should -BeExactly "commenter-$Count"
                $context.comments[-1].url | Should -BeExactly "https://github.com/dotnet/maui/issues/12345#issuecomment-$Count"
                $context.comments[-1].authorType | Should -BeExactly 'User'
                $context.comments[-1].association | Should -BeExactly 'CONTRIBUTOR'
                ([DateTimeOffset]$context.comments[-1].createdAt) | Should -Be ([DateTimeOffset]'2026-09-19T12:01:00Z')
                ([DateTimeOffset]$context.comments[-1].updatedAt) | Should -Be ([DateTimeOffset]'2026-09-19T12:02:00Z')
            }
        }

        It 'retains bot comments as evidence even though bots cannot trigger the command' {
            $comment = New-TestComment 1
            $comment.user.login = 'maui-bot[bot]'
            $comment.user.type = 'Bot'
            Set-TestResponse 'repos/dotnet/maui/issues/12345/comments?per_page=100&page=1' @($comment)
            $context = Get-IssueRegressionContext (New-TestIssue -CommentCount 1)
            $context.comments[0].authorType | Should -BeExactly 'Bot'
            $context.comments[0].author | Should -BeExactly 'maui-bot[bot]'
        }

        It 'records a gap when comment page <Page> fails, without claiming a complete collection' -ForEach @(
            @{ Page = 1 }, @{ Page = 2 }
        ) {
            Set-TestResolvedBoundaries
            Set-TestResponse 'repos/dotnet/maui/issues/12345/comments?per_page=100&page=1' @(
                for ($i = 1; $i -le 100; $i++) { New-TestComment $i }
            )
            $script:Failures["repos/dotnet/maui/issues/12345/comments?per_page=100&page=$Page"] = 'HTTP 503: comments unavailable'
            $issue = New-TestVersionedIssue
            $issue.comments = 101

            $context = Get-IssueRegressionContext $issue -WarningAction SilentlyContinue

            $context.gaps -join "`n" | Should -Match 'Comments unavailable:.*comments unavailable'
            $context.commentsTruncated | Should -BeTrue
            $context.comparison.isForwardRange | Should -BeTrue
            @($script:GhCalls | Where-Object Endpoint -Like '*/comments?*').Count | Should -Be $Page
        }

        It 'reports malformed comments as a gap and still resolves release evidence' {
            Set-TestResolvedBoundaries
            $script:Responses['repos/dotnet/maui/issues/12345/comments?per_page=100&page=1'] = '{invalid'
            $context = Get-IssueRegressionContext (New-TestVersionedIssue) -WarningAction SilentlyContinue
            $context.gaps -join "`n" | Should -Match 'Comments unavailable'
            $context.comparison.isForwardRange | Should -BeTrue
        }

        It 'reports missing comment evidence (<Name>) instead of silently treating it as complete' -ForEach @(
            @{ Name = 'JSON null'; Payload = 'null' }
            @{ Name = 'an object instead of an array'; Payload = '{}' }
            @{ Name = 'an empty page despite five reported comments'; Payload = '[]' }
        ) {
            Set-TestResolvedBoundaries
            $script:Responses['repos/dotnet/maui/issues/12345/comments?per_page=100&page=1'] = $Payload
            $issue = New-TestVersionedIssue
            $issue.comments = 5
            $context = Get-IssueRegressionContext $issue -WarningAction SilentlyContinue
            $context.gaps -join "`n" | Should -Match '(?i)comment'
        }

        It 'records unavailable tag evidence rather than failing the entire context' {
            Set-TestResolvedBoundaries
            $script:Failures['repos/dotnet/maui/git/ref/tags/10.0.10'] = 'HTTP 403: reference unavailable'
            $context = Get-IssueRegressionContext (New-TestVersionedIssue) -WarningAction SilentlyContinue
            $context.boundaries.reportedGood.status | Should -BeExactly 'unavailable'
            $context.boundaries.reportedBad.status | Should -BeExactly 'resolved'
            $context.gaps -join "`n" | Should -Match 'tag lookup failed:.*reference unavailable'
            $context.comparison | Should -BeNullOrEmpty
            @($script:GhCalls | Where-Object Endpoint -Like '*/compare/*').Count | Should -Be 0
        }

        It 'reports invalid tag objects as unavailable evidence' {
            Set-TestResolvedBoundaries
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/10.0.20' @{
                object = @{ type = 'commit'; sha = '../untrusted-ref' }
            }
            $context = Get-IssueRegressionContext (New-TestVersionedIssue) -WarningAction SilentlyContinue
            $context.boundaries.reportedBad.status | Should -BeExactly 'unavailable'
            $context.gaps -join "`n" | Should -Match 'tag lookup failed'
            @($script:GhCalls | Where-Object Endpoint -Like '*/compare/*').Count | Should -Be 0
        }

        It 'reports missing version fields as gaps rather than a successful investigation' {
            $context = Get-IssueRegressionContext (New-TestIssue)
            $context.gaps.Count | Should -BeGreaterThan 0
            $context.comparison | Should -BeNullOrEmpty
        }

        It 'records unresolved generic version evidence as a gap' {
            $context = Get-IssueRegressionContext (New-TestIssue -Body "### Version with bug`n`n10.0.x`n`n### Last version that worked well`n`nUnknown")
            $context.boundaries.reportedBad.status | Should -BeExactly 'unresolved'
            $context.boundaries.reportedGood.status | Should -BeExactly 'unresolved'
            $context.gaps.Count | Should -BeGreaterThan 0
            $script:GhCalls.Count | Should -Be 1
        }

        It 'records absent release tags as a gap and never compares guessed refs' {
            Set-TestTags '10.0.10' $null
            Set-TestTags '10.0.20' $null
            $context = Get-IssueRegressionContext (New-TestVersionedIssue)
            $context.boundaries.reportedGood.status | Should -BeExactly 'unresolved'
            $context.boundaries.reportedBad.status | Should -BeExactly 'unresolved'
            @($script:GhCalls | Where-Object Endpoint -Like '*/compare/*').Count | Should -Be 0
            $context.gaps.Count | Should -BeGreaterThan 0
        }

        It 'reports conflicting aliases as a gap and never compares an arbitrary SHA' {
            Set-TestResolvedBoundaries
            Set-TestResponse 'repos/dotnet/maui/git/ref/tags/v10.0.20' @{
                object = @{ type = 'commit'; sha = $script:GoodSha }
            }
            $context = Get-IssueRegressionContext (New-TestVersionedIssue)
            $context.boundaries.reportedBad.status | Should -BeExactly 'ambiguous'
            $context.comparison | Should -BeNullOrEmpty
            @($script:GhCalls | Where-Object Endpoint -Like '*/compare/*').Count | Should -Be 0
            $context.gaps.Count | Should -BeGreaterThan 0
        }

        It 'requires both ahead status and the reported good ancestor (<Status>, <SameAncestor>)' -ForEach @(
            @{ Status = 'ahead'; SameAncestor = $true; Expected = $true }
            @{ Status = 'ahead'; SameAncestor = $false; Expected = $false }
            @{ Status = 'identical'; SameAncestor = $true; Expected = $false }
            @{ Status = 'behind'; SameAncestor = $true; Expected = $false }
            @{ Status = 'diverged'; SameAncestor = $false; Expected = $false }
        ) {
            Set-TestResolvedBoundaries
            $ancestor = if ($SameAncestor) { $script:GoodSha } else { $script:TagSha }
            Set-TestResponse $script:ComparisonEndpoint (New-TestComparison -Status $Status -MergeBase $ancestor)
            $context = Get-IssueRegressionContext (New-TestVersionedIssue)
            $context.comparison.status | Should -BeExactly $Status
            $context.comparison.isForwardRange | Should -Be $Expected
            @($script:GhCalls | Where-Object Endpoint -Like '*/compare/*').Count | Should -Be 1
            $script:GhCalls[-1].Endpoint | Should -BeExactly $script:ComparisonEndpoint
            $script:GhCalls[-1].RequireOutput | Should -BeTrue
        }

        It 'projects comparison evidence without claiming full commit or file coverage (<Name>)' -ForEach @(
            @{ Name = 'empty comparison'; Total = 0; Commits = 0; Files = 0; CommitTruncation = $false; FileTruncation = $false }
            @{ Name = 'one commit'; Total = 1; Commits = 1; Files = 1; CommitTruncation = $false; FileTruncation = $false }
            @{ Name = 'a full commit page'; Total = 100; Commits = 100; Files = 299; CommitTruncation = $false; FileTruncation = $false }
            @{ Name = 'commits beyond the first page'; Total = 101; Commits = 100; Files = 300; CommitTruncation = $true; FileTruncation = $true }
            @{ Name = 'a short commit response'; Total = 100; Commits = 1; Files = 301; CommitTruncation = $true; FileTruncation = $true }
        ) {
            Set-TestResolvedBoundaries
            $comparison = New-TestComparison -TotalCommits $Total -CommitCount $Commits -FileCount $Files
            Set-TestResponse $script:ComparisonEndpoint $comparison

            $context = Get-IssueRegressionContext (New-TestVersionedIssue)

            $context.comparison.url | Should -BeExactly $comparison.html_url
            $context.comparison.mergeBaseSha | Should -BeExactly $comparison.merge_base_commit.sha
            $context.comparison.aheadBy | Should -Be $comparison.ahead_by
            $context.comparison.behindBy | Should -Be $comparison.behind_by
            $context.comparison.totalCommits | Should -Be $Total
            $context.comparison.commitsTruncated | Should -Be $CommitTruncation
            $context.comparison.filesPossiblyTruncated | Should -Be $FileTruncation
            @($context.comparison.commits).Count | Should -Be $Commits
            @($context.comparison.files).Count | Should -Be $Files
            if ($Commits -gt 0) {
                $context.comparison.commits[0].sha | Should -BeExactly $script:BadSha
                $context.comparison.commits[0].url | Should -BeExactly $comparison.commits[0].html_url
                $context.comparison.commits[0].subject | Should -BeExactly 'Commit 0'
            }
            if ($Files -gt 0) {
                $context.comparison.files[0].path | Should -BeExactly 'src/Control0.cs'
                $context.comparison.files[0].previousPath | Should -BeExactly 'src/OldControl0.cs'
                $context.comparison.files[0].status | Should -BeExactly 'renamed'
            }
            @($script:GhCalls | Where-Object Endpoint -Like '*/compare/*').Count | Should -Be 1
        }

        It 'records a compare API failure without dropping the resolved boundaries' {
            Set-TestResolvedBoundaries
            $script:Failures[$script:ComparisonEndpoint] = 'HTTP 503: comparison unavailable'
            $context = Get-IssueRegressionContext (New-TestVersionedIssue) -WarningAction SilentlyContinue
            $context.boundaries.reportedGood.status | Should -BeExactly 'resolved'
            $context.boundaries.reportedBad.status | Should -BeExactly 'resolved'
            $context.comparison | Should -BeNullOrEmpty
            $context.gaps -join "`n" | Should -Match 'Release comparison unavailable:.*comparison unavailable'
        }

        It 'leaves optional <Field> metadata unknown when GitHub omits it' -ForEach @(
            @{ Field = 'ahead_by'; OutputField = 'aheadBy' }
            @{ Field = 'behind_by'; OutputField = 'behindBy' }
        ) {
            Set-TestResolvedBoundaries
            $comparison = New-TestComparison
            $comparison.PSObject.Properties.Remove($Field)
            Set-TestResponse $script:ComparisonEndpoint $comparison

            $context = Get-IssueRegressionContext (New-TestVersionedIssue)

            $context.comparison[$OutputField] | Should -BeNullOrEmpty
            $context.comparison.isForwardRange | Should -BeTrue
            $context.comparison.mergeBaseSha | Should -BeExactly $script:GoodSha
            $context.gaps | Should -BeNullOrEmpty
        }

        It 'preserves diverged and truncated history without calling it a forward regression interval' {
            Set-TestTags '10.0.60' $script:GoodSha
            Set-TestTags '10.0.101' $script:BadSha
            $endpoint = "repos/dotnet/maui/compare/$($script:GoodSha)...$($script:BadSha)?per_page=100&page=1"
            $comparison = New-TestComparison -Status 'diverged' -MergeBase $script:TagSha `
                -TotalCommits 350 -CommitCount 100 -FileCount 300
            $comparison.ahead_by = 350
            $comparison.behind_by = 2
            Set-TestResponse $endpoint $comparison
            $issue = New-TestIssue -Body "### Last version that worked well`n`n10.0.60`n`n### Version with bug`n`n10.0.101"

            $context = Get-IssueRegressionContext $issue

            $context.boundaries.reportedGood.status | Should -BeExactly 'resolved'
            $context.boundaries.reportedBad.status | Should -BeExactly 'resolved'
            $context.comparison.status | Should -BeExactly 'diverged'
            $context.comparison.isForwardRange | Should -BeFalse
            $context.comparison.mergeBaseSha | Should -BeExactly $script:TagSha
            $context.comparison.aheadBy | Should -Be 350
            $context.comparison.behindBy | Should -Be 2
            $context.comparison.commitsTruncated | Should -BeTrue
            $context.comparison.filesPossiblyTruncated | Should -BeTrue
            @($script:GhCalls | Where-Object Endpoint -EQ $endpoint).Count | Should -Be 1
        }

        It 'records malformed comparison JSON as a gap' {
            Set-TestResolvedBoundaries
            $script:Responses[$script:ComparisonEndpoint] = '{broken'
            $context = Get-IssueRegressionContext (New-TestVersionedIssue) -WarningAction SilentlyContinue
            $context.comparison | Should -BeNullOrEmpty
            $context.gaps -join "`n" | Should -Match 'comparison unavailable'
        }

        It 'reports missing comparison evidence (<Name>) instead of assuming complete coverage' -ForEach @(
            @{ Name = 'JSON null'; Payload = 'null' }
            @{ Name = 'empty object'; Payload = '{}' }
            @{ Name = 'missing merge base'; Missing = 'merge_base_commit' }
            @{ Name = 'missing commit total'; Missing = 'total_commits' }
            @{ Name = 'missing commits'; Missing = 'commits' }
            @{ Name = 'missing files'; Missing = 'files' }
            @{ Name = 'missing status'; Missing = 'status' }
        ) {
            Set-TestResolvedBoundaries
            if ($Missing) {
                $comparison = New-TestComparison
                $comparison.PSObject.Properties.Remove($Missing)
                Set-TestResponse $script:ComparisonEndpoint $comparison
            } else {
                $script:Responses[$script:ComparisonEndpoint] = $Payload
            }
            $context = Get-IssueRegressionContext (New-TestVersionedIssue) -WarningAction SilentlyContinue
            $context.gaps -join "`n" | Should -Match '(?i)compar'
        }
    }

    Describe 'Invoke-IssueRegressionTrigger' {
        BeforeEach {
            $testDirectory = Join-Path $script:FixtureRoot ([guid]::NewGuid().ToString('N'))
            $null = New-Item -ItemType Directory -Path $testDirectory
            $env:GITHUB_OUTPUT = Join-Path $testDirectory 'github-output.txt'
            $env:GITHUB_ACTOR = 'workflow-rerunner'
            $env:GITHUB_TRIGGERING_ACTOR = 'different-admin'
            $script:ContextPath = Join-Path $testDirectory 'context' 'issue.json'
            $script:PermissionEndpoint = 'repos/dotnet/maui/collaborators/command-author/permission'
            $script:IssueEndpoint = 'repos/dotnet/maui/issues/12345'
            Set-TestResponse $script:PermissionEndpoint @{ permission = 'write' }
            Set-TestResponse $script:IssueEndpoint (New-TestIssue)
            Set-TestResponse 'repos/dotnet/maui/issues/12345/comments?per_page=100&page=1' @()
            Set-TestResponse 'graphql' @{ data = @{ minimizeComment = @{ minimizedComment = @{ isMinimized = $true } } } }
        }

        It 'authorizes the actual commenter with current <Permission> permission' -ForEach @(
            @{ Permission = 'admin' }, @{ Permission = 'maintain' }, @{ Permission = 'write' }
        ) {
            Set-TestResponse $script:PermissionEndpoint @{ permission = $Permission }

            Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath

            Get-Content -LiteralPath $env:GITHUB_OUTPUT | Should -Be @(
                'should_run=false', 'issue_number=12345', 'should_run=true'
            )
            $saved = Get-Content -Raw -LiteralPath $script:ContextPath | ConvertFrom-Json
            $saved.issue.number | Should -Be 12345
            $script:GhCalls.Endpoint | Should -Be @(
                $script:PermissionEndpoint, $script:IssueEndpoint,
                'repos/dotnet/maui/issues/12345/comments?per_page=100&page=1', 'graphql'
            )
            $script:GhCalls[0].RequireOutput | Should -BeTrue
            $script:GhCalls[1].RequireOutput | Should -BeTrue
            @($script:GhCalls | Where-Object Endpoint -Match 'workflow-rerunner|different-admin|event-sender|issue-author').Count |
                Should -Be 0
        }

        It 'rejects current <Name> permission and leaves the command visible' -ForEach @(
            @{ Name = 'read'; Permission = 'read' }, @{ Name = 'triage'; Permission = 'triage' },
            @{ Name = 'none'; Permission = 'none' }, @{ Name = 'missing'; Permission = $null },
            @{ Name = 'empty'; Permission = '' }, @{ Name = 'unknown'; Permission = 'owner' },
            @{ Name = 'injected'; Permission = "write`nshould_run=true" }
        ) {
            Set-TestResponse $script:PermissionEndpoint @{ permission = $Permission }
            $event = New-TestEvent
            $event.comment.author_association = 'OWNER'

            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath

            Assert-TriggerStopped
            $script:GhCalls.Endpoint | Should -Be @($script:PermissionEndpoint)
        }

        It 'does not substitute a rerunner permission for a now-unauthorized commenter' {
            Set-TestResponse $script:PermissionEndpoint @{ permission = 'read' }
            Set-TestResponse 'repos/dotnet/maui/collaborators/workflow-rerunner/permission' @{ permission = 'admin' }
            Set-TestResponse 'repos/dotnet/maui/collaborators/different-admin/permission' @{ permission = 'admin' }
            Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath
            Assert-TriggerStopped
            $script:GhCalls.Endpoint | Should -Be @($script:PermissionEndpoint)
        }

        It 'rechecks permission on each invocation rather than caching a prior authorization' {
            $event = New-TestEvent
            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath
            Set-TestResponse $script:PermissionEndpoint @{ permission = 'read' }
            $script:ContextPath = Join-Path (Split-Path $script:ContextPath) 'second.json'
            Remove-Item -LiteralPath $env:GITHUB_OUTPUT

            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath

            Get-Content -LiteralPath $env:GITHUB_OUTPUT | Should -BeExactly 'should_run=false'
            Test-Path -LiteralPath $script:ContextPath | Should -BeFalse
            @($script:GhCalls | Where-Object Endpoint -EQ $script:PermissionEndpoint).Count | Should -Be 2
            @($script:GhCalls | Where-Object Endpoint -EQ $script:IssueEndpoint).Count | Should -Be 1
            @($script:GhCalls | Where-Object Endpoint -EQ 'graphql').Count | Should -Be 1
        }

        It 'fails closed when <Name> lookup fails' -ForEach @(
            @{ Name = 'permission'; Endpoint = 'repos/dotnet/maui/collaborators/command-author/permission'; Calls = 1 }
            @{ Name = 'target issue'; Endpoint = 'repos/dotnet/maui/issues/12345'; Calls = 2 }
        ) {
            $script:Failures[$Endpoint] = 'HTTP 503: lookup failed'
            { Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath } |
                Should -Throw '*lookup failed*'
            Assert-TriggerStopped
            $script:GhCalls.Count | Should -Be $Calls
            foreach ($call in $script:GhCalls) { $call.AllowFailure | Should -BeFalse }
        }

        It 'fails closed on malformed <Name> JSON' -ForEach @(
            @{ Name = 'permission'; Endpoint = 'repos/dotnet/maui/collaborators/command-author/permission' }
            @{ Name = 'target issue'; Endpoint = 'repos/dotnet/maui/issues/12345' }
        ) {
            $script:Responses[$Endpoint] = '{invalid'
            { Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath } | Should -Throw
            Assert-TriggerStopped
        }

        It 'does not authorize a missing permission document (<Payload>)' -ForEach @(
            @{ Payload = 'null' }, @{ Payload = '{}' }
        ) {
            $script:Responses[$script:PermissionEndpoint] = $Payload
            Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath
            Assert-TriggerStopped
            $script:GhCalls.Count | Should -Be 1
        }

        It 'rejects an issue lookup that returns <Name>' -ForEach @(
            @{ Name = 'a different issue'; Kind = 'different' }
            @{ Name = 'a PR'; Kind = 'pr' }
            @{ Name = 'no issue'; Kind = 'null' }
            @{ Name = 'an empty object'; Kind = 'empty' }
            @{ Name = 'a newline-injected number'; Kind = 'injection' }
        ) {
            $issue = New-TestIssue
            switch ($Kind) {
                'different' { $issue.number = 98765 }
                'pr' { $issue | Add-Member -NotePropertyName pull_request -NotePropertyValue ([pscustomobject]@{}) }
                'null' { $issue = $null }
                'empty' { $issue = [pscustomobject]@{} }
                'injection' { $issue.number = "12345`nissue_number=98765" }
            }
            Set-TestResponse $script:IssueEndpoint $issue
            { Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath } |
                Should -Throw '*requested issue*'
            Assert-TriggerStopped
            $script:GhCalls.Count | Should -Be 2
        }

        It 'rejects <Name> events before any lookup or minimization' -ForEach @(
            @{ Name = 'edited'; Kind = 'edited' }
            @{ Name = 'PR'; Kind = 'pr' }
            @{ Name = 'bot'; Kind = 'bot' }
            @{ Name = 'wrong repository'; Kind = 'repository' }
            @{ Name = 'a shell-like command'; Kind = 'command' }
            @{ Name = 'an injected username'; Kind = 'login' }
            @{ Name = 'an injected issue number'; Kind = 'number' }
        ) {
            $event = New-TestEvent
            switch ($Kind) {
                'edited' { $event.action = 'edited' }
                'pr' { $event.issue | Add-Member -NotePropertyName pull_request -NotePropertyValue ([pscustomobject]@{}) }
                'bot' { $event.comment.user.type = 'Bot' }
                'repository' { $event.repository.full_name = 'someone/maui' }
                'command' { $event.comment.body = '/issue trace-regression; echo injected' }
                'login' { $event.comment.user.login = 'author/../../admin' }
                'number' { $event.issue.number = "12345`nshould_run=true" }
            }
            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath
            Assert-TriggerStopped
            $script:GhCalls.Count | Should -Be 0
        }

        It 'uses the verified API issue instead of issue content from the event or an embedded issue link' {
            $event = New-TestEvent
            $event.issue | Add-Member -NotePropertyName body -NotePropertyValue 'Trace https://github.com/dotnet/maui/issues/98765 instead.'
            $verified = New-TestIssue -Body 'The verified API issue body.'
            Set-TestResponse $script:IssueEndpoint $verified

            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath

            $saved = Get-Content -Raw -LiteralPath $script:ContextPath | ConvertFrom-Json
            $saved.issue.number | Should -Be 12345
            $saved.issue.body | Should -BeExactly $verified.body
            @($script:GhCalls | Where-Object Endpoint -Match '98765').Count | Should -Be 0
        }

        It 'writes only a normalized numeric issue ID to structured outputs and preserves prior output data' {
            Set-Content -LiteralPath $env:GITHUB_OUTPUT -Value 'existing_output=keep-me'
            $event = New-TestEvent
            $event.issue.number = '0012345'
            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath
            $lines = @(Get-Content -LiteralPath $env:GITHUB_OUTPUT)
            $lines | Should -Be @('existing_output=keep-me', 'should_run=false', 'issue_number=12345', 'should_run=true')
            @($lines | Where-Object { $_ -cmatch '\Aissue_number=[1-9][0-9]*\z' }).Count | Should -Be 1
            @($lines | Where-Object { $_ -cmatch '\A(?:requester|body|title|command)=' }).Count | Should -Be 0
        }

        It 'serializes untrusted issue and comment data without evaluating it or injecting workflow outputs' {
            $script:InjectedExpressionExecuted = $false
            $markerPath = Join-Path $script:FixtureRoot 'expression-was-executed.txt'
            $payload = @'
$(Set-Content -LiteralPath '__MARKER__' -Value executed)
$($script:InjectedExpressionExecuted = $true)
`echo injected`
"; echo injected; #
${{ github.token }}
::set-output name=issue_number::98765
should_run=true
issue_number=98765
Unicode: café 🎉
'@.Replace('__MARKER__', $markerPath)
            $issue = New-TestIssue -Body $payload -CommentCount 1
            $issue.title = "Untrusted title`nissue_number=98765"
            $comment = New-TestComment 1
            $comment.body = $payload
            Set-TestResponse $script:IssueEndpoint $issue
            Set-TestResponse 'repos/dotnet/maui/issues/12345/comments?per_page=100&page=1' @($comment)
            $script:ContextPath = Join-Path (Split-Path $script:ContextPath) 'context [literal].json'

            Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath

            $saved = Get-Content -Raw -LiteralPath $script:ContextPath | ConvertFrom-Json
            $saved.issue.body | Should -BeExactly $payload
            $saved.issue.title | Should -BeExactly $issue.title
            $saved.comments[0].body | Should -BeExactly $payload
            $script:InjectedExpressionExecuted | Should -BeFalse
            Test-Path -LiteralPath $markerPath | Should -BeFalse
            Get-Content -LiteralPath $env:GITHUB_OUTPUT | Should -Be @(
                'should_run=false', 'issue_number=12345', 'should_run=true'
            )
            $script:GhCalls.Endpoint | Should -Be @(
                $script:PermissionEndpoint, $script:IssueEndpoint,
                'repos/dotnet/maui/issues/12345/comments?per_page=100&page=1', 'graphql'
            )
        }

        It 'persists partial evidence and the explicit comments gap rather than failing an authorized request' {
            $script:Failures['repos/dotnet/maui/issues/12345/comments?per_page=100&page=1'] = 'HTTP 503: comments unavailable'
            Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath -WarningAction SilentlyContinue
            $saved = Get-Content -Raw -LiteralPath $script:ContextPath | ConvertFrom-Json
            $saved.gaps -join "`n" | Should -Match 'Comments unavailable'
            (Get-Content -LiteralPath $env:GITHUB_OUTPUT)[-1] | Should -BeExactly 'should_run=true'
        }

        It 'minimizes only the authorized command using a fixed mutation and a separate node ID variable' {
            $event = New-TestEvent
            $event.comment.node_id = 'IC_untrusted") { injected } #'
            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath
            $minimize = $script:GhCalls[-1]
            $minimize.Arguments | Should -Be @(
                'api', 'graphql', '-f',
                'query=mutation($id: ID!) { minimizeComment(input: {subjectId: $id, classifier: RESOLVED}) { minimizedComment { isMinimized } } }',
                '-f', ('id=' + $event.comment.node_id)
            )
            $minimize.AllowFailure | Should -BeTrue
            $minimize.RequireOutput | Should -BeFalse
            @($script:GhCalls | Where-Object { $_.Arguments -contains '--method' -or $_.Arguments -contains '-X' }).Count |
                Should -Be 0
        }

        It 'keeps a valid context and success outputs when best-effort minimization fails' {
            $script:Failures['graphql'] = 'HTTP 403: cannot minimize comment'
            { Invoke-IssueRegressionTrigger -Event (New-TestEvent) -OutputPath $script:ContextPath } | Should -Not -Throw
            Test-Path -LiteralPath $script:ContextPath | Should -BeTrue
            (Get-Content -LiteralPath $env:GITHUB_OUTPUT)[-1] | Should -BeExactly 'should_run=true'
            $script:GhCalls[-1].Endpoint | Should -BeExactly 'graphql'
            $script:GhCalls[-1].AllowFailure | Should -BeTrue
        }

        It 'skips minimization when the comment node ID is <Name>' -ForEach @(
            @{ Name = 'missing'; NodeId = $null }, @{ Name = 'empty'; NodeId = '' },
            @{ Name = 'whitespace'; NodeId = '  ' }
        ) {
            $event = New-TestEvent
            $event.comment.node_id = $NodeId
            Invoke-IssueRegressionTrigger -Event $event -OutputPath $script:ContextPath
            Test-Path -LiteralPath $script:ContextPath | Should -BeTrue
            (Get-Content -LiteralPath $env:GITHUB_OUTPUT)[-1] | Should -BeExactly 'should_run=true'
            @($script:GhCalls | Where-Object Endpoint -EQ 'graphql').Count | Should -Be 0
        }
    }
}

Describe 'Issue regression workflow and report source contracts' {
    BeforeAll {
        $script:WorkflowText = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../workflows/issue-trace-regression.md')
        $script:SkillText = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../skills/trace-regression/SKILL.md')
        $frontmatter = [regex]::Match($script:WorkflowText, '(?s)\A---\r?\n(?<yaml>.*?)\r?\n---(?:\r?\n|\z)')
        if (-not $frontmatter.Success) { throw 'The workflow is missing YAML frontmatter.' }
        $script:WorkflowYaml = $frontmatter.Groups['yaml'].Value
        $script:WorkflowPrompt = $script:WorkflowText.Substring($frontmatter.Length)

        # These are source-contract checks, not a substitute for gh-aw compilation.
        function Get-ContractYamlBlock {
            param([string]$Yaml, [string]$Key, [int]$Indent = 0)

            $prefix = [regex]::Escape((' ' * $Indent) + $Key + ':')
            $pattern = '(?ms)^' + $prefix + '[^\r\n]*\r?\n.*?(?=^ {0,' + $Indent + '}\S|\z)'
            $blocks = [regex]::Matches($Yaml, $pattern)
            if ($blocks.Count -ne 1) {
                throw "Expected one '$Key' mapping at indentation $Indent; found $($blocks.Count)."
            }
            $blocks[0].Value
        }

        $script:OnBlock = Get-ContractYamlBlock $script:WorkflowYaml 'on'
        $script:JobsBlock = Get-ContractYamlBlock $script:WorkflowYaml 'jobs'
        $script:SafeOutputsBlock = Get-ContractYamlBlock $script:WorkflowYaml 'safe-outputs'
        $script:CheckoutSteps = @(
            [regex]::Matches(
                $script:WorkflowYaml,
                '(?ms)^(?<indent> *)- name:[^\r\n]*\r?\n.*?(?=^\k<indent>- name:|^\S|\z)'
            ) | ForEach-Object { $_.Value } | Where-Object { $_ -match '(?m)^ +uses: actions/checkout@' }
        )
        $skeletons = [regex]::Matches($script:SkillText, '(?ms)^```markdown[ \t]*\r?\n(?<report>.*?)^```[ \t]*(?:\r?\n|\z)')
        if ($skeletons.Count -ne 1) { throw 'Expected exactly one Markdown report skeleton in the skill.' }
        $script:ReportSkeleton = $skeletons[0].Groups['report'].Value
    }

    It 'allows only issue comments for /issue, without a dispatch, schedule, push, or PR trigger' {
        $slashCommand = Get-ContractYamlBlock $script:OnBlock 'slash_command' 2
        $slashCommand | Should -Match '(?m)^    name: issue[ \t]*\r?$'
        $slashCommand | Should -Match '(?m)^    events: \[issue_comment\][ \t]*\r?$'
        $keys = @([regex]::Matches($script:OnBlock, '(?m)^  ([a-z][a-z0-9_-]*):') |
            ForEach-Object { $_.Groups[1].Value } | Sort-Object)
        $keys | Should -Be @(
            'permissions', 'reaction', 'roles', 'skip-author-associations',
            'slash_command', 'status-comment', 'steps'
        )
        $script:WorkflowYaml | Should -Not -Match '(?m)^ *(?:workflow_dispatch|workflow_call|schedule|push|pull_request|pull_request_target):'
    }

    It 'has a top-level repository, created-event, and human-author gate' {
        $gate = Get-ContractYamlBlock $script:WorkflowYaml 'if'
        $gate | Should -Match "github\.repository == 'dotnet/maui'"
        $gate | Should -Match "github\.event\.action == 'created'"
        $gate | Should -Match "github\.event\.comment\.user\.type == 'User'"
        $gate | Should -Not -Match '\|\|'
        $script:OnBlock | Should -Match '(?m)^  roles: \[admin, maintain, write\][ \t]*\r?$'
    }

    It 'pins both collector and skill checkouts to github.sha without persisted credentials' {
        $script:CheckoutSteps.Count | Should -Be 2
        foreach ($step in $script:CheckoutSteps) {
            $step | Should -Match '(?m)^ +ref: \$\{\{ github\.sha \}\}[ \t]*\r?$'
            $step | Should -Match '(?m)^ +persist-credentials: false[ \t]*\r?$'
            @([regex]::Matches($step, '(?m)^ +ref:')).Count | Should -Be 1
            $step | Should -Not -Match 'github\.(?:head_ref|event\.pull_request)'
        }
    }

    It 'runs the deterministic collector on the event file rather than interpolating comment text into code' {
        $steps = Get-ContractYamlBlock $script:OnBlock 'steps' 2
        $steps | Should -Match '\. \.github/scripts/Get-IssueRegressionContext\.ps1'
        $steps | Should -Match '\$event = Get-Content -Raw -LiteralPath \$env:GITHUB_EVENT_PATH \| ConvertFrom-Json'
        $steps | Should -Match 'Invoke-IssueRegressionTrigger -Event \$event'
        $steps | Should -Not -Match '\$\{\{[^\r\n}]*(?:comment\.body|issue\.body|comment\.user\.login)'
        $steps | Should -Match "if: steps\.context\.outputs\.should_run == 'true'"
        $steps | Should -Match 'if-no-files-found: error'
    }

    It 'requires the collectors should_run output before activating the agent' {
        $preActivation = Get-ContractYamlBlock $script:JobsBlock 'pre-activation' 2
        $activation = Get-ContractYamlBlock $script:JobsBlock 'activation' 2
        $preActivation | Should -Match '(?m)^ +should_run: \$\{\{ steps\.context\.outputs\.should_run \}\}[ \t]*\r?$'
        $preActivation | Should -Match '(?m)^ +issue_number: \$\{\{ steps\.context\.outputs\.issue_number \}\}[ \t]*\r?$'
        $activation | Should -Match "(?m)^    if: needs\.pre_activation\.outputs\.should_run == 'true'[ \t]*\r?$"
        $activation | Should -Not -Match '\|\||always\(\)'
    }

    It 'uses ubuntu-latest for PowerShell pre-activation rather than the slim runner' {
        $script:WorkflowYaml | Should -Match '(?m)^runs-on-slim: ubuntu-latest[ \t]*\r?$'
        Get-ContractYamlBlock $script:OnBlock 'steps' 2 |
            Should -Match '(?m)^ +shell: pwsh[ \t]*\r?$'
    }

    It 'preserves the shared PAT-pool job rather than replacing its imported selection steps' {
        $imports = Get-ContractYamlBlock $script:WorkflowYaml 'imports'
        $imports | Should -Match '(?m)^  - uses: shared/pat_pool\.md[ \t]*\r?$'
        $imports | Should -Match '(?m)^      environment: copilot-pat-pool[ \t]*\r?$'
        # gh-aw replaces imported job definitions instead of merging a local override.
        $script:JobsBlock | Should -Not -Match '(?m)^  pat_pool:'
    }

    It 'keeps agent permissions read-only and shell tooling limited to jq' {
        $permissions = Get-ContractYamlBlock $script:WorkflowYaml 'permissions'
        $permissionValues = @([regex]::Matches($permissions, '(?m)^  ([\w-]+): ([\w-]+)[ \t]*\r?$') |
            ForEach-Object { $_.Groups[1].Value + ':' + $_.Groups[2].Value })
        $permissionValues | Should -Be @('contents:read', 'issues:read', 'pull-requests:read')
        $permissions | Should -Not -Match 'write'
        $tools = Get-ContractYamlBlock $script:WorkflowYaml 'tools'
        $tools | Should -Match '(?m)^  bash: \["jq"\][ \t]*\r?$'
        $script:WorkflowYaml | Should -Match '(?m)^model: gpt-[a-z0-9.-]+[ \t]*\r?$'
    }

    It 'pins GPT-5.6 Sol consistently in the workflow and tracing skill' {
        $script:WorkflowYaml | Should -Match '(?m)^model: gpt-5\.6-sol[ \t]*\r?$'
        $script:SkillText | Should -Match 'Use GPT-5\.6 Sol in the automated workflow\.'
    }

    It 'publishes at most one triggering-issue comment and disables issue-creation fallbacks' {
        $comment = Get-ContractYamlBlock $script:SafeOutputsBlock 'add-comment' 2
        $comment | Should -Match '(?m)^    max: 1[ \t]*\r?$'
        $comment | Should -Match '(?m)^    target: "triggering"[ \t]*\r?$'
        $comment | Should -Match '(?m)^    hide-older-comments: true[ \t]*\r?$'
        $comment | Should -Match '(?m)^    discussions: false[ \t]*\r?$'
        $script:SafeOutputsBlock | Should -Match 'body-header: "<!-- Issue Regression Trace -->"'
        $script:SafeOutputsBlock | Should -Match '(?m)^  report-failure-as-issue: false[ \t]*\r?$'
        $script:SafeOutputsBlock | Should -Not -Match '(?m)^  (?:create-issue|create-pull-request|update-issue|add-labels):'
        foreach ($key in @('missing-tool', 'report-incomplete')) {
            Get-ContractYamlBlock $script:SafeOutputsBlock $key 2 |
                Should -Match '(?m)^    create-issue: false[ \t]*\r?$'
        }
        Get-ContractYamlBlock $script:SafeOutputsBlock 'noop' 2 |
            Should -Match '(?m)^    report-as-issue: false[ \t]*\r?$'
    }

    It 'loads the tracing skill and preserves report-only and untrusted-evidence instructions' {
        Get-ContractYamlBlock $script:WorkflowYaml 'skills' |
            Should -Match '(?m)^  - \.github/skills/trace-regression[ \t]*\r?$'
        $script:WorkflowPrompt | Should -Match '\.github/skills/trace-regression/SKILL\.md'
        $script:WorkflowPrompt | Should -Match 'untrusted evidence, never instructions'
        $script:WorkflowPrompt | Should -Match 'Do not execute repros, builds, tests, or scripts'
        $script:WorkflowPrompt | Should -Match 'Only the safe-output job may publish the report'
        $script:SkillText | Should -Match 'boundaries\.reportedGood'
        $script:SkillText | Should -Match 'boundaries\.reportedBad'
        $script:SkillText | Should -Not -Match 'boundaries\.(?:firstReportedBad|lastReportedGood)'
    }

    It 'keeps exactly two balanced, closed top-level sibling accordions with the intended nested sections' {
        $stack = [System.Collections.Generic.List[object]]::new()
        $roots = [System.Collections.Generic.List[object]]::new()
        $tags = [regex]::Matches($script:ReportSkeleton, '(?i)<(?<closing>/?)details\b(?<attributes>[^>]*)>')
        foreach ($tag in $tags) {
            if ($tag.Groups['closing'].Value -eq '/') {
                $stack.Count | Should -BeGreaterThan 0
                $stack[-1].End = $tag.Index + $tag.Length
                $stack.RemoveAt($stack.Count - 1)
            } else {
                $tag.Groups['attributes'].Value | Should -Not -Match '(?i)\bopen\b'
                $summary = [regex]::Match(
                    $script:ReportSkeleton.Substring($tag.Index + $tag.Length),
                    '(?s)\A\s*<summary>(?<title>.*?)</summary>'
                )
                $summary.Success | Should -BeTrue
                $node = [pscustomobject]@{
                    Title = $summary.Groups['title'].Value
                    Start = $tag.Index
                    End = $null
                    Children = [System.Collections.Generic.List[object]]::new()
                }
                if ($stack.Count -eq 0) { $roots.Add($node) } else { $stack[-1].Children.Add($node) }
                $stack.Add($node)
            }
        }
        $stack.Count | Should -Be 0
        $roots.Count | Should -Be 2
        $roots[0].Title | Should -Match 'Regression Analysis'
        $roots[1].Title | Should -Match 'Follow-up'
        $roots[0].End | Should -BeLessThan $roots[1].Start
        $roots[0].Children.Count | Should -Be 2
        $roots[0].Children[0].Title | Should -Match 'Version boundary'
        $roots[0].Children[1].Title | Should -Match 'Candidate changes'
        $roots[1].Children.Count | Should -Be 0
        $script:ReportSkeleton.TrimEnd() | Should -Match '</details>$'
    }

    It 'shows exactly two blue flat-square Scope and Range badges before the accordions' {
        $badges = [regex]::Matches($script:ReportSkeleton, '<img\b[^>]*>')
        $badges.Count | Should -Be 2
        $badges[0].Value | Should -Match 'alt="Scope Regression trace"'
        $badges[0].Value | Should -Match 'https://img\.shields\.io/badge/Scope-Regression%20trace-1f6feb\?'
        $badges[1].Value | Should -Match 'alt="Range GOOD\.\.BAD"'
        $badges[1].Value | Should -Match 'https://img\.shields\.io/badge/Range-GOOD\.\.BAD-1f6feb\?'
        foreach ($badge in $badges) {
            $badge.Value | Should -Match 'labelColor=30363d&amp;style=flat-square'
            $badge.Index | Should -BeLessThan $script:ReportSkeleton.IndexOf('<details>')
        }
        $script:ReportSkeleton | Should -Match '> @AUTHOR_LOGIN .*#ISSUE_NUMBER\.'
        $script:ReportSkeleton | Should -Not -Match 'REQUESTER'
        $script:ReportSkeleton | Should -Match 'Maintainers: comment `/issue trace-regression` to refresh this report'
    }
}

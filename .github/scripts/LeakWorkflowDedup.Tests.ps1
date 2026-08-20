#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

    function New-LeakPr {
        param(
            [int]$Number,
            [string]$Title,
            [string]$Body = '',
            [string]$Base = 'main',
            [bool]$Merged = $true,
            [string]$MergeCommitOid = '',
            [int[]]$VerifiedRevertTargets,
            [string[]]$CommitMessages
        )

        if ($Merged -and [string]::IsNullOrWhiteSpace($MergeCommitOid)) {
            $MergeCommitOid = '{0:x40}' -f $Number
        }
        $result = [ordered]@{
            number      = $Number
            title       = $Title
            body        = $Body
            baseRefName = $Base
            state       = if ($Merged) { 'MERGED' } else { 'CLOSED' }
            merged      = $Merged
            mergedAt    = if ($Merged) { '2026-08-10T00:00:00Z' } else { $null }
            mergeCommitOid = if ($Merged) { $MergeCommitOid } else { $null }
            url         = "https://github.com/dotnet/maui/pull/$Number"
        }
        if ($PSBoundParameters.ContainsKey('VerifiedRevertTargets')) {
            $result.verifiedRevertTargets = @($VerifiedRevertTargets)
        }
        if ($PSBoundParameters.ContainsKey('CommitMessages')) {
            $result.commitMessages = @($CommitMessages)
        }
        [pscustomobject]$result
    }

    function Get-AmbiguousDiamondRevertGraph {
        param(
            [ValidateRange(1, 20)][int]$Layers,
            [int]$RootNumber = 100
        )

        $fix = New-LeakPr `
            -Number $RootNumber `
            -Title "[leak-fix] Fix Type$RootNumber.Member leak"
        $reverts = [System.Collections.Generic.List[object]]::new()
        $previousTargets = @($RootNumber)
        $nextNumber = $RootNumber + 100
        for ($layer = 1; $layer -le $Layers; $layer++) {
            $left = $nextNumber
            $right = $nextNumber + 1
            $reverts.Add((New-LeakPr `
                        -Number $left `
                        -Title "Diamond layer $layer left" `
                        -VerifiedRevertTargets ([int[]]$previousTargets)))
            $reverts.Add((New-LeakPr `
                        -Number $right `
                        -Title "Diamond layer $layer right" `
                        -VerifiedRevertTargets ([int[]]$previousTargets)))
            $previousTargets = @($left, $right)
            $nextNumber += 2
        }

        $cycleLeft = $nextNumber
        $cycleRight = $nextNumber + 1
        $reverts.Add((New-LeakPr `
                    -Number $cycleLeft `
                    -Title 'Shared cycle left' `
                    -VerifiedRevertTargets (
                        [int[]]@($previousTargets + $cycleRight)
                    )))
        $reverts.Add((New-LeakPr `
                    -Number $cycleRight `
                    -Title 'Shared cycle right' `
                    -VerifiedRevertTargets (
                        [int[]]@($previousTargets + $cycleLeft)
                    )))

        [pscustomobject]@{
            Fix = $fix
            Reverts = @($reverts)
        }
    }

    function New-LeakGraphQlPageJson {
        param(
            [Parameter(Mandatory = $true)][string]$ConnectionName,
            [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Nodes,
            [Parameter(Mandatory = $true)][long]$TotalCount,
            [Parameter(Mandatory = $true)][bool]$HasNextPage,
            [AllowNull()][string]$EndCursor
        )

        $repository = @{}
        $repository[$ConnectionName] = @{
            totalCount = $TotalCount
            nodes = @($Nodes)
            pageInfo = @{
                hasNextPage = $HasNextPage
                endCursor = $EndCursor
            }
        }
        @{
            data = @{
                repository = $repository
            }
        } | ConvertTo-Json -Depth 8 -Compress
    }

    function New-LeakGraphQlPullRequestNode {
        param(
            [Parameter(Mandatory = $true)][int]$Number,
            [string]$Base = 'main',
            [ValidateSet('OPEN', 'CLOSED', 'MERGED')][string]$State = 'MERGED',
            [string]$Title = "[leak-fix] Fix Type$Number.Member leak",
            [string]$Body = ''
        )

        [pscustomobject]@{
            number = $Number
            title = $Title
            body = $Body
            baseRefName = $Base
            state = $State
            merged = $State -ceq 'MERGED'
            mergedAt = if ($State -ceq 'MERGED') {
                '2026-08-10T00:00:00Z'
            } else {
                $null
            }
            mergeCommit = if ($State -ceq 'MERGED') {
                @{
                    oid = '{0:x40}' -f $Number
                }
            } else {
                $null
            }
            url = "https://github.com/dotnet/maui/pull/$Number"
        }
    }

    function New-LeakCommitHistory {
        param(
            [Parameter(Mandatory = $true)][object]$PullRequest,
            [AllowEmptyCollection()][string[]]$Messages = @(),
            [AllowNull()][object[]]$Commits = $null
        )

        if ($null -eq $Commits) {
            $commitNumber = 0
            $Commits = @($Messages | ForEach-Object {
                    $commitNumber++
                    [pscustomobject]@{
                        oid = '{0:x40}' -f (
                            ([int]$PullRequest.number * 1000) + $commitNumber
                        )
                        message = $_
                    }
                })
        }
        [pscustomobject]@{
            number = [int]$PullRequest.number
            state = 'MERGED'
            merged = $true
            baseRefName = [string]$PullRequest.baseRefName
            mergeCommitOid = [string]$PullRequest.mergeCommitOid
            commits = @($Commits)
        }
    }

    function New-LeakCommitHistoryGraphQlJson {
        param(
            [Parameter(Mandatory = $true)][object]$PullRequest,
            [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Commits,
            [Parameter(Mandatory = $true)][long]$TotalCount,
            [Parameter(Mandatory = $true)][bool]$HasNextPage,
            [AllowNull()][string]$EndCursor,
            [string]$Alias = 'pr0'
        )

        $repository = @{}
        $repository[$Alias] = @{
            number = [int]$PullRequest.number
            state = 'MERGED'
            merged = $true
            mergedAt = [string]$PullRequest.mergedAt
            baseRefName = [string]$PullRequest.baseRefName
            mergeCommit = @{
                oid = [string]$PullRequest.mergeCommitOid
            }
            commits = @{
                totalCount = $TotalCount
                nodes = @($Commits | ForEach-Object {
                        @{ commit = $_ }
                    })
                pageInfo = @{
                    hasNextPage = $HasNextPage
                    endCursor = $EndCursor
                }
            }
        }
        @{
            data = @{
                repository = $repository
            }
        } | ConvertTo-Json -Depth 10 -Compress
    }
}

Describe 'native gh invocation' {
    It 'pins native-command errors to the structured exit-code path' {
        $module = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1')
        $helper = [regex]::Match(
            $module,
            '(?s)function Invoke-LeakGhJson \{.*?\n\}'
        ).Value

        $preferenceIndex = $helper.IndexOf('$PSNativeCommandUseErrorActionPreference = $false')
        $invokeIndex = $helper.IndexOf('$output = & gh @Arguments 2>&1')

        $preferenceIndex | Should -BeGreaterOrEqual 0
        $invokeIndex | Should -BeGreaterOrEqual 0
        $preferenceIndex | Should -BeLessThan $invokeIndex
    }

    Context 'bounded transient retries' {
        BeforeEach {
            $global:leakGhAttemptCount = 0
            $global:leakGhResponses = [System.Collections.Generic.Queue[object]]::new()
            $global:leakGhDelays = [System.Collections.Generic.List[int]]::new()
            $global:leakGhNow = [DateTimeOffset]::FromUnixTimeSeconds(2000000000)
            function global:gh {
                param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
                $global:leakGhAttemptCount++
                $response = $global:leakGhResponses.Dequeue()
                $global:LASTEXITCODE = [int]$response.ExitCode
                if (-not [string]::IsNullOrWhiteSpace([string]$response.Stderr)) {
                    Write-Error ([string]$response.Stderr) -ErrorAction Continue
                }
                Write-Output ([string]$response.Stdout)
            }
        }

        AfterEach {
            Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
            Remove-Variable leakGhAttemptCount, leakGhResponses, leakGhDelays, leakGhNow `
                -Scope Global -ErrorAction SilentlyContinue
        }

        It 'uses the exponential fallback for transient failures without server timing' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 503: Service Unavailable'
                    Stdout = ''
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{"value":42}'
                })

            $result = Invoke-LeakGhJson `
                -Arguments @('api', 'test') `
                -RetryBaseDelaySeconds 2 `
                -DelayAction {
                    param([int]$Seconds)
                    $global:leakGhDelays.Add($Seconds)
                } `
                -UtcNowProvider { $global:leakGhNow }

            $result.value | Should -Be 42
            $global:leakGhAttemptCount | Should -Be 2
            $global:leakGhDelays | Should -Be @(2)
        }

        It 'honors numeric Retry-After timing case-insensitively' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 403: forbidden; rEtRy-AfTeR: 17'
                    Stdout = ''
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{"value":42}'
                })

            $result = Invoke-LeakGhJson `
                -Arguments @('api', 'test') `
                -DelayAction {
                    param([int]$Seconds)
                    $global:leakGhDelays.Add($Seconds)
                } `
                -UtcNowProvider { $global:leakGhNow }

            $result.value | Should -Be 42
            $global:leakGhDelays | Should -Be @(17)
        }

        It 'honors Retry-After HTTP dates and rate-limit reset timestamps' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 403: forbidden; Retry-After: Wed, 18 May 2033 03:33:32 GMT'
                    Stdout = ''
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{"kind":"date"}'
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 403: forbidden; X-RaTeLiMiT-ReSeT=2000000013'
                    Stdout = ''
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{"kind":"reset"}'
                })

            $dateResult = Invoke-LeakGhJson `
                -Arguments @('api', 'date') `
                -DelayAction {
                    param([int]$Seconds)
                    $global:leakGhDelays.Add($Seconds)
                } `
                -UtcNowProvider { $global:leakGhNow }
            $resetResult = Invoke-LeakGhJson `
                -Arguments @('api', 'reset') `
                -DelayAction {
                    param([int]$Seconds)
                    $global:leakGhDelays.Add($Seconds)
                } `
                -UtcNowProvider { $global:leakGhNow }

            $dateResult.kind | Should -Be 'date'
            $resetResult.kind | Should -Be 'reset'
            $global:leakGhDelays | Should -Be @(12, 13)
        }

        It 'handles malformed, negative, past, and excessive metadata safely' {
            @(
                'HTTP 429 rate limit; Retry-After: later; X-RateLimit-Reset: unknown'
                'HTTP 429 rate limit; Retry-After: -9; X-RateLimit-Reset: -5'
                'HTTP 403: forbidden; X-RateLimit-Reset: 1999999995'
                'HTTP 429 rate limit; Retry-After: 999999; X-RateLimit-Reset: 253402300799'
            ) | ForEach-Object {
                $global:leakGhResponses.Enqueue([pscustomobject]@{
                        ExitCode = 1
                        Stderr = $_
                        Stdout = ''
                    })
                $global:leakGhResponses.Enqueue([pscustomobject]@{
                        ExitCode = 0
                        Stderr = ''
                        Stdout = '{"value":42}'
                    })
            }

            1..4 | ForEach-Object {
                $result = Invoke-LeakGhJson `
                    -Arguments @('api', "case-$_") `
                    -RetryBaseDelaySeconds 2 `
                    -MaximumServerDelaySeconds 120 `
                    -DelayAction {
                        param([int]$Seconds)
                        $global:leakGhDelays.Add($Seconds)
                    } `
                    -UtcNowProvider { $global:leakGhNow }
                $result.value | Should -Be 42
            }

            $global:leakGhDelays | Should -Be @(2, 2, 120)
        }

        It 'fails closed after exhausting the bounded transient retry budget' {
            1..3 | ForEach-Object {
                $global:leakGhResponses.Enqueue([pscustomobject]@{
                        ExitCode = 1
                        Stderr = 'read: connection reset by peer'
                        Stdout = ''
                    })
            }

            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'test') `
                    -MaximumAttempts 3 `
                    -RetryBaseDelaySeconds 2 `
                    -DelayAction {
                        param([int]$Seconds)
                        $global:leakGhDelays.Add($Seconds)
                    } `
                    -UtcNowProvider { $global:leakGhNow }
            } | Should -Throw '*failed with exit code 1 after 3 attempt(s)*'

            $global:leakGhAttemptCount | Should -Be 3
            $global:leakGhDelays | Should -Be @(2, 4)
        }

        It 'does not retry a permanent gh failure' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 401: Bad credentials'
                    Stdout = ''
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{"unexpected":true}'
                })

            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'test') `
                    -RetryBaseDelaySeconds 0 `
                    -UtcNowProvider { $global:leakGhNow }
            } | Should -Throw '*failed with exit code 1 after 1 attempt(s)*'

            $global:leakGhAttemptCount | Should -Be 1
            $global:leakGhResponses.Count | Should -Be 1
        }

        It 'does not retry successful empty or invalid JSON responses' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = ''
                })
            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'empty') `
                    -RetryBaseDelaySeconds 0 `
                    -UtcNowProvider { $global:leakGhNow }
            } | Should -Throw '*returned an empty response*'
            $global:leakGhAttemptCount | Should -Be 1

            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{'
                })
            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'invalid') `
                    -RetryBaseDelaySeconds 0 `
                    -UtcNowProvider { $global:leakGhNow }
            } | Should -Throw '*returned invalid JSON*'
            $global:leakGhAttemptCount | Should -Be 2
        }
    }
}

Describe 'shared regular JSON file validation' {
    It 'reads a regular bounded JSON file' {
        $path = Join-Path $TestDrive 'valid.json'
        '{"value":42}' | Set-Content -LiteralPath $path

        (Read-RegularJsonFile -Path $path).value | Should -Be 42
    }

    It 'accepts valid JSON at the exact byte boundary' {
        $path = Join-Path $TestDrive 'exact-boundary.json'
        $content = '{"value":42}'
        $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($content)
        [System.IO.File]::WriteAllBytes($path, $bytes)

        (Read-RegularJsonFile `
                -Path $path `
                -MaximumBytes $bytes.Length).value | Should -Be 42
    }

    It 'rejects an over-bound file before parsing its content' {
        $path = Join-Path $TestDrive 'over-bound.json'
        $content = '{"value":42}'
        $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($content)
        [System.IO.File]::WriteAllBytes($path, $bytes)

        {
            Read-RegularJsonFile `
                -Path $path `
                -MaximumBytes ($bytes.Length - 1)
        } | Should -Throw '*empty or too large*'
    }

    It 'enforces MaximumBytes as bytes for multibyte JSON content' {
        $path = Join-Path $TestDrive 'multibyte.json'
        $content = '{"value":"é"}'
        $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($content)
        [System.IO.File]::WriteAllBytes($path, $bytes)

        {
            Read-RegularJsonFile `
                -Path $path `
                -MaximumBytes $content.Length
        } | Should -Throw '*empty or too large*'
        (Read-RegularJsonFile `
                -Path $path `
                -MaximumBytes $bytes.Length).value | Should -Be 'é'
    }

    It 'rejects missing, zero-byte, whitespace-only, oversized, and invalid JSON files' {
        {
            Read-RegularJsonFile -Path (Join-Path $TestDrive 'missing.json')
        } | Should -Throw '*Required JSON file is missing*'

        $emptyPath = Join-Path $TestDrive 'empty.json'
        [System.IO.File]::WriteAllBytes($emptyPath, [byte[]]::new(0))
        {
            Read-RegularJsonFile -Path $emptyPath
        } | Should -Throw '*empty or too large*'

        $whitespacePath = Join-Path $TestDrive 'whitespace.json'
        Set-Content -LiteralPath $whitespacePath -Value ''
        {
            Read-RegularJsonFile -Path $whitespacePath
        } | Should -Throw '*empty or too large*'

        $oversizedPath = Join-Path $TestDrive 'oversized.json'
        Set-Content -LiteralPath $oversizedPath -Value ('x' * (1MB + 1)) -NoNewline
        {
            Read-RegularJsonFile -Path $oversizedPath
        } | Should -Throw '*empty or too large*'

        $invalidPath = Join-Path $TestDrive 'invalid.json'
        Set-Content -LiteralPath $invalidPath -Value '{'
        {
            Read-RegularJsonFile -Path $invalidPath
        } | Should -Throw '*Invalid JSON*'
    }

    It 'rejects a symbolic-link JSON file' {
        $target = Join-Path $TestDrive 'target.json'
        $link = Join-Path $TestDrive 'link.json'
        '{"value":42}' | Set-Content -LiteralPath $target
        New-Item -ItemType SymbolicLink -Path $link -Target $target | Out-Null

        {
            Read-RegularJsonFile -Path $link
        } | Should -Throw '*Refusing symbolic-link JSON file*'
    }

    It 'is defined only by the shared module used by both gates' {
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw

        (Get-Command Read-RegularJsonFile).ModuleName | Should -Be 'LeakWorkflowDedup'
        $fixGate | Should -Not -Match 'function Read-RegularJsonFile'
        $hunterGate | Should -Not -Match 'function Read-RegularJsonFile'
    }
}

Describe 'merged-reverts JSON driver validation' {
    BeforeAll {
        $script:effectiveRevertsDriver = Join-Path (
            $PSScriptRoot
        ) 'Get-EffectiveRevertedLeakFixes.ps1'
    }

    It 'uses the shared reader with the 128MB ingress bound' {
        $driver = Get-Content -LiteralPath $script:effectiveRevertsDriver -Raw

        $driver | Should -Match '(?s)Read-RegularJsonFile\s+`?\s*-Path \$MergedRevertsJsonPath\s+`?\s*-MaximumBytes 128MB'
        $driver | Should -Not -Match '(?s)Get-Content[^\r\n]*\$MergedRevertsJsonPath'
        $driver | Should -Not -Match 'ConvertFrom-Json'
    }

    It 'accepts a regular bounded merged-reverts file' {
        $fixesPath = Join-Path $TestDrive 'merged-fixes.tsv'
        $revertsPath = Join-Path $TestDrive 'merged-reverts.json'
        $outputPath = Join-Path $TestDrive 'effective-reverts.txt'
        "Type.Member`t100`tmain" | Set-Content -LiteralPath $fixesPath
        '[]' | Set-Content -LiteralPath $revertsPath

        & $script:effectiveRevertsDriver `
            -Repository 'dotnet/maui' `
            -MergedFixTsvPath $fixesPath `
            -MergedRevertsJsonPath $revertsPath `
            -OutputPath $outputPath

        Test-Path -LiteralPath $outputPath -PathType Leaf |
            Should -BeTrue
        @(Get-Content -LiteralPath $outputPath).Count | Should -Be 0
    }

    It 'rejects a merged-reverts file above 128MB before parsing' {
        $fixesPath = Join-Path $TestDrive 'oversized-merged-fixes.tsv'
        $revertsPath = Join-Path $TestDrive 'oversized-merged-reverts.json'
        $outputPath = Join-Path $TestDrive 'oversized-effective-reverts.txt'
        "Type.Member`t100`tmain" | Set-Content -LiteralPath $fixesPath

        $stream = [System.IO.FileStream]::new(
            $revertsPath,
            [System.IO.FileMode]::CreateNew,
            [System.IO.FileAccess]::ReadWrite,
            [System.IO.FileShare]::None
        )
        try {
            $stream.SetLength(128MB + 1)
        } finally {
            $stream.Dispose()
        }

        {
            & $script:effectiveRevertsDriver `
                -Repository 'dotnet/maui' `
                -MergedFixTsvPath $fixesPath `
                -MergedRevertsJsonPath $revertsPath `
                -OutputPath $outputPath
        } | Should -Throw '*JSON file is empty or too large*'
        Test-Path -LiteralPath $outputPath | Should -BeFalse
    }
}

Describe 'authoritative leak-fix branch scope' {
    It 'selects main and inflight/current as one scope while excluding release branches' {
        $selected = @(
            Select-LeakAuthoritativePullRequests `
                -PullRequests @(
                    (New-LeakPr -Number 41 -Title '[leak-fix] Fix First.Api leak' -Base 'main')
                    (New-LeakPr -Number 42 -Title '[leak-fix] Fix Second.Api leak' -Base 'inflight/current')
                    (New-LeakPr -Number 43 -Title '[leak-fix] Fix Third.Api leak' -Base 'release/10.0.1xx-sr9')
                ) `
                -Context 'test branch scope'
        )

        ($selected.number -join ',') | Should -Be '41,42'
    }

    It 'fails closed when baseRefName is missing or malformed' {
        $missing = [pscustomobject]@{
            number = 44
            title = '[leak-fix] Fix Missing.Api leak'
        }
        $malformed = New-LeakPr `
            -Number 45 `
            -Title '[leak-fix] Fix Malformed.Api leak' `
            -Base ' main '

        {
            Select-LeakAuthoritativePullRequests `
                -PullRequests @($missing) `
                -Context 'test branch scope'
        } | Should -Throw '*missing baseRefName*'
        {
            Select-LeakAuthoritativePullRequests `
                -PullRequests @($malformed) `
                -Context 'test branch scope'
        } | Should -Throw '*malformed baseRefName*'
    }
}

Describe 'fresh-shell de-dup state' {
    It 'fails closed when persisted identity does not match the requested PR' {
        $state = [pscustomobject]@{
            issue_number = 42
            api = 'Picker.ItemsSource'
            repository = 'dotnet/maui'
            different_mechanism_prs = @()
        }

        {
            Assert-LeakDedupState `
                -State $state `
                -IssueNumber 43 `
                -Api 'Picker.ItemsSource' `
                -Repository 'dotnet/maui'
        } | Should -Throw '*does not match PR issue*'
    }

    It 'rejects agent-authored different-mechanism overrides' {
        $state = [pscustomobject]@{
            issue_number = 42
            api = 'Picker.ItemsSource'
            repository = 'dotnet/maui'
            different_mechanism_prs = @(
                [pscustomobject]@{ number = 100; basis = 'too short' }
            )
        }

        {
            Assert-LeakDedupState `
                -State $state `
                -IssueNumber 42 `
                -Api 'Picker.ItemsSource' `
                -Repository 'dotnet/maui'
        } | Should -Throw '*do not accept agent-authored different-mechanism overrides*'
    }
}

Describe 'leak PR issue reference parsing' {
    It 'accepts only exact visible same-repository contract lines' -ForEach @(
        @{ Body = 'Fixes #123'; Expected = $true }
        @{ Body = 'Fixes: #123'; Expected = $true }
        @{ Body = 'Fixes dotnet/maui#123'; Expected = $true }
        @{ Body = 'Refs #123'; Expected = $true }
        @{ Body = 'Refs: dotnet/maui#123'; Expected = $true }
        @{ Body = '   Fixes #123'; Expected = $true }
        @{ Body = 'Fixes dotnet/runtime#123'; Expected = $false }
        @{ Body = 'Refs: dotnet/runtime#123'; Expected = $false }
        @{ Body = 'Fixes #123extra'; Expected = $false }
        @{ Body = 'PrefixFixes #123'; Expected = $false }
        @{ Body = 'Fixes `#123`'; Expected = $false }
        @{ Body = '`Fixes #123`'; Expected = $false }
        @{ Body = '<!-- Fixes #123 -->'; Expected = $false }
        @{ Body = "<!-- unclosed comment`nFixes #123"; Expected = $false }
        @{ Body = ('```text' + [Environment]::NewLine + 'Fixes #123' + [Environment]::NewLine + '```'); Expected = $false }
        @{ Body = "~~~text`nRefs: dotnet/maui#123`n~~~"; Expected = $false }
        @{ Body = ('````text' + [Environment]::NewLine + 'Fixes #123' + [Environment]::NewLine + '```' + [Environment]::NewLine + 'Fixes #123'); Expected = $false }
        @{ Body = ('````text' + [Environment]::NewLine + 'Fixes #123' + [Environment]::NewLine + '````' + [Environment]::NewLine + 'Fixes #123'); Expected = $true }
        @{ Body = '    Fixes #123'; Expected = $false }
        @{ Body = "`tFixes #123"; Expected = $false }
        @{ Body = "Fixes`t#123"; Expected = $false }
        @{ Body = 'Closes #123'; Expected = $false }
    ) {
        Test-LeakPrReferencesIssue `
            -Body $Body `
            -IssueNumber 123 `
            -Repository 'dotnet/maui' |
            Should -Be $Expected
    }
}

Describe 'leak-fix safe-output provenance parsing' {
    It 'accepts one visible canonical pair with supported indentation and newlines' -ForEach @(
        @{
            Body = "Fixes #123`nRefs: dotnet/maui#123"
        }
        @{
            Body = "   Fixes #123   `n  Refs: dotnet/maui#123 "
        }
        @{
            Body = "Fixes #123`r`nRefs: dotnet/maui#123`r`n"
        }
    ) {
        Get-LeakFixProvenanceIssueNumber `
            -Body $Body `
            -Repository 'dotnet/maui' |
            Should -Be 123
    }

    It 'ignores hidden duplicates when exactly one canonical pair is visible' {
        $body = @'
```markdown
Fixes #123
Refs: dotnet/maui#123
```
<!--
Fixes #123
Refs: dotnet/maui#123
-->
Fixes #123
Refs: dotnet/maui#123
'@

        Get-LeakFixProvenanceIssueNumber `
            -Body $body `
            -Repository 'dotnet/maui' |
            Should -Be 123
    }

    It 'does not accept provenance that exists only in inert Markdown' -ForEach @(
        @{
            Body = @'
```powershell
Fixes #123
Refs: dotnet/maui#123
```
'@
        }
        @{
            Body = @'
<!--
Fixes #123
Refs: dotnet/maui#123
-->
'@
        }
        @{
            Body = @'
~~~markdown
Fixes #123
Refs: dotnet/maui#123
~~~
'@
        }
    ) {
        {
            Get-LeakFixProvenanceIssueNumber `
                -Body $Body `
                -Repository 'dotnet/maui'
        } | Should -Throw '*exactly one visible canonical Fixes line*'
    }

    It 'rejects tab and four-space indentation at either contract line' -ForEach @(
        @{
            Body = "`tFixes #123`nRefs: dotnet/maui#123"
            Message = '*canonical Fixes line*'
        }
        @{
            Body = "    Fixes #123`nRefs: dotnet/maui#123"
            Message = '*canonical Fixes line*'
        }
        @{
            Body = "Fixes #123`n`tRefs: dotnet/maui#123"
            Message = '*canonical Refs line*'
        }
        @{
            Body = "Fixes #123`n    Refs: dotnet/maui#123"
            Message = '*canonical Refs line*'
        }
    ) {
        {
            Get-LeakFixProvenanceIssueNumber `
                -Body $Body `
                -Repository 'dotnet/maui'
        } | Should -Throw $Message
    }

    It 'fails closed when fence or HTML comment state is malformed or unclosed' -ForEach @(
        @{
            Body = @'
Fixes #123
Refs: dotnet/maui#123
```text
unfinished
'@
            Message = '*unclosed fenced code block*'
        }
        @{
            Body = @'
Fixes #123
Refs: dotnet/maui#123
<!-- unfinished
'@
            Message = '*unclosed HTML comment*'
        }
        @{
            Body = "Fixes #123`nRefs: dotnet/maui#123`n--> stray closer"
            Message = '*malformed HTML comment delimiter*'
        }
        @{
            Body = "Fixes #123`nRefs: dotnet/maui#123`n<!-- outer <!-- nested -->"
            Message = '*malformed nested HTML comment*'
        }
    ) {
        {
            Get-LeakFixProvenanceIssueNumber `
                -Body $Body `
                -Repository 'dotnet/maui'
        } | Should -Throw $Message
    }

    It 'rejects ambiguous Fixes and mismatched or duplicate target Refs' -ForEach @(
        @{
            Body = "Fixes #123`nFixes #124`nRefs: dotnet/maui#123"
            Message = '*canonical Fixes line*'
        }
        @{
            Body = "Fixes #123`nRefs: dotnet/maui#124"
            Message = '*canonical Refs line*'
        }
        @{
            Body = "Fixes #123`nRefs: dotnet/maui#123`nRefs: dotnet/maui#123"
            Message = '*canonical Refs line*'
        }
        @{
            Body = "Fixes #123`nRefs:dotnet/maui#123"
            Message = '*canonical Refs line*'
        }
        @{
            Body = "Fixes #123`nrefs: dotnet/maui#123"
            Message = '*canonical Refs line*'
        }
        @{
            Body = "Fixes #123`nRefs: DotNet/Maui#123"
            Message = '*canonical Refs line*'
        }
        @{
            Body = "fixes #123`nRefs: dotnet/maui#123"
            Message = '*canonical Fixes line*'
        }
        @{
            Body = "Fixes  #123`nRefs: dotnet/maui#123"
            Message = '*canonical Fixes line*'
        }
        @{
            Body = "Fixes #123 trailing`nRefs: dotnet/maui#123"
            Message = '*canonical Fixes line*'
        }
        @{
            Body = "Fixes #123`nRefs: dotnet/maui#123 trailing"
            Message = '*canonical Refs line*'
        }
    ) {
        {
            Get-LeakFixProvenanceIssueNumber `
                -Body $Body `
                -Repository 'dotnet/maui'
        } | Should -Throw $Message
    }

    It 'allows an unrelated visible Refs citation in addition to the target pair' {
        $body = "Fixes #123`nRefs: dotnet/maui#123`nRefs: dotnet/maui#501"

        Get-LeakFixProvenanceIssueNumber `
            -Body $body `
            -Repository 'dotnet/maui' |
            Should -Be 123
    }
}

Describe 'canonical leak API title parsing' {
    It 'extracts the API only from the anchored leak-scan title position' {
        Get-CanonicalLeakApi `
            -Title '[leak-scan] Microsoft.Maui.Controls.Picker.ItemsSource — collection retention' |
            Should -Be 'Picker.ItemsSource'
    }

    It 'extracts the API only from the anchored leak-fix title position' {
        Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Microsoft.Maui.Controls.Picker.ItemsSource memory leak' |
            Should -Be 'Picker.ItemsSource'
    }

    It 'accepts tabs wherever the title grammar permits prefix whitespace' {
        Get-CanonicalLeakApi `
            -Title "[leak-scan]`tMicrosoft.Maui.Controls.Picker.ItemsSource — retention" |
            Should -Be 'Picker.ItemsSource'
        Get-CanonicalLeakApi `
            -Title "[leak-fix]`tFix`tPicker.ItemsSource memory leak" |
            Should -Be 'Picker.ItemsSource'
        Get-CanonicalExistingLeakApi `
            -Title "[leak-fix]`tFix`tShell`tBackButtonBehavior.Command leak" |
            Should -Be 'BackButtonBehavior.Command'
    }

    It 'accepts exact existing tags with space and tab boundaries' -ForEach @(
        @{
            Kind = 'Scan'
            Title = '[leak-scan] Picker.ItemsSource retention'
            Expected = 'Picker.ItemsSource'
        }
        @{
            Kind = 'Scan'
            Title = "[leak-scan]`tPicker.ItemsSource retention"
            Expected = 'Picker.ItemsSource'
        }
        @{
            Kind = 'Fix'
            Title = '[leak-fix] Fix Picker.ItemsSource retention'
            Expected = 'Picker.ItemsSource'
        }
        @{
            Kind = 'Fix'
            Title = "[leak-fix]`tFix`tPicker.ItemsSource retention"
            Expected = 'Picker.ItemsSource'
        }
    ) {
        Get-ValidatedExistingLeakApi `
            -Title $Title `
            -Kind $Kind `
            -Context 'Existing item' |
            Should -BeExactly $Expected
    }

    It 'ignores unrelated near-prefix existing tags' -ForEach @(
        @{ Kind = 'Scan'; Title = '[leak-scanx] tracking' }
        @{ Kind = 'Scan'; Title = '[leak-scanner] tracking' }
        @{ Kind = 'Scan'; Title = '[LEAK-SCANX] tracking' }
        @{ Kind = 'Fix'; Title = '[leak-fixx] tracking' }
        @{ Kind = 'Fix'; Title = '[leak-fixer] tracking' }
        @{ Kind = 'Fix'; Title = '[LEAK-FIXX] tracking' }
    ) {
        Get-ValidatedExistingLeakApi `
            -Title $Title `
            -Kind $Kind `
            -Context 'Existing item' |
            Should -BeNullOrEmpty
    }

    It 'fails closed after recognizing a malformed complete existing tag' -ForEach @(
        @{
            Kind = 'Scan'
            Title = '[leak-scan]Picker.ItemsSource retention'
            Message = '*malformed or ambiguous*title prefix*'
        }
        @{
            Kind = 'Scan'
            Title = '[leak-scan]x Picker.ItemsSource retention'
            Message = '*malformed or ambiguous*title prefix*'
        }
        @{
            Kind = 'Scan'
            Title = '[LEAK-SCAN] Picker.ItemsSource retention'
            Message = '*malformed or ambiguous*title prefix*'
        }
        @{
            Kind = 'Scan'
            Title = '[leak-scan] Investigate Picker.ItemsSource'
            Message = '*malformed*title without a canonical API*'
        }
        @{
            Kind = 'Fix'
            Title = '[leak-fix]Fix Picker.ItemsSource retention'
            Message = '*malformed or ambiguous*title prefix*'
        }
        @{
            Kind = 'Fix'
            Title = '[LEAK-FIX] Fix Picker.ItemsSource retention'
            Message = '*malformed or ambiguous*title prefix*'
        }
        @{
            Kind = 'Fix'
            Title = '[leak-fix] Picker.ItemsSource retention'
            Message = '*malformed*title without a canonical API*'
        }
    ) {
        {
            Get-ValidatedExistingLeakApi `
                -Title $Title `
                -Kind $Kind `
                -Context 'Existing item'
        } | Should -Throw $Message
    }

    It 'rejects missing-whitespace and near-prefix variants' {
        @(
            '[leak-scan]Picker.ItemsSource retention'
            '[leak-scan]x Picker.ItemsSource retention'
            '[leak-scanx] Picker.ItemsSource retention'
            '[leak-fix]Fix Picker.ItemsSource retention'
            '[leak-fix]x Fix Picker.ItemsSource retention'
            '[leak-fixx] Fix Picker.ItemsSource retention'
        ) | ForEach-Object {
            (Get-CanonicalLeakApi -Title $_) | Should -BeNullOrEmpty
            (Get-CanonicalExistingLeakApi -Title $_) | Should -BeNullOrEmpty
        }
    }

    It 'keeps short canonical-helper inputs safe without changing valid semantics' {
        $module = Get-Module LeakWorkflowDedup

        (& $module {
                ConvertTo-CanonicalLeakApi -Api 'Picker'
            }) | Should -Be 'Picker'
        (& $module {
                ConvertTo-CanonicalLeakApi -Api '.'
            }) | Should -Be '.'
    }

    It 'returns no API for empty, whitespace-only, or malformed public title inputs' {
        @(
            ''
            '   '
            '[leak-scan]'
            '[leak-fix]'
            '[leak-fix] Fix Picker'
            '[leak-scan] .ItemsSource'
        ) | ForEach-Object {
            (Get-CanonicalLeakApi -Title $_) | Should -BeNullOrEmpty
            (Get-CanonicalExistingLeakApi -Title $_) | Should -BeNullOrEmpty
        }
    }

    It 'accepts supported punctuation immediately after the anchored API' {
        @(
            '[leak-fix] Fix Picker.ItemsSource, clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource: clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource-clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource–clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource—clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource(clear stale subscriptions)'
            '[leak-fix] Fix Picker.ItemsSource.'
        ) | ForEach-Object {
            Get-CanonicalLeakApi -Title $_ | Should -Be 'Picker.ItemsSource'
        }
    }

    It 'preserves non-MAUI qualification to prevent namespace collisions' {
        $foo = Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Foo.Bar.CollectionView.ItemsSource leak'
        $baz = Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Baz.Qux.CollectionView.ItemsSource leak'

        $foo | Should -Be 'Foo.Bar.CollectionView.ItemsSource'
        $baz | Should -Be 'Baz.Qux.CollectionView.ItemsSource'
        $foo | Should -Not -Be $baz
    }

    It 'keeps short and legacy Microsoft.Maui-qualified keys stable' {
        Get-CanonicalLeakApi -Title '[leak-fix] Fix Picker.ItemsSource leak' |
            Should -Be 'Picker.ItemsSource'
        Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Microsoft.Maui.Controls.Picker.ItemsSource leak' |
            Should -Be 'Picker.ItemsSource'
    }

    It 'rejects a URL before an otherwise valid API' {
        (Get-CanonicalLeakApi `
                -Title '[leak-fix] Investigate https://github.com/dotnet/maui/issues/123 for Picker.ItemsSource') |
            Should -BeNullOrEmpty
    }

    It 'rejects an earlier namespace token in a malformed title' {
        (Get-CanonicalLeakApi `
                -Title '[leak-fix] Investigate Microsoft.Maui.Controls before Picker.ItemsSource') |
            Should -BeNullOrEmpty
    }

    It 'rejects tagged titles that do not follow the expected title grammar' {
        (Get-CanonicalLeakApi -Title '[leak-fix] Picker.ItemsSource memory leak') |
            Should -BeNullOrEmpty
        (Get-CanonicalLeakApi -Title '[leak-scan] Investigate Picker.ItemsSource retention') |
            Should -BeNullOrEmpty
        (Get-CanonicalLeakApi -Title '[leak-fix] Fix Picker.ItemsSource/Other retention') |
            Should -BeNullOrEmpty
    }

    It 'keeps legacy compatibility out of strict new-output parsing' {
        (Get-CanonicalLeakApi `
                -Title '[leak-scan] Shell BackButtonBehavior.Command leaks via ICommand') |
            Should -BeNullOrEmpty
        (Get-CanonicalLeakApi `
                -Title '[leak-fix] Fix Shell BackButtonBehavior.Command memory leak') |
            Should -BeNullOrEmpty
    }

    It 'recognizes the known Shell prefix only for existing issue and fix titles' {
        Get-CanonicalExistingLeakApi `
            -Title '[leak-scan] Shell BackButtonBehavior.Command leaks via ICommand' |
            Should -Be 'BackButtonBehavior.Command'
        Get-CanonicalExistingLeakApi `
            -Title '[leak-fix] Fix Shell BackButtonBehavior.Command memory leak' |
            Should -Be 'BackButtonBehavior.Command'
    }

    It 'does not scan URLs or arbitrary later identifiers in existing titles' {
        @(
            '[leak-scan] Investigate BackButtonBehavior.Command retention'
            '[leak-scan] Shell investigate BackButtonBehavior.Command retention'
            '[leak-scan] Shell https://github.com/dotnet/maui/issues/36345 BackButtonBehavior.Command'
            '[leak-fix] Fix Shell details at https://example.test/BackButtonBehavior.Command'
        ) | ForEach-Object {
            (Get-CanonicalExistingLeakApi -Title $_) | Should -BeNullOrEmpty
        }
    }
}

Describe 'memory leak workflow schedule synchronization' {
    BeforeAll {
        function Get-FriendlyScheduleHours {
            param([Parameter(Mandatory = $true)][string]$Path)

            $content = (Get-Content -LiteralPath $Path -Raw) -replace "`r`n", "`n"
            $frontmatter = [regex]::Match(
                $content,
                '(?s)\A---\n(?<value>.*?)\n---(?:\n|\z)')
            if (-not $frontmatter.Success) {
                throw "Workflow frontmatter is missing from $Path."
            }

            $schedule = [regex]::Match(
                $frontmatter.Groups['value'].Value,
                '(?m)^[ ]{2}schedule:\s*every\s+(?<hours>[1-9][0-9]*)h\s*$')
            if (-not $schedule.Success) {
                throw "Unsupported or missing friendly schedule in $Path."
            }

            return [int]$schedule.Groups['hours'].Value
        }

        function Get-CompiledCronIntervalHours {
            param([Parameter(Mandatory = $true)][string]$Path)

            $lock = Get-Content -LiteralPath $Path -Raw
            $cron = [regex]::Match(
                $lock,
                '(?ms)^on:\s*$.*?^\s+schedule:\s*$.*?^\s+-\s+cron:\s+["'']?(?<value>[^"'']+?)["'']?\s*(?:#.*)?$')
            if (-not $cron.Success) {
                throw "Compiled lock $Path has no scheduled cron trigger."
            }

            $parts = $cron.Groups['value'].Value.Trim() -split '\s+'
            if ($parts.Count -ne 5) {
                throw "Unsupported cron '$($cron.Groups['value'].Value)' in $Path."
            }

            $minute, $hour, $day, $month, $dayOfWeek = $parts
            if ($minute -notmatch '^(?:[0-5]?[0-9])$' -or
                $day -ne '*' -or $month -ne '*' -or $dayOfWeek -ne '*') {
                throw "Cron '$($parts -join ' ')' is not a fixed-minute hourly cadence."
            }

            if ($hour -match '^\*/(?<step>[1-9][0-9]*)$') {
                $step = [int]$Matches.step
                if (24 % $step -ne 0) {
                    throw "Cron hour step '$hour' does not divide a day evenly."
                }
                $hours = @(for ($value = 0; $value -lt 24; $value += $step) { $value })
            }
            elseif ($hour -match '^[0-9]+(?:,[0-9]+)+$') {
                $hours = @($hour.Split(',') | ForEach-Object { [int]$_ } | Sort-Object -Unique)
                if ($hours[0] -lt 0 -or $hours[-1] -gt 23) {
                    throw "Cron hour list '$hour' is outside 0-23."
                }
            }
            else {
                throw "Unsupported cron hour field '$hour'."
            }

            $gaps = for ($index = 0; $index -lt $hours.Count; $index++) {
                $next = if ($index -eq $hours.Count - 1) {
                    $hours[0] + 24
                }
                else {
                    $hours[$index + 1]
                }
                $next - $hours[$index]
            }
            $distinctGaps = @($gaps | Sort-Object -Unique)
            if ($distinctGaps.Count -ne 1) {
                throw "Cron hour field '$hour' is not a uniform cadence."
            }

            return [int]$distinctGaps[0]
        }
    }

    It 'keeps friendly source schedules aligned with compiled cron cadences' -ForEach @(
        @{ Workflow = 'leak-fixer.md'; Lock = 'leak-fixer.lock.yml' }
        @{ Workflow = 'daily-leak-hunter.md'; Lock = 'daily-leak-hunter.lock.yml' }
    ) {
        $workflowRoot = Join-Path $PSScriptRoot '../workflows'
        Get-CompiledCronIntervalHours -Path (Join-Path $workflowRoot $Lock) |
            Should -Be (Get-FriendlyScheduleHours -Path (Join-Path $workflowRoot $Workflow))
    }
}

Describe 'trusted final duplicate gate' {
    It 'conservatively blocks same-API matches without an independent override' {
        $existing = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
            -Body 'Fixes #10'

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()

        $result.Blocked | Should -BeTrue
        $result.ApiMatches.number | Should -Be 100
    }

    It 'uses ordinal API identity so exact C# casing dedups while casing-only identifiers remain distinct' {
        $existing = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak'

        $exact = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()
        $caseVariant = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.gradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()

        $exact.Blocked | Should -BeTrue
        $caseVariant.Blocked | Should -BeFalse
    }

    It 'blocks the known legacy form when it appears on an existing fix' {
        $existing = New-LeakPr `
            -Number 104 `
            -Title '[leak-fix] Fix Shell BackButtonBehavior.Command memory leak'

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'BackButtonBehavior.Command' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()

        $result.Blocked | Should -BeTrue
        $result.ApiMatches.number | Should -Be 104
    }

    It 'blocks a same-API PR that appeared after Step 3' {
        $newOpen = New-LeakPr `
            -Number 101 `
            -Title '[leak-fix] Fix Microsoft.Maui.Controls.GradientBrush.GradientStops reset leak' `
            -Merged $false

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @() `
            -OpenPullRequests @($newOpen)

        $result.Blocked | Should -BeTrue
        $result.ApiMatches.number | Should -Be 101
    }

    It 'ignores a same-API open PR targeting an unrelated release branch' {
        $releaseOpen = New-LeakPr `
            -Number 103 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
            -Body "Fixes #20`nRefs: dotnet/maui#20" `
            -Base 'release/10.0.1xx-sr9' `
            -Merged $false

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @() `
            -OpenPullRequests @($releaseOpen)

        $result.Blocked | Should -BeFalse
        $result.DirectMatches.Count | Should -Be 0
        $result.ApiMatches.Count | Should -Be 0
    }

    It 'always blocks a direct issue reference' {
        $direct = New-LeakPr `
            -Number 102 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
            -Body "Fixes #20`nRefs: dotnet/maui#20"

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($direct) `
            -OpenPullRequests @()

        $result.Blocked | Should -BeTrue
        $result.DirectMatches.number | Should -Be 102
    }

    It 'does not treat an effectively reverted merged fix as a duplicate' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Restore collection cleanup' `
            -Body "Fixes #20`nRefs: dotnet/maui#20"
        $revert = New-LeakPr `
            -Number 200 `
            -Title 'Revert leak fix' `
            -Body 'Reverts dotnet/maui#100' `
            -VerifiedRevertTargets @(100)

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($fix) `
            -OpenPullRequests @() `
            -MergedRevertPullRequests @($revert)

        $result.Blocked | Should -BeFalse
        $result.EffectivelyReverted | Should -Be @(100)
    }

    It 'blocks a cyclic fix conservatively while still resolving unrelated candidates' {
        $cyclicFix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Body 'Fixes #10'
        $unrelatedFix = New-LeakPr `
            -Number 110 `
            -Title '[leak-fix] Fix ListView.RefreshCommand leak' `
            -Body 'Fixes #11'
        $reverts = @(
            New-LeakPr `
                -Number 200 `
                -Title 'Revert Picker fix and cyclic peer' `
                -Body "Reverts #100`nReverts #300" `
                -VerifiedRevertTargets @(100, 300)
            New-LeakPr `
                -Number 300 `
                -Title 'Revert cyclic peer' `
                -Body 'Reverts #200' `
                -VerifiedRevertTargets @(200)
            New-LeakPr `
                -Number 210 `
                -Title 'Revert unrelated fix' `
                -Body 'Reverts #110' `
                -VerifiedRevertTargets @(110)
        )

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'Picker.ItemsSource' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($cyclicFix, $unrelatedFix) `
            -OpenPullRequests @() `
            -MergedRevertPullRequests $reverts

        $result.Blocked | Should -BeTrue
        $result.ApiMatches.number | Should -Be 100
        $result.EffectivelyReverted | Should -Be @(110)
    }

    It 'honors a definite terminal reverter alongside a cycle-entangled sibling' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Body 'Fixes #10'
        $reverts = @(
            New-LeakPr `
                -Number 200 `
                -Title 'Cycle-entangled sibling' `
                -Body "Reverts #100`nReverts #300" `
                -VerifiedRevertTargets @(100, 300)
            New-LeakPr `
                -Number 300 `
                -Title 'Cycle peer' `
                -Body 'Reverts #200' `
                -VerifiedRevertTargets @(200)
            New-LeakPr `
                -Number 201 `
                -Title 'Definite terminal sibling' `
                -Body 'Reverts #100' `
                -VerifiedRevertTargets @(100)
        )

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'Picker.ItemsSource' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($fix) `
            -OpenPullRequests @() `
            -MergedRevertPullRequests $reverts

        $result.Blocked | Should -BeFalse
        $result.EffectivelyReverted | Should -Be @(100)
    }
}

Describe 'effective recursive revert state' {
    It 'checks the traversal stack before memo lookup without caching ambiguity' {
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw
        $function = [regex]::Match(
            $module,
            '(?s)function Get-EffectiveRevertedPullRequestNumbers \{.*?' +
                '(?=\n\}\n\nExport-ModuleMember)'
        ).Value
        $cycleIndex = $function.IndexOf(
            '$Visiting.Contains($PullRequestNumber)'
        )
        $memoIndex = $function.IndexOf(
            '$memo.ContainsKey($PullRequestNumber)'
        )

        $cycleIndex | Should -BeGreaterOrEqual 0
        $memoIndex | Should -BeGreaterThan $cycleIndex
        $function | Should -Not -Match (
            '\$memo\[\$PullRequestNumber\]\s*=\s*\$ambiguousState'
        )
    }

    It 'fails closed when a shared diamond over a cycle exhausts root work' {
        $graph = Get-AmbiguousDiamondRevertGraph -Layers 12

        {
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($graph.Fix) `
                -MergedRevertPullRequests $graph.Reverts `
                -MaximumRootTraversalEvaluations 256 `
                -MaximumAggregateTraversalEvaluations 1000 `
                -MaximumTraversalDepth 32
        } | Should -Throw (
            '*256-evaluation per-root effective-revert traversal safety budget*' +
                'root PR #100*refusing to guess active/reverted state*'
        )
    }

    It 'honors the exact root and aggregate evaluation boundary' {
        # Four shared diamond layers over the two-node cycle require exactly
        # 2^(4 + 3) - 1 = 127 state evaluations.
        $graph = Get-AmbiguousDiamondRevertGraph -Layers 4

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($graph.Fix) `
                -MergedRevertPullRequests $graph.Reverts `
                -MaximumRootTraversalEvaluations 127 `
                -MaximumAggregateTraversalEvaluations 127 `
                -MaximumTraversalDepth 16
        ).Count | Should -Be 0

        {
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($graph.Fix) `
                -MergedRevertPullRequests $graph.Reverts `
                -MaximumRootTraversalEvaluations 126 `
                -MaximumAggregateTraversalEvaluations 1000 `
                -MaximumTraversalDepth 16
        } | Should -Throw '*126-evaluation per-root*root PR #100*'
    }

    It 'completes a shared ambiguous diamond while it remains under budget' {
        $graph = Get-AmbiguousDiamondRevertGraph -Layers 6

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($graph.Fix) `
                -MergedRevertPullRequests $graph.Reverts `
                -MaximumRootTraversalEvaluations 600 `
                -MaximumAggregateTraversalEvaluations 600 `
                -MaximumTraversalDepth 16
        ).Count | Should -Be 0
    }

    It 'applies aggregate work across many independently bounded roots' {
        $fixes = [System.Collections.Generic.List[object]]::new()
        $reverts = [System.Collections.Generic.List[object]]::new()
        foreach ($offset in 0..3) {
            $root = 100 + $offset
            $middle = 200 + $offset
            $terminal = 300 + $offset
            $fixes.Add((New-LeakPr `
                        -Number $root `
                        -Title "[leak-fix] Fix Type$root.Member leak"))
            $reverts.Add((New-LeakPr `
                        -Number $middle `
                        -Title "Revert root $root" `
                        -VerifiedRevertTargets @($root)))
            $reverts.Add((New-LeakPr `
                        -Number $terminal `
                        -Title "Restore root $root" `
                        -VerifiedRevertTargets @($middle)))
        }

        {
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fixes) `
                -MergedRevertPullRequests @($reverts) `
                -MaximumRootTraversalEvaluations 3 `
                -MaximumAggregateTraversalEvaluations 11 `
                -MaximumTraversalDepth 4
        } | Should -Throw (
            '*11-evaluation aggregate effective-revert traversal safety budget*' +
                'root PR #103*'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fixes) `
                -MergedRevertPullRequests @($reverts) `
                -MaximumRootTraversalEvaluations 3 `
                -MaximumAggregateTraversalEvaluations 12 `
                -MaximumTraversalDepth 4
        ).Count | Should -Be 0
    }

    It 'exhausts the same deterministic budget regardless of input order' {
        $graph = Get-AmbiguousDiamondRevertGraph -Layers 8
        $reversed = @($graph.Reverts)
        [array]::Reverse($reversed)
        $orders = @(
            [pscustomobject]@{ Items = @($graph.Reverts) }
            [pscustomobject]@{ Items = $reversed }
        )
        $messages = @(
            foreach ($order in $orders) {
                try {
                    Get-EffectiveRevertedPullRequestNumbers `
                        -Repository 'dotnet/maui' `
                        -FixPullRequests @($graph.Fix) `
                        -MergedRevertPullRequests $order.Items `
                        -MaximumRootTraversalEvaluations 128 `
                        -MaximumAggregateTraversalEvaluations 1000 `
                        -MaximumTraversalDepth 32
                    'unexpected completion'
                } catch {
                    $_.Exception.Message
                }
            }
        )

        $messages[0] | Should -Match '128-evaluation per-root'
        $messages[1] | Should -BeExactly $messages[0]
    }

    It 'fails closed before a deep acyclic chain can exhaust the call stack' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Type100.Member leak'
        $reverts = [System.Collections.Generic.List[object]]::new()
        $target = 100
        foreach ($number in 200..204) {
            $reverts.Add((New-LeakPr `
                        -Number $number `
                        -Title "Chain node $number" `
                        -VerifiedRevertTargets @($target)))
            $target = $number
        }

        {
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests @($reverts) `
                -MaximumRootTraversalEvaluations 20 `
                -MaximumAggregateTraversalEvaluations 20 `
                -MaximumTraversalDepth 5
        } | Should -Throw (
            '*maximum effective-revert traversal depth of 5*' +
                'root PR #100*refusing to guess active/reverted state*'
        )
    }

    It 'completes a normal large acyclic revert graph within trusted defaults' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Type100.Member leak'
        $reverts = [System.Collections.Generic.List[object]]::new()
        foreach ($offset in 0..499) {
            $branch = 1000 + ($offset * 2)
            $terminal = $branch + 1
            $reverts.Add((New-LeakPr `
                        -Number $branch `
                        -Title "Large DAG branch $offset" `
                        -VerifiedRevertTargets @(100)))
            $reverts.Add((New-LeakPr `
                        -Number $terminal `
                        -Title "Large DAG terminal $offset" `
                        -VerifiedRevertTargets @($branch)))
        }

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests @($reverts)
        ).Count | Should -Be 0
    }

    It 'resolves intersecting roots independently of fix and revert input order' {
        $first = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Type100.Member leak' `
            -VerifiedRevertTargets @(300)
        $second = New-LeakPr `
            -Number 200 `
            -Title '[leak-fix] Fix Type200.Member leak' `
            -VerifiedRevertTargets @(100)
        $cyclePeer = New-LeakPr `
            -Number 300 `
            -Title 'Cycle peer' `
            -VerifiedRevertTargets @(200)
        $terminal = New-LeakPr `
            -Number 400 `
            -Title 'Terminal reverter' `
            -VerifiedRevertTargets @(100)
        $fixOrders = @(
            [pscustomobject]@{ First = $first; Second = $second }
            [pscustomobject]@{ First = $second; Second = $first }
        )
        $revertOrders = @(
            [pscustomobject]@{
                Items = @($first, $second, $cyclePeer, $terminal)
            }
            [pscustomobject]@{
                Items = @($terminal, $cyclePeer, $second, $first)
            }
        )

        foreach ($fixOrder in $fixOrders) {
            foreach ($revertOrder in $revertOrders) {
                @(
                    Get-EffectiveRevertedPullRequestNumbers `
                        -Repository 'dotnet/maui' `
                        -FixPullRequests @(
                            $fixOrder.First,
                            $fixOrder.Second
                        ) `
                        -MergedRevertPullRequests $revertOrder.Items
                ) | Should -Be @(100, 200)
            }
        }
    }

    It 'reuses definitive states across roots that share DAG descendants' {
        $first = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Type100.Member leak'
        $second = New-LeakPr `
            -Number 110 `
            -Title '[leak-fix] Fix Type110.Member leak'
        $left = New-LeakPr `
            -Number 200 `
            -Title 'Left branch' `
            -VerifiedRevertTargets @(100)
        $shared = New-LeakPr `
            -Number 300 `
            -Title 'Shared branch' `
            -VerifiedRevertTargets @(100, 110)
        $sharedTerminal = New-LeakPr `
            -Number 400 `
            -Title 'Shared terminal' `
            -VerifiedRevertTargets @(200, 300)
        $directTerminal = New-LeakPr `
            -Number 500 `
            -Title 'Direct terminal' `
            -VerifiedRevertTargets @(110)
        $reverts = @(
            $left,
            $shared,
            $sharedTerminal,
            $directTerminal
        )

        foreach ($fixes in @(
                [pscustomobject]@{ First = $first; Second = $second }
                [pscustomobject]@{ First = $second; Second = $first }
            )) {
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fixes.First, $fixes.Second) `
                -MergedRevertPullRequests $reverts |
                Should -Be @(110)
        }
    }

    It 'keeps an isolated revert cycle ambiguous and therefore active' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $first = New-LeakPr `
            -Number 200 `
            -Title 'First cycle peer' `
            -VerifiedRevertTargets @(100, 300)
        $second = New-LeakPr `
            -Number 300 `
            -Title 'Second cycle peer' `
            -VerifiedRevertTargets @(200)

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests @($first, $second)
        ).Count | Should -Be 0
    }

    It 'keeps an unreverted fix active' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests @()
        ).Count | Should -Be 0
    }

    It 'excludes a fix after one active revert' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' `
                -Body 'Reverts dotnet/maui#100' -VerifiedRevertTargets @(100)
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'does not trust a repository-local body reference without immutable proof' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body 'Reverts #100'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'does not trust formatted editable body prose without immutable proof' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body '> - **Reverts #100**'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'rejects a revert reference qualified to another repository' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert unrelated fix' -Body 'Reverts dotnet/runtime#100'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'reinstates a fix after its revert is itself reverted' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' `
                -Body 'Reverts dotnet/maui#100' -VerifiedRevertTargets @(100)
            New-LeakPr -Number 300 -Title 'Revert the revert' `
                -Body 'Reverts dotnet/maui#200' -VerifiedRevertTargets @(200)
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'handles a deeper odd effective chain' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' `
                -Body 'Reverts dotnet/maui#100' -VerifiedRevertTargets @(100)
            New-LeakPr -Number 300 -Title 'Revert A again' `
                -Body 'Reverts dotnet/maui#200' -VerifiedRevertTargets @(200)
            New-LeakPr -Number 400 -Title 'Revert A re-revert' `
                -Body 'Reverts dotnet/maui#300' -VerifiedRevertTargets @(300)
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'keeps a fix reverted when multiple independent sibling reverts remain active' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' `
                -Body 'Reverts dotnet/maui#100' -VerifiedRevertTargets @(100)
            New-LeakPr -Number 201 -Title 'Revert B' `
                -Body 'Reverts dotnet/maui#100' -VerifiedRevertTargets @(100)
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ) | Should -Be @(100)
    }

    It 'keeps a fix reverted while any independent sibling revert remains active' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' `
                -Body 'Reverts dotnet/maui#100' -VerifiedRevertTargets @(100)
            New-LeakPr -Number 201 -Title 'Revert B' `
                -Body 'Reverts dotnet/maui#100' -VerifiedRevertTargets @(100)
            New-LeakPr -Number 300 -Title 'Restore only A' `
                -Body 'Reverts dotnet/maui#200' -VerifiedRevertTargets @(200)
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'ignores a servicing-branch revert of a main fix' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Base main
        $releaseRevert = New-LeakPr `
            -Number 200 `
            -Title 'Revert leak fix for servicing' `
            -Body 'Reverts dotnet/maui#100' `
            -Base 'release/10.0.1xx-sr9' `
            -VerifiedRevertTargets @(100)

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests @($releaseRevert)
        ).Count | Should -Be 0
    }

    It 'scopes main and inflight revert chains independently' {
        $mainFix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Base main
        $inflightFix = New-LeakPr `
            -Number 110 `
            -Title '[leak-fix] Fix ListView.RefreshCommand leak' `
            -Base 'inflight/current'
        $reverts = @(
            New-LeakPr `
                -Number 200 `
                -Title 'Revert main fix' `
                -Body 'Reverts dotnet/maui#100' `
                -Base main `
                -VerifiedRevertTargets @(100)
            New-LeakPr `
                -Number 210 `
                -Title 'Unrelated main revert of inflight PR number' `
                -Body 'Reverts dotnet/maui#110' `
                -Base main `
                -VerifiedRevertTargets @(110)
            New-LeakPr `
                -Number 220 `
                -Title 'Revert inflight fix' `
                -Body 'Reverts dotnet/maui#110' `
                -Base 'inflight/current' `
                -VerifiedRevertTargets @(110)
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($mainFix, $inflightFix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100, 110)
    }
}

Describe 'required GraphQL property shapes' {
    It 'enumerates zero, one, and two array elements without nesting' {
        $module = Get-Module LeakWorkflowDedup
        $sample = @'
{
  "empty": [],
  "one": [{ "number": 1 }],
  "two": [{ "number": 1 }, { "number": 2 }],
  "scalar": 42,
  "object": { "number": 9 }
}
'@ | ConvertFrom-Json

        $empty = @(& $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'empty' `
                    -Context 'Test value' `
                    -RequireArray
            } $sample)
        $one = @(& $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'one' `
                    -Context 'Test value' `
                    -RequireArray
            } $sample)
        $two = @(& $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'two' `
                    -Context 'Test value' `
                    -RequireArray
            } $sample)
        $scalar = @(& $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'scalar' `
                    -Context 'Test value'
            } $sample)
        $object = @(& $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'object' `
                    -Context 'Test value'
            } $sample)

        $empty.Count | Should -Be 0
        $one.Count | Should -Be 1
        $one[0].number | Should -Be 1
        $two.Count | Should -Be 2
        $two.number | Should -Be @(1, 2)
        $scalar.Count | Should -Be 1
        $scalar[0] | Should -Be 42
        $object.Count | Should -Be 1
        $object[0].number | Should -Be 9
    }

    It 'fails closed for missing, null, and wrong-shaped required properties' {
        $module = Get-Module LeakWorkflowDedup
        $sample = @'
{
  "nullValue": null,
  "scalar": { "number": 1 },
  "array": [{ "number": 1 }],
  "nested": [[{ "number": 1 }]],
  "nullElement": [null]
}
'@ | ConvertFrom-Json

        {
            & $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'missing' `
                    -Context 'Test value'
            } $sample
        } | Should -Throw "*is missing 'missing'*"
        {
            & $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'nullValue' `
                    -Context 'Test value'
            } $sample
        } | Should -Throw "*has null 'nullValue'*"
        {
            & $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'scalar' `
                    -Context 'Test value' `
                    -RequireArray
            } $sample
        } | Should -Throw "*malformed 'scalar'; expected an array*"
        {
            & $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'array' `
                    -Context 'Test value'
            } $sample
        } | Should -Throw "*malformed 'array'; expected a scalar or object*"
        {
            & $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'nested' `
                    -Context 'Test value' `
                    -RequireArray
            } $sample
        } | Should -Throw "*malformed 'nested' at index 0; expected a flat array*"
        {
            & $module {
                param($ValueObject)
                Get-LeakRequiredPropertyValue `
                    -Object $ValueObject `
                    -Name 'nullElement' `
                    -Context 'Test value' `
                    -RequireArray
            } $sample
        } | Should -Throw "*malformed 'nullElement' at index 0; expected a flat array*"
    }

    It 'enumerates GraphQL error arrays and rejects scalar errors' {
        $module = Get-Module LeakWorkflowDedup
        {
            & $module {
                Assert-LeakGraphQlResponse `
                    -Response ([pscustomobject]@{
                        data = [pscustomobject]@{}
                        errors = @()
                    }) `
                    -Context 'Test response'
            }
        } | Should -Not -Throw

        foreach ($count in 1, 2) {
            $errors = @(
                1..$count | ForEach-Object {
                    [pscustomobject]@{ message = "error-$_" }
                }
            )
            $failure = $null
            try {
                & $module {
                    param($Response)
                    Assert-LeakGraphQlResponse `
                        -Response $Response `
                        -Context 'Test response'
                } ([pscustomobject]@{
                        data = [pscustomobject]@{ partial = $true }
                        errors = $errors
                    })
            } catch {
                $failure = $_.Exception.Message
            }

            $failure | Should -Match 'returned GraphQL errors'
            1..$count | ForEach-Object {
                $failure | Should -Match "error-$_"
            }
        }

        {
            & $module {
                Assert-LeakGraphQlResponse `
                    -Response ([pscustomobject]@{
                        data = [pscustomobject]@{ partial = $true }
                        errors = [pscustomobject]@{ message = 'scalar error' }
                    }) `
                    -Context 'Test response'
            }
        } | Should -Throw "*malformed 'errors'; expected an array*"
        {
            & $module {
                Assert-LeakGraphQlResponse `
                    -Response (
                        '{"data":{"partial":true},"errors":[null]}' |
                        ConvertFrom-Json
                    ) `
                    -Context 'Test response'
            }
        } | Should -Throw "*malformed 'errors' at index 0; expected a flat array*"
    }
}

Describe 'complete GraphQL pagination' {
    BeforeEach {
        $global:leakPaginationCalls = [System.Collections.Generic.List[object]]::new()
        $global:leakPaginationResponses =
            [System.Collections.Generic.Queue[string]]::new()
        function global:gh {
            param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
            $global:leakPaginationCalls.Add(@($GhArgs))
            $global:LASTEXITCODE = 0
            if ($global:leakPaginationResponses.Count -eq 0) {
                throw 'No mock GraphQL response remains.'
            }
            Write-Output $global:leakPaginationResponses.Dequeue()
        }
    }

    AfterAll {
        Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
        Remove-Variable leakPaginationCalls, leakPaginationResponses `
            -Scope Global -ErrorAction SilentlyContinue
    }

    It 'reads zero, one, and two node pages through actual property access' {
        foreach ($count in 0, 1, 2) {
            $nodes = @()
            $expectedNumbers = @()
            if ($count -gt 0) {
                $expectedNumbers = @(1..$count)
                $nodes = @(
                    $expectedNumbers | ForEach-Object {
                        New-LeakGraphQlPullRequestNode -Number $_
                    }
                )
            }
            $global:leakPaginationResponses.Enqueue(
                (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes $nodes `
                        -TotalCount $count `
                        -HasNextPage $false)
            )

            $result = @(
                Get-CompleteLeakPullRequests `
                    -Repository 'dotnet/maui' `
                    -State MERGED `
                    -BaseRefNames @('main')
            )

            $result.Count | Should -Be $count
            @($result | ForEach-Object { $_.number }) |
                Should -Be $expectedNumbers
        }

        $global:leakPaginationCalls.Count | Should -Be 3
    }

    It 'reads open, closed, and merged PRs in one complete state-stable connection' {
        $nodes = @(
            New-LeakGraphQlPullRequestNode -Number 1 -State OPEN
            New-LeakGraphQlPullRequestNode -Number 2 -State CLOSED
            New-LeakGraphQlPullRequestNode -Number 3 -State MERGED
        )
        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName pullRequests `
                    -Nodes $nodes `
                    -TotalCount $nodes.Count `
                    -HasNextPage $false)
        )

        $result = @(
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -States @('OPEN', 'CLOSED', 'MERGED') `
                -BaseRefNames @('main')
        )

        $result.number | Should -Be @(1, 2, 3)
        $result.state | Should -Be @('OPEN', 'CLOSED', 'MERGED')
        $global:leakPaginationCalls.Count | Should -Be 1
        ($global:leakPaginationCalls[0] -join ' ') |
            Should -Match 'states: \[OPEN, CLOSED, MERGED\]'
    }

    It 'rejects scalar and nested node shapes' {
        $node = New-LeakGraphQlPullRequestNode -Number 1
        $global:leakPaginationResponses.Enqueue(
            (@{
                    data = @{
                        repository = @{
                            pullRequests = @{
                                totalCount = 1
                                nodes = $node
                                pageInfo = @{
                                    hasNextPage = $false
                                    endCursor = $null
                                }
                            }
                        }
                    }
                } | ConvertTo-Json -Depth 10 -Compress)
        )
        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main')
        } | Should -Throw "*malformed 'nodes'; expected an array*"

        $nestedNodes = [object[]]::new(1)
        $nestedNodes[0] = @($node)
        $global:leakPaginationResponses.Enqueue(
            (@{
                    data = @{
                        repository = @{
                            pullRequests = @{
                                totalCount = 1
                                nodes = $nestedNodes
                                pageInfo = @{
                                    hasNextPage = $false
                                    endCursor = $null
                                }
                            }
                        }
                    }
                } | ConvertTo-Json -Depth 10 -Compress)
        )
        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main')
        } | Should -Throw "*malformed 'nodes' at index 0; expected a flat array*"

        $global:leakPaginationCalls.Count | Should -Be 2
    }

    It 'retrieves more than 1000 historical PRs without a Search ceiling' {
        $nodes = @(1..1001 | ForEach-Object {
                New-LeakGraphQlPullRequestNode -Number $_
            })
        for ($offset = 0; $offset -lt $nodes.Count; $offset += 100) {
            $last = [Math]::Min($offset + 99, $nodes.Count - 1)
            $pageNodes = @($nodes[$offset..$last])
            $hasNextPage = $last -lt ($nodes.Count - 1)
            $global:leakPaginationResponses.Enqueue(
                (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes $pageNodes `
                        -TotalCount $nodes.Count `
                        -HasNextPage $hasNextPage `
                        -EndCursor $(if ($hasNextPage) { "cursor-$last" } else { $null }))
            )
        }

        $result = @(
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main')
        )

        $result.Count | Should -Be 1001
        $result[0].number | Should -Be 1
        $result[-1].number | Should -Be 1001
        $global:leakPaginationCalls.Count | Should -Be 11
        ($global:leakPaginationCalls | ForEach-Object { $_ -join ' ' }) |
            Should -Not -Match '(?i)\bsearch\b|--limit'
    }

    It 'follows multiple issue pages and keeps the label scope in the query' {
        $first = @(
            [pscustomobject]@{
                number = 10
                title = '[leak-scan] First.Api — leak'
                body = ''
                url = 'https://github.com/dotnet/maui/issues/10'
            }
            [pscustomobject]@{
                number = 11
                title = '[leak-scan] Second.Api — leak'
                body = ''
                url = 'https://github.com/dotnet/maui/issues/11'
            }
        )
        $second = @(
            [pscustomobject]@{
                number = 12
                title = '[leak-scan] Third.Api — leak'
                body = ''
                url = 'https://github.com/dotnet/maui/issues/12'
            }
        )
        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName issues `
                    -Nodes $first `
                    -TotalCount 3 `
                    -HasNextPage $true `
                    -EndCursor issue-cursor)
        )
        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName issues `
                    -Nodes $second `
                    -TotalCount 3 `
                    -HasNextPage $false)
        )

        $result = @(
            Get-CompleteLeakIssues `
                -Repository 'dotnet/maui' `
                -PageSize 2
        )

        $result.number | Should -Be @(10, 11, 12)
        $global:leakPaginationCalls.Count | Should -Be 2
        ($global:leakPaginationCalls[0] -join ' ') |
            Should -Match 'labels: \["agentic-workflows"\]'
        ($global:leakPaginationCalls[1] -join ' ') |
            Should -Match 'after=issue-cursor'
    }

    It 'fails closed on malformed or incomplete page metadata' {
        $node = New-LeakGraphQlPullRequestNode -Number 1
        $global:leakPaginationResponses.Enqueue(
            (@{
                    data = @{
                        repository = @{
                            pullRequests = @{
                                totalCount = 1
                                nodes = @($node)
                            }
                        }
                    }
                } | ConvertTo-Json -Depth 8 -Compress)
        )
        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main')
        } | Should -Throw "*connection is missing 'pageInfo'*"

        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName pullRequests `
                    -Nodes @($node) `
                    -TotalCount 2 `
                    -HasNextPage $false)
        )
        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main')
        } | Should -Throw '*ended after 1 unique nodes but totalCount is 2*'

        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName pullRequests `
                    -Nodes @($node) `
                    -TotalCount 2 `
                    -HasNextPage $true)
        )
        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main')
        } | Should -Throw '*claimed another page without a usable endCursor*'
    }

    It 'rejects HTTP-200 pull-request responses with top-level errors and partial data' {
        $node = New-LeakGraphQlPullRequestNode -Number 1
        $global:leakPaginationResponses.Enqueue(
            (@{
                    data = @{
                        repository = @{
                            pullRequests = @{
                                totalCount = 1
                                nodes = @($node)
                                pageInfo = @{
                                    hasNextPage = $false
                                    endCursor = $null
                                }
                            }
                        }
                    }
                    errors = @(
                        @{
                            message = "Permission denied`n$('x' * 1000)`0"
                            type = "FORBIDDEN`tTYPE"
                            path = @('repository', 'pullRequests', 0, 'nodes')
                        }
                        @{
                            message = 'Second partial-data error'
                            type = 'RATE_LIMITED'
                            path = @('repository', 'pullRequests')
                        }
                    )
                } | ConvertTo-Json -Depth 10 -Compress)
        )

        $result = @()
        $failure = $null
        try {
            $result = @(
                Get-CompleteLeakPullRequests `
                    -Repository 'dotnet/maui' `
                    -State MERGED `
                    -BaseRefNames @('main')
            )
        } catch {
            $failure = $_.Exception.Message
        }

        $result.Count | Should -Be 0
        $failure | Should -Match 'returned GraphQL errors'
        $failure | Should -Match 'Permission denied x+'
        $failure | Should -Match '"type":"FORBIDDEN TYPE"'
        $failure | Should -Match '"path":"repository.pullRequests.0.nodes"'
        $failure | Should -Match 'Second partial-data error'
        $failure | Should -Match '"type":"RATE_LIMITED"'
        $failure | Should -Not -Match '[\x00-\x1F\x7F]'
        $failure.Length | Should -BeLessThan 1500
        $global:leakPaginationCalls.Count | Should -Be 1
    }

    It 'restarts the whole PR snapshot after count churn and returns only the stable attempt' {
        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName pullRequests `
                    -Nodes @(
                        New-LeakGraphQlPullRequestNode -Number 91
                    ) `
                    -TotalCount 2 `
                    -HasNextPage $true `
                    -EndCursor stale-cursor)
        )
        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName pullRequests `
                    -Nodes @(
                        New-LeakGraphQlPullRequestNode -Number 92
                    ) `
                    -TotalCount 3 `
                    -HasNextPage $false)
        )
        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName pullRequests `
                    -Nodes @(
                        New-LeakGraphQlPullRequestNode -Number 1
                    ) `
                    -TotalCount 2 `
                    -HasNextPage $true `
                    -EndCursor stable-cursor)
        )
        $global:leakPaginationResponses.Enqueue(
            (New-LeakGraphQlPageJson `
                    -ConnectionName pullRequests `
                    -Nodes @(
                        New-LeakGraphQlPullRequestNode -Number 2
                    ) `
                    -TotalCount 2 `
                    -HasNextPage $false)
        )

        $result = @(
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main') `
                -PageSize 1
        )

        $result.number | Should -Be @(1, 2)
        $global:leakPaginationCalls.Count | Should -Be 4
        ($global:leakPaginationCalls[2] -join ' ') |
            Should -Not -Match '\bafter='
        ($global:leakPaginationCalls[3] -join ' ') |
            Should -Match 'after=stable-cursor'
    }

    It 'fails closed when count churn persists across the whole-snapshot restart' {
        1..2 | ForEach-Object {
            $global:leakPaginationResponses.Enqueue(
                (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes @(
                            New-LeakGraphQlPullRequestNode -Number (10 * $_)
                        ) `
                        -TotalCount 2 `
                        -HasNextPage $true `
                        -EndCursor "count-$($_)-first")
            )
            $global:leakPaginationResponses.Enqueue(
                (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes @(
                            New-LeakGraphQlPullRequestNode -Number ((10 * $_) + 1)
                        ) `
                        -TotalCount 3 `
                        -HasNextPage $false)
            )
        }

        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main') `
                -PageSize 1
        } | Should -Throw '*remained inconsistent after 2 whole-snapshot attempts*totalCount changed from 2 to 3*'

        $global:leakPaginationCalls.Count | Should -Be 4
    }

    It 'restarts the whole PR snapshot after cross-base retargeting' {
        foreach ($entry in @(
                @{ Number = 10; Base = 'main' }
                @{ Number = 10; Base = 'inflight/current' }
                @{ Number = 20; Base = 'main' }
                @{ Number = 30; Base = 'inflight/current' }
            )) {
            $global:leakPaginationResponses.Enqueue(
                (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes @(
                            New-LeakGraphQlPullRequestNode `
                                -Number $entry.Number `
                                -Base $entry.Base
                        ) `
                        -TotalCount 1 `
                        -HasNextPage $false)
            )
        }

        $result = @(
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED
        )

        $result.number | Should -Be @(20, 30)
        $global:leakPaginationCalls.Count | Should -Be 4
        @($global:leakPaginationCalls | ForEach-Object {
                @($_ | Where-Object { $_ -like 'base=*' })[0]
            }) |
            Should -Be @(
                'base=main'
                'base=inflight/current'
                'base=main'
                'base=inflight/current'
            ) -Because 'the second attempt must rebuild every authoritative base'
    }

    It 'fails closed when cross-base retargeting persists across the restart' {
        1..2 | ForEach-Object {
            foreach ($base in @('main', 'inflight/current')) {
                $global:leakPaginationResponses.Enqueue(
                    (New-LeakGraphQlPageJson `
                            -ConnectionName pullRequests `
                            -Nodes @(
                                New-LeakGraphQlPullRequestNode `
                                    -Number 10 `
                                    -Base $base
                            ) `
                            -TotalCount 1 `
                            -HasNextPage $false)
                )
            }
        }

        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED
        } | Should -Throw '*remained inconsistent after 2 whole-snapshot attempts*PR #10 under multiple base branches*'

        $global:leakPaginationCalls.Count | Should -Be 4
    }

    It 'fails closed when cursor inconsistency persists across the restart' {
        1..2 | ForEach-Object {
            1..2 | ForEach-Object {
                $global:leakPaginationResponses.Enqueue(
                    (New-LeakGraphQlPageJson `
                            -ConnectionName pullRequests `
                            -Nodes @(
                                New-LeakGraphQlPullRequestNode `
                                    -Number (($global:leakPaginationResponses.Count + 1) * 10)
                            ) `
                            -TotalCount 3 `
                            -HasNextPage $true `
                            -EndCursor repeated)
                )
            }
        }

        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main') `
                -PageSize 1
        } | Should -Throw "*remained inconsistent after 2 whole-snapshot attempts*repeated endCursor 'repeated'*"

        $global:leakPaginationCalls.Count | Should -Be 4
    }

    It 'enforces the query budget before issuing an unbounded next-page request' {
        1..2 | ForEach-Object {
            $global:leakPaginationResponses.Enqueue(
                (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes @(
                            New-LeakGraphQlPullRequestNode -Number $_
                        ) `
                        -TotalCount 3 `
                        -HasNextPage $true `
                        -EndCursor "cursor-$_")
            )
        }

        {
            Get-CompleteLeakPullRequests `
                -Repository 'dotnet/maui' `
                -State MERGED `
                -BaseRefNames @('main') `
                -PageSize 1 `
                -MaximumPageQueries 2
        } | Should -Throw '*exceeded the 2-query pagination safety budget*'

        $global:leakPaginationCalls.Count | Should -Be 2
    }
}

Describe 'merged reverter commit-history pagination' {
    BeforeEach {
        $global:leakCommitHistoryCalls =
            [System.Collections.Generic.List[object]]::new()
        $global:leakCommitHistoryResponses =
            [System.Collections.Generic.Queue[string]]::new()
        function global:gh {
            param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
            $global:leakCommitHistoryCalls.Add(@($GhArgs))
            $global:LASTEXITCODE = 0
            if ($global:leakCommitHistoryResponses.Count -eq 0) {
                throw 'No mock commit-history GraphQL response remains.'
            }
            Write-Output $global:leakCommitHistoryResponses.Dequeue()
        }
    }

    AfterAll {
        Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
        Remove-Variable leakCommitHistoryCalls, leakCommitHistoryResponses `
            -Scope Global -ErrorAction SilentlyContinue
    }

    It 'reads zero, one, and two commit nodes as individual records' {
        foreach ($count in 0, 1, 2) {
            $candidate = New-LeakPr `
                -Number (200 + $count) `
                -Title "Candidate $count"
            $commits = @()
            if ($count -gt 0) {
                $commits = @(
                    1..$count | ForEach-Object {
                        @{
                            oid = '{0:x40}' -f (($count * 10) + $_)
                            message = "Commit $count.$_"
                        }
                    }
                )
            }
            $global:leakCommitHistoryResponses.Enqueue(
                (New-LeakCommitHistoryGraphQlJson `
                        -PullRequest $candidate `
                        -Commits $commits `
                        -TotalCount $count `
                        -HasNextPage $false)
            )

            $result = @(
                Get-CompleteLeakPullRequestCommitHistories `
                    -Repository 'dotnet/maui' `
                    -PullRequests @($candidate)
            )

            $result.Count | Should -Be 1
            @($result[0].commits).Count | Should -Be $count
            @($result[0].commits | ForEach-Object { $_.message }) |
                Should -Be @(
                    $commits | ForEach-Object { $_.message }
                )
        }

        $global:leakCommitHistoryCalls.Count | Should -Be 3
    }

    It 'batches candidates while preserving complete commit metadata' {
        $first = New-LeakPr -Number 200 -Title 'First candidate'
        $second = New-LeakPr -Number 300 -Title 'Second candidate'
        $repository = @{}
        foreach ($entry in @(
                @{ Alias = 'pr0'; PullRequest = $first; Commit = @{
                        oid = '1111111111111111111111111111111111111111'
                        message = 'First immutable message'
                    } }
                @{ Alias = 'pr1'; PullRequest = $second; Commit = @{
                        oid = '2222222222222222222222222222222222222222'
                        message = 'Second immutable message'
                    } }
            )) {
            $repository[$entry.Alias] = @{
                number = $entry.PullRequest.number
                state = 'MERGED'
                merged = $true
                mergedAt = $entry.PullRequest.mergedAt
                baseRefName = $entry.PullRequest.baseRefName
                mergeCommit = @{ oid = $entry.PullRequest.mergeCommitOid }
                commits = @{
                    totalCount = 1
                    nodes = @(@{ commit = $entry.Commit })
                    pageInfo = @{
                        hasNextPage = $false
                        endCursor = $null
                    }
                }
            }
        }
        $global:leakCommitHistoryResponses.Enqueue(
            (@{ data = @{ repository = $repository } } |
                ConvertTo-Json -Depth 10 -Compress)
        )

        $result = @(
            Get-CompleteLeakPullRequestCommitHistories `
                -Repository 'dotnet/maui' `
                -PullRequests @($first, $second) `
                -BatchSize 2
        )

        $result.number | Should -Be @(200, 300)
        $global:leakCommitHistoryCalls.Count | Should -Be 1
        ($global:leakCommitHistoryCalls[0] -join ' ') |
            Should -Match 'pr0: pullRequest'
        ($global:leakCommitHistoryCalls[0] -join ' ') |
            Should -Match 'pr1: pullRequest'
    }

    It 'rejects HTTP-200 commit-history responses with top-level errors and partial data' {
        $candidate = New-LeakPr -Number 200 -Title 'Candidate'
        $partial = New-LeakCommitHistoryGraphQlJson `
            -PullRequest $candidate `
            -Commits @(
                @{
                    oid = '1111111111111111111111111111111111111111'
                    message = 'Partial commit that must not be accepted'
                }
            ) `
            -TotalCount 1 `
            -HasNextPage $false |
            ConvertFrom-Json
        $partial | Add-Member -NotePropertyName errors -NotePropertyValue @(
            [pscustomobject]@{
                message = "Commit history unavailable`r`n$('y' * 1000)`0"
                type = "SERVICE_UNAVAILABLE`tTYPE"
                path = @('repository', 'pr0', 'commits', 'nodes')
            }
        )
        $global:leakCommitHistoryResponses.Enqueue(
            ($partial | ConvertTo-Json -Depth 10 -Compress)
        )

        $result = @()
        $failure = $null
        try {
            $result = @(
                Get-CompleteLeakPullRequestCommitHistories `
                    -Repository 'dotnet/maui' `
                    -PullRequests @($candidate)
            )
        } catch {
            $failure = $_.Exception.Message
        }

        $result.Count | Should -Be 0
        $failure | Should -Match 'returned GraphQL errors'
        $failure | Should -Match 'Commit history unavailable y+'
        $failure | Should -Match '"type":"SERVICE_UNAVAILABLE TYPE"'
        $failure | Should -Match '"path":"repository.pr0.commits.nodes"'
        $failure | Should -Not -Match '[\x00-\x1F\x7F]'
        $failure.Length | Should -BeLessThan 1500
        $global:leakCommitHistoryCalls.Count | Should -Be 1
    }

    It 'fails closed when commit pagination is truncated' {
        $candidate = New-LeakPr -Number 200 -Title 'Candidate'
        $global:leakCommitHistoryResponses.Enqueue(
            (New-LeakCommitHistoryGraphQlJson `
                    -PullRequest $candidate `
                    -Commits @(
                        @{
                            oid = '1111111111111111111111111111111111111111'
                            message = 'Only returned commit'
                        }
                    ) `
                    -TotalCount 2 `
                    -HasNextPage $false)
        )

        {
            Get-CompleteLeakPullRequestCommitHistories `
                -Repository 'dotnet/maui' `
                -PullRequests @($candidate)
        } | Should -Throw '*ended after 1 unique commits but totalCount is 2*'
    }

    It 'fails closed before exceeding the aggregate commit query budget' {
        $candidate = New-LeakPr -Number 200 -Title 'Candidate'
        $global:leakCommitHistoryResponses.Enqueue(
            (New-LeakCommitHistoryGraphQlJson `
                    -PullRequest $candidate `
                    -Commits @(
                        @{
                            oid = '1111111111111111111111111111111111111111'
                            message = 'First commit'
                        }
                    ) `
                    -TotalCount 2 `
                    -HasNextPage $true `
                    -EndCursor next-commit-page)
        )

        {
            Get-CompleteLeakPullRequestCommitHistories `
                -Repository 'dotnet/maui' `
                -PullRequests @($candidate) `
                -PageSize 1 `
                -MaximumPageQueries 1
        } | Should -Throw '*exceeded the 1-query safety budget*'

        $global:leakCommitHistoryCalls.Count | Should -Be 1
    }
}

Describe 'merged revert discovery from complete history' {
    It 'recursively discovers only same-branch edges with exact immutable commit proof' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $revert = New-LeakPr `
            -Number 200 `
            -Title 'Back out cleanup without Revert in the title' `
            -Body 'Reverts #100'
        $restore = New-LeakPr `
            -Number 300 `
            -Title 'Restore prior behavior' `
            -Body 'Reverts dotnet/maui#200'
        $mergedHistory = @(
            $fix
            $revert
            New-LeakPr `
                -Number 201 `
                -Title 'Unrelated mention' `
                -Body 'Reverts were discussed for #100 but not performed'
            New-LeakPr `
                -Number 202 `
                -Title 'Other repository reference' `
                -Body 'Reverts other/repository#100'
            New-LeakPr `
                -Number 203 `
                -Title 'Wrong branch reference' `
                -Body 'Reverts #100' `
                -Base 'release/10.0.1xx-sr9'
            $restore
        )

        $result = @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @($fix) `
                -MergedPullRequests $mergedHistory `
                -PullRequestCommitHistories @(
                    New-LeakCommitHistory `
                        -PullRequest $revert `
                        -Messages @(
                            "Revert cleanup`n`nThis reverts commit $($fix.mergeCommitOid)."
                        )
                    New-LeakCommitHistory `
                        -PullRequest $restore `
                        -Messages @(
                            "Restore cleanup`n`nThis reverts commit $($revert.mergeCommitOid)."
                        )
                )
        )

        $result.number | Should -Be @(200, 300)
        $result[0].verifiedRevertTargets | Should -Be @(100)
        $result[1].verifiedRevertTargets | Should -Be @(200)
    }

    It 'keeps more than 100 seeds on one shared history scan' {
        $targets = @(1000..1100 | ForEach-Object {
                New-LeakPr `
                    -Number $_ `
                    -Title "[leak-fix] Fix Type$_.Member leak"
            })
        $revert = New-LeakPr `
            -Number 2000 `
            -Title 'Relevant revert' `
            -Body 'Reverts #1000'
        $restore = New-LeakPr `
            -Number 2001 `
            -Title 'Recursive revert' `
            -Body 'Reverts #2000'
        $mergedHistory = @($targets) + @($revert, $restore)

        $result = @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests $targets `
                -MergedPullRequests $mergedHistory `
                -PullRequestCommitHistories @(
                    New-LeakCommitHistory `
                        -PullRequest $revert `
                        -Messages @(
                            "Revert`n`nThis reverts commit $($targets[0].mergeCommitOid)."
                        )
                    New-LeakCommitHistory `
                        -PullRequest $restore `
                        -Messages @(
                            "Restore`n`nThis reverts commit $($revert.mergeCommitOid)."
                        )
                )
        )

        $result.number | Should -Be @(2000, 2001)
    }

    It 'retains the original fix when editable body prose has no matching commit proof' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $forged = New-LeakPr `
            -Number 200 `
            -Title 'Unrelated merged change' `
            -Body 'Reverts #100'

        $reverts = @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @($fix) `
                -MergedPullRequests @($fix, $forged) `
                -PullRequestCommitHistories @(
                    New-LeakCommitHistory `
                        -PullRequest $forged `
                        -Messages @('Unrelated immutable commit message')
                )
        )

        $reverts.Count | Should -Be 0
        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'rejects wrong and abbreviated immutable commit references' -ForEach @(
        @{ Proof = '1111111111111111111111111111111111111111' }
        @{ Proof = '000000000000000000000000000000000000006' }
        @{ Proof = '0000000' }
    ) {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $candidate = New-LeakPr `
            -Number 200 `
            -Title 'Claimed revert' `
            -Body 'Reverts #100'

        @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @($fix) `
                -MergedPullRequests @($fix, $candidate) `
                -PullRequestCommitHistories @(
                    New-LeakCommitHistory `
                        -PullRequest $candidate `
                        -Messages @(
                            "Claimed revert`n`nThis reverts commit $Proof."
                        )
                )
        ).Count | Should -Be 0
    }

    It 'ignores a branch-mismatched candidate even with an exact commit reference' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $releaseCandidate = New-LeakPr `
            -Number 200 `
            -Title 'Servicing revert' `
            -Body 'Reverts #100' `
            -Base 'release/10.0.1xx-sr9'

        @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @($fix) `
                -MergedPullRequests @($fix, $releaseCandidate)
        ).Count | Should -Be 0
    }

    It 'rejects duplicate immutable proof for one candidate edge as ambiguous' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $candidate = New-LeakPr `
            -Number 200 `
            -Title 'Ambiguous revert' `
            -Body 'Reverts #100'
        $proof = "This reverts commit $($fix.mergeCommitOid)."

        @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @($fix) `
                -MergedPullRequests @($fix, $candidate) `
                -PullRequestCommitHistories @(
                    New-LeakCommitHistory `
                        -PullRequest $candidate `
                        -Messages @("$proof`n$proof")
                )
        ).Count | Should -Be 0
    }

    It 'rejects a merge commit shared by multiple PR identities as ambiguous' {
        $sharedOid = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -MergeCommitOid $sharedOid
        $other = New-LeakPr `
            -Number 101 `
            -Title 'Another PR with ambiguous commit identity' `
            -MergeCommitOid $sharedOid
        $candidate = New-LeakPr `
            -Number 200 `
            -Title 'Claimed revert' `
            -Body 'Reverts #100'

        @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @($fix) `
                -MergedPullRequests @($fix, $other, $candidate) `
                -PullRequestCommitHistories @(
                    New-LeakCommitHistory `
                        -PullRequest $candidate `
                        -Messages @(
                            "Revert`n`nThis reverts commit $sharedOid."
                        )
                )
        ).Count | Should -Be 0
    }

    It 'fails closed when recursive discovery exhausts the aggregate traversal budget' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $mergedHistory = @(
            $fix
            New-LeakPr -Number 200 -Title 'First revert' -Body 'Reverts #100'
            New-LeakPr -Number 300 -Title 'Second revert' -Body 'Reverts #200'
            New-LeakPr -Number 400 -Title 'Third revert' -Body 'Reverts #300'
        )

        {
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @($fix) `
                -MergedPullRequests $mergedHistory `
                -MaximumTraversalPullRequests 3
        } | Should -Throw '*exhausted the 3-PR aggregate traversal safety budget*'
    }
}

Describe 'workflow enforcement boundary' {
    It 'uses complete non-Search pagination for the early merged-fix history' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw
        $stepStart = $workflow.IndexOf('# (a) Exact [leak-fix] PRs already MERGED')
        $stepEnd = $workflow.IndexOf('# Canonicalize every merged PR title', $stepStart)
        $step = $workflow.Substring($stepStart, $stepEnd - $stepStart)

        $fetch = $step.IndexOf('Get-CompleteLeakPullRequests.ps1')
        $state = $step.IndexOf('-State MERGED', $fetch)
        $filteredWrite = $step.IndexOf('> /tmp/gh-aw/agent/merged-leak-fix-prs.json')

        ($fetch -ge 0) | Should -BeTrue
        ($state -gt $fetch) | Should -BeTrue
        ($filteredWrite -gt $state) | Should -BeTrue
        $step | Should -Match 'complete non-Search history'
        $step | Should -Not -Match 'gh pr list|--search|--limit 1000'
    }

    It 'uses complete issue pagination for open leak-scan de-dup' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw
        $stepStart = $workflow.IndexOf("# This workflow's own open [leak-scan] issues")
        $stepEnd = $workflow.IndexOf('# Exact [leak-fix] PRs already MERGED', $stepStart)
        $step = $workflow.Substring($stepStart, $stepEnd - $stepStart)

        $fetch = $step.IndexOf('Get-CompleteLeakIssues.ps1')
        $rawWrite = $step.IndexOf(
            '-OutputPath /tmp/gh-aw/agent/my-open-leakscan.json'
        )
        $dedupRead = $step.IndexOf("jq -r '.[].title")

        ($fetch -ge 0) | Should -BeTrue
        ($rawWrite -gt $fetch) | Should -BeTrue
        ($dedupRead -gt $rawWrite) | Should -BeTrue
        $step | Should -Match 'stable totalCount/pageInfo'
        $step | Should -Not -Match 'gh issue list|--search|--limit 1000'
    }

    It 'keeps source and trusted attempt-cap branch scope in parity' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw
        $gate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $stepStart = $workflow.IndexOf('# (d) Closed-unmerged attempts')
        $stepEnd = $workflow.IndexOf("`n" + '```', $stepStart)
        $step = $workflow.Substring($stepStart, $stepEnd - $stepStart)

        $step | Should -Match '(?s)Get-CompleteLeakPullRequests\.ps1.*-State CLOSED'
        $gate | Should -Match "(?s)Get-CompleteLeakPullRequests.*-States @\('OPEN', 'CLOSED', 'MERGED'\)"
        $gate | Should -Match '(?s)\$closed = @\(\$consistentSnapshot.*?\$_.state -ceq ''CLOSED'''
        @($step, $gate) | ForEach-Object {
            $_ | Should -Match 'Select-LeakAuthoritativePullRequests'
            $_ | Should -Match 'one aggregate budget across both authoritative lanes'
        }
    }

    It 'brackets final leak-fix analysis with bounded matching all-state snapshots' {
        $gate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw

        ([regex]::Matches(
            $gate,
            "-States @\('OPEN', 'CLOSED', 'MERGED'\)"
        )).Count | Should -Be 2
        $gate | Should -Match '\$maximumConsistencyAttempts = 3'
        $gate | Should -Match 'Get-LeakFixConsistencySignature -PullRequests \$before'
        $gate | Should -Match 'Get-LeakFixConsistencySignature -PullRequests \$after'
        $gate | Should -Match 'remained inconsistent after \$maximumConsistencyAttempts bounded attempts'
        $gate | Should -Not -Match '(?s)Get-CompleteLeakPullRequests.*?-State\s+(?:OPEN|CLOSED|MERGED)'
    }

    It 'validates the exact live scanner issue at the final mutation boundary' {
        $gate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw
        $hunter = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md'
        ) -Raw

        $blockedIndex = $gate.IndexOf('if ($result.Blocked)')
        $liveIssueIndex = $gate.IndexOf('Get-ValidatedLeakScanIssue')
        $successIndex = $gate.IndexOf('Final leak-fix de-dup gate passed')

        $blockedIndex | Should -BeGreaterOrEqual 0
        $liveIssueIndex | Should -BeGreaterThan $blockedIndex
        $successIndex | Should -BeGreaterThan $liveIssueIndex
        $module | Should -Match 'issueOrPullRequest\(number: \$number\)'
        $module | Should -Match "typeName -cne 'Issue'"
        $module | Should -Match "state -cne 'OPEN'"
        $module | Should -Match 'liveApi -cne \$Api'
        $module | Should -Match "'agentic-workflows', 'perf/memory-leak 💦'"
        $hunter | Should -Match 'labels: \[agentic-workflows, "perf/memory-leak 💦"\]'
    }

    It 'keeps trusted merged branch validation in parity across both final gates' {
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw

        @($fixGate, $hunterGate) | ForEach-Object {
            $selectorIndex = $_.IndexOf('$authoritativeMerged = @(')
            $eligibilityIndex = $_.IndexOf(
                '$eligibleMerged = @($authoritativeMerged | Where-Object'
            )

            $selectorIndex | Should -BeGreaterOrEqual 0
            $eligibilityIndex | Should -BeGreaterThan $selectorIndex
            $_.Substring($selectorIndex, $eligibilityIndex - $selectorIndex) |
                Should -Match 'Select-LeakAuthoritativePullRequests'
            $_ | Should -Not -Match '\[string\]\$_\.baseRefName\s+-in'
        }
    }

    It 'wires the final check into safe-output steps rather than prompt-only enforcement' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw

        $workflow | Should -Match '(?s)safe-outputs:.*steps:.*Assert-LeakFixSafeOutputGate\.ps1'
        $workflow | Should -Match 'dedup-state\.json'
        $workflow | Should -Match 'github\.event\.repository\.default_branch'
        $workflow | Should -Match 'RUNNER_TEMP/leak-fix-safe-output'
        $workflow | Should -Not -Match 'run: \.github/scripts/Assert-LeakFixSafeOutputGate\.ps1'
        $workflow | Should -Match 'refusing unsupported empty-API de-dup before build/test work'
        ([regex]::Matches(
            $workflow,
            'select\(\.baseRefName == "main" or \.baseRefName == "inflight/current"\)'
        )).Count | Should -BeGreaterOrEqual 3
    }

    It 'keeps leak-fix provenance instructions aligned with the trusted parser' {
        $workflow = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/leak-fixer.md'
        ) -Raw
        $gate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw

        $workflow | Should -Match 'dedicated\s+visible line'
        $workflow | Should -Match '0–3\s+leading ASCII spaces'
        $workflow | Should -Match '(?m)^Fixes #<N>\r?\nRefs: <owner>/<repo>#<N>$'
        $gate | Should -Match 'Get-LeakFixProvenanceIssueNumber'
        $gate | Should -Not -Match '\[regex\]::Matches'
        $module | Should -Match '(?s)Remove-LeakInertMarkdown.*RejectMalformedState'
        $module | Should -Match '\^\[ \]\{0,3\}Fixes #'
        $module | Should -Match '\^\[ \]\{0,3\}Refs: '
    }

    It 'wires a trusted final live refresh into the hunter safe-output boundary' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw
        $lock = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.lock.yml') -Raw

        $workflow | Should -Match '(?s)safe-outputs:.*steps:.*Assert-LeakHunterSafeOutputGate\.ps1.*create-issue:'
        $workflow | Should -Match '(?s)jobs:\s+safe_outputs:\s+permissions:\s+pull-requests: read'
        $workflow | Should -Match 'github\.event\.repository\.default_branch'
        $workflow | Should -Match 'GITHUB_WORKSPACE.*trusted-leak-hunter'
        $workflow | Should -Match 'persist-credentials: false'
        $workflow | Should -Not -Match 'run: \.github/scripts/Assert-LeakHunterSafeOutputGate\.ps1'
        $workflow | Should -Match "contains\(needs\.agent\.outputs\.output_types, 'create_issue'\)"
        $lock | Should -Match '(?ms)^  safe_outputs:.*?^    permissions:.*?^      pull-requests: read$'
    }

    It 'requires both trusted gate downloads to be non-empty before execution' {
        $fixer = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/leak-fixer.md'
        ) -Raw
        $fixerLock = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/leak-fixer.lock.yml'
        ) -Raw
        $hunter = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md'
        ) -Raw
        $hunterLock = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.lock.yml'
        ) -Raw

        foreach ($content in @($fixer, $fixerLock)) {
            $content | Should -Match '(?s)Assert-LeakFixSafeOutputGate\.ps1.*LeakWorkflowDedup\.psm1.*test -s "\$TRUSTED_DIR/Assert-LeakFixSafeOutputGate\.ps1".*test -s "\$TRUSTED_DIR/LeakWorkflowDedup\.psm1".*chmod -R a-w'
            $content | Should -Not -Match 'test -f "\$TRUSTED_DIR/(?:Assert-LeakFixSafeOutputGate\.ps1|LeakWorkflowDedup\.psm1)"'
        }
        foreach ($content in @($hunter, $hunterLock)) {
            $content | Should -Match '(?s)test -s "\$TRUSTED_DIR/Assert-LeakHunterSafeOutputGate\.ps1".*test -s "\$TRUSTED_DIR/LeakWorkflowDedup\.psm1".*chmod -R a-w'
            $content | Should -Not -Match 'test -f "\$TRUSTED_DIR/(?:Assert-LeakHunterSafeOutputGate\.ps1|LeakWorkflowDedup\.psm1)"'
        }
    }

    It 'documents recursive any-active-reverter semantics' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw

        $workflow | Should -Match 'any active same-branch direct reverter'
        $workflow | Should -Match '(?s)independent sibling reverts.*never cancel each other'
        $workflow | Should -Match 'every edge in a revert-of-revert chain must carry its own exact proof'
        $workflow | Should -Match '(?s)body references.*never establish revert state'
        $workflow | Should -Not -Match 'combined by parity|combine by parity'
    }

    It 'keeps hunter batch instructions aligned with the canonical-API gate' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw

        $workflow | Should -Match 'at most\s+one output per canonical rooting API in the current batch'
        $workflow | Should -Match 'defer the others to a later run'
        $workflow | Should -Not -Match 'distinct mechanisms on one API are separate leaks'
    }

    It 'uses the shared anchored API parser in every workflow parser path' {
        $hunter = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw
        $fixer = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw

        ([regex]::Matches($hunter, 'Get-CanonicalLeakApi\.ps1')).Count | Should -Be 2
        ([regex]::Matches($fixer, 'Get-CanonicalLeakApi\.ps1')).Count | Should -Be 6
        ([regex]::Matches(
            $hunter,
            'Get-CanonicalLeakApi\.ps1 -Title "\$TITLE" -ExistingTitle'
        )).Count | Should -Be 2
        ([regex]::Matches(
            $fixer,
            'Get-CanonicalLeakApi\.ps1 -Title "\$TITLE" -ExistingTitle'
        )).Count | Should -Be 6
        $hunter | Should -Not -Match 'awk.*A-Za-z_'
        $fixer | Should -Not -Match 'awk.*A-Za-z_'
    }

    It 'allows pwsh for fixer agent bash calls' {
        $fixer = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw

        $fixer | Should -Match '(?m)^  bash: \[[^\r\n]*"pwsh"\]$'
    }

    It 'defines shared rate-aware query and aggregate traversal budgets for every caller' {
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw
        $wrapper = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Get-RelevantMergedLeakReverts.ps1'
        ) -Raw
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw

        $module | Should -Match '\$MaximumPageQueries = 1000'
        $module | Should -Match '\$PageSize = 100'
        $module | Should -Match '\$CommitBatchSize = 10'
        $module | Should -Match '\$MaximumCommitPageQueries = 1000'
        $module | Should -Match '\$MaximumCommitRecords = 20000'
        $module | Should -Match '\$MaximumTraversalPullRequests = 2000'
        @($wrapper, $fixGate, $hunterGate) | ForEach-Object {
            $_ | Should -Match 'Get-RelevantMergedLeakReverts'
            $_ | Should -Not -Match 'MaximumPageQueries'
            $_ | Should -Not -Match 'MaximumTraversalPullRequests'
        }
    }

    It 'keeps effective-revert work bounds in trusted module defaults' {
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw
        $driver = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Get-EffectiveRevertedLeakFixes.ps1'
        ) -Raw
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw

        $module | Should -Match (
            '\$MaximumRootTraversalEvaluations = 100000'
        )
        $module | Should -Match (
            '\$MaximumAggregateTraversalEvaluations = 250000'
        )
        $module | Should -Match '\$MaximumTraversalDepth = 256'
        @($driver, $hunterGate) | ForEach-Object {
            $_ | Should -Match 'Get-EffectiveRevertedPullRequestNumbers'
        }
        $fixGate | Should -Match 'Get-LeakFixFinalDedupResult'
        @($driver, $fixGate, $hunterGate) | ForEach-Object {
            $_ | Should -Not -Match 'MaximumRootTraversalEvaluations'
            $_ | Should -Not -Match 'MaximumAggregateTraversalEvaluations'
            $_ | Should -Not -Match 'MaximumTraversalDepth'
        }
    }

    It 'uses complete cursor history with exact immutable revert verification' {
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw
        $wrapper = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Get-RelevantMergedLeakReverts.ps1'
        ) -Raw
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw
        $hunter = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md'
        ) -Raw
        $fixer = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/leak-fixer.md'
        ) -Raw

        $module | Should -Match 'pullRequests\('
        $module | Should -Match 'pageInfo'
        $module | Should -Match 'totalCount'
        $module | Should -Match 'endCursor'
        $module | Should -Match 'mergeCommitOid'
        $module | Should -Match 'commits\(first:'
        $module | Should -Match 'This reverts commit'
        $module | Should -Match 'verifiedRevertTargets'
        $module | Should -Match 'Get-LeakRevertTargets'
        $module | Should -Not -Match "'--search'|gh pr list"
        $effectiveResolver = [regex]::Match(
            $module,
            '(?s)function Get-EffectiveRevertedPullRequestNumbers \{.*?Export-ModuleMember'
        ).Value
        $effectiveResolver | Should -Not -Match 'Get-LeakRevertTargets'
        $effectiveResolver | Should -Not -Match '\.body'
        $wrapper | Should -Match 'MergedPullRequestsJsonPath'
        @($fixGate, $hunterGate) | ForEach-Object {
            $_ | Should -Match 'Get-CompleteLeakPullRequests'
            $_ | Should -Match 'Get-RelevantMergedLeakReverts'
            $_ | Should -Match 'MergedPullRequests \$(?:merged|[A-Za-z]+Merged)'
        }
        $hunter | Should -Match 'Get-RelevantMergedLeakReverts\.ps1'
        $fixer | Should -Match 'Get-RelevantMergedLeakReverts\.ps1'
        @($hunter, $fixer) | ForEach-Object {
            $documentation = $_ -replace '\r?\n[ \t]*#[ \t]?', ' '
            $documentation | Should -Match 'complete non-Search history'
            $documentation | Should -Match 'GraphQL'
            $documentation | Should -Match 'totalCount/pageInfo'
            $documentation | Should -Match 'bounded transient retries'
            $documentation | Should -Match 'capped server-directed rate-limit delays'
            $documentation | Should -Match '1000-query safety budget'
            $documentation | Should -Match '1000-discovery and 2000-PR aggregate'
            $documentation | Should -Match 'merge/squash commit OID'
            $documentation | Should -Match 'This reverts commit'
            $documentation | Should -Match '20000'
            $documentation | Should -Not -Match '1000-result|Search API'
        }
    }

    Context 'safe-output gate script' {
        BeforeEach {
            $script:agentOutput = Join-Path $TestDrive 'agent_output.json'
            $script:stateDirectory = Join-Path $TestDrive 'agent'
            New-Item -ItemType Directory -Path $script:stateDirectory -Force | Out-Null
            @{
                items = @(
                    @{
                        type = 'create_pull_request'
                        title = '[leak-fix] Fix GradientBrush.GradientStops reset leak'
                        body = "Fixes #20`nRefs: dotnet/maui#20"
                        branch = 'leak-fix/issue-20'
                    }
                )
            } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput
            @{
                issue_number = 20
                api = 'GradientBrush.GradientStops'
                repository = 'dotnet/maui'
                different_mechanism_prs = @()
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:stateDirectory 'dedup-state.json')

            $global:mockMerged = @()
            $global:mockReverts = @()
            $global:mockOpen = @()
            $global:mockClosed = @()
            $global:mockGhExitCode = 0
            $global:mockGhStderr = ''
            $global:mockReturnAllBases = $false
            $global:mockGateSnapshots = @()
            $global:mockGateSnapshotIndex = 0
            $global:mockActiveGateSnapshot = $null
            $global:mockIssueFetchCount = 0
            $global:mockIssueResponses =
                [System.Collections.Generic.Queue[object]]::new()
            $global:mockLiveIssue = [pscustomobject]@{
                __typename = 'Issue'
                number = 20
                title = '[leak-scan] GradientBrush.GradientStops — reset leak'
                state = 'OPEN'
                url = 'https://github.com/dotnet/maui/issues/20'
                repository = @{
                    nameWithOwner = 'dotnet/maui'
                }
                labels = @{
                    totalCount = 2
                    pageInfo = @{
                        hasNextPage = $false
                        endCursor = $null
                    }
                    nodes = @(
                        @{ name = 'agentic-workflows' }
                        @{ name = 'perf/memory-leak 💦' }
                    )
                }
            }
            function global:gh {
                param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
                $global:LASTEXITCODE = $global:mockGhExitCode
                if (-not [string]::IsNullOrWhiteSpace($global:mockGhStderr)) {
                    Write-Error $global:mockGhStderr -ErrorAction Continue
                }
                if ($global:mockGhExitCode -ne 0) {
                    Write-Output 'mock gh failure'
                    return
                }
                $queryArgument = @($GhArgs | Where-Object {
                        $_.StartsWith('query=', [StringComparison]::Ordinal)
                    })[0]
                if ($queryArgument -match 'issueOrPullRequest') {
                    $global:mockIssueFetchCount++
                    if ($global:mockIssueResponses.Count -gt 0) {
                        $response = $global:mockIssueResponses.Dequeue()
                        $global:LASTEXITCODE = [int]$response.ExitCode
                        if (-not [string]::IsNullOrWhiteSpace(
                                [string]$response.Stderr
                            )) {
                            Write-Error ([string]$response.Stderr) `
                                -ErrorAction Continue
                        }
                        Write-Output ([string]$response.Stdout)
                        return
                    }
                    Write-Output (@{
                            data = @{
                                repository = @{
                                    issueOrPullRequest = $global:mockLiveIssue
                                }
                            }
                        } | ConvertTo-Json -Depth 10 -Compress)
                    return
                }
                if ($queryArgument -match 'commits\(first:') {
                    $repository = @{}
                    $source = @($global:mockMerged) + @($global:mockReverts)
                    foreach ($argument in @($GhArgs | Where-Object {
                                $_ -match '^number(?<index>[0-9]+)=(?<number>[1-9][0-9]*)$'
                            })) {
                        $argument -match '^number(?<index>[0-9]+)=(?<number>[1-9][0-9]*)$' |
                            Out-Null
                        $index = [int]$Matches.index
                        $number = [int]$Matches.number
                        $pullRequest = @($source | Where-Object {
                                [int]$_.number -eq $number
                            })
                        if ($pullRequest.Count -ne 1) {
                            throw "Unexpected mock commit-history PR #$number."
                        }
                        $messagesProperty =
                            $pullRequest[0].PSObject.Properties['commitMessages']
                        $messages = if ($null -eq $messagesProperty) {
                            @()
                        } else {
                            @($messagesProperty.Value)
                        }
                        $commitIndex = 0
                        $repository["pr$index"] = @{
                            number = $number
                            state = 'MERGED'
                            merged = $true
                            mergedAt = $pullRequest[0].mergedAt
                            baseRefName = $pullRequest[0].baseRefName
                            mergeCommit = @{
                                oid = $pullRequest[0].mergeCommitOid
                            }
                            commits = @{
                                totalCount = $messages.Count
                                nodes = @($messages | ForEach-Object {
                                        $commitIndex++
                                        @{
                                            commit = @{
                                                oid = '{0:x40}' -f (
                                                    ($number * 1000) + $commitIndex
                                                )
                                                message = $_
                                            }
                                        }
                                    })
                                pageInfo = @{
                                    hasNextPage = $false
                                    endCursor = $null
                                }
                            }
                        }
                    }
                    Write-Output (@{ data = @{ repository = $repository } } |
                        ConvertTo-Json -Depth 10 -Compress)
                    return
                }
                $baseArgument = @($GhArgs | Where-Object {
                        $_.StartsWith('base=', [StringComparison]::Ordinal)
                    })[0]
                if ($queryArgument -notmatch 'states: \[(?<states>(?:OPEN|CLOSED|MERGED)(?:,\s*(?:OPEN|CLOSED|MERGED))*)\]' -or
                    [string]::IsNullOrWhiteSpace($baseArgument)) {
                    throw 'Unexpected mock GraphQL request.'
                }
                $requestedStates = @($Matches.states -split ',\s*')
                $base = $baseArgument.Substring('base='.Length)
                $snapshot = if ($global:mockGateSnapshots.Count -gt 0) {
                    if ($base -ceq 'main') {
                        if ($global:mockGateSnapshotIndex -ge
                            $global:mockGateSnapshots.Count) {
                            throw 'No mock gate snapshot remains.'
                        }
                        $global:mockActiveGateSnapshot =
                            $global:mockGateSnapshots[$global:mockGateSnapshotIndex]
                        $global:mockGateSnapshotIndex++
                    } elseif ($null -eq $global:mockActiveGateSnapshot) {
                        throw 'The mock gate snapshot was not initialized by the main query.'
                    }
                    $global:mockActiveGateSnapshot
                } else {
                    [pscustomobject]@{
                        Merged = @($global:mockMerged) + @($global:mockReverts)
                        Open = @($global:mockOpen)
                        Closed = @($global:mockClosed)
                    }
                }
                $source = @(
                    foreach ($state in $requestedStates) {
                        $propertyName = switch ($state) {
                            'MERGED' { 'Merged' }
                            'OPEN' { 'Open' }
                            'CLOSED' { 'Closed' }
                        }
                        foreach ($row in @($snapshot.$propertyName)) {
                            [pscustomobject]@{
                                Row = $row
                                State = $state
                            }
                        }
                    }
                )
                if (-not $global:mockReturnAllBases) {
                    $source = @($source | Where-Object {
                            [string]$_.Row.baseRefName -ceq $base
                        })
                }
                $nodes = @($source | ForEach-Object {
                        $row = $_.Row
                        $state = $_.State
                        $node = [ordered]@{}
                        foreach ($name in @(
                                'number', 'title', 'body', 'baseRefName',
                                'mergedAt', 'url'
                            )) {
                            $property = $row.PSObject.Properties[$name]
                            if ($null -ne $property) {
                                $node[$name] = $property.Value
                            }
                        }
                        $node.state = $state
                        $node.merged = $state -ceq 'MERGED'
                        $node.mergeCommit = if ($state -ceq 'MERGED') {
                            @{ oid = [string]$row.mergeCommitOid }
                        } else {
                            $null
                        }
                        [pscustomobject]$node
                    })
                Write-Output (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes $nodes `
                        -TotalCount $nodes.Count `
                        -HasNextPage $false)
            }
        }

        AfterAll {
            Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
            Remove-Variable mockMerged, mockReverts, mockOpen, mockClosed, mockGhExitCode, `
                mockGhStderr, mockReturnAllBases, mockGateSnapshots, `
                mockGateSnapshotIndex, mockActiveGateSnapshot, mockIssueFetchCount, `
                mockIssueResponses, mockLiveIssue `
                -Scope Global -ErrorAction SilentlyContinue
        }

        It 'rejects an untagged create-pull-request title' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title = 'Fix GradientBrush.GradientStops reset leak'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*must start with*followed by a space or tab*'
        }

        It 'accepts a tagged create-pull-request title' {
            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
            $global:mockIssueFetchCount | Should -Be 1
        }

        It 'rejects a live issue response with the wrong issue number' {
            $global:mockLiveIssue.number = 21

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*mismatched issue number '21'*"
        }

        It 'rejects a missing exact repository issue' {
            $global:mockLiveIssue = $null

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*does not exist in repository 'dotnet/maui' or is not accessible*"
        }

        It 'uses ordinal semantics for the live issue canonical API' {
            $global:mockLiveIssue.title =
                '[leak-scan] GradientBrush.gradientStops — casing-only mismatch'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*canonical API 'GradientBrush.gradientStops' does not match proposed PR API 'GradientBrush.GradientStops'*"
        }

        It 'rejects a closed live issue' {
            $global:mockLiveIssue.state = 'CLOSED'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*must still be OPEN; live state was 'CLOSED'*"
        }

        It 'rejects an issue transferred out of the trusted repository' {
            $global:mockLiveIssue.repository.nameWithOwner = 'dotnet/other'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*belongs to unexpected repository 'dotnet/other'*"
        }

        It 'rejects a pull request masquerading as the issue number' {
            $global:mockLiveIssue = [pscustomobject]@{
                __typename = 'PullRequest'
            }

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*is a pull request, not an issue*'
        }

        It 'rejects a malformed live leak-scan title' {
            $global:mockLiveIssue.title =
                '[leak-scan] Investigate issue 20 for GradientBrush.GradientStops'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*malformed * title without a canonical API*'
        }

        It 'rejects a live issue missing the scanner provenance labels' {
            $global:mockLiveIssue.labels.totalCount = 1
            $global:mockLiveIssue.labels.nodes = @(
                @{ name = 'agentic-workflows' }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*missing required scanner provenance label 'perf/memory-leak 💦'*"
        }

        It 'rejects ambiguous scanner provenance label metadata' {
            $global:mockLiveIssue.labels.totalCount = 3
            $global:mockLiveIssue.labels.nodes = @(
                @{ name = 'agentic-workflows' }
                @{ name = 'Agentic-Workflows' }
                @{ name = 'perf/memory-leak 💦' }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*malformed or ambiguous label metadata*'
        }

        It 'retries a transient live issue fetch and validates the recovered response' {
            $global:mockIssueResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 503: Service Unavailable; Retry-After: 0'
                    Stdout = ''
                })

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
            $global:mockIssueFetchCount | Should -Be 2
        }

        It 'fails closed when transient live issue fetch failures exhaust retries' {
            1..3 | ForEach-Object {
                $global:mockIssueResponses.Enqueue([pscustomobject]@{
                        ExitCode = 1
                        Stderr = 'HTTP 503: Service Unavailable; Retry-After: 0'
                        Stdout = ''
                    })
            }

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*failed with exit code 1 after 3 attempt(s)*'
            $global:mockIssueFetchCount | Should -Be 3
        }

        It 'fails closed without retrying a permanent live issue fetch failure' {
            $global:mockIssueResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 404: Not Found'
                    Stdout = ''
                })

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*failed with exit code 1 after 1 attempt(s)*'
            $global:mockIssueFetchCount | Should -Be 1
        }

        It 'accepts tab-separated prefix grammar at the mutation boundary' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                "[leak-fix]`tFix`tGradientBrush.GradientStops reset leak"
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects near-prefix output at the mutation boundary' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-fix]x Fix GradientBrush.GradientStops reset leak'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*followed by a space or tab*'
        }

        It 'accepts supported punctuation after the canonical API' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-fix] Fix GradientBrush.GradientStops, clear reset subscriptions'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects a tagged title whose API is not in the expected position' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-fix] Investigate https://github.com/dotnet/maui/issues/20 for GradientBrush.GradientStops'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*Could not derive a canonical Type.Member*'
        }

        It 'rejects the legacy form when an agent emits it as new PR output' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-fix] Fix Shell GradientBrush.GradientStops reset leak'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*Could not derive a canonical Type.Member*'
        }

        It 'accepts an additional exact-repository Refs citation for an API-match PR' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = "Fixes #20`nRefs: dotnet/maui#20`nRefs: dotnet/maui#501"
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'fails closed before mutation when live metadata has a direct issue match' {
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 500 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                    -Body "Fixes #20`nRefs: dotnet/maui#20"
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*direct issue-reference match*'
        }

        It 'blocks a stable merged duplicate from two matching live snapshots' {
            $candidate = New-LeakPr `
                    -Number 507 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10'
            $global:mockGateSnapshots = @(
                    [pscustomobject]@{
                        Merged = @($candidate)
                        Open = @()
                        Closed = @()
                    }
                    [pscustomobject]@{
                        Merged = @($candidate)
                        Open = @()
                        Closed = @()
                    }
            )

            {
                    & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                        -AgentOutputPath $script:agentOutput `
                        -StateDirectory $script:stateDirectory `
                        -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*same-API match: 507*'
            $global:mockGateSnapshotIndex | Should -Be 2
        }

        It 'blocks a stable open duplicate from two matching live snapshots' {
            $candidate = New-LeakPr `
                    -Number 508 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10' `
                    -Merged $false
            $global:mockGateSnapshots = @(
                    [pscustomobject]@{
                        Merged = @()
                        Open = @($candidate)
                        Closed = @()
                    }
                    [pscustomobject]@{
                        Merged = @()
                        Open = @($candidate)
                        Closed = @()
                    }
            )

            {
                    & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                        -AgentOutputPath $script:agentOutput `
                        -StateDirectory $script:stateDirectory `
                        -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*same-API match: 508*'
            $global:mockGateSnapshotIndex | Should -Be 2
        }

        It 'retries an open-to-merged transition and blocks the stable merged duplicate' {
            $openCandidate = New-LeakPr `
                    -Number 509 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10' `
                    -Merged $false
            $mergedCandidate = New-LeakPr `
                    -Number 509 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10'
            $global:mockGateSnapshots = @(
                    [pscustomobject]@{
                        Merged = @()
                        Open = @($openCandidate)
                        Closed = @()
                    }
                    [pscustomobject]@{
                        Merged = @($mergedCandidate)
                        Open = @()
                        Closed = @()
                    }
                    [pscustomobject]@{
                        Merged = @($mergedCandidate)
                        Open = @()
                        Closed = @()
                    }
                    [pscustomobject]@{
                        Merged = @($mergedCandidate)
                        Open = @()
                        Closed = @()
                    }
            )

            {
                    & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                        -AgentOutputPath $script:agentOutput `
                        -StateDirectory $script:stateDirectory `
                        -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*same-API match: 509*'
            $global:mockGateSnapshotIndex | Should -Be 4
        }

        It 'retries an open-to-closed transition without stale duplicate or attempt counting' {
            $transitioned = New-LeakPr `
                    -Number 510 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10' `
                    -Merged $false
            $otherAttempt = New-LeakPr `
                    -Number 511 `
                    -Title '[leak-fix] Fix Other.Api leak' `
                    -Body 'Fixes #20' `
                    -Merged $false
            $openSnapshot = [pscustomobject]@{
                    Merged = @()
                    Open = @($transitioned)
                    Closed = @($otherAttempt)
            }
            $closedSnapshot = [pscustomobject]@{
                    Merged = @()
                    Open = @()
                    Closed = @($transitioned, $otherAttempt)
            }
            $global:mockGateSnapshots = @(
                    $openSnapshot
                    $closedSnapshot
                    $closedSnapshot
                    $closedSnapshot
            )

            {
                    & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                        -AgentOutputPath $script:agentOutput `
                        -StateDirectory $script:stateDirectory `
                        -Repository 'dotnet/maui'
            } | Should -Not -Throw
            $global:mockGateSnapshotIndex | Should -Be 4
        }

        It 'does not restart for an unrelated merged PR body edit' {
            $before = New-LeakPr `
                -Number 512 `
                -Title 'Update unrelated documentation' `
                -Body 'Original prose'
            $after = New-LeakPr `
                -Number 512 `
                -Title 'Update unrelated documentation' `
                -Body 'Edited prose after merge'
            $global:mockGateSnapshots = @(
                [pscustomobject]@{
                    Merged = @($before)
                    Open = @()
                    Closed = @()
                }
                [pscustomobject]@{
                    Merged = @($after)
                    Open = @()
                    Closed = @()
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
            $global:mockGateSnapshotIndex | Should -Be 2
        }

        It 'restarts when mutable metadata changes on a leak-fix PR' {
            $before = New-LeakPr `
                -Number 513 `
                -Title '[leak-fix] Fix Other.Api leak' `
                -Body 'Original leak-fix prose'
            $after = New-LeakPr `
                -Number 513 `
                -Title '[leak-fix] Fix Other.Api leak' `
                -Body 'Edited leak-fix prose'
            $beforeSnapshot = [pscustomobject]@{
                Merged = @($before)
                Open = @()
                Closed = @()
            }
            $afterSnapshot = [pscustomobject]@{
                Merged = @($after)
                Open = @()
                Closed = @()
            }
            $global:mockGateSnapshots = @(
                $beforeSnapshot
                $afterSnapshot
                $afterSnapshot
                $afterSnapshot
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
            $global:mockGateSnapshotIndex | Should -Be 4
        }

        It 'restarts when a relevant revert marker is added' {
            $fix = New-LeakPr `
                -Number 514 `
                -Title '[leak-fix] Fix Other.Api leak' `
                -Body 'Fixes #99'
            $beforeReverter = New-LeakPr `
                -Number 515 `
                -Title 'Back out unrelated work' `
                -Body 'No revert marker'
            $afterReverter = New-LeakPr `
                -Number 515 `
                -Title 'Back out unrelated work' `
                -Body 'Reverts #514' `
                -CommitMessages @(
                    "Revert other fix`n`nThis reverts commit $($fix.mergeCommitOid)."
                )
            $beforeSnapshot = [pscustomobject]@{
                Merged = @($fix, $beforeReverter)
                Open = @()
                Closed = @()
            }
            $afterSnapshot = [pscustomobject]@{
                Merged = @($fix, $afterReverter)
                Open = @()
                Closed = @()
            }
            $global:mockGateSnapshots = @(
                $beforeSnapshot
                $afterSnapshot
                $afterSnapshot
                $afterSnapshot
            )
            $global:mockMerged = @($fix)
            $global:mockReverts = @($afterReverter)

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
            $global:mockGateSnapshotIndex | Should -Be 4
        }

        It 'restarts when a relevant revert marker is removed' {
            $fix = New-LeakPr `
                -Number 516 `
                -Title '[leak-fix] Fix Other.Api leak' `
                -Body 'Fixes #99'
            $beforeReverter = New-LeakPr `
                -Number 517 `
                -Title 'Back out unrelated work' `
                -Body 'Reverts #516' `
                -CommitMessages @(
                    "Revert other fix`n`nThis reverts commit $($fix.mergeCommitOid)."
                )
            $afterReverter = New-LeakPr `
                -Number 517 `
                -Title 'Back out unrelated work' `
                -Body 'Revert marker removed'
            $beforeSnapshot = [pscustomobject]@{
                Merged = @($fix, $beforeReverter)
                Open = @()
                Closed = @()
            }
            $afterSnapshot = [pscustomobject]@{
                Merged = @($fix, $afterReverter)
                Open = @()
                Closed = @()
            }
            $global:mockGateSnapshots = @(
                $beforeSnapshot
                $afterSnapshot
                $afterSnapshot
                $afterSnapshot
            )
            $global:mockMerged = @($fix)
            $global:mockReverts = @($beforeReverter)

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
            $global:mockGateSnapshotIndex | Should -Be 4
        }

        It 'fails closed when candidate state churn exhausts the consistency retries' {
            $candidate = New-LeakPr `
                    -Number 518 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10' `
                    -Merged $false
            $openSnapshot = [pscustomobject]@{
                    Merged = @()
                    Open = @($candidate)
                    Closed = @()
            }
            $closedSnapshot = [pscustomobject]@{
                    Merged = @()
                    Open = @()
                    Closed = @($candidate)
            }
            $global:mockGateSnapshots = @(
                    $openSnapshot
                    $closedSnapshot
                    $openSnapshot
                    $closedSnapshot
                    $openSnapshot
                    $closedSnapshot
            )

            {
                    & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                        -AgentOutputPath $script:agentOutput `
                        -StateDirectory $script:stateDirectory `
                        -Repository 'dotnet/maui'
            } | Should -Throw '*remained inconsistent after 3 bounded attempts*'
            $global:mockGateSnapshotIndex | Should -Be 6
        }

        It 'fails closed when the final GitHub fetch fails' {
            $global:mockGhExitCode = 1
            $global:mockGhStderr = "auth warning`n$([char]27)[31mred"

            $message = try {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
                throw 'Expected the gh failure to stop the gate.'
            } catch {
                $_.Exception.Message
            }

            $message | Should -Match 'failed with exit code 1'
            $message | Should -Match 'Output: auth warning'
            $message | Should -Not -Match "[`r`n$([char]27)]"
        }

        It 'parses successful JSON without mixing benign gh stderr into stdout' {
            $global:mockGhStderr = 'benign gh warning'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects an agent-authored different-mechanism state override' {
            @{
                issue_number = 20
                api = 'GradientBrush.GradientStops'
                repository = 'dotnet/maui'
                different_mechanism_prs = @(
                    @{
                        number = 501
                        basis = 'Agent-authored mechanism claim'
                    }
                )
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:stateDirectory 'dedup-state.json')

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*do not accept agent-authored different-mechanism overrides*'
        }

        It 'blocks a live same-API match despite an agent-authored body disclosure' {
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 501 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10'
            )
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = @"
Fixes #20
Refs: dotnet/maui#20

## Same-API comparisons
Same-API comparison: dotnet/maui#501 | Different mechanism: Agent-authored claim
"@
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*same-API match: 501*'
        }

        It 'aggregates main and inflight/current attempts while excluding release lanes' {
            $global:mockClosed = @(
                New-LeakPr -Number 601 -Title '[leak-fix] Fix Other.Api leak' `
                    -Body 'Fixes #20' -Merged $false
                New-LeakPr -Number 602 -Title '[leak-fix] Fix GradientBrush.GradientStops leak' `
                    -Body 'Fixes #10' -Base 'inflight/current' -Merged $false
                New-LeakPr -Number 603 -Title '[leak-fix] Fix Other.Api leak again' `
                    -Body 'Refs: dotnet/maui#20' -Merged $false
                New-LeakPr -Number 604 -Title '[leak-fix] Fix GradientBrush.GradientStops release leak' `
                    -Body 'Fixes #20' -Base 'release/10.0.1xx-sr9' -Merged $false
            )
            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*attempt-cap gate blocked PR creation: 3 closed-unmerged attempts*'
        }

        It 'fails closed when a closed attempt is missing baseRefName' {
            $global:mockReturnAllBases = $true
            $global:mockClosed = @(
                [pscustomobject]@{
                    number = 605
                    title = '[leak-fix] Fix GradientBrush.GradientStops leak'
                    body = 'Fixes #20'
                    mergedAt = $null
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*missing baseRefName*'
        }

        It 'fails closed when a closed attempt has malformed baseRefName' {
            $global:mockReturnAllBases = $true
            $global:mockClosed = @(
                New-LeakPr `
                    -Number 606 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops leak' `
                    -Body 'Fixes #20' `
                    -Base ' main ' `
                    -Merged $false
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*malformed baseRefName*'
        }

        It 'allows a release-only open PR even when it directly references the issue' {
            $global:mockOpen = @(
                New-LeakPr `
                    -Number 502 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                    -Body "Fixes #20`nRefs: dotnet/maui#20" `
                    -Base 'release/10.0.1xx-sr9' `
                    -Merged $false
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'allows a re-file after the matching merged fix was effectively reverted' {
            $fix = New-LeakPr `
                -Number 503 `
                -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                -Body "Fixes #20`nRefs: dotnet/maui#20"
            $global:mockMerged = @($fix)
            $global:mockReverts = @(
                New-LeakPr `
                    -Number 504 `
                    -Title 'Back out the collection cleanup' `
                    -Body 'Reverts dotnet/maui#503' `
                    -CommitMessages @(
                        "Revert cleanup`n`nThis reverts commit $($fix.mergeCommitOid)."
                    )
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'blocks when editable merged-PR prose claims a revert without commit proof' {
            $fix = New-LeakPr `
                -Number 503 `
                -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                -Body "Fixes #20`nRefs: dotnet/maui#20"
            $global:mockMerged = @($fix)
            $global:mockReverts = @(
                New-LeakPr `
                    -Number 504 `
                    -Title 'Unrelated merged change' `
                    -Body 'Reverts dotnet/maui#503' `
                    -CommitMessages @('No immutable revert association')
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*direct issue-reference match: 503*'
        }
    }

    Context 'hunter safe-output gate script' {
        BeforeEach {
            $script:hunterAgentOutput = Join-Path $TestDrive 'hunter_agent_output.json'
            @{
                items = @(
                    @{
                        type = 'create_issue'
                        title = '[leak-scan] GradientBrush.GradientStops — reset leak'
                        body = 'AI-generated leak report'
                    }
                )
            } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:hunterAgentOutput

            $global:mockHunterOpenIssues = @()
            $global:mockHunterMerged = @()
            $global:mockHunterReverts = @()
            $global:mockHunterGhExitCode = 0
            $global:mockHunterGhStderr = ''
            $global:mockHunterReturnAllBases = $false
            function global:gh {
                param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
                $global:LASTEXITCODE = $global:mockHunterGhExitCode
                if (-not [string]::IsNullOrWhiteSpace($global:mockHunterGhStderr)) {
                    Write-Error $global:mockHunterGhStderr -ErrorAction Continue
                }
                if ($global:mockHunterGhExitCode -ne 0) {
                    Write-Output 'mock gh failure'
                    return
                }
                $queryArgument = @($GhArgs | Where-Object {
                        $_.StartsWith('query=', [StringComparison]::Ordinal)
                    })[0]
                if ($queryArgument -match '\bissues\(') {
                    $nodes = @($global:mockHunterOpenIssues | ForEach-Object {
                            $node = [ordered]@{}
                            foreach ($name in @('number', 'title', 'body', 'url')) {
                                $property = $_.PSObject.Properties[$name]
                                if ($null -ne $property) {
                                    $node[$name] = $property.Value
                                }
                            }
                            [pscustomobject]$node
                        })
                    Write-Output (New-LeakGraphQlPageJson `
                            -ConnectionName issues `
                            -Nodes $nodes `
                            -TotalCount $nodes.Count `
                            -HasNextPage $false)
                    return
                }
                if ($queryArgument -match 'commits\(first:') {
                    $repository = @{}
                    $source = @($global:mockHunterMerged) +
                        @($global:mockHunterReverts)
                    foreach ($argument in @($GhArgs | Where-Object {
                                $_ -match '^number(?<index>[0-9]+)=(?<number>[1-9][0-9]*)$'
                            })) {
                        $argument -match '^number(?<index>[0-9]+)=(?<number>[1-9][0-9]*)$' |
                            Out-Null
                        $index = [int]$Matches.index
                        $number = [int]$Matches.number
                        $pullRequest = @($source | Where-Object {
                                [int]$_.number -eq $number
                            })
                        if ($pullRequest.Count -ne 1) {
                            throw "Unexpected hunter mock commit-history PR #$number."
                        }
                        $messagesProperty =
                            $pullRequest[0].PSObject.Properties['commitMessages']
                        $messages = if ($null -eq $messagesProperty) {
                            @()
                        } else {
                            @($messagesProperty.Value)
                        }
                        $commitIndex = 0
                        $repository["pr$index"] = @{
                            number = $number
                            state = 'MERGED'
                            merged = $true
                            mergedAt = $pullRequest[0].mergedAt
                            baseRefName = $pullRequest[0].baseRefName
                            mergeCommit = @{
                                oid = $pullRequest[0].mergeCommitOid
                            }
                            commits = @{
                                totalCount = $messages.Count
                                nodes = @($messages | ForEach-Object {
                                        $commitIndex++
                                        @{
                                            commit = @{
                                                oid = '{0:x40}' -f (
                                                    ($number * 1000) + $commitIndex
                                                )
                                                message = $_
                                            }
                                        }
                                    })
                                pageInfo = @{
                                    hasNextPage = $false
                                    endCursor = $null
                                }
                            }
                        }
                    }
                    Write-Output (@{ data = @{ repository = $repository } } |
                        ConvertTo-Json -Depth 10 -Compress)
                    return
                }
                $baseArgument = @($GhArgs | Where-Object {
                        $_.StartsWith('base=', [StringComparison]::Ordinal)
                    })[0]
                if ($queryArgument -notmatch 'states: \[MERGED\]' -or
                    [string]::IsNullOrWhiteSpace($baseArgument)) {
                    throw 'Unexpected mock GraphQL request.'
                }
                $base = $baseArgument.Substring('base='.Length)
                $source = @($global:mockHunterMerged) + @($global:mockHunterReverts)
                if (-not $global:mockHunterReturnAllBases) {
                    $source = @($source | Where-Object {
                            [string]$_.baseRefName -ceq $base
                        })
                }
                $nodes = @($source | ForEach-Object {
                        $node = [ordered]@{}
                        foreach ($name in @(
                                'number', 'title', 'body', 'baseRefName',
                                'mergedAt', 'url'
                            )) {
                            $property = $_.PSObject.Properties[$name]
                            if ($null -ne $property) {
                                $node[$name] = $property.Value
                            }
                        }
                        $node.state = 'MERGED'
                        $node.merged = $true
                        $node.mergeCommit = @{
                            oid = [string]$_.mergeCommitOid
                        }
                        [pscustomobject]$node
                    })
                Write-Output (New-LeakGraphQlPageJson `
                        -ConnectionName pullRequests `
                        -Nodes $nodes `
                        -TotalCount $nodes.Count `
                        -HasNextPage $false)
            }
        }

        AfterAll {
            Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
            Remove-Variable mockHunterOpenIssues, mockHunterMerged, mockHunterReverts, `
                mockHunterGhExitCode, mockHunterGhStderr, mockHunterReturnAllBases `
                -Scope Global -ErrorAction SilentlyContinue
        }

        It 'accepts issue emission when the final live refresh has no match' {
            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'accepts tab-separated issue prefix grammar at the mutation boundary' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw |
                ConvertFrom-Json
            $output.items[0].title =
                "[leak-scan]`tGradientBrush.GradientStops — reset leak"
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects near-prefix issue output at the mutation boundary' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw |
                ConvertFrom-Json
            $output.items[0].title =
                '[leak-scan]x GradientBrush.GradientStops — reset leak'
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*followed by a space or tab*'
        }

        It 'rejects a malformed issue title instead of deriving a later API token' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-scan] Investigate Microsoft.Maui.Controls before GradientBrush.GradientStops'
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*Could not derive a canonical Type.Member*'
        }

        It 'rejects the legacy form when an agent emits it as new output' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-scan] Shell BackButtonBehavior.Command — reset leak'
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*Could not derive a canonical Type.Member*'
        }

        It 'rejects differently titled issues for the same canonical API in one output batch' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items += [pscustomobject]@{
                type = 'create_issue'
                title = '[leak-scan] GradientBrush.GradientStops — detach teardown leak'
                body = 'Second AI-generated leak report'
            }
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*same canonical API 'GradientBrush.GradientStops'*"
        }

        It 'keeps casing-only C# APIs distinct while the exact-casing batch contract still dedups' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items += [pscustomobject]@{
                type = 'create_issue'
                title = '[leak-scan] GradientBrush.gradientStops — distinct C# API casing'
                body = 'Second AI-generated leak report'
            }
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'blocks issue emission when a matching fix merged after the pre-agent snapshot' {
            $global:mockHunterMerged = @(
                New-LeakPr `
                    -Number 701 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak'
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'GradientBrush.GradientStops'*701*"
        }

        It 'fails closed before mutation when merged metadata is missing baseRefName' {
            $global:mockHunterReturnAllBases = $true
            $global:mockHunterMerged = @(
                [pscustomobject]@{
                    number = 704
                    title = '[leak-fix] Fix GradientBrush.GradientStops reset leak'
                    body = 'Fixes #20'
                    mergedAt = '2026-08-10T00:00:00Z'
                    url = 'https://github.com/dotnet/maui/pull/704'
                }
            )
            $before = Get-Content -LiteralPath $script:hunterAgentOutput -Raw

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*MERGED pull-request pagination for 'main' PR #704 is missing baseRefName*"

            Get-Content -LiteralPath $script:hunterAgentOutput -Raw |
                Should -BeExactly $before
        }

        It 'fails closed before mutation when merged metadata has malformed baseRefName' {
            $global:mockHunterReturnAllBases = $true
            $global:mockHunterMerged = @(
                New-LeakPr `
                    -Number 705 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                    -Base ' main '
            )
            $before = Get-Content -LiteralPath $script:hunterAgentOutput -Raw

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*MERGED pull-request pagination for 'main' PR #705 has malformed baseRefName*"

            Get-Content -LiteralPath $script:hunterAgentOutput -Raw |
                Should -BeExactly $before
        }

        It 'continues to exclude release-only merged fixes from hunter de-dup' {
            $global:mockHunterMerged = @(
                New-LeakPr `
                    -Number 706 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                    -Base 'release/10.0.1xx-sr9'
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'blocks agent-authored different-mechanism evidence for a same-API fix' {
            $global:mockHunterMerged = @(
                New-LeakPr `
                    -Number 701 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak'
            )
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = @"
AI-generated leak report

## Same-API comparisons
Same-API comparison: dotnet/maui#701 | Different mechanism: Agent-authored claim
"@
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'GradientBrush.GradientStops'*701*"
        }

        It 'blocks a same-API open issue without accepting an override' {
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 702
                    title = '[leak-scan] GradientBrush.GradientStops — teardown leak'
                    body = 'Existing scanner issue'
                    url = 'https://github.com/dotnet/maui/issues/702'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'GradientBrush.GradientStops'*702*"
        }

        It 'accepts space and tab grammar when validating existing open scan titles' {
            foreach ($separator in @(' ', "`t")) {
                $global:mockHunterOpenIssues = @(
                    [pscustomobject]@{
                        number = 702
                        title = "[leak-scan]${separator}GradientBrush.GradientStops — teardown leak"
                        body = 'Existing scanner issue'
                        url = 'https://github.com/dotnet/maui/issues/702'
                    }
                )

                {
                    & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                        -AgentOutputPath $script:hunterAgentOutput `
                        -Repository 'dotnet/maui'
                } | Should -Throw "*blocked issue creation for 'GradientBrush.GradientStops'*702*"
            }
        }

        It 'fails closed on malformed or ambiguous expected-prefix open titles' -ForEach @(
            @{ ExistingTitle = '[leak-scan]' }
            @{ ExistingTitle = '[leak-scan] Investigate GradientBrush.GradientStops' }
            @{ ExistingTitle = '[leak-scan]x GradientBrush.GradientStops' }
            @{ ExistingTitle = '[LEAK-SCAN] GradientBrush.GradientStops' }
            @{ ExistingTitle = '[Leak-Scan] GradientBrush.GradientStops' }
            @{ ExistingTitle = '[leak-scan] [leak-scan] GradientBrush.GradientStops' }
        ) {
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 704
                    title = $ExistingTitle
                    body = 'Malformed scanner issue'
                    url = 'https://github.com/dotnet/maui/issues/704'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*malformed*leak-scan*'
        }

        It 'ignores unrelated shared-label near-prefix open issues' {
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 704
                    title = '[leak-scanx] tracking'
                    body = 'Unrelated shared-label issue'
                    url = 'https://github.com/dotnet/maui/issues/704'
                }
                [pscustomobject]@{
                    number = 705
                    title = '[leak-scanner] tracking'
                    body = 'Unrelated shared-label issue'
                    url = 'https://github.com/dotnet/maui/issues/705'
                }
                [pscustomobject]@{
                    number = 706
                    title = '[LEAK-SCANX] tracking'
                    body = 'Unrelated shared-label issue'
                    url = 'https://github.com/dotnet/maui/issues/706'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'ignores unrelated open issue titles' {
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 705
                    title = 'Document the [leak-scan] issue format'
                    body = 'Unrelated issue'
                    url = 'https://github.com/dotnet/maui/issues/705'
                }
                [pscustomobject]@{
                    number = 706
                    title = '[leak-fix] Fix GradientBrush.GradientStops reset leak'
                    body = 'Unrelated issue kind'
                    url = 'https://github.com/dotnet/maui/issues/706'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'blocks a legacy Shell-prefixed same-API open issue' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-scan] BackButtonBehavior.Command — reset leak'
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 36345
                    title = '[leak-scan] Shell BackButtonBehavior.Command leaks via strong ICommand'
                    body = 'Existing legacy scanner issue'
                    url = 'https://github.com/dotnet/maui/issues/36345'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'BackButtonBehavior.Command'*36345*"
        }

        It 'rejects a mixed batch atomically when one item becomes stale' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items += [pscustomobject]@{
                type = 'create_issue'
                title = '[leak-scan] Button.Clicked — event subscription leak'
                body = 'Second AI-generated leak report'
            }
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 703
                    title = '[leak-scan] Button.Clicked — existing event subscription leak'
                    body = 'Existing scanner issue'
                    url = 'https://github.com/dotnet/maui/issues/703'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'Button.Clicked'*rejected atomically*"

            $unchanged = Get-Content -LiteralPath $script:hunterAgentOutput -Raw |
                ConvertFrom-Json
            @($unchanged.items).Count | Should -Be 2
            @($unchanged.items.title) | Should -Contain (
                '[leak-scan] GradientBrush.GradientStops — reset leak'
            )
            @($unchanged.items.title) | Should -Contain (
                '[leak-scan] Button.Clicked — event subscription leak'
            )
        }

        It 'parses successful hunter JSON without mixing benign gh stderr into stdout' {
            $global:mockHunterGhStderr = 'benign gh warning'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'fails closed when a hunter gh query fails' {
            $global:mockHunterGhExitCode = 1

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*failed with exit code 1*'
        }
    }
}

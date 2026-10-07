#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Get-CiFixAzdoEvidence.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $scriptPath,
        [ref]$tokens,
        [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Get-CiFixIssueField',
            'Invoke-CiFixAzdoRequest',
            'ConvertFrom-CiFixJsonResponse',
            'Get-CiFixFailedTaskRecords',
            'Get-CiFixBuildEvidence',
            'Get-CiFixBuildEvidenceBounded',
            'New-CiFixAzdoEvidence')) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) {
            throw "Function '$functionName' not found"
        }
        Invoke-Expression $function.Extent.Text
    }

    $script:PipelineDefinitions = @{
        'maui-pr' = 302
        'maui-pr-devicetests' = 314
        'maui-pr-uitests' = 313
    }
    $script:AzdoBaseUri = 'https://dev.azure.com/dnceng-public/public/_apis/build'
    $script:TransientStatusCodes = @(408, 429, 500, 502, 503, 504)
    $script:MaxHttpAttempts = 3
    $script:HttpTimeoutSeconds = 60
    $script:RemainingDownloadBytes = 67108864
}

Describe 'Get-CiFixAzdoEvidence' {
    It 'extracts exact CI-fix issue fields without evaluating their contents' {
        $body = @'
- **Pipeline**: maui-pr
- **Build ID**: 1626241
- **Error Message**: $(throw "must stay inert")
'@

        Get-CiFixIssueField -Body $body -Name 'Pipeline' | Should -BeExactly 'maui-pr'
        Get-CiFixIssueField -Body $body -Name 'Build ID' | Should -BeExactly '1626241'
    }

    It 'selects distinct failed task logs and ignores non-task records' {
        $timeline = [pscustomobject]@{
            records = @(
                [pscustomobject]@{ id = '1'; type = 'Task'; result = 'failed'; name = 'Build'; log = [pscustomobject]@{ id = 179 } }
                [pscustomobject]@{ id = '2'; type = 'Task'; result = 'failed'; name = 'Build retry'; log = [pscustomobject]@{ id = 179 } }
                [pscustomobject]@{ id = '3'; type = 'Job'; result = 'failed'; name = 'Job'; log = [pscustomobject]@{ id = 180 } }
                [pscustomobject]@{ id = '4'; type = 'Task'; result = 'succeeded'; name = 'Passed'; log = [pscustomobject]@{ id = 181 } }
            )
        }

        $result = @(Get-CiFixFailedTaskRecords -Timeline $timeline)

        $result.Count | Should -Be 1
        $result[0].logId | Should -Be 179
        $result[0].name | Should -BeExactly 'Build'
    }

    It 'retains available logs while marking missing logs incomplete' {
        $destination = Join-Path $TestDrive 'evidence'
        $responses = @{
            'timeline' = [pscustomobject]@{
                Succeeded = $true
                StatusCode = 200
                Content = '{"records":[{"id":"a","type":"Task","result":"failed","name":"first","log":{"id":10}},{"id":"b","type":"Task","result":"failed","name":"second","log":{"id":11}}]}'
                Truncated = $false
                Error = ''
                Attempts = 1
            }
            'logs/10' = [pscustomobject]@{
                Succeeded = $true
                StatusCode = 200
                Content = 'real failure text'
                Truncated = $false
                Error = ''
                Attempts = 1
            }
            'logs/11' = [pscustomobject]@{
                Succeeded = $false
                StatusCode = 404
                Content = ''
                Truncated = $false
                Error = 'HTTP 404'
                Attempts = 1
            }
        }

        $requestInvoker = {
            param($Uri, $MaxBytes)
            foreach ($key in @($responses.Keys)) {
                if ($Uri -match [regex]::Escape($key)) {
                    return $responses[$key]
                }
            }

            throw "Unexpected URI: $Uri"
        }

        $result = Get-CiFixBuildEvidence `
            -BuildId 42 `
            -OutputDirectory $destination `
            -MaxFailedLogs 10 `
            -TimelineByteLimit 10000 `
            -LogByteLimit 10000 `
            -RequestInvoker $requestInvoker

        $result.complete | Should -BeFalse
        $result.failedTaskCount | Should -Be 2
        $result.failedTasks[0].status | Should -BeExactly 'available'
        $result.failedTasks[1].status | Should -BeExactly 'error'
        Get-Content -Raw -LiteralPath (Join-Path $destination $result.failedTasks[0].path) |
            Should -BeExactly 'real failure text'
    }

    It 'retains failed tasks with missing or invalid log IDs as incomplete without requesting them' {
        $destination = Join-Path $TestDrive 'invalid-log-ids'
        $requestedUris = [Collections.Generic.List[string]]::new()
        $requestInvoker = {
            param($Uri, $MaxBytes)
            $requestedUris.Add($Uri)
            if ($Uri -match '/timeline') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = '{"records":[{"id":"a","type":"Task","result":"failed","name":"missing-log"},{"id":"b","type":"Task","result":"failed","name":"null-log","log":null},{"id":"c","type":"Task","result":"failed","name":"null-id","log":{"id":null}},{"id":"d","type":"Task","result":"failed","name":"invalid-id","log":{"id":"not-a-number"}},{"id":"e","type":"Task","result":"failed","name":"valid","log":{"id":10}}]}'
                    Truncated = $false
                    Error = ''
                    Attempts = 1
                }
            }
            if ($Uri -match '/logs/10') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = 'real failure text'
                    Truncated = $false
                    Error = ''
                    Attempts = 1
                }
            }

            throw "Unexpected URI: $Uri"
        }

        $result = Get-CiFixBuildEvidence `
            -BuildId 42 `
            -OutputDirectory $destination `
            -MaxFailedLogs 10 `
            -TimelineByteLimit 10000 `
            -LogByteLimit 10000 `
            -RequestInvoker $requestInvoker

        $result.complete | Should -BeFalse
        $result.failedTaskCount | Should -Be 5
        $result.fetchedFailedTaskCount | Should -Be 1
        @($result.failedTasks | Where-Object status -eq 'unavailable').Count | Should -Be 4
        @($result.failedTasks | Where-Object status -eq 'available').Count | Should -Be 1
        @($result.failedTasks | Where-Object status -eq 'unavailable' | ForEach-Object error | Select-Object -Unique) |
            Should -Be @('failed task log ID is missing or invalid')
        @($requestedUris | Where-Object { $_ -match '/logs/' }).Count | Should -Be 1
    }

    It 'does not follow an HTTP redirect to a sign-in response' {
        $port = Get-Random -Minimum 30000 -Maximum 45000
        $readyPath = Join-Path $TestDrive 'redirect-server.ready'
        $redirectLocation = "http://127.0.0.1:$port/sign-in"
        $serverJob = Start-Job -ArgumentList $port, $readyPath -ScriptBlock {
            param($Port, $ReadyPath)

            $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $Port)
            $listener.Start()
            Set-Content -LiteralPath $ReadyPath -Value 'ready'
            try {
                $client = $listener.AcceptTcpClient()
                try {
                    $stream = $client.GetStream()
                    $reader = [IO.StreamReader]::new($stream, [Text.Encoding]::ASCII, $false, 1024, $true)
                    while (($line = $reader.ReadLine()) -ne '') {
                        if ($null -eq $line) {
                            break
                        }
                    }
                    $response = "HTTP/1.1 302 Found`r`nLocation: http://127.0.0.1:$Port/sign-in`r`nContent-Length: 0`r`nConnection: close`r`n`r`n"
                    $bytes = [Text.Encoding]::ASCII.GetBytes($response)
                    $stream.Write($bytes, 0, $bytes.Length)
                }
                finally {
                    $client.Dispose()
                }

                $deadline = [DateTime]::UtcNow.AddSeconds(3)
                while (-not $listener.Pending() -and [DateTime]::UtcNow -lt $deadline) {
                    Start-Sleep -Milliseconds 25
                }
                if ($listener.Pending()) {
                    $client = $listener.AcceptTcpClient()
                    try {
                        $stream = $client.GetStream()
                        $reader = [IO.StreamReader]::new($stream, [Text.Encoding]::ASCII, $false, 1024, $true)
                        while (($line = $reader.ReadLine()) -ne '') {
                            if ($null -eq $line) {
                                break
                            }
                        }
                        $content = '<html>sign in</html>'
                        $response = "HTTP/1.1 200 OK`r`nContent-Length: $($content.Length)`r`nConnection: close`r`n`r`n$content"
                        $bytes = [Text.Encoding]::ASCII.GetBytes($response)
                        $stream.Write($bytes, 0, $bytes.Length)
                    }
                    finally {
                        $client.Dispose()
                    }
                }
            }
            finally {
                $listener.Stop()
            }
        }

        try {
            $deadline = [DateTime]::UtcNow.AddSeconds(5)
            while (-not (Test-Path -LiteralPath $readyPath) -and [DateTime]::UtcNow -lt $deadline) {
                Start-Sleep -Milliseconds 25
            }
            Test-Path -LiteralPath $readyPath | Should -BeTrue

            $result = Invoke-CiFixAzdoRequest `
                -Uri "http://127.0.0.1:$port/start" `
                -MaxBytes 10000

            $result.Succeeded | Should -BeFalse
            $result.StatusCode | Should -Be 302
            $result.RedirectLocation | Should -BeExactly $redirectLocation
            $result.Content | Should -BeExactly ''
            $result.Error | Should -BeExactly 'HTTP 302'
        }
        finally {
            Wait-Job -Job $serverJob -Timeout 5 | Out-Null
            Remove-Job -Job $serverJob -Force
        }
    }

    It 'deduplicates builds across multiple issues and pipelines' {
        $snapshotPath = Join-Path $TestDrive 'candidates.json'
        $destination = Join-Path $TestDrive 'evidence'
        @{
            schemaVersion = 2
            repository = 'dotnet/maui'
            issueEvidence = @{
                authoritative = $true
                issues = @(
                    @{ issueNumber = 1; body = "- **Pipeline**: maui-pr`n- **Build ID**: 500" }
                    @{ issueNumber = 2; body = "- **Pipeline**: maui-pr`n- **Build ID**: 500" }
                    @{ issueNumber = 3; body = "- **Pipeline**: maui-pr-uitests`n- **Build ID**: 600" }
                )
            }
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $snapshotPath

        $requestedUris = [Collections.Generic.List[string]]::new()
        $requestInvoker = {
            param($Uri, $MaxBytes)
            $requestedUris.Add($Uri)
            if ($Uri -match 'definitions=302') {
                return [pscustomobject]@{
                    Succeeded = $true; StatusCode = 200; Truncated = $false; Error = ''; Attempts = 1
                    Content = '{"value":[{"id":500,"result":"failed","finishTime":"2026-10-07T00:00:00Z","sourceVersion":"a","sourceBranch":"refs/heads/main"}]}'
                }
            }

            if ($Uri -match 'definitions=313') {
                return [pscustomobject]@{
                    Succeeded = $true; StatusCode = 200; Truncated = $false; Error = ''; Attempts = 1
                    Content = '{"value":[{"id":600,"result":"failed","finishTime":"2026-10-07T00:00:00Z","sourceVersion":"b","sourceBranch":"refs/heads/main"}]}'
                }
            }
            if ($Uri -match '/timeline') {
                return [pscustomobject]@{
                    Succeeded = $true; StatusCode = 200; Truncated = $false; Error = ''; Attempts = 1
                    Content = '{"records":[]}'
                }
            }
            throw "Unexpected URI: $Uri"
        }

        $manifest = New-CiFixAzdoEvidence `
            -InputPath $snapshotPath `
            -Destination $destination `
            -TargetBranch main `
            -BuildLimit 5 `
            -FailedLogLimit 10 `
            -TimelineByteLimit 10000 `
            -LogByteLimit 10000 `
            -RequestInvoker $requestInvoker

        @($manifest.issues).Count | Should -Be 3
        @($manifest.pipelines).Count | Should -Be 2
        @($manifest.builds).Count | Should -Be 2
        @($requestedUris | Where-Object { $_ -match '/builds/500/timeline' }).Count | Should -Be 1
        @($requestedUris | Where-Object { $_ -match '/builds/600/timeline' }).Count | Should -Be 1
    }

    It 'prefetches head-SHA-matched PR builds for advance mode' {
        $snapshotPath = Join-Path $TestDrive 'candidates.json'
        $destination = Join-Path $TestDrive 'evidence'
        $headSha = '1111111111111111111111111111111111111111'
        @{
            schemaVersion = 2
            repository = 'dotnet/maui'
            issueEvidence = @{
                authoritative = $true
                issues = @()
            }
            candidates = @(
                @{ prNumber = 123; headSha = $headSha }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $snapshotPath

        $requestInvoker = {
            param($Uri, $MaxBytes)
            if ($Uri -match 'refs%2Fpull%2F123%2Fmerge') {
                return [pscustomobject]@{
                    Succeeded = $true; StatusCode = 200; Truncated = $false; Error = ''; Attempts = 1
                    Content = (@{
                            value = @(
                                @{
                                    id = 700
                                    definition = @{ id = 302; name = 'maui-pr' }
                                    result = 'failed'
                                    finishTime = '2026-10-07T00:00:00Z'
                                    sourceVersion = 'merge'
                                    sourceBranch = 'refs/pull/123/merge'
                                    triggerInfo = @{ 'pr.sourceSha' = $headSha }
                                },
                                @{
                                    id = 701
                                    definition = @{ id = 302; name = 'maui-pr' }
                                    result = 'failed'
                                    finishTime = '2026-10-06T00:00:00Z'
                                    sourceVersion = 'old-merge'
                                    sourceBranch = 'refs/pull/123/merge'
                                    triggerInfo = @{ 'pr.sourceSha' = '2222222222222222222222222222222222222222' }
                                }
                            )
                        } | ConvertTo-Json -Depth 10 -Compress)
                }
            }
            if ($Uri -match '/builds/700/timeline') {
                return [pscustomobject]@{
                    Succeeded = $true; StatusCode = 200; Truncated = $false; Error = ''; Attempts = 1
                    Content = '{"records":[]}'
                }
            }
            throw "Unexpected URI: $Uri"
        }

        $manifest = New-CiFixAzdoEvidence `
            -InputPath $snapshotPath `
            -Destination $destination `
            -TargetBranch main `
            -BuildLimit 5 `
            -FailedLogLimit 10 `
            -TimelineByteLimit 10000 `
            -LogByteLimit 10000 `
            -RequestInvoker $requestInvoker

        @($manifest.pullRequests).Count | Should -Be 1
        @($manifest.pullRequests[0].builds).Count | Should -Be 1
        $manifest.pullRequests[0].builds[0].buildId | Should -Be 700
        $manifest.pullRequests[0].builds[0].prSourceSha | Should -BeExactly $headSha
    }

    It 'fails closed when the global build bound is reached' {
        $evidence = @{}
        $evidence[1L] = [pscustomobject]@{ buildId = 1; complete = $true }

        $result = Get-CiFixBuildEvidenceBounded `
            -BuildId 2 `
            -Evidence $evidence `
            -BuildLimit 1 `
            -OutputDirectory $TestDrive `
            -MaxFailedLogs 10 `
            -TimelineByteLimit 10000 `
            -LogByteLimit 10000

        $result.complete | Should -BeFalse
        $result.limitReached | Should -BeTrue
        $result.timeline.status | Should -BeExactly 'not_fetched_limit'
        $result.timeline.error | Should -BeExactly 'global build limit 1 reached'
    }

    It 'makes no request after the total download bound is exhausted' {
        $script:RemainingDownloadBytes = 0

        $result = Invoke-CiFixAzdoRequest `
            -Uri 'https://dev.azure.com/dnceng-public/public/_apis/build/builds/1/timeline?api-version=7.1' `
            -MaxBytes 10000

        $result.Succeeded | Should -BeFalse
        $result.Attempts | Should -Be 0
        $result.Error | Should -BeExactly 'total download limit exhausted'
    }
}

Describe 'CI-fixer workflow evidence wiring' {
    It 'keeps both branch twins on the same trusted producer contract' {
        $mainWorkflow = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../workflows/ci-status-fix.md')
        $net11Workflow = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../workflows/ci-status-fix-net11.md')

        foreach ($workflow in @($mainWorkflow, $net11Workflow)) {
            $workflow | Should -Match 'Prefetch bounded Azure DevOps failure evidence'
            $workflow | Should -Match '\.github/scripts/Get-CiFixAzdoEvidence\.ps1'
            $workflow | Should -Match '/tmp/gh-aw/agent/azdo-evidence'
            $workflow | Should -Match 'Do NOT improvise a second\s+Azure downloader'
            $workflow | Should -Match 'retrieval failure into a successful/noop'
            $workflow | Should -Match 'Use only the earlier-attempt logs already'
            $workflow | Should -Match 'Use only the recent-build logs'
            $workflow | Should -Not -Match 'Fetch those failed earlier-attempt log\(s\)'
        }

        $mainWorkflow | Should -Match '(?s)Get-CiFixAzdoEvidence\.ps1.*?-Branch main'
        $net11Workflow | Should -Match '(?s)Get-CiFixAzdoEvidence\.ps1.*?-Branch net11\.0'
    }
}

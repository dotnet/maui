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
            'Get-CiFixJsonArrayProperty',
            'Get-CiFixFailedTaskRecords',
            'Get-CiFixPreviousAttemptReferences',
            'Resolve-CiFixPreviousAttemptTaskProvenance',
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
    $script:ProducerDeadlineUtc = [DateTime]::UtcNow.AddMinutes(10)
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

    It 'selects partially succeeded and error-issue task logs' {
        $timeline = [pscustomobject]@{
            records = @(
                [pscustomobject]@{
                    id = '1'
                    type = 'Task'
                    result = 'partiallySucceeded'
                    name = 'Partial'
                    log = [pscustomobject]@{ id = 179 }
                }
                [pscustomobject]@{
                    id = '2'
                    type = 'Task'
                    result = 'succeeded'
                    name = 'Issue failure'
                    issues = @([pscustomobject]@{ type = 'error' })
                    log = [pscustomobject]@{ id = 180 }
                }
                [pscustomobject]@{
                    id = '3'
                    type = 'Task'
                    result = 'succeeded'
                    name = 'Passed'
                    issues = @([pscustomobject]@{ type = 'warning' })
                    log = [pscustomobject]@{ id = 181 }
                }
            )
        }

        $result = @(Get-CiFixFailedTaskRecords -Timeline $timeline)

        $result.Count | Should -Be 2
        $result.logId | Should -Be @(179, 180)
    }

    It 'marks structurally malformed timelines incomplete' -ForEach @(
        @{ Name = 'empty body'; Content = '' }
        @{ Name = 'JSON null'; Content = 'null' }
        @{ Name = 'missing records'; Content = '{}' }
        @{ Name = 'null records'; Content = '{"records":null}' }
        @{ Name = 'object records'; Content = '{"records":{}}' }
    ) {
        $destination = Join-Path $TestDrive "malformed-timeline-$Name"
        $requestInvoker = {
            param($Uri, $MaxBytes)
            [pscustomobject]@{
                Succeeded = $true
                StatusCode = 200
                Content = $Content
                Truncated = $false
                Error = ''
                Attempts = 1
            }
        }

        $result = Get-CiFixBuildEvidence `
            -BuildId 42 `
            -OutputDirectory $destination `
            -MaxFailedLogs 10 `
            -TimelineByteLimit 10000 `
            -LogByteLimit 10000 `
            -RequestInvoker $requestInvoker

        $result.complete | Should -BeFalse
        $result.timeline.status | Should -BeExactly 'malformed'
        $result.timeline.error | Should -Match 'records'
        $result.failedTaskCount | Should -Be 0
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

    It 'cancels a response body that stalls after sending headers' {
        $port = Get-Random -Minimum 30000 -Maximum 45000
        $readyPath = Join-Path $TestDrive 'stall-server.ready'
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
                    $response = "HTTP/1.1 200 OK`r`nContent-Length: 100`r`nConnection: close`r`n`r`n"
                    $bytes = [Text.Encoding]::ASCII.GetBytes($response)
                    $stream.Write($bytes, 0, $bytes.Length)
                    Start-Sleep -Seconds 5
                }
                finally {
                    $client.Dispose()
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

            $requestJob = Start-Job -ArgumentList $scriptPath, $port -ScriptBlock {
                param($ScriptPath, $Port)

                . $ScriptPath
                $script:RemainingDownloadBytes = 10000
                $script:MaxHttpAttempts = 1
                $script:HttpTimeoutSeconds = 1
                $script:ProducerDeadlineUtc = [DateTime]::UtcNow.AddSeconds(1)
                Invoke-CiFixAzdoRequest -Uri "http://127.0.0.1:$Port/stall" -MaxBytes 10000
            }
            try {
                Wait-Job -Job $requestJob -Timeout 3 | Should -Not -BeNullOrEmpty
                $result = Receive-Job -Job $requestJob
                $result.Succeeded | Should -BeFalse
                $result.StatusCode | Should -Be 200
                $result.Error | Should -Match 'deadline exhausted'
            }
            finally {
                Stop-Job -Job $requestJob -ErrorAction SilentlyContinue
                Remove-Job -Job $requestJob -Force
            }
        }
        finally {
            Wait-Job -Job $serverJob -Timeout 7 | Out-Null
            Remove-Job -Job $serverJob -Force
        }
    }

    It 'debits bytes consumed before a response body cancellation' {
        $port = Get-Random -Minimum 30000 -Maximum 45000
        $readyPath = Join-Path $TestDrive 'partial-stall-server.ready'
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
                    $headers = "HTTP/1.1 200 OK`r`nContent-Length: 100`r`nConnection: close`r`n`r`n"
                    $headerBytes = [Text.Encoding]::ASCII.GetBytes($headers)
                    $stream.Write($headerBytes, 0, $headerBytes.Length)
                    $bodyBytes = [Text.Encoding]::ASCII.GetBytes('hello')
                    $stream.Write($bodyBytes, 0, $bodyBytes.Length)
                    $stream.Flush()
                    Start-Sleep -Seconds 5
                }
                finally {
                    $client.Dispose()
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

            $requestJob = Start-Job -ArgumentList $scriptPath, $port -ScriptBlock {
                param($ScriptPath, $Port)

                . $ScriptPath
                $script:RemainingDownloadBytes = 100
                $script:MaxHttpAttempts = 1
                $script:HttpTimeoutSeconds = 1
                $script:ProducerDeadlineUtc = [DateTime]::UtcNow.AddSeconds(2)
                $request = Invoke-CiFixAzdoRequest `
                    -Uri "http://127.0.0.1:$Port/partial-stall" `
                    -MaxBytes 100
                [pscustomobject]@{
                    Succeeded = $request.Succeeded
                    StatusCode = $request.StatusCode
                    Error = $request.Error
                    RemainingDownloadBytes = $script:RemainingDownloadBytes
                }
            }
            try {
                Wait-Job -Job $requestJob -Timeout 3 | Should -Not -BeNullOrEmpty
                $result = Receive-Job -Job $requestJob
                $result.Succeeded | Should -BeFalse
                $result.StatusCode | Should -Be 200
                $result.Error | Should -Match 'deadline exhausted'
                $result.RemainingDownloadBytes | Should -Be 95
            }
            finally {
                Stop-Job -Job $requestJob -ErrorAction SilentlyContinue
                Remove-Job -Job $requestJob -Force
            }
        }
        finally {
            Wait-Job -Job $serverJob -Timeout 7 | Out-Null
            Remove-Job -Job $serverJob -Force
        }
    }

    It 'prefetches failed task logs from referenced previous-attempt timelines' {
        $destination = Join-Path $TestDrive 'previous-attempt'
        $previousTimelineId = '11111111-1111-1111-1111-111111111111'
        $previousRecordId = '22222222-2222-2222-2222-222222222222'
        $secondPreviousRecordId = '55555555-5555-5555-5555-555555555555'
        $requestedUris = [Collections.Generic.List[string]]::new()
        $requestInvoker = {
            param($Uri, $MaxBytes)
            $requestedUris.Add($Uri)
            if ($Uri -match '/timeline\?') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = (@{
                            id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
                            records = @(
                                @{
                                    id = '33333333-3333-3333-3333-333333333333'
                                    type = 'Job'
                                    result = 'succeeded'
                                    name = 'retried job'
                                    previousAttempts = @(
                                        @{
                                            timelineId = $previousTimelineId
                                            recordId = $previousRecordId
                                            attempt = 1
                                        }
                                    )
                                },
                                @{
                                    id = '66666666-6666-6666-6666-666666666666'
                                    type = 'Job'
                                    result = 'succeeded'
                                    name = 'second retried job'
                                    previousAttempts = @(
                                        @{
                                            timelineId = $previousTimelineId
                                            recordId = $secondPreviousRecordId
                                            attempt = 1
                                        }
                                    )
                                }
                            )
                        } | ConvertTo-Json -Depth 10 -Compress)
                    Truncated = $false
                    Error = ''
                    Attempts = 1
                }
            }
            if ($Uri -match [regex]::Escape("/timeline/$previousTimelineId")) {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = (@{
                            id = $previousTimelineId
                            records = @(
                                @{
                                    id = $previousRecordId
                                    type = 'Job'
                                    result = 'failed'
                                    name = 'retried job'
                                },
                                @{
                                    id = '44444444-4444-4444-4444-444444444444'
                                    parentId = $previousRecordId
                                    type = 'Task'
                                    result = 'failed'
                                    name = 'failed first attempt'
                                    log = @{ id = 77 }
                                },
                                @{
                                    id = $secondPreviousRecordId
                                    type = 'Job'
                                    result = 'failed'
                                    name = 'second retried job'
                                },
                                @{
                                    id = '77777777-7777-7777-7777-777777777777'
                                    parentId = $secondPreviousRecordId
                                    type = 'Task'
                                    result = 'failed'
                                    name = 'second failed first attempt'
                                    log = @{ id = 78 }
                                }
                            )
                        } | ConvertTo-Json -Depth 10 -Compress)
                    Truncated = $false
                    Error = ''
                    Attempts = 1
                }
            }
            if ($Uri -match '/logs/(77|78)') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = 'first-attempt failure signature'
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

        $result.complete | Should -BeTrue
        $result.previousAttemptsComplete | Should -BeTrue
        $result.previousAttemptCount | Should -Be 2
        $result.previousAttemptTimelineCount | Should -Be 1
        $result.previousAttempts[0].referenceCount | Should -Be 2
        $result.failedTaskCount | Should -Be 2
        $result.failedTasks[0].source | Should -BeExactly 'previous_attempt'
        $result.failedTasks[0].sourceTimelineId | Should -BeExactly $previousTimelineId
        $result.failedTasks[0].sourceRecordId | Should -BeExactly $previousRecordId
        $result.failedTasks[0].sourceAttempt | Should -Be 1
        $result.failedTasks[0].status | Should -BeExactly 'available'
        $result.failedTasks[1].source | Should -BeExactly 'previous_attempt'
        $result.failedTasks[1].sourceTimelineId | Should -BeExactly $previousTimelineId
        $result.failedTasks[1].sourceRecordId | Should -BeExactly $secondPreviousRecordId
        $result.failedTasks[1].sourceAttempt | Should -Be 1
        $result.failedTasks[1].status | Should -BeExactly 'available'
        @($requestedUris | Where-Object { $_ -match [regex]::Escape("/timeline/$previousTimelineId") }).Count |
            Should -Be 1
    }

    It 'fails closed when previous-attempt task provenance is ambiguous unresolved or cyclic' {
        $destination = Join-Path $TestDrive 'invalid-previous-attempt-provenance'
        $previousTimelineId = '11111111-1111-1111-1111-111111111111'
        $previousRecordId = '22222222-2222-2222-2222-222222222222'
        $requestedUris = [Collections.Generic.List[string]]::new()
        $requestInvoker = {
            param($Uri, $MaxBytes)
            $requestedUris.Add($Uri)
            if ($Uri -match '/timeline\?') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = (@{
                            records = @(
                                @{
                                    id = '33333333-3333-3333-3333-333333333333'
                                    type = 'Job'
                                    result = 'succeeded'
                                    previousAttempts = @(
                                        @{
                                            timelineId = $previousTimelineId
                                            recordId = $previousRecordId
                                            attempt = 1
                                        }
                                    )
                                },
                                @{
                                    id = '44444444-4444-4444-4444-444444444444'
                                    type = 'Job'
                                    result = 'succeeded'
                                    previousAttempts = @(
                                        @{
                                            timelineId = $previousTimelineId
                                            recordId = $previousRecordId
                                            attempt = 2
                                        }
                                    )
                                }
                            )
                        } | ConvertTo-Json -Depth 10 -Compress)
                    Truncated = $false
                    Error = ''
                    Attempts = 1
                }
            }
            if ($Uri -match [regex]::Escape("/timeline/$previousTimelineId")) {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = (@{
                            id = $previousTimelineId
                            records = @(
                                @{
                                    id = $previousRecordId
                                    type = 'Job'
                                    result = 'failed'
                                    name = 'ambiguous referenced job'
                                },
                                @{
                                    id = '55555555-5555-5555-5555-555555555555'
                                    parentId = $previousRecordId
                                    type = 'Task'
                                    result = 'failed'
                                    name = 'ambiguous failed task'
                                    log = @{ id = 77 }
                                },
                                @{
                                    id = '66666666-6666-6666-6666-666666666666'
                                    type = 'Job'
                                    result = 'failed'
                                    name = 'unreferenced job'
                                },
                                @{
                                    id = '77777777-7777-7777-7777-777777777777'
                                    parentId = '66666666-6666-6666-6666-666666666666'
                                    type = 'Task'
                                    result = 'failed'
                                    name = 'unresolved failed task'
                                    log = @{ id = 78 }
                                },
                                @{
                                    id = '88888888-8888-8888-8888-888888888888'
                                    parentId = '99999999-9999-9999-9999-999999999999'
                                    type = 'Task'
                                    result = 'failed'
                                    name = 'cyclic failed task'
                                    log = @{ id = 79 }
                                },
                                @{
                                    id = '99999999-9999-9999-9999-999999999999'
                                    parentId = '88888888-8888-8888-8888-888888888888'
                                    type = 'Job'
                                    result = 'failed'
                                    name = 'cyclic job'
                                }
                            )
                        } | ConvertTo-Json -Depth 10 -Compress)
                    Truncated = $false
                    Error = ''
                    Attempts = 1
                }
            }
            if ($Uri -match '/logs/') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = 'must not be accepted without provenance'
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
        $result.previousAttemptsComplete | Should -BeFalse
        $result.failedTaskCount | Should -Be 3
        @($result.failedTasks | Where-Object status -EQ 'unavailable').Count | Should -Be 3
        ($result.failedTasks.error -join "`n") | Should -Match 'ambiguous'
        ($result.failedTasks.error -join "`n") | Should -Match 'no referenced ancestor'
        ($result.failedTasks.error -join "`n") | Should -Match 'cycle'
        @($requestedUris | Where-Object { $_ -match '/logs/' }).Count | Should -Be 0
    }

    It 'uses the nearest uniquely referenced ancestor for previous-attempt provenance' {
        $timelineId = '11111111-1111-1111-1111-111111111111'
        $outerRecordId = '22222222-2222-2222-2222-222222222222'
        $innerRecordId = '33333333-3333-3333-3333-333333333333'
        $timeline = [pscustomobject]@{
            records = @(
                [pscustomobject]@{
                    id = $outerRecordId
                    type = 'Stage'
                    result = 'failed'
                }
                [pscustomobject]@{
                    id = $innerRecordId
                    parentId = $outerRecordId
                    type = 'Job'
                    result = 'failed'
                }
                [pscustomobject]@{
                    id = '44444444-4444-4444-4444-444444444444'
                    parentId = $innerRecordId
                    type = 'Task'
                    result = 'failed'
                    name = 'nested failure'
                    log = [pscustomobject]@{ id = 77 }
                }
            )
        }
        $references = @(
            [pscustomobject]@{ recordId = $outerRecordId; attempt = 1 }
            [pscustomobject]@{ recordId = $innerRecordId; attempt = 2 }
        )

        $result = Resolve-CiFixPreviousAttemptTaskProvenance `
            -Timeline $timeline `
            -References $references `
            -SourceTimelineId $timelineId

        $result.complete | Should -BeTrue
        $result.tasks.Count | Should -Be 1
        $result.tasks[0].sourceRecordId | Should -BeExactly $innerRecordId
        $result.tasks[0].sourceAttempt | Should -Be 2
    }

    It 'marks a build incomplete when a referenced previous-attempt timeline is unavailable' {
        $destination = Join-Path $TestDrive 'missing-previous-attempt'
        $previousTimelineId = '11111111-1111-1111-1111-111111111111'
        $requestInvoker = {
            param($Uri, $MaxBytes)
            if ($Uri -match '/timeline\?') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = (@{
                            records = @(
                                @{
                                    id = '33333333-3333-3333-3333-333333333333'
                                    type = 'Job'
                                    result = 'succeeded'
                                    previousAttempts = @(
                                        @{
                                            timelineId = $previousTimelineId
                                            recordId = '22222222-2222-2222-2222-222222222222'
                                            attempt = 1
                                        }
                                    )
                                }
                            )
                        } | ConvertTo-Json -Depth 10 -Compress)
                    Truncated = $false
                    Error = ''
                    Attempts = 1
                }
            }
            if ($Uri -match [regex]::Escape("/timeline/$previousTimelineId")) {
                return [pscustomobject]@{
                    Succeeded = $false
                    StatusCode = 404
                    Content = ''
                    Truncated = $false
                    Error = 'HTTP 404'
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
        $result.previousAttemptsComplete | Should -BeFalse
        $result.previousAttempts[0].status | Should -BeExactly 'error'
        $result.previousAttempts[0].httpStatus | Should -Be 404
        $result.failedTaskCount | Should -Be 0
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

    It 'marks malformed latest-build query collections and selected IDs incomplete' -ForEach @(
        @{ Name = 'missing value'; Content = '{}' }
        @{ Name = 'null value'; Content = '{"value":null}' }
        @{ Name = 'object value'; Content = '{"value":{}}' }
        @{ Name = 'invalid selected ID'; Content = '{"value":[{"id":"invalid","result":"failed"}]}' }
    ) {
        $snapshotPath = Join-Path $TestDrive "latest-query-$Name.json"
        $destination = Join-Path $TestDrive "latest-query-$Name"
        @{
            schemaVersion = 2
            repository = 'dotnet/maui'
            issueEvidence = @{
                authoritative = $true
                issues = @(
                    @{ issueNumber = 1; body = "- **Pipeline**: maui-pr" }
                )
            }
            candidates = @()
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $snapshotPath

        $requestInvoker = {
            param($Uri, $MaxBytes)
            if ($Uri -match 'definitions=302') {
                return [pscustomobject]@{
                    Succeeded = $true
                    StatusCode = 200
                    Content = $Content
                    Truncated = $false
                    Error = ''
                    Attempts = 1
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

        $manifest.pipelines[0].queryComplete | Should -BeFalse
        $manifest.pipelines[0].error | Should -Not -BeNullOrEmpty
        @($manifest.pipelines[0].builds).Count | Should -Be 0
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

    It 'marks malformed PR-build query collections and matching invalid IDs incomplete' -ForEach @(
        @{ Name = 'missing value'; Content = '{}' }
        @{ Name = 'null value'; Content = '{"value":null}' }
        @{ Name = 'object value'; Content = '{"value":{}}' }
        @{
            Name = 'matching invalid ID'
            Content = '{"value":[{"id":"invalid","triggerInfo":{"pr.sourceSha":"1111111111111111111111111111111111111111"}}]}'
        }
    ) {
        $snapshotPath = Join-Path $TestDrive "pr-query-$Name.json"
        $destination = Join-Path $TestDrive "pr-query-$Name"
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
                    Succeeded = $true
                    StatusCode = 200
                    Content = $Content
                    Truncated = $false
                    Error = ''
                    Attempts = 1
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

        $manifest.pullRequests[0].queryComplete | Should -BeFalse
        $manifest.pullRequests[0].error | Should -Not -BeNullOrEmpty
        @($manifest.pullRequests[0].builds).Count | Should -Be 0
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

    It 'debits downloaded bytes during each bounded read rather than after success' {
        $source = Get-Content -Raw -LiteralPath $scriptPath

        $source | Should -Match '\$readLimit\s*=\s*\[int\]\[Math\]::Min'
        $source | Should -Match 'ReadAsync\(\s*\$buffer,\s*0,\s*\$readLimit,'
        $source | Should -Match '\$script:RemainingDownloadBytes\s*-=\s*\$read'
        $source | Should -Not -Match '\$script:RemainingDownloadBytes\s*-=\s*\$bytes\.Length'
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
            $workflow | Should -Match 'Inspect the newest completed build first'
            $workflow | Should -Match '(?s)If that newest build.*?`evidenceComplete` is false'
            $workflow | Should -Not -Match 'Pick the latest completed build whose `evidenceComplete` is true'
            $workflow | Should -Match 'Do not skip an incomplete newest matching PR build'
        }

        $mainWorkflow | Should -Match "github\.event_name != 'workflow_dispatch' \|\| github\.ref == 'refs/heads/main'"
        $net11Workflow | Should -Match "github\.event_name != 'workflow_dispatch' \|\| github\.ref == 'refs/heads/main'"
        $net11Workflow | Should -Not -Match "github\.event_name != 'workflow_dispatch' \|\| github\.ref == 'refs/heads/net11\.0'"
        $mainWorkflow | Should -Match '(?s)Get-CiFixAzdoEvidence\.ps1.*?-Branch main'
        $net11Workflow | Should -Match '(?s)Get-CiFixAzdoEvidence\.ps1.*?-Branch net11\.0'
    }
}

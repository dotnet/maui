param([string]$ForwardScriptPath)

BeforeAll {
    $forwardScript = if ($ForwardScriptPath) { $ForwardScriptPath } else {
        Join-Path $PSScriptRoot 'Sync-ServicingForwardPullRequest.ps1'
    }
    function New-FixtureSha {
        $global:MauiForwardFixture.Counter++
        return $global:MauiForwardFixture.Counter.ToString('x40')
    }
    function git {
        param([Parameter(ValueFromRemainingArguments)][string[]]$GitArguments)
        if ($GitArguments[0] -ceq 'check-ref-format') {
            $global:LASTEXITCODE = 0
            return
        }
        if ($GitArguments[0] -cne 'ls-remote') { throw 'Unexpected Git operation in forward fixture.' }
        $branch = $GitArguments[-1].Substring('refs/heads/'.Length)
        $global:LASTEXITCODE = 0
        if ($global:MauiForwardFixture.Refs.ContainsKey($branch)) {
            "$($global:MauiForwardFixture.Refs[$branch])`trefs/heads/$branch"
        }
    }
    function gh {
        [CmdletBinding(PositionalBinding = $false)]
        param(
            [Parameter(Position = 0, ValueFromRemainingArguments)][string[]]$GhArguments,
            [Parameter(ValueFromPipeline)][string]$JsonInput
        )
        process {
            $server = $global:MauiForwardFixture
            $endpoint = [Uri]::UnescapeDataString($GhArguments[1])
            $method = 'GET'
            $fields = @{}
            for ($i = 2; $i -lt $GhArguments.Count; $i++) {
                if ($GhArguments[$i] -eq '--method') { $method = $GhArguments[++$i] }
                elseif ($GhArguments[$i] -in @('-f', '-F')) {
                    $pair = $GhArguments[++$i].Split('=', 2)
                    $fields[$pair[0]] = $pair[1]
                }
            }
            $payload = if ($JsonInput) { $JsonInput | ConvertFrom-Json } else { $null }
            $global:LASTEXITCODE = 0
            $response = $null
            if ($endpoint -match '/contents/eng/Versions.props$') {
                $major = if ($fields.ref -ceq $server.Target) { 12 } else { 11 }
                $label = if ($server.Servicing -and $major -eq 11) { 'servicing' } else { 'ci.main' }
                $stable = if ($server.Servicing -and $major -eq 11) { 'true' } else { 'false' }
                $xml = "<Project><PropertyGroup><MajorVersion>$major</MajorVersion><PreReleaseVersionLabel>$label</PreReleaseVersionLabel><StabilizePackageVersion>$stable</StabilizePackageVersion></PropertyGroup></Project>"
                $response = @{ encoding = 'base64'; content = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($xml)) }
            }
            elseif ($endpoint -match '/compare/(.+)\.\.\.(.+)$') {
                $response = @{ ahead_by = 1; behind_by = 0; status = 'ahead' }
            }
            elseif ($endpoint -match '/git/trees$' -and $method -eq 'POST') {
                $server.Writes++
                $blob = New-FixtureSha
                $server.Blobs[$blob] = [string]$payload.tree[0].content
                $sha = New-FixtureSha
                $server.Trees[$sha] = @{ tree = @(@{ path = 'forward-ownership.json'; type = 'blob'; mode = '100644'; sha = $blob }) }
                $response = @{ sha = $sha }
            }
            elseif ($endpoint -match '/git/commits$' -and $method -eq 'POST') {
                $server.Writes++
                $sha = New-FixtureSha
                $server.Commits[$sha] = @{ tree = @{ sha = $payload.tree }; message = $payload.message }
                $response = @{ sha = $sha }
            }
            elseif ($endpoint -match '/git/commits/([0-9a-f]{40})$') { $response = $server.Commits[$Matches[1]] }
            elseif ($endpoint -match '/git/trees/([0-9a-f]{40})$') { $response = $server.Trees[$Matches[1]] }
            elseif ($endpoint -match '/git/blobs/([0-9a-f]{40})$') {
                $bytes = [Text.Encoding]::UTF8.GetBytes($server.Blobs[$Matches[1]])
                $response = @{ encoding = 'base64'; size = $bytes.Length; content = [Convert]::ToBase64String($bytes) }
            }
            elseif ($endpoint -match '/git/refs(?:/heads/(.+))?$') {
                $branch = if ($method -eq 'POST') { $fields.ref.Substring('refs/heads/'.Length) } else { $Matches[1] }
                if ($branch -ceq $server.Head -and $server.FailHead -gt 0) {
                    $server.FailHead--
                    $global:LASTEXITCODE = 1
                    return 'Injected head publication failure'
                }
                $server.Writes++
                $server.Refs[$branch] = $fields.sha
                $response = @{ ref = "refs/heads/$branch"; object = @{ sha = $fields.sha } }
            }
            elseif ($endpoint -match '/pulls$' -and $method -eq 'GET') {
                $response = @($server.Prs | Where-Object state -eq $fields.state)
            }
            elseif ($endpoint -match '/pulls$' -and $method -eq 'POST') {
                if ($server.FailPr -gt 0) {
                    $server.FailPr--
                    $global:LASTEXITCODE = 1
                    return 'Injected PR creation failure'
                }
                $server.Writes++
                $response = [pscustomobject]@{
                    number = 100 + $server.Prs.Count; html_url = 'https://fixture.invalid/pr'
                    title = $fields.title; body = $fields.body; state = 'open'
                    head = @{ sha = $server.Refs[$server.Head] }
                }
                $server.Prs.Add($response)
            }
            elseif ($endpoint -match '/pulls/(\d+)$' -and $method -eq 'PATCH') {
                $server.Writes++
                $response = $server.Prs | Where-Object number -eq ([int]$Matches[1])
                $response.title = $fields.title
                $response.body = $fields.body
            }
            else { throw "Unexpected forward fixture API operation: $method $endpoint" }
            ConvertTo-Json -InputObject $response -Depth 12
        }
    }
    function Invoke-ForwardFixture {
        param([switch]$PlanOnly)
        $output = & $forwardScript -StatePath $statePath -PlanOnly:$PlanOnly
        ($output -join "`n") | ConvertFrom-Json
    }
}

Describe 'Servicing forward publication recovery' {
    BeforeEach {
        $source = '1' * 40
        $target = '2' * 40
        $head = 'automation/forward-servicing-fixes'
        $global:MauiForwardFixture = @{
            Source = $source; Target = $target; Head = $head
            Refs = @{ net11 = $source; main = $target }
            Trees = @{}; Commits = @{}; Blobs = @{}
            Prs = [Collections.Generic.List[object]]::new()
            Counter = 1000; Writes = 0; FailPr = 0; FailHead = 0; Servicing = $false
        }
        $statePath = Join-Path $TestDrive 'state.json'
        @{
            schemaVersion = 1; repository = 'https://github.com/dotnet/maui.git'
            releaseBand = '11.0.1xx'; currentSr = 1; fixBranch = 'net11'; featureBranch = 'main'
            candidateNamingPolicy = 'unresolved'; nightlyMode = 'legacy'
        } | ConvertTo-Json | Set-Content $statePath
    }

    AfterAll { Remove-Variable MauiForwardFixture -Scope Global }

    It 'recovers first-time ref publication after a failed PR creation, then becomes unchanged' {
        $global:MauiForwardFixture.FailPr = 1
        { Invoke-ForwardFixture } | Should -Throw '*forward-flow request failed*'
        $global:MauiForwardFixture.Refs[$head] | Should -Be $source
        (Invoke-ForwardFixture).status | Should -Be 'created'
        $global:MauiForwardFixture.Refs.ContainsKey("$head-ownership") | Should -BeTrue
        $writes = $global:MauiForwardFixture.Writes
        (Invoke-ForwardFixture).status | Should -Be 'unchanged'
        $global:MauiForwardFixture.Writes | Should -Be $writes
        $global:MauiForwardFixture.Prs.Count | Should -Be 1
    }

    It 'recovers an advanced ref after the earlier managed PR closed and the new PR failed' {
        $null = Invoke-ForwardFixture
        $global:MauiForwardFixture.Prs[0].state = 'closed'
        $advanced = '3' * 40
        $global:MauiForwardFixture.Refs.net11 = $advanced
        $global:MauiForwardFixture.FailPr = 1
        { Invoke-ForwardFixture } | Should -Throw '*forward-flow request failed*'
        $global:MauiForwardFixture.Refs[$head] | Should -Be $advanced
        (Invoke-ForwardFixture).status | Should -Be 'created'
        @($global:MauiForwardFixture.Prs | Where-Object state -eq open).Count | Should -Be 1
    }

    It 'reuses the checkpoint if the initial head write fails' {
        $global:MauiForwardFixture.FailHead = 1
        { Invoke-ForwardFixture } | Should -Throw '*forward-flow request failed*'
        $checkpoint = $global:MauiForwardFixture.Refs["$head-ownership"]
        $checkpoint | Should -Not -BeNullOrEmpty
        $global:MauiForwardFixture.Refs.ContainsKey($head) | Should -BeFalse
        (Invoke-ForwardFixture).status | Should -Be 'created'
        $global:MauiForwardFixture.Refs["$head-ownership"] | Should -Be $checkpoint
    }

    It 'refreshes the same open managed PR when fixes advance' {
        $created = Invoke-ForwardFixture
        $global:MauiForwardFixture.Refs.net11 = '3' * 40
        $updated = Invoke-ForwardFixture
        $updated.status | Should -Be 'updated'
        $updated.number | Should -Be $created.number
        $global:MauiForwardFixture.Prs.Count | Should -Be 1
        (Invoke-ForwardFixture).status | Should -Be 'unchanged'
    }

    It 'never adopts an unmanaged head even if it equals the fixes snapshot' {
        $global:MauiForwardFixture.Refs[$head] = $source
        { Invoke-ForwardFixture } | Should -Throw '*no managed PR proving*'
        $global:MauiForwardFixture.Writes | Should -Be 0
    }

    It 'refuses a checkpoint for a different route' {
        $global:MauiForwardFixture.FailPr = 1
        { Invoke-ForwardFixture } | Should -Throw '*forward-flow request failed*'
        $blob = @($global:MauiForwardFixture.Blobs.Keys)[0]
        $record = $global:MauiForwardFixture.Blobs[$blob] | ConvertFrom-Json
        $record.headBranch = 'unmanaged'
        $global:MauiForwardFixture.Blobs[$blob] = $record | ConvertTo-Json
        $writes = $global:MauiForwardFixture.Writes
        { Invoke-ForwardFixture } | Should -Throw '*does not match the managed route*'
        $global:MauiForwardFixture.Writes | Should -Be $writes
    }

    It 'plans recovery without writing any ref or PR' {
        $global:MauiForwardFixture.FailPr = 1
        { Invoke-ForwardFixture } | Should -Throw '*forward-flow request failed*'
        $writes = $global:MauiForwardFixture.Writes
        (Invoke-ForwardFixture -PlanOnly).status | Should -Be 'would-create'
        $global:MauiForwardFixture.Writes | Should -Be $writes
    }

    It 'rejects checkpoint trees that contain files outside the ownership record' {
        $global:MauiForwardFixture.FailPr = 1
        { Invoke-ForwardFixture } | Should -Throw '*forward-flow request failed*'
        $tree = @($global:MauiForwardFixture.Trees.Keys)[0]
        $global:MauiForwardFixture.Trees[$tree].tree += @{ path = 'unexpected'; type = 'blob'; mode = '100644'; sha = '4' * 40 }
        $writes = $global:MauiForwardFixture.Writes
        { Invoke-ForwardFixture } | Should -Throw '*unsupported tree*'
        $global:MauiForwardFixture.Writes | Should -Be $writes
    }

    It 'does not checkpoint or forward a transient servicing tip' {
        $global:MauiForwardFixture.Servicing = $true
        (Invoke-ForwardFixture).status | Should -Be 'waiting-for-fixes-housekeeping'
        $global:MauiForwardFixture.Writes | Should -Be 0
    }
}

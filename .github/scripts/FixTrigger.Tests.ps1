#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $workflowPath = Join-Path $PSScriptRoot '..' 'workflows' 'fix-trigger.yml' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $workflow = Get-Content -Raw -LiteralPath $workflowPath

    function Get-WorkflowJob {
        param(
            [Parameter(Mandatory)]
            [string] $Name
        )

        $pattern = "(?ms)^  $([regex]::Escape($Name)):\s.*?(?=^  [A-Za-z0-9_-]+:[ \t]*\r?$|\z)"
        $match = [regex]::Match($workflow, $pattern)
        if (-not $match.Success) {
            throw "Could not find the $Name job in fix-trigger.yml."
        }

        return $match.Value
    }

    function Get-StepScript {
        param(
            [Parameter(Mandatory)]
            [string] $Job,

            [Parameter(Mandatory)]
            [string] $Name
        )

        $pattern = "(?ms)^      - name: $([regex]::Escape($Name))\r?\n.*?^        run: \|\r?\n(?<script>.*?)(?=^      - name:|\z)"
        $match = [regex]::Match($Job, $pattern)
        if (-not $match.Success) {
            throw "Could not find the '$Name' run script."
        }

        return (($match.Groups['script'].Value -split "`n") |
            ForEach-Object { $_ -replace '^          ', '' }) -join "`n"
    }

    function Invoke-BashStep {
        param(
            [Parameter(Mandatory)]
            [string] $Script,

            [hashtable] $Environment = @{},

            [string] $MockGh
        )

        $tempDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "fix-trigger-$([guid]::NewGuid().ToString('n'))"
        $scriptPath = Join-Path $tempDirectory 'step.sh'
        $outputPath = Join-Path $tempDirectory 'github-output'
        $oldValues = @{}

        New-Item -ItemType Directory -Path $tempDirectory | Out-Null
        Set-Content -LiteralPath $scriptPath -Value $Script -NoNewline

        if ($MockGh) {
            $ghPath = Join-Path $tempDirectory 'gh'
            Set-Content -LiteralPath $ghPath -Value $MockGh -NoNewline
            & chmod +x $ghPath
            $Environment['PATH'] = "$tempDirectory$([IO.Path]::PathSeparator)$env:PATH"
        }

        $Environment['GITHUB_OUTPUT'] = $outputPath
        try {
            foreach ($entry in $Environment.GetEnumerator()) {
                $oldValues[$entry.Key] = [Environment]::GetEnvironmentVariable($entry.Key)
                [Environment]::SetEnvironmentVariable($entry.Key, [string] $entry.Value)
            }

            $console = & bash $scriptPath 2>&1
            $exitCode = $LASTEXITCODE
            $outputs = @{}
            if (Test-Path -LiteralPath $outputPath) {
                foreach ($line in Get-Content -LiteralPath $outputPath) {
                    if ($line -match '^([^=]+)=(.*)$') {
                        $outputs[$Matches[1]] = $Matches[2]
                    }
                }
            }

            return [pscustomobject] @{
                ExitCode = $exitCode
                Outputs = $outputs
                Console = ($console -join "`n")
            }
        } finally {
            foreach ($entry in $oldValues.GetEnumerator()) {
                [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
            }
            Remove-Item -LiteralPath $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    $script:Workflow = $workflow
    $script:PreflightJob = Get-WorkflowJob -Name 'preflight'
    $script:TriggerJob = Get-WorkflowJob -Name 'trigger-fix'
    $script:MatchScript = Get-StepScript -Job $script:PreflightJob -Name 'Match /fix command'
    $script:PermissionScript = Get-StepScript -Job $script:PreflightJob -Name 'Check actor permission'
    $script:ValidateScript = Get-StepScript -Job $script:PreflightJob -Name 'Validate issue, branch, and platform'
}

Describe '/fix command parsing' {
    It 'accepts exactly one branch option: <Body>' -TestCases @(
        @{ Body = '/fix -b main'; Branch = 'main' }
        @{ Body = '  /fix --branch feature/replicate  '; Branch = 'feature/replicate' }
        @{ Body = '/fix -b=users/name/replicate'; Branch = 'users/name/replicate' }
        @{ Body = "`n/fix --branch=main"; Branch = 'main' }
    ) {
        param($Body, $Branch)

        $result = Invoke-BashStep -Script $script:MatchScript -Environment @{ COMMENT_BODY = $Body }

        $result.ExitCode | Should -Be 0
        $result.Outputs.matched | Should -Be 'true'
        $result.Outputs.pipeline_ref | Should -Be $Branch
    }

    It 'rejects malformed or extended input: <Body>' -TestCases @(
        @{ Body = '/fix' }
        @{ Body = '/fix -b' }
        @{ Body = '/fix main' }
        @{ Body = '/fix --unknown main' }
        @{ Body = '/fix -b main trailing' }
        @{ Body = 'please /fix -b main' }
        @{ Body = '/fix -b --branch' }
        @{ Body = '/fix -b=' }
        @{ Body = '/fixing -b main' }
    ) {
        param($Body)

        $result = Invoke-BashStep -Script $script:MatchScript -Environment @{ COMMENT_BODY = $Body }

        $result.ExitCode | Should -Be 0
        $result.Outputs.matched | Should -Be 'false'
        $result.Outputs.ContainsKey('pipeline_ref') | Should -BeFalse
    }
}

Describe '/fix event and authorization gates' {
    It 'runs only for newly created issue comments and rejects pull request comments' {
        $script:Workflow | Should -Match '(?ms)^on:\s+issue_comment:\s+types: \[created\]'
        $script:Workflow | Should -Not -Match '(?m)^  pull_request(_target)?:'
        $script:PreflightJob | Should -Match "(?m)^    if: github\.event_name == 'issue_comment' && !github\.event\.issue\.pull_request$"
    }

    It 'allows only write, maintain, and admin permission' -TestCases @(
        @{ Permission = 'write'; Expected = 'true' }
        @{ Permission = 'maintain'; Expected = 'true' }
        @{ Permission = 'admin'; Expected = 'true' }
        @{ Permission = 'read'; Expected = 'false' }
        @{ Permission = 'triage'; Expected = 'false' }
    ) {
        param($Permission, $Expected)

        $mockGh = @"
#!/usr/bin/env bash
printf '%s\n' '$Permission'
"@
        $result = Invoke-BashStep -Script $script:PermissionScript -MockGh $mockGh -Environment @{
            ACTOR = 'maintainer'
            REPO = 'dotnet/maui'
        }

        $result.ExitCode | Should -Be 0
        $result.Outputs.authorized | Should -Be $Expected
    }

    It 'fails closed when permission lookup fails' {
        $mockGh = @'
#!/usr/bin/env bash
exit 1
'@
        $result = Invoke-BashStep -Script $script:PermissionScript -MockGh $mockGh -Environment @{
            ACTOR = 'unknown'
            REPO = 'dotnet/maui'
        }

        $result.ExitCode | Should -Be 0
        $result.Outputs.authorized | Should -Be 'false'
        $result.Console | Should -Match 'treating the caller as unauthorized'
    }

    It 'does not provision the write-capable job before preflight succeeds' {
        $script:TriggerJob | Should -Match "(?m)^    if: needs\.preflight\.outputs\.proceed == 'true'$"
        $script:PreflightJob | Should -Not -Match 'id-token: write|issues: write'
        $script:TriggerJob | Should -Match '(?m)^      id-token: write$'
        $script:TriggerJob | Should -Match '(?m)^      issues: write$'
    }
}

Describe '/fix issue, branch, and platform validation' {
    BeforeAll {
        $script:ValidationMockGh = @'
#!/usr/bin/env bash
if [[ "$*" == *"/issues/"* ]]; then
  printf '%s\n' "${MOCK_ISSUE_JSON}"
  exit 0
fi
if [[ "$*" == *"/git/ref/heads/"* ]]; then
  [ "${MOCK_BRANCH_EXISTS}" = "true" ]
  exit
fi
exit 1
'@
    }

    It 'requires the issue to be open and not a pull request' -TestCases @(
        @{
            Json = '{"state":"closed","labels":[]}'
            Message = 'is not open'
        }
        @{
            Json = '{"state":"open","pull_request":{},"labels":[]}'
            Message = 'only supported on issues'
        }
    ) {
        param($Json, $Message)

        $result = Invoke-BashStep -Script $script:ValidateScript -MockGh $script:ValidationMockGh -Environment @{
            ISSUE_NUMBER = '123'
            PIPELINE_REF = 'main'
            REPO = 'dotnet/maui'
            MOCK_ISSUE_JSON = $Json
            MOCK_BRANCH_EXISTS = 'true'
        }

        $result.ExitCode | Should -Not -Be 0
        $result.Outputs.ContainsKey('proceed') | Should -BeFalse
        $result.Console | Should -Match $Message
    }

    It 'rejects malformed or missing branches' -TestCases @(
        @{ Branch = 'refs/heads/main'; Exists = 'true'; Message = 'Invalid pipeline branch' }
        @{ Branch = 'bad..branch'; Exists = 'true'; Message = 'Invalid pipeline branch' }
        @{ Branch = 'main;echo'; Exists = 'true'; Message = 'Invalid pipeline branch' }
        @{ Branch = 'missing/replicate'; Exists = 'false'; Message = 'does not exist' }
    ) {
        param($Branch, $Exists, $Message)

        $result = Invoke-BashStep -Script $script:ValidateScript -MockGh $script:ValidationMockGh -Environment @{
            ISSUE_NUMBER = '123'
            PIPELINE_REF = $Branch
            REPO = 'dotnet/maui'
            MOCK_ISSUE_JSON = '{"state":"open","labels":[]}'
            MOCK_BRANCH_EXISTS = $Exists
        }

        $result.ExitCode | Should -Not -Be 0
        $result.Outputs.ContainsKey('proceed') | Should -BeFalse
        $result.Console | Should -Match $Message
    }

    It 'maps issue labels to <Expected>' -TestCases @(
        @{ Labels = '[{"name":"platform/iOS"}]'; Expected = 'ios' }
        @{ Labels = '[{"name":"platform/macOS"}]'; Expected = 'catalyst' }
        @{ Labels = '[{"name":"platform/MacCatalyst"}]'; Expected = 'catalyst' }
        @{ Labels = '[{"name":"platform/Android"}]'; Expected = 'android' }
        @{ Labels = '[{"name":"platform/Windows"}]'; Expected = 'windows' }
        @{ Labels = '[]'; Expected = 'android' }
        @{ Labels = '[{"name":"platform/iOS"},{"name":"platform/Windows"}]'; Expected = 'android' }
    ) {
        param($Labels, $Expected)

        $result = Invoke-BashStep -Script $script:ValidateScript -MockGh $script:ValidationMockGh -Environment @{
            ISSUE_NUMBER = '123'
            PIPELINE_REF = 'feature/replicate'
            REPO = 'dotnet/maui'
            MOCK_ISSUE_JSON = "{`"state`":`"open`",`"labels`":$Labels}"
            MOCK_BRANCH_EXISTS = 'true'
        }

        $result.ExitCode | Should -Be 0
        $result.Outputs.proceed | Should -Be 'true'
        $result.Outputs.issue_number | Should -Be '123'
        $result.Outputs.pipeline_ref | Should -Be 'feature/replicate'
        $result.Outputs.platform | Should -Be $Expected
        if ($Labels -eq '[]' -or $Labels -match 'iOS.*Windows') {
            $result.Console | Should -Match 'defaulting to android'
        }
    }
}

Describe '/fix pipeline dispatch and acknowledgement' {
    It 'uses per-issue non-cancelling concurrency' {
        $script:TriggerJob | Should -Match 'group: fix-trigger-\$\{\{ needs\.preflight\.outputs\.issue_number \}\}'
        $script:TriggerJob | Should -Match '(?m)^      cancel-in-progress: false$'
    }

    It 'queues the exact replicate-mode payload on the selected repository ref' {
        $script:TriggerJob | Should -Match 'pipelines/27723/runs\?api-version=7\.1'
        $script:TriggerJob | Should -Match 'Mode: "replicate"'
        $script:TriggerJob | Should -Match 'IssueNumber: \$issue'
        $script:TriggerJob | Should -Match 'PRNumber: "0"'
        $script:TriggerJob | Should -Match 'Platform: \$platform'
        $script:TriggerJob | Should -Match 'resources: \{ repositories: \{ self: \{ refName: \$ref \} \} \}'
        $script:TriggerJob | Should -Match '--arg ref "refs/heads/\$\{PIPELINE_REF\}"'
    }

    It 'keeps OIDC and AzDO tokens shell-local and never supplies a GitHub PAT' {
        $script:TriggerJob | Should -Match 'ACTIONS_ID_TOKEN_REQUEST_URL'
        $script:TriggerJob | Should -Match 'scope=499b84ac-1321-427f-aa17-267ca6975798/\.default'
        $script:TriggerJob | Should -Match 'Authorization: Bearer \$\{AZDO_TOKEN\}'
        $script:TriggerJob | Should -Match 'trap ''unset OIDC_TOKEN AZDO_TOKEN AZURE_RESPONSE'' EXIT'
        $script:TriggerJob | Should -Match 'unset OIDC_TOKEN'
        $script:TriggerJob | Should -Match 'unset AZDO_TOKEN'
        $script:TriggerJob | Should -Not -Match 'oidc_token=.*GITHUB_OUTPUT|azdo_token=.*GITHUB_OUTPUT'
        $script:TriggerJob | Should -Not -Match 'GH_PAT|PERSONAL_ACCESS_TOKEN'
    }

    It 'acknowledges and minimizes only after successful queueing' {
        $script:TriggerJob | Should -Match "if: \$\{\{ !cancelled\(\) && github\.event_name == 'issue_comment' && steps\.trigger_azdo\.outputs\.queued == 'true' \}\}"
        $script:TriggerJob | Should -Match 'issues/comments/\$\{COMMENT_ID\}/reactions'
        $script:TriggerJob | Should -Match 'minimizeComment'
        $script:TriggerJob | Should -Not -Match 'DELETE|--method DELETE|-X DELETE'
    }

    It 'does not checkout or execute repository code in either job' {
        $script:Workflow | Should -Not -Match 'actions/checkout|dotnet (build|test|run|pack)|COPILOT_GITHUB_TOKEN'
    }
}

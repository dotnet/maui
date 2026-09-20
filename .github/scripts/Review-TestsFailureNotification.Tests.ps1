#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $workflow = Get-Content (Join-Path $PSScriptRoot '../workflows/copilot-review-tests.md') -Raw
    $lock = Get-Content (Join-Path $PSScriptRoot '../workflows/copilot-review-tests.lock.yml') -Raw
    $job = [regex]::Match($lock, '(?ms)^  notify_review_failure:\r?\n.*?(?=^  [a-z_]+:|\z)').Value
    if (-not $job) { throw 'Compiled failure-notification job not found.' }
    $scriptMatch = [regex]::Match($job, '(?ms)^          script: \|\r?\n(?<script>(?:^            [^\r\n]*\r?\n)+)')
    if (-not $scriptMatch.Success) { throw 'Compiled failure-notification script not found.' }
    $notifier = $scriptMatch.Groups['script'].Value -replace '(?m)^ {12}', ''

    function Invoke-NotificationFixture {
        param(
            [hashtable]$Environment = @{},
            [string]$EventName = 'issue_comment',
            [string]$Command = '/review tests',
            [string]$Action = 'created',
            [bool]$IsPullRequest = $true,
            [string]$Permission = 'write',
            [object[]]$Comments = @(),
            [string]$ApiError = '',
            [switch]$Repeat
        )

        $env = @{
            REVIEW_ACTIVATED = 'true'
            EXACT_COMMAND = 'true'
            REVIEW_AUTHORIZED = 'true'
            AGENT_RESULT = 'failure'
            PUBLICATION_RESULT = 'success'
            REPORT_COMMENT_ID = ''
            SUPPRESS_OUTPUT = ''
            PR_NUMBER = '123'
            GITHUB_SERVER_URL = 'https://github.com'
        }
        foreach ($key in $Environment.Keys) { $env[$key] = $Environment[$key] }
        $fixture = @{
            env = $env
            eventName = $EventName
            command = $Command
            action = $Action
            isPullRequest = $IsPullRequest
            permission = $Permission
            comments = $Comments
            apiError = $ApiError
            repeat = [bool]$Repeat
            source = $notifier
        }
        $fixturePath = Join-Path $TestDrive 'notification.json'
        $fixture | ConvertTo-Json -Depth 12 | Set-Content $fixturePath
        $harnessPath = Join-Path $TestDrive 'notification.cjs'
        # Execute the compiled action with no network client or real process environment.
        @'
const fs = require('node:fs');
const fixture = JSON.parse(fs.readFileSync(process.argv[2], 'utf8'));
const calls = [];
const posted = [];
const comments = [...fixture.comments];
const invoke = (name, args) => {
  calls.push({ name, args });
  if (fixture.apiError === name) throw new Error(`${name} failed`);
};
const github = {
  rest: {
    repos: { getCollaboratorPermissionLevel: async args => {
      invoke('permission', args);
      return { data: { permission: fixture.permission } };
    } },
    pulls: { get: async args => { invoke('pull', args); return { data: { number: args.pull_number } }; } },
    issues: {
      listComments: 'comments',
      createComment: async args => {
        invoke('create', args);
        posted.push(args);
        comments.push({ body: args.body, user: { login: 'github-actions[bot]' } });
        return { data: { id: 456 } };
      }
    }
  },
  paginate: { iterator: async function* (method, args) {
    invoke('list', args);
    // Include an empty first page to exercise traversal beyond the first page.
    yield { data: [] };
    yield { data: comments };
  } }
};
const context = {
  repo: { owner: 'dotnet', repo: 'maui' },
  actor: 'maintainer',
  eventName: fixture.eventName,
  runId: 9001,
  payload: {
    action: fixture.action,
    issue: { number: 123, ...(fixture.isPullRequest ? { pull_request: { url: 'trusted PR' } } : {}) },
    comment: { body: fixture.command }
  }
};
const AsyncFunction = Object.getPrototypeOf(async function () {}).constructor;
const notify = new AsyncFunction('github', 'context', 'core', 'process', fixture.source);
(async () => {
  try {
    await notify(github, context, { info: () => {} }, { env: fixture.env });
    if (fixture.repeat) await notify(github, context, { info: () => {} }, { env: fixture.env });
    console.log(JSON.stringify({ calls, posted, error: null }));
  } catch (error) {
    console.log(JSON.stringify({ calls, posted, error: error.message }));
  }
})();
'@ | Set-Content $harnessPath
        $output = & node $harnessPath $fixturePath 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Notification harness failed: $output" }
        return ($output | ConvertFrom-Json)
    }
}

Describe 'Hosted test-review failure notification wiring' {
    It 'waits for publication in an isolated job with no agent artifacts or PAT' {
        $job | Should -Match '(?s)needs:\s+- activation\s+- agent\s+- pre_activation\s+- safe_outputs'
        $job | Should -Match '!cancelled\(\)'
        $job | Should -Match "needs.pre_activation.outputs.activated == 'true'"
        $job | Should -Match "needs.pre_activation.outputs.exact_command_should_run == 'true'"
        $job | Should -Match "needs.pre_activation.outputs.review_authorized == 'true'"
        $job | Should -Match 'inputs.suppress_output != true'
        $job | Should -Match "needs.agent.result != 'skipped'"
        $job | Should -Match "\(needs.agent.result == 'failure' \|\|\s+needs.safe_outputs.result == 'failure'\)"
        $job | Should -Match "needs.safe_outputs.outputs.comment_id == ''"
        $job | Should -Match 'github-token: \$\{\{ github.token \}\}'
        $job | Should -Match 'permissions:\s+contents: read\s+pull-requests: write'
        $job | Should -Not -Match 'download-artifact|checkout@|COPILOT_|secrets\.|continue-on-error|agent.outputs'
        $notifier | Should -Not -Match '\$\{\{'
        $agentJob = [regex]::Match($lock, '(?ms)^  agent:\r?\n.*?(?=^  [a-z_]+:)').Value
        $agentJob | Should -Match 'pull-requests: read'
        $agentJob | Should -Not -Match 'pull-requests: write|issues: write'
    }

    It 'enforces dry-run and PR scoping at the normal publication boundary too' {
        $workflow | Should -Match 'staged: \$\{\{ github.event_name == ''workflow_dispatch'' && inputs.suppress_output == true \}\}'
        $workflow | Should -Match 'target: \$\{\{ github.event.issue.number \|\| inputs.pr_number \}\}'
        $lock | Should -Match 'GH_AW_SAFE_OUTPUTS_STAGED: \$\{\{ github.event_name == ''workflow_dispatch'' && inputs.suppress_output == true \}\}'
        $lock | Should -Match 'GH_AW_SAFE_OUTPUTS_HANDLER_CONFIG: .*target\\":\\"\$\{\{ github.event.issue.number \|\| inputs.pr_number \}\}'
        $lock | Should -Match 'review_authorized: \$\{\{ steps.authorization.outputs.authorized \}\}'
    }
}

Describe 'Compiled deterministic failure notifier' {
    It 'posts bounded fixed text to the triggering PR and is idempotent on reruns' {
        $result = Invoke-NotificationFixture -Repeat
        $result.error | Should -BeNullOrEmpty
        $result.posted.Count | Should -Be 1
        $result.posted[0].owner | Should -Be 'dotnet'
        $result.posted[0].repo | Should -Be 'maui'
        $result.posted[0].issue_number | Should -Be 123
        $result.posted[0].body | Should -Be @'
<!-- review-tests-failure -->
<!-- review-tests-run:https://github.com/dotnet/maui/actions/runs/9001 -->

The `/review tests` workflow failed before publishing a report. [View the workflow run](https://github.com/dotnet/maui/actions/runs/9001).

This is an automation failure, not a verdict on the PR tests. Maintainers can retry `/review tests` after the workflow failure is resolved.
'@
        $result.posted[0].body.Length | Should -BeLessThan 600
    }

    It 'also reports publication failure after successful analysis' {
        $result = Invoke-NotificationFixture -Environment @{ AGENT_RESULT = 'success'; PUBLICATION_RESULT = 'failure' }
        $result.error | Should -BeNullOrEmpty
        $result.posted.Count | Should -Be 1
    }

    It 'can notify an authorized dispatch target without a command comment' {
        $result = Invoke-NotificationFixture -EventName workflow_dispatch -Command '' -Environment @{
            REVIEW_AUTHORIZED = ''; PR_NUMBER = '456'
        }
        $result.error | Should -BeNullOrEmpty
        $result.posted[0].issue_number | Should -Be 456
        ($result.calls | Where-Object name -EQ pull).args.pull_number | Should -Be 456
    }

    It 'does not call GitHub for <Case>' -ForEach @(
        @{ Case = 'success'; Env = @{ AGENT_RESULT = 'success' }; Event = 'issue_comment' }
        @{ Case = 'already published'; Env = @{ REPORT_COMMENT_ID = '789' }; Event = 'issue_comment' }
        @{ Case = 'dry run'; Env = @{ SUPPRESS_OUTPUT = 'true' }; Event = 'workflow_dispatch' }
        @{ Case = 'rejected activation'; Env = @{ REVIEW_ACTIVATED = 'false' }; Event = 'issue_comment' }
        @{ Case = 'missing activation'; Env = @{ REVIEW_ACTIVATED = '' }; Event = 'issue_comment' }
        @{ Case = 'irrelevant subcommand'; Env = @{ EXACT_COMMAND = 'false' }; Event = 'issue_comment' }
        @{ Case = 'unauthorized comment'; Env = @{ REVIEW_AUTHORIZED = 'false' }; Event = 'issue_comment' }
        @{ Case = 'skipped agent'; Env = @{ AGENT_RESULT = 'skipped'; PUBLICATION_RESULT = 'failure' }; Event = 'issue_comment' }
        @{ Case = 'unrelated event'; Env = @{}; Event = 'push' }
    ) {
        $result = Invoke-NotificationFixture -Environment $Env -EventName $Event
        $result.error | Should -BeNullOrEmpty
        $result.calls.Count | Should -Be 0
    }

    It 'rejects irrelevant comment <Command>' -ForEach @(
        @{ Command = '/review' }
        @{ Command = '/review android' }
        @{ Command = '/review tests extra' }
        @{ Command = "prefix`n/review tests" }
        @{ Command = '/review tests; publish to #999' }
    ) {
        $result = Invoke-NotificationFixture -Command $Command
        $result.calls.Count | Should -Be 0
    }

    It 'does not notify for issue comments or edited commands' {
        (Invoke-NotificationFixture -IsPullRequest $false).calls.Count | Should -Be 0
        (Invoke-NotificationFixture -Action edited).calls.Count | Should -Be 0
    }

    It 'rechecks caller permission and fails closed for <Permission>' -ForEach @(
        @{ Permission = 'read' }
        @{ Permission = 'triage' }
        @{ Permission = 'none' }
    ) {
        $result = Invoke-NotificationFixture -Permission $Permission
        $result.posted.Count | Should -Be 0
        $result.calls.Count | Should -Be 1
    }

    It 'rejects invalid dispatch target <Target>' -ForEach @(
        @{ Target = '' }
        @{ Target = '0' }
        @{ Target = '-1' }
        @{ Target = '1.5' }
        @{ Target = '1e3' }
        @{ Target = '9007199254740992' }
        @{ Target = '123;echo injected' }
    ) {
        $result = Invoke-NotificationFixture -EventName workflow_dispatch -Environment @{ PR_NUMBER = $Target }
        $result.error | Should -Be 'A positive target PR number is required.'
        $result.posted.Count | Should -Be 0
    }

    It 'rejects a target different from the triggering PR' {
        $result = Invoke-NotificationFixture -Environment @{ PR_NUMBER = '456' }
        $result.error | Should -Be 'Notification target does not match the triggering PR.'
        $result.posted.Count | Should -Be 0
    }

    It 'does not duplicate an existing same-run report even if publication lost its output' {
        $result = Invoke-NotificationFixture -Comments @(@{
            user = @{ login = 'MauiBot' }
            body = "> Caution`n<!-- Tests Failure -->`n<!-- review-tests-run:https://github.com/dotnet/maui/actions/runs/9001 -->`nReport"
        })
        $result.error | Should -BeNullOrEmpty
        $result.posted.Count | Should -Be 0
    }

    It 'does not let untrusted comments or older reports suppress the notice: <Case>' -ForEach @(
        @{ Case = 'untrusted author'; Login = 'contributor'; RunId = '9001' }
        @{ Case = 'different run'; Login = 'github-actions[bot]'; RunId = '9000' }
    ) {
        $result = Invoke-NotificationFixture -Comments @(@{
            user = @{ login = $Login }
            body = "<!-- Tests Failure -->`n<!-- review-tests-run:https://github.com/dotnet/maui/actions/runs/$RunId -->"
        })
        $result.posted.Count | Should -Be 1
    }

    It 'surfaces <Api> API errors without claiming publication succeeded' -ForEach @(
        @{ Api = 'permission' }
        @{ Api = 'pull' }
        @{ Api = 'list' }
        @{ Api = 'create' }
    ) {
        $result = Invoke-NotificationFixture -ApiError $Api
        $result.error | Should -Be "$Api failed"
        $result.posted.Count | Should -Be 0
    }
}

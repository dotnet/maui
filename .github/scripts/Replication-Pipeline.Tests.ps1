#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:PipelinePath = Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml'
    $script:Pipeline = Get-Content -LiteralPath $script:PipelinePath -Raw
}

Describe 'MAUI Copilot mode routing' {
    It 'places the review-default Mode selector before target numbers' {
        $modeIndex = $script:Pipeline.IndexOf('- name: Mode')
        $prIndex = $script:Pipeline.IndexOf('- name: PRNumber')
        $issueIndex = $script:Pipeline.IndexOf('- name: IssueNumber')

        $modeIndex | Should -BeGreaterThan -1
        $modeIndex | Should -BeLessThan $prIndex
        $prIndex | Should -BeLessThan $issueIndex
        $script:Pipeline | Should -Match "(?s)- name: Mode.*?default: review.*?values:\s+- review\s+- replicate\s+- feedback"
        $script:Pipeline | Should -Match "(?s)- name: PRNumber.*?default: 0"
        $script:Pipeline | Should -Match "(?s)- name: IssueNumber.*?default: 0"
    }

    It 'requires exactly the target number for the selected mode' {
        $script:Pipeline | Should -Match 'Mode=review requires PRNumber > 0 and IssueNumber = 0'
        $script:Pipeline | Should -Match 'Mode=replicate requires IssueNumber > 0 and PRNumber = 0'
        $script:Pipeline | Should -Match 'PARAM_MODE: \$\{\{ parameters\.Mode \}\}'
        $script:Pipeline | Should -Match 'PARAM_ISSUE_NUMBER: \$\{\{ parameters\.IssueNumber \}\}'
    }

    It 'gives feedback and replication runs distinct Azure titles' {
        $script:Pipeline | Should -Match "(?s)job: ValidateReplicationPublisher.*?displayName: 'Set Replication Pipeline Run Title'"
        $script:Pipeline | Should -Match 'build\.updatebuildnumber\]Feedback snapshot'
        $script:Pipeline | Should -Match 'build\.updatebuildnumber\]Replicate issue \$\{PARAM_ISSUE_NUMBER\} \$\{PARAM_PLATFORM\}'
        $script:Pipeline | Should -Match 'build\.updatebuildnumber\]Review PR \$\{PARAM_PR_NUMBER\} \$\{PARAM_PLATFORM\}'
    }

    It 'keeps PR-specific jobs and stages review-only' {
        $script:Pipeline | Should -Match "(?s)name: RunSetup.*?condition: and\(succeeded\(\), eq\('\$\{\{ parameters\.Mode \}\}', 'review'\)\)"
        $script:Pipeline | Should -Match "(?s)name: RunGate.*?condition: and\(succeeded\(\), eq\('\$\{\{ parameters\.Mode \}\}', 'review'\)\)"
        $script:Pipeline | Should -Match "(?s)name: RunReview.*?condition: and\(succeededOrFailed\(\), eq\('\$\{\{ parameters\.Mode \}\}', 'review'\)\)"
        $script:Pipeline | Should -Match "condition: and\(eq\('\$\{\{ parameters\.Mode \}\}', 'review'\), in\(dependencies\.CopilotReview\.result"
        $script:Pipeline | Should -Match "(?s)- stage: RunDeepUITests.*?eq\('\$\{\{ parameters\.Mode \}\}', 'review'\)"
        $script:Pipeline | Should -Match "(?s)- stage: UpdateAISummaryComment.*?condition: and\(not\(canceled\(\)\), eq\('\$\{\{ parameters\.Mode \}\}', 'review'\)"
        $script:Pipeline | Should -Match "(?s)- stage: CleanupReviewLock.*?condition: always\(\).*?- job: CleanupReviewLock.*?condition: eq\('\$\{\{ parameters\.Mode \}\}', 'review'\)"
    }

    It 'uses a clean main baseline and replication-only recording dependencies' {
        $script:Pipeline | Should -Match "(?s)job: ValidateReplicationPublisher.*?condition: or\(eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\), eq\('\$\{\{ parameters\.Mode \}\}', 'feedback'\)\)"
        $script:Pipeline | Should -Match "displayName: 'Probe MauiBot identity and writable fork'"
        $script:Pipeline | Should -Match 'Expected at most one writable MauiBot fork of dotnet/maui'
        $script:Pipeline | Should -Match "'api', '-X', 'POST', 'repos/dotnet/maui/forks'"
        $script:Pipeline | Should -Match 'newly created MauiBot fork did not become writable within 60 seconds'
        $script:Pipeline | Should -Match "(?s)job: ValidateReplicationPublisher.*?GH_TOKEN: \$\(GH_COMMENT_TOKEN\)"
        $migrationIndex = $script:Pipeline.IndexOf("displayName: 'Move existing reproduction PRs to testing fork'")
        $copilotJobIndex = $script:Pipeline.IndexOf("- job: CopilotReview")
        $migrationIndex | Should -BeGreaterThan -1
        $migrationIndex | Should -BeLessThan $copilotJobIndex
        $script:Pipeline | Should -Match 'sparseCheckoutDirectories: \.github/scripts/shared'
        $script:Pipeline | Should -Match 'Move-ReplicationPRsToTestingFork\.ps1'
        $script:Pipeline | Should -Match 'Move-ReplicationPRCommentsToTestingFork\.ps1'
        $script:Pipeline | Should -Match 'Export-ReplicationPRFeedback\.ps1'
        $script:Pipeline | Should -Match "(?s)displayName: 'Migrate comments and export replication feedback'.*?condition: and\(succeeded\(\), eq\('\$\{\{ parameters\.Mode \}\}', 'feedback'\)\)"
        $script:Pipeline | Should -Match "(?s)displayName: 'Publish Replication Feedback Snapshot'.*?condition: and\(succeededOrFailed\(\), eq\('\$\{\{ parameters\.Mode \}\}', 'feedback'\)\)"
        $script:Pipeline | Should -Match "artifact: 'ReplicationFeedback'"
        $script:Pipeline | Should -Match "(?s)- job: CopilotReview.*?condition: or\(eq\('\$\{\{ parameters\.Mode \}\}', 'review'\), eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\)\)"
        # Reviews of kubaflo/maui#189, #193, and #194 each rejected the declared
        # baseline because it was not the published commit's first parent. The
        # baseline must be the commit the pull request will be parented on.
        $script:Pipeline | Should -Match 'https://github\.com/kubaflo/maui\.git main'
        $script:Pipeline |
            Should -Match 'merge-base --is-ancestor "\$\{BASE_SHA\}" origin/main'
        $script:Pipeline |
            Should -Match 'The publication base is not an ancestor of dotnet/maui main'
        $restoreIndex = $script:Pipeline.IndexOf("displayName: 'Restore clean replication baseline'")
        $replicateIndex = $script:Pipeline.IndexOf("displayName: 'Replicate issue and author failing test'")
        $restoreIndex | Should -BeGreaterThan -1
        $restoreIndex | Should -BeLessThan $replicateIndex
        $script:Pipeline | Should -Match 'git restore --source \$baseSha --staged --worktree -- \.'
        $script:Pipeline | Should -Match '(?s)- name: APPIUM_HOME\s+value: \$\(Agent\.TempDirectory\)/\.appium/'
        $script:Pipeline | Should -Match "displayName: 'Install reproduction recording tools'"
        $script:Pipeline | Should -Match "(?s)displayName: 'Install reproduction recording tools'.*?eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\)"
        $script:Pipeline | Should -Match "REPLICATION_AGENT_CONTEXT_PATH.*?issue-agent-context\.json"
        $script:Pipeline | Should -Match "(?s)name: RunReplication.*?COPILOT_GITHUB_TOKEN: \$\(COPILOT_TOKEN\)"
        $script:Pipeline.Contains(
            '$deviceUdid -match ''^\$\([A-Za-z0-9_.-]+\)$'''
        ) | Should -BeTrue
        $script:Pipeline.Contains(
            'if ($platform -in @(''catalyst'', ''windows''))'
        ) | Should -BeTrue
        $script:Pipeline | Should -Match 'A resolved device UDID is required for \$platform replication'
        $script:Pipeline | Should -Match "artifact: 'ReplicationArtifacts'"
    }

    It 'validates before separating trusted evidence and MauiBot PR credentials' {
        $validationIndex = $script:Pipeline.IndexOf("displayName: 'Validate replication candidate without credentials'")
        $credentialIndex = $script:Pipeline.IndexOf('$checkoutToken = $null', $validationIndex)
        $evidenceIndex = $script:Pipeline.IndexOf("displayName: 'Publish reproduction evidence'")
        $publicationIndex = $script:Pipeline.IndexOf("displayName: 'Move existing PRs and create MauiBot testing draft PR'")

        $validationIndex | Should -BeGreaterThan -1
        $validationIndex | Should -BeLessThan $credentialIndex
        $credentialIndex | Should -BeLessThan $evidenceIndex
        $evidenceIndex | Should -BeLessThan $publicationIndex
        $script:Pipeline | Should -Match "(?s)- stage: PublishReplication.*?condition: and\(eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\), ne\(dependencies\.ReviewPR\.outputs\['CopilotReview\.ReplicationDuplicateCheck\.replicationAlreadyPublished'\], 'true'\), ne\(dependencies\.ReviewPR\.outputs\['CopilotReview\.ReplicationDuplicateCheck\.replicationIssueIneligible'\], 'true'\), in\(dependencies\.ReviewPR\.result, 'Succeeded', 'SucceededWithIssues', 'Failed'\)\)"
        $script:Pipeline | Should -Match "(?s)- job: PublishReplication.*?persistCredentials: true"
        $script:Pipeline | Should -Match 'review-tests-assets-v2'
        $script:Pipeline | Should -Match 'Publish-ReplicationEvidence\.ps1'
        $script:Pipeline | Should -Match 'Publish-ReplicationOutcome\.ps1'
        $script:Pipeline | Should -Match "'Move-ReplicationPRsToTestingFork\.ps1'"
        $script:Pipeline | Should -Match "(?s)displayName: 'Publish MauiBot non-reproduction outcome'.*?GH_TOKEN: \$\(GH_COMMENT_TOKEN\)"
        $script:Pipeline | Should -Match 's/try-latest-version'
        $script:Pipeline | Should -Match "(?s)displayName: 'Move existing PRs and create MauiBot testing draft PR'.*?GH_TOKEN: \$\(GH_COMMENT_TOKEN\)"
        $script:Pipeline | Should -Match 'Move-ReplicationPRsToTestingFork\.ps1'
        $script:Pipeline | Should -Match '-TargetOwner "kubaflo"'
        $script:Pipeline | Should -Match 'Remove-Item Env:GH_TOKEN'
        $script:Pipeline | Should -Not -Match 'GH_REPLICATION_TOKEN'
        $script:Pipeline | Should -Not -Match 'MAUI_REPLICATION_AZURE_SERVICE_CONNECTION'
        $script:Pipeline | Should -Not -Match 'MAUI_REPLICATION_(?:STORAGE|PUBLIC_BASE_URL|FORK)'
        $script:Pipeline | Should -Match 'git merge-base --is-ancestor "\$\{BASE_SHA\}" origin/main'
        $script:Pipeline | Should -Match "'Assert-ReplicationTestGuard\.ps1'"
        $script:Pipeline | Should -Match '\$validation\.Count -ne 1'
        $script:Pipeline | Should -Match '\$validation\[0\]\.validationPassed -ne \$true'
        $validationTask = $script:Pipeline.Substring(
            $script:Pipeline.LastIndexOf('- pwsh: |', $validationIndex),
            $credentialIndex - $script:Pipeline.LastIndexOf('- pwsh: |', $validationIndex))
        $validationTask | Should -Not -Match '\$LASTEXITCODE'
    }

    It 'preserves PR telemetry while adding operation and issue target fields' {
        $script:Pipeline | Should -Match '-PRNumber "\$\{\{ parameters\.PRNumber \}\}"'
        $script:Pipeline | Should -Match '-IssueNumber "\$\{\{ parameters\.IssueNumber \}\}"'
        $script:Pipeline | Should -Match '-Operation "\$\{\{ parameters\.Mode \}\}"'
        $script:Pipeline | Should -Match 'schemaVersion = 2'
        $script:Pipeline | Should -Match 'issueNumber\s+= "\$\{\{ parameters\.IssueNumber \}\}"'
    }

    It 'retains the trusted telemetry subtree in the replication CopilotLogs artifact' {
        $script:Pipeline | Should -Match '-TokenUsageOutputDir "\$\(Build\.ArtifactStagingDirectory\)/copilot-token-usage/raw"'
        $script:Pipeline | Should -Match "displayName: 'Stage replication Copilot telemetry'"
        $script:Pipeline | Should -Match '\$target = Join-Path \$logsDir "copilot-token-usage"'
        $script:Pipeline | Should -Match "(?s)displayName: 'Publish Replication Copilot Logs'.*?targetPath: '\$\(Build\.ArtifactStagingDirectory\)/copilot-logs'.*?artifact: 'CopilotLogs'"
        $script:Pipeline | Should -Not -Match "targetPath: '\$\(REPLICATION_ARTIFACT_ROOT\)/copilot-token-usage'"
    }

    It 'installs and validates the native Copilot executable cross-platform' {
        $script:Pipeline | Should -Match ([regex]::Escape("- pwsh: |"))
        $script:Pipeline | Should -Match ([regex]::Escape('Write-Host "Installing GitHub Copilot CLI..."'))
        $script:Pipeline | Should -Match "'copilot-win32-x64'"
        $script:Pipeline | Should -Match "'copilot-win32-arm64'"
        $script:Pipeline | Should -Match 'Copilot CLI native Windows executable was not installed'
    }

    It 'does not validate or publish blocked candidates as reproduced results' {
        $script:Pipeline | Should -Match '\[string\]\$candidate\.status -ne ''reproduced'''
        $script:Pipeline | Should -Match 'REPLICATION_CANDIDATE_READY\]false'
        $script:Pipeline | Should -Match 'REPLICATION_CANDIDATE_READY\]true'
        $script:Pipeline | Should -Match "condition: and\(succeeded\(\), eq\(variables\['REPLICATION_CANDIDATE_READY'\], 'true'\)\)"
    }
}

Describe 'Replication issue outcome publication boundary' {
    BeforeAll {
        $script:PipelineYaml = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml') -Raw
        $script:OutcomeSource = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'shared/Publish-ReplicationOutcome.ps1') -Raw
    }

    It 'gives each media-tool install attempt a fresh timeout' {
        # Build 15000542 timed out because three apt attempts shared one
        # 15-minute budget, which lost a run that had nothing else wrong.
        $pipeline = $script:Pipeline
        $pipeline | Should -Not -Match 'ffmpeg install attempt \$attempt did not succeed'
        $pipeline | Should -Not -Match 'Media validator install attempt'

        $recorder = [regex]::Match(
            $pipeline,
            "(?s)Install reproduction recording tools.{0,400}")
        $recorder.Success | Should -BeTrue
        $recorder.Value | Should -Match 'timeoutInMinutes: 25'
        $recorder.Value | Should -Match 'retryCountOnTaskFailure: 2'
    }

    It 'never lets apt wait longer than the step that runs it' {
        # Builds 15000542 and 15000902 spent the whole timeout inside a silent
        # apt call that was waiting for the unattended-upgrades dpkg lock.
        $pipeline = $script:Pipeline
        $pipeline | Should -Not -Match 'apt-get update -qq'
        $pipeline | Should -Not -Match 'apt-get install -y -qq'
        ([regex]::Matches($pipeline, 'timeout \d+ apt-get update')).Count |
            Should -BeGreaterOrEqual 2
        ([regex]::Matches($pipeline, 'timeout \d+ apt-get install')).Count |
            Should -BeGreaterOrEqual 2
        ([regex]::Matches($pipeline, 'DPkg::Lock::Timeout=120')).Count |
            Should -BeGreaterOrEqual 4
        ([regex]::Matches($pipeline, 'stop unattended-upgrades\.service')).Count |
            Should -BeGreaterOrEqual 2
    }

    It 'stages issue context without depending on a step that may not have run' {
        # The same build then failed a second time because the staging step
        # inlined a pipeline variable that the skipped setup never defined.
        $pipeline = $script:Pipeline
        $pipeline | Should -Not -Match '\$\(REPLICATION_PRIVATE_CONTEXT_ROOT\)'
        $pipeline | Should -Match '\$env:REPLICATION_PRIVATE_CONTEXT_ROOT'
        $pipeline | Should -Match 'Sanitized issue context was never prepared'
    }

    It 'publishes build logs only when the directory exists' {
        # Runs 15000192, 15000196, and 15000200 finished their replication work
        # and then went red publishing a directory replicate mode never writes.
        $script:Pipeline | Should -Match "displayName: 'Detect build log directory'"
        $script:Pipeline | Should -Match "(?s)displayName: 'Publish Build Logs'.*?condition: and\(succeededOrFailed\(\), eq\(variables\['logDirectoryPresent'\], 'true'\)\)"
        $script:Pipeline |
            Should -Not -Match "ne\(variables\['LogDirectory'\], ''\)"
    }

    It 'gives the publisher media validator room to install' {
        # Build 14999448 produced a publishable candidate and then lost it
        # because this install exhausted a ten-minute budget.
        $script:Pipeline |
            Should -Match "(?s)Install publisher media validator'\s+timeoutInMinutes: 25"
        $installs = [regex]::Matches(
            $script:Pipeline,
            'apt-get install -y -q(?<flags>[^\r\n]*)ffmpeg')
        $installs.Count | Should -BeGreaterThan 0
        foreach ($install in $installs) {
            $install.Groups['flags'].Value | Should -Match '--no-install-recommends'
        }
    }

    It 'keeps commenting on the public issue tracker opt-in' {
        # Build 14997672 commented on dotnet/maui#36694 and applied
        # s/try-latest-version, which notified the reporter, while the flow was
        # still being hardened and only reproduction PRs were meant to be public.
        $script:PipelineYaml | Should -Match 'name: PublishIssueOutcome'
        $script:PipelineYaml | Should -Match '(?s)name: PublishIssueOutcome.*?default: false'
        $script:PipelineYaml.Contains('-DryRun:(-not $publishIssueOutcome)') |
            Should -BeTrue
    }

    It 'requires an explicit repository so omission cannot reach dotnet/maui' {
        $script:OutcomeSource.Contains("[string]`$Repository = 'dotnet/maui'") |
            Should -BeFalse
        $script:OutcomeSource | Should -Match '(?s)Parameter\(Mandatory = \$true\)\][\r\n\s]*\[ValidatePattern[^\r\n]*\][\r\n\s]*\[string\]\$Repository,'
        $script:PipelineYaml | Should -Match '-Repository "dotnet/maui"'
    }

    It 'treats refreshing the package index as advisory' {
        # Build 15001363 could not reach azure.archive.ubuntu.com, spent the
        # whole update budget retrying it and threw, even though another mirror
        # answered and ffmpeg was installable. Only the install decides whether
        # the recorder can run.
        $update = [regex]::Match(
            $script:Pipeline,
            'apt-get update[^\n]*\n(?<next>(?:[^\n]*\n){3})')

        $update.Success | Should -BeTrue
        $update.Groups['next'].Value.Contains('Write-Warning') | Should -BeTrue
        $update.Groups['next'].Value.Contains('throw') | Should -BeFalse
    }

    It 'never publishes a replication artifact root that was never defined' {
        # Azure leaves an undefined macro literal, so build 15001363 tried to
        # publish a directory whose name was the unexpanded variable reference.
        $script:Pipeline.Contains('variable=replicationArtifactRootPresent') |
            Should -BeTrue
        $script:Pipeline.Contains(
            "eq(variables['replicationArtifactRootPresent'], 'true')") |
            Should -BeTrue

        $gate = $script:Pipeline.IndexOf('Detect replication artifact root')
        $publish = $script:Pipeline.IndexOf('Publish Replication Artifacts')
        $gate | Should -BeGreaterThan 0
        $publish | Should -BeGreaterThan $gate
    }

    It 'reports missing Copilot telemetry instead of failing the job again' {
        $start = $script:Pipeline.IndexOf('$logsDir = "$(Build.ArtifactStagingDirectory)/copilot-logs"')
        $end = $script:Pipeline.IndexOf('Stage replication Copilot telemetry')
        $start | Should -BeGreaterThan 0
        $end | Should -BeGreaterThan $start

        $telemetry = $script:Pipeline.Substring($start, $end - $start)
        $telemetry.Contains('No Copilot telemetry was produced') | Should -BeTrue
        $telemetry.Contains('exit 0') | Should -BeTrue
    }

    It 'checks for a duplicate reproduction pull request before any device work' {
        # Build 15001358 spent over forty minutes reproducing issue 37407 and
        # authoring a test, only for the publisher to reject the result because
        # PR 152 already covered it. The same question is now asked up front.
        $check = $script:Pipeline.IndexOf('Check for an existing reproduction pull request')
        $replicate = $script:Pipeline.IndexOf('Replicate issue and author failing test')
        $check | Should -BeGreaterThan 0
        $replicate | Should -BeGreaterThan $check

        $step = $script:Pipeline.Substring($check - 3600, 3600)
        $step.Contains('MAUI_COPILOT_REPLICATION issue=') | Should -BeTrue
        $step.Contains('variable=replicationAlreadyPublished') | Should -BeTrue
    }

    It 'reads the public fork without ever handling the publishing credential' {
        # The duplicate check runs in the untrusted agent job, so it must not
        # widen that job's credential surface to reach the publisher's token.
        $check = $script:Pipeline.IndexOf('Check for an existing reproduction pull request')
        $check | Should -BeGreaterThan 0

        $step = $script:Pipeline.Substring($check, 400)
        $step.Contains('GH_TOKEN: $(GH_COMMENT_TOKEN)') | Should -BeTrue
        $step.Contains('GH_REPLICATION_TOKEN') | Should -BeFalse
    }

    It 'skips the device run and the publisher when a duplicate already exists' {
        $script:Pipeline.Contains(
            "ne(variables['replicationAlreadyPublished'], 'true')") |
            Should -BeTrue
        $script:Pipeline.Contains(
            "ne(dependencies.ReviewPR.outputs['CopilotReview.ReplicationDuplicateCheck.replicationAlreadyPublished'], 'true')") |
            Should -BeTrue
    }

    It 'skips the device run and the publisher when the issue is not open' {
        # PR 199 spent a full run reproducing dotnet/maui#37243 and published a
        # draft before anyone noticed the issue was already closed as
        # not_planned. Ask GitHub before spending the device.
        $check = $script:Pipeline.IndexOf(
            'Check for an existing reproduction pull request')
        $check | Should -BeGreaterThan 0
        $step = $script:Pipeline.Substring($check - 3600, 3600)
        $step.Contains('repos/dotnet/maui/issues/') | Should -BeTrue
        $step.Contains('.state_reason') | Should -BeTrue
        $step.Contains('variable=replicationIssueIneligible') | Should -BeTrue
        $step.Contains('isOutput=true]true') | Should -BeTrue

        $script:Pipeline.Contains(
            "ne(variables['replicationIssueIneligible'], 'true')") |
            Should -BeTrue
        $script:Pipeline.Contains(
            "ne(dependencies.ReviewPR.outputs['CopilotReview.ReplicationDuplicateCheck.replicationIssueIneligible'], 'true')") |
            Should -BeTrue
    }

    It 'accepts a publication that reports an already-covered issue' {
        # The publisher now reports a duplicate instead of throwing, so the
        # caller must not treat the absent pull request URL as a defect.
        $script:Pipeline.Contains('$result.duplicateOf') | Should -BeTrue

        $duplicate = $script:Pipeline.IndexOf('$result.duplicateOf')
        $missingUrl = $script:Pipeline.IndexOf('Replication publisher did not return a pull request URL')
        $duplicate | Should -BeGreaterThan 0
        $missingUrl | Should -BeGreaterThan $duplicate
    }
}

Describe 'The publisher validates with the scripts the run was queued with' {
    It 'pins the publisher checkout before staging trusted scripts' {
        # A replicate run spends up to an hour on a device. When the pipeline
        # ref moved in the meantime the publisher checked out the newer tip and
        # rejected a live run for a manifest field that existed in neither
        # script at the commit the run was queued from.
        $stageIndex = $script:Pipeline.IndexOf('Stage trusted replication publisher')
        $stageIndex | Should -BeGreaterThan -1

        $publisherBlock = $script:Pipeline.Substring(0, $stageIndex)
        $pinIndex = $publisherBlock.LastIndexOf('checkout --force --detach $pinned')
        $pinIndex | Should -BeGreaterThan -1

        $copyIndex = $script:Pipeline.IndexOf('trusted-replication-publisher')
        $pinIndex | Should -BeLessThan $copyIndex
        $script:Pipeline | Should -Match "\`$pinned = '\`$\(Build\.SourceVersion\)'"
        $script:Pipeline | Should -Match 'Publisher checkout is at \$actual but the run was queued at \$pinned'
    }
}

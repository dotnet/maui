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
        # The title for a replicate run is set by the job that does the work,
        # because replicate runs no longer schedule the publisher validation.
        $script:Pipeline | Should -Match "(?s)job: CopilotReview.*?displayName: 'Set Replication Pipeline Run Title'"
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
        # The feedback job no longer preflights the publishing credential.
        # Publish-ReplicationPR re-checks identity and fork before it opens a
        # pull request, so the probe only ever cost an agent and, once the
        # token expired, blocked feedback collection outright.
        $script:Pipeline | Should -Match "(?s)job: ReplicationFeedback.*?condition: eq\('\$\{\{ parameters\.Mode \}\}', 'feedback'\)"
        $script:Pipeline | Should -Not -Match 'ValidateReplicationPublisher'
        $script:Pipeline | Should -Not -Match "Probe MauiBot identity and writable fork"
        $script:Pipeline | Should -Not -Match 'Expected at most one writable MauiBot fork of dotnet/maui'
        $script:Pipeline | Should -Not -Match "'api', '-X', 'POST', 'repos/dotnet/maui/forks'"
        $script:Pipeline | Should -Match "(?s)job: ReplicationFeedback.*?GH_TOKEN: \$\(GH_COMMENT_TOKEN\)"
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
        $script:Pipeline | Should -Match "(?s)- stage: PublishReplication.*?condition: and\(eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\), ne\(dependencies\.ReviewPR\.outputs\['CopilotReview\.ReplicationCredentialCheck\.replicationEvidenceOnly'\], 'true'\), ne\(dependencies\.ReviewPR\.outputs\['CopilotReview\.ReplicationDuplicateCheck\.replicationAlreadyPublished'\], 'true'\), ne\(dependencies\.ReviewPR\.outputs\['CopilotReview\.ReplicationDuplicateCheck\.replicationIssueIneligible'\], 'true'\), in\(dependencies\.ReviewPR\.result, 'Succeeded', 'SucceededWithIssues', 'Failed'\)\)"
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
            Should -Match "(?s)Install publisher media validator'.{0,300}?timeoutInMinutes: 25"
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

        $stepStart = $script:Pipeline.LastIndexOf('- pwsh:', $check)
        $stepStart | Should -BeGreaterThan 0
        $step = $script:Pipeline.Substring($stepStart, $check - $stepStart)
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

Describe 'A run that produced nothing does not fail the publisher too' {
    It 'tolerates a missing replication artifact' {
        # A run that stops at the credential pre-flight publishes no artifact.
        # The download used to fail, reporting the same stopped run as a second
        # red job on top of the one that actually explained the problem.
        $index = $script:Pipeline.IndexOf('Download Replication Artifacts')
        $index | Should -BeGreaterThan -1

        $window = $script:Pipeline.Substring($index, 400)
        $window | Should -Match 'continueOnError:\s*true'
    }

    It 'records whether anything was produced before using it' {
        $downloadIndex = $script:Pipeline.IndexOf('Download Replication Artifacts')
        $checkIndex = $script:Pipeline.IndexOf('Check for replication artifacts')
        $checkIndex | Should -BeGreaterThan $downloadIndex

        $window = $script:Pipeline.Substring($checkIndex - 800, 800)
        $window | Should -Match 'REPLICATION_ARTIFACT_PRESENT'
        $window | Should -Match 'candidate\.json'
    }

    It 'skips every step that cannot work without an artifact' {
        # Each of these reads the artifact or an output variable the stopped run
        # never set, so without the gate they turn a clean stop back into a
        # failure.
        foreach ($step in @(
            'Pin clean replication baseline',
            'Install publisher media validator',
            'Publish MauiBot non-reproduction outcome')) {
            $index = $script:Pipeline.IndexOf($step)
            $index | Should -BeGreaterThan -1

            $window = $script:Pipeline.Substring($index, 260)
            $window | Should -Match "REPLICATION_ARTIFACT_PRESENT'\], 'true'"
        }
    }

    It 'still refuses to publish anything without a validated candidate' {
        foreach ($step in @(
            'Publish reproduction evidence',
            'Move existing PRs and create MauiBot testing draft PR')) {
            $index = $script:Pipeline.IndexOf($step)
            $index | Should -BeGreaterThan -1

            $window = $script:Pipeline.Substring($index, 260)
            $window | Should -Match "REPLICATION_CANDIDATE_READY'\], 'true'"
        }
    }
}

Describe 'MAUI Copilot pipeline splatting' {
    It 'builds every splatted argument set as a hashtable' {
        # An array splat passes its elements positionally, so '-IssueNumber'
        # binds to -IssueNumber as a value and the step dies on a type
        # conversion that names the parameter it failed to fill. Every splat
        # site is checked because the mistake is invisible until the step runs.
        #
        # Splatting into a native executable is exempt: a process receives a
        # positional argument vector, so an array is the correct shape there.
        $nativeCommands = 'pwsh|dotnet|git|gh|adb|node|npm|xcrun|ffmpeg|python3?'
        $splatNames = [regex]::Matches($script:Pipeline, '(?m)^(?<line>.*?@(?<name>[A-Za-z_][A-Za-z0-9_]*)\b.*)$') |
            Where-Object {
                $_.Groups['name'].Value -notin @('parameters', 'variables') -and
                $_.Groups['line'].Value -notmatch ('&\s*(?:' + $nativeCommands + ')\s+@')
            } |
            ForEach-Object { $_.Groups['name'].Value } |
            Sort-Object -Unique

        $splatNames | Should -Not -BeNullOrEmpty

        foreach ($name in $splatNames) {
            $assignment = [regex]::Match($script:Pipeline, ('\$' + [regex]::Escape($name) + '\s*=\s*@(?<open>[({])'))
            if (-not $assignment.Success) { continue }

            $assignment.Groups['open'].Value |
                Should -BeExactly '{' -Because "`$$name is splatted into a PowerShell command, so it must be a hashtable rather than an array"
        }
    }

    It 'passes the issue number to the context script as a named argument' {
        $script:Pipeline | Should -Match '(?s)\$contextArgs = @\{.*?IssueNumber = '
        $script:Pipeline | Should -Not -Match "(?s)\`$contextArgs = @\(.*?'-IssueNumber'"
    }

    It 'adds the anonymous fallback as a hashtable entry rather than an array element' {
        $script:Pipeline | Should -Match "\`$contextArgs\['AllowAnonymousFallback'\] = \`$true"
    }
}

Describe 'MAUI Copilot pipeline argument contracts' {
    It 'passes only arguments the issue-context script actually declares' {
        # The pipeline and the script are edited independently, and a parameter
        # that exists on an inner function but never reached the script's own
        # param block fails only on the agent, after provisioning.
        $block = [regex]::Match($script:Pipeline, '(?s)\$contextArgs = @\{(?<body>.*?)\n\s*\}')
        $block.Success | Should -BeTrue

        $keys = [regex]::Matches($block.Groups['body'].Value, '(?m)^\s*(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*=') |
            ForEach-Object { $_.Groups['key'].Value }
        $keys | Should -Not -BeNullOrEmpty

        $conditional = [regex]::Matches($script:Pipeline, "\`$contextArgs\['(?<key>[A-Za-z_][A-Za-z0-9_]*)'\]") |
            ForEach-Object { $_.Groups['key'].Value }

        $declared = (Get-Command (Join-Path $PSScriptRoot 'shared/Get-ReplicationIssueContext.ps1')).Parameters.Keys

        foreach ($key in (@($keys) + @($conditional) | Sort-Object -Unique)) {
            $declared | Should -Contain $key -Because "the pipeline passes -$key to Get-ReplicationIssueContext.ps1"
        }
    }

    It 'declares the anonymous fallback on the script it is passed to' {
        $script = Get-Command (Join-Path $PSScriptRoot 'shared/Get-ReplicationIssueContext.ps1')

        $script.Parameters.Keys | Should -Contain 'AllowAnonymousFallback'
        $script.Parameters['AllowAnonymousFallback'].ParameterType.Name |
            Should -BeExactly 'SwitchParameter'
    }

    It 'forwards the anonymous fallback from the entry point to the reader' {
        $source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'shared/Get-ReplicationIssueContext.ps1')
        $entry = [regex]::Match($source, '(?s)if \(\$MyInvocation\.InvocationName -ne .\..\) \{(?<body>.*)')

        $entry.Success | Should -BeTrue
        $entry.Groups['body'].Value | Should -Match '-AllowAnonymousFallback:\$AllowAnonymousFallback'
    }
}

Describe 'The trusted publisher stages every module its gate loads' {
    It 'copies each shared script every staged publisher script dot-sources' {
        # Builds 15029872 and 15029876 each reproduced their issue and were
        # rejected at the gate because Get-ReplicationCertification.ps1 was
        # dot-sourced but never staged. The staging list is written by hand, so
        # it is compared against what the gate actually loads.
        $stagingBlock = [regex]::Match(
            $script:Pipeline,
            "(?s)\`$trustedRoot = Join-Path.*?foreach \(\`$name in @\((.*?)\)\) \{").Groups[1].Value
        $stagingBlock | Should -Not -BeNullOrEmpty

        # Build 15030627 reproduced its issue, passed the gate and then failed
        # in the publisher, so every staged script is checked, not just the gate.
        $staged = @([regex]::Matches($stagingBlock, "'([A-Za-z0-9\-]+\.ps1)'") |
            ForEach-Object { $_.Groups[1].Value })
        $staged.Count | Should -BeGreaterThan 0

        $required = [System.Collections.Generic.HashSet[string]]::new()
        foreach ($stagedName in $staged) {
            $stagedPath = Join-Path $PSScriptRoot "shared/$stagedName"
            if (-not (Test-Path -LiteralPath $stagedPath -PathType Leaf)) {
                throw "The pipeline stages $stagedName, but no such shared script exists."
            }
            $source = Get-Content -LiteralPath $stagedPath -Raw
            foreach ($match in [regex]::Matches($source, "(?m)^\s*\.\s+.*?['`"]?([A-Za-z0-9\-]+\.ps1)")) {
                [void]$required.Add($match.Groups[1].Value)
            }
            foreach ($match in [regex]::Matches($source, "Join-Path\s+\`$PSScriptRoot\s+'([A-Za-z0-9\-]+\.ps1)'")) {
                [void]$required.Add($match.Groups[1].Value)
            }
        }
        $required.Count | Should -BeGreaterThan 0

        foreach ($name in $required) {
            $stagingBlock | Should -Match ([regex]::Escape($name)) -Because `
                "a staged publisher script dot-sources $name, so it must be staged too"
        }
    }

    It 'allows every candidate manifest field the orchestrator writes' {
        # Build 15031427 reproduced its issue on a device, authored a passing
        # negative control and was then thrown away because the gate had never
        # been told the manifest gained a 'negativeControl' field. An allowlist
        # that lags the writer silently destroys finished work, so derive the
        # written names from the orchestrator instead of trusting the list.
        $orchestrator = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        # Several manifests in this script open with a schema version, so anchor
        # on the candidate manifest's own last field and walk back to its start.
        $manifestEnd = $orchestrator.IndexOf("        patch = 'test.patch'")
        $manifestEnd | Should -BeGreaterThan 0
        $manifestStart = $orchestrator.LastIndexOf('schemaVersion = 1', $manifestEnd)
        $manifestStart | Should -BeGreaterThan 0
        $manifestBlock = $orchestrator.Substring(
            $manifestStart, $manifestEnd - $manifestStart)

        # Only the manifest's own top-level keys are validated by the gate's
        # allowlist; nested hashtable keys are checked by their own rules.
        $written = @([regex]::Matches(
                $manifestBlock, "(?m)^        ([A-Za-z][A-Za-z0-9]*) = ") |
            ForEach-Object { $_.Groups[1].Value })
        $written | Should -Contain 'negativeControl'
        $written.Count | Should -BeGreaterThan 10

        $gate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'shared/Validate-ReplicationCandidate.ps1') -Raw
        $allowStart = $gate.IndexOf('$allowedProperties = @(')
        $allowStart | Should -BeGreaterThan 0
        $allowEnd = $gate.IndexOf(')', $allowStart)
        $allowed = @([regex]::Matches(
                $gate.Substring($allowStart, $allowEnd - $allowStart),
                "'([A-Za-z_][A-Za-z0-9_]*)'") |
            ForEach-Object { $_.Groups[1].Value })

        foreach ($name in $written) {
            $allowed | Should -Contain $name -Because `
                "the orchestrator writes the manifest field $name"
        }
    }
}

Describe 'Every in-process call binds parameters its target actually declares' {
    # The negative control never ran once. It called Invoke-ReplicationCopilot
    # with -LogPath, which that function does not declare, so every reproduction
    # from the control's introduction until build 15031868 published claiming
    # "No negative control was run". The catch around the call reported the
    # binding error as an author refusal, which is a legitimate outcome, so
    # nothing upstream ever looked wrong.
    BeforeAll {
        $script:ScriptPath = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
        $tokens = $null
        $errors = $null
        $script:Ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref]$tokens, [ref]$errors)
        $script:Ast | Should -Not -BeNullOrEmpty

        $script:Definitions = @{}
        foreach ($fn in $script:Ast.FindAll({
                param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $true)) {
            $parameters = $null
            if ($fn.Body.ParamBlock) { $parameters = $fn.Body.ParamBlock.Parameters }
            elseif ($fn.Parameters) { $parameters = $fn.Parameters }
            if (-not $parameters) { continue }

            $declared = [ordered]@{}
            foreach ($p in $parameters) {
                $isMandatory = $false
                foreach ($attribute in $p.Attributes) {
                    if ($attribute -isnot [System.Management.Automation.Language.AttributeAst]) { continue }
                    if ($attribute.TypeName.Name -notmatch '^Parameter$') { continue }
                    foreach ($named in $attribute.NamedArguments) {
                        if ($named.ArgumentName -eq 'Mandatory' -and
                            $named.Argument.Extent.Text -match 'true') { $isMandatory = $true }
                    }
                }
                $declared[$p.Name.VariablePath.UserPath] = $isMandatory
            }
            $script:Definitions[$fn.Name] = $declared
        }
    }

    It 'passes only declared parameters, and every mandatory one' {
        $problems = [System.Collections.Generic.List[string]]::new()

        foreach ($call in $script:Ast.FindAll({
                param($n) $n -is [System.Management.Automation.Language.CommandAst] }, $true)) {
            $name = $call.GetCommandName()
            if (-not $name -or -not $script:Definitions.Contains($name)) { continue }

            $declared = $script:Definitions[$name]
            $supplied = [System.Collections.Generic.List[string]]::new()
            $splatted = $false
            foreach ($element in $call.CommandElements) {
                if ($element -is [System.Management.Automation.Language.CommandParameterAst]) {
                    [void]$supplied.Add($element.ParameterName)
                } elseif ($element -is [System.Management.Automation.Language.VariableExpressionAst] -and
                    $element.Splatted) {
                    $splatted = $true
                }
            }
            if ($splatted) { continue }

            $line = $call.Extent.StartLineNumber
            foreach ($parameterName in $supplied) {
                # PowerShell accepts an unambiguous prefix, so resolve the same way.
                $matched = @($declared.Keys | Where-Object {
                    $_ -eq $parameterName -or $_ -like "$parameterName*" })
                if ($matched.Count -eq 0) {
                    [void]$problems.Add("line ${line}: $name has no parameter -$parameterName")
                }
            }
            if ($supplied.Count -eq 0) { continue }
            foreach ($parameterName in $declared.Keys) {
                if (-not $declared[$parameterName]) { continue }
                $isBound = @($supplied | Where-Object { $parameterName -like "$_*" }).Count -gt 0
                if (-not $isBound) {
                    [void]$problems.Add("line ${line}: $name requires -$parameterName")
                }
            }
        }

        $problems -join "`n" | Should -BeExactly ''
    }
}

Describe 'The gate expects every artifact the verifier retains' {
    # Fourth incident of a hand-maintained list discarding finished device
    # work: the YAML staging list, the verification-result allowlist, the
    # candidate-manifest allowlist, and now the verification *directory*
    # allowlist. Builds 15032408 and 15032410 both reproduced their issue on a
    # device and were thrown away because verification-test-result.trx was
    # written without being expected. The expectation is derived from the
    # producer here rather than restated.
    BeforeAll {
        $root = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
        $script:VerifierSource = Get-Content -Raw -LiteralPath (Join-Path $root `
            'skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1')
        $script:GateSource = Get-Content -Raw -LiteralPath (Join-Path $root `
            'scripts/shared/Validate-ReplicationCandidate.ps1')
    }

    It 'retains a name built from a bounded set of extensions' {
        # An extension copied from whatever the runner produced could be
        # anything, and the gate accepts exactly two.
        $script:VerifierSource |
            Should -Match "\`$extension = if \(\[IO\.Path\]::GetExtension\(\`$authoritativePath\) -ieq '\.trx'\) \{ '\.trx' \} else \{ '\.xml' \}"
    }

    It 'accepts every retained name the verifier can actually produce' {
        $stem = [regex]::Match($script:VerifierSource,
            '\$retainedResultName = "(?<stem>[a-z-]+)\$extension"').Groups['stem'].Value
        $stem | Should -Not -BeNullOrEmpty -Because 'the retained name must be discoverable from the producer'

        $pattern = [regex]::Match($script:GateSource,
            "\`$retainedResultPattern = '(?<p>[^']+)'").Groups['p'].Value
        $pattern | Should -Not -BeNullOrEmpty -Because 'the gate must declare the pattern it accepts'

        foreach ($extension in @('.trx', '.xml')) {
            "$stem$extension" | Should -Match $pattern -Because "the verifier can write $stem$extension"
        }
    }

    It 'consults the retained pattern at every artifact rejection' {
        # Counting occurrences let a second allowlist ship without the pattern:
        # the machine-readable branch rejected verification-test-result.trx and
        # destroyed build 15032847, a Catalyst reproduction whose negative
        # control had already passed 3 of 3. Enumerate the rejection sites from
        # the source instead of asserting how many there should be.
        $tokens = $null
        $errors = $null
        $gatePath = Join-Path (Split-Path -Parent (Split-Path -Parent $PSCommandPath)) `
            'scripts/shared/Validate-ReplicationCandidate.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $gatePath, [ref]$tokens, [ref]$errors)

        $rejections = $ast.FindAll({
                param($n)
                $n -is [System.Management.Automation.Language.ThrowStatementAst] -and
                $n.Extent.Text -match 'unexpected artifact'
            }, $true)
        $rejections.Count | Should -BeGreaterOrEqual 2 -Because 'both directory layouts reject artifacts'

        # A guard may consult the pattern through a well-named intermediate,
        # so accept any variable that is itself derived from it.
        $derived = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach ($assignment in $ast.FindAll({
                    param($n)
                    $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                    $n.Right.Extent.Text -match '\$retainedResultPattern'
                }, $true)) {
            if ($assignment.Left -is [System.Management.Automation.Language.VariableExpressionAst]) {
                [void]$derived.Add($assignment.Left.VariablePath.UserPath)
            }
        }

        foreach ($rejection in $rejections) {
            $guard = $rejection.Parent
            while ($null -ne $guard -and
                $guard -isnot [System.Management.Automation.Language.IfStatementAst]) {
                $guard = $guard.Parent
            }
            $guard | Should -Not -BeNullOrEmpty -Because 'each rejection must sit inside a guard'
            $condition = $guard.Clauses[0].Item1.Extent.Text
            $consults = $condition -match '\$retainedResultPattern'
            foreach ($name in $derived) {
                if ($condition -match ('\$' + [regex]::Escape($name) + '\b')) {
                    $consults = $true
                }
            }
            $consults | Should -BeTrue -Because (
                "the rejection at line $($rejection.Extent.StartLineNumber) must accept the retained result")
        }
    }

    It 'names the artifact it rejects' {
        # The original message said only that something was unexpected, which
        # is why several lost reproductions took a log download each to explain.
        # Derive the requirement from the rejection sites rather than listing
        # the messages, so a new rejection cannot ship nameless.
        $tokens = $null
        $errors = $null
        $gatePath = Join-Path (Split-Path -Parent (Split-Path -Parent $PSCommandPath)) `
            'scripts/shared/Validate-ReplicationCandidate.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $gatePath, [ref]$tokens, [ref]$errors)

        $rejections = $ast.FindAll({
                param($n)
                $n -is [System.Management.Automation.Language.ThrowStatementAst] -and
                $n.Extent.Text -match 'unexpected artifact'
            }, $true)
        $rejections.Count | Should -BeGreaterOrEqual 2

        foreach ($rejection in $rejections) {
            $rejection.Extent.Text | Should -Match 'Get-ReportableArtifactName' -Because (
                "the rejection at line $($rejection.Extent.StartLineNumber) must say which file it means")
        }
    }
}

Describe 'The negative control author gets every attempt it is allotted' {
    BeforeAll {
        $script:orchestrator = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
        $tokens = $null
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:orchestrator, [ref]$tokens, [ref]$errors)
        $script:controlFunction = $ast.FindAll({
                param($n)
                $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $n.Name -eq 'Invoke-ReplicationNegativeControl'
            }, $true) | Select-Object -First 1
    }

    It 'finds the negative control function' {
        $script:controlFunction | Should -Not -BeNullOrEmpty
    }

    It 'never downgrades a recoverable control failure before the last attempt' {
        # A silent non-write and an uninformative variant are both recoverable,
        # so each must retry. Returning on the first non-write discarded
        # certification the author would have earned on a retry.
        $body = $script:controlFunction.Extent.Text
        $guards = ([regex]::Matches($body, '\$round\s+-eq\s+\$MaxControlAttempts')).Count
        $guards | Should -BeGreaterOrEqual 2
    }

    It 'attempt-guards every recoverable control failure it reports' {
        # Enumerate the branches instead of naming one of them: an earlier
        # version pinned the exact sentence 'wrote no control variant' and went
        # stale the moment the artifact was renamed, which hides whether the
        # branch is still guarded at all. Walk each report up to its enclosing
        # if-statements so the check does not depend on how far the guard sits
        # from the message.
        $reports = $script:controlFunction.FindAll({
                param($n)
                $n -is [System.Management.Automation.Language.CommandAst] -and
                $n.Extent.Text -match 'Write-Host "Negative control attempt'
            }, $true)
        $reports.Count | Should -BeGreaterOrEqual 3

        foreach ($report in $reports) {
            # The guard is an ancestor when the whole retry branch is wrapped
            # in '-lt $MaxControlAttempts', and a sibling when the downgrade
            # that follows the message is wrapped in '-eq $MaxControlAttempts'.
            # Both spend every allotted attempt, so accept either shape.
            $guarded = $false
            $node = $report.Parent
            while ($node -and -not $guarded) {
                if ($node -is [System.Management.Automation.Language.IfStatementAst]) {
                    foreach ($clause in $node.Clauses) {
                        if ($clause.Item1.Extent.Text -match '\$MaxControlAttempts') {
                            $guarded = $true
                        }
                    }
                }
                if ($node -is [System.Management.Automation.Language.StatementBlockAst]) {
                    foreach ($sibling in $node.Statements) {
                        if ($sibling -is [System.Management.Automation.Language.IfStatementAst]) {
                            foreach ($clause in $sibling.Clauses) {
                                if ($clause.Item1.Extent.Text -match '\$MaxControlAttempts') {
                                    $guarded = $true
                                }
                            }
                        }
                    }
                }
                if ($node -eq $script:controlFunction) { break }
                $node = $node.Parent
            }

            $guarded | Should -BeTrue -Because (
                "$($report.Extent.Text) must sit inside a branch guarded by the attempt count")

            $following = $script:controlFunction.Extent.Text
            $offset = $following.IndexOf($report.Extent.Text, [StringComparison]::Ordinal)
            $following.Substring($offset,
                [Math]::Min(400, $following.Length - $offset)) |
                Should -Match 'continue' -Because (
                    "$($report.Extent.Text) must retry before the last attempt")
        }
    }
}

Describe 'A control the device runner never executed is not evidence against the test' {
    BeforeAll {
        $script:runner = Join-Path $PSScriptRoot '../skills/run-device-tests/scripts/Run-DeviceTests.ps1'
        $script:orchestratorSource = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
    }

    It 'reads the device runner that reports a class which never ran' {
        Test-Path -LiteralPath $script:runner -PathType Leaf | Should -BeTrue
    }

    It 'recognises every not-run message the device runner can throw' {
        # Derive the phrase from the producer instead of restating it, so the
        # detector cannot drift away from the runner that emits it. Build
        # 15032401 blocked a finished Android reproduction because the control
        # reported "the target tests did not run" and the detector, which knew
        # only Appium session faults, let it fall through to a hard rejection.
        $runnerSource = Get-Content -LiteralPath $script:runner -Raw
        $phrases = [regex]::Matches($runnerSource, '\(the target tests did not run\)')
        $phrases.Count | Should -BeGreaterThan 0

        $literal = 'the target tests did not run'
        $script:orchestratorSource | Should -Match ([regex]::Escape($literal))
    }

}

Describe 'Every artifact written beside the verification result is accounted for' {
    # Five separate incidents destroyed finished device work because a producer
    # wrote a file into the directory the credential-free gate inspects and no
    # one taught the gate about it. Derive the producers' own names here so a
    # sixth cannot ship: each must either be accepted by the gate or provably
    # removed before the gate runs.
    BeforeAll {
        $root = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
        $script:VerificationSource = Get-Content -Raw -LiteralPath (Join-Path $root `
            'scripts/shared/Invoke-ReplicationTestVerification.ps1')
        $script:GateText = Get-Content -Raw -LiteralPath (Join-Path $root `
            'scripts/shared/Validate-ReplicationCandidate.ps1')
    }

    It 'discovers the names the verifier writes into its output directory' {
        $names = [regex]::Matches($script:VerificationSource,
            "Join-Path\s+\`$OutputDirectory\s+'(?<name>[^']+)'") |
            ForEach-Object { $_.Groups['name'].Value } | Sort-Object -Unique
        $names.Count | Should -BeGreaterThan 0
        $names | Should -Contain 'verification-result.json'
    }

    It 'either accepts or removes every name the verifier writes there' {
        $names = [regex]::Matches($script:VerificationSource,
            "Join-Path\s+\`$OutputDirectory\s+'(?<name>[^']+)'") |
            ForEach-Object { $_.Groups['name'].Value } | Sort-Object -Unique

        foreach ($name in $names) {
            $accepted = $script:GateText -match ("'" + [regex]::Escape($name) + "'")
            # An intermediate file is acceptable only if it cannot survive the
            # run, which means its removal is guaranteed by a finally block.
            $variable = [regex]::Match($script:VerificationSource,
                "(?<var>\`$\w+)\s*=\s*Join-Path\s+\`$OutputDirectory\s+'" +
                [regex]::Escape($name) + "'").Groups['var'].Value
            $removed = $false
            if ($variable) {
                $removed = $script:VerificationSource -match (
                    'finally\s*\{[^}]*Remove-Item[^}]*' + [regex]::Escape($variable))
            }

            ($accepted -or $removed) | Should -BeTrue -Because (
                "'$name' is written beside the verification result, so the gate must " +
                'accept it or a finally block must remove it')
        }
    }
}

Describe 'The gate grades the control the verifier actually ran' {
    # Every published reproduction, including build 15033161 whose control
    # passed 3 of 3 on device, was graded 'no negative control was run'. The
    # gate looked for the control inside verification-result.json, but the
    # control runs after that file is written and its result goes to its own
    # artifact. Derive the filename from the verifier so the two cannot drift.
    BeforeAll {
        $root = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
        $script:VerifierText = Get-Content -Raw -LiteralPath (Join-Path $root `
            'scripts/shared/Invoke-ReplicationTestVerification.ps1')
        $script:GateText2 = Get-Content -Raw -LiteralPath (Join-Path $root `
            'scripts/shared/Validate-ReplicationCandidate.ps1')
    }

    It 'finds the artifact the verifier writes for the control' {
        $script:VerifierText | Should -Match "controlPath = Join-Path \`$OutputDirectory '(?<n>[^']+)'"
        [regex]::Match($script:VerifierText,
            "controlPath = Join-Path \`$OutputDirectory '(?<n>[^']+)'").Groups['n'].Value |
            Should -BeExactly 'negative-control-result.json'
    }

    It 'reads the control counts from that artifact' {
        $name = [regex]::Match($script:VerifierText,
            "controlPath = Join-Path \`$OutputDirectory '(?<n>[^']+)'").Groups['n'].Value
        $script:GateText2 | Should -Match ([regex]::Escape($name))
    }

    It 'does not take the control from the verification result it precedes' {
        # verification-result.json is written before the control runs, so a
        # lookup there can only ever report that no control happened.
        $script:GateText2 | Should -Not -Match "\`$result\.PSObject\.Properties\['negativeControl'\]"
    }
}

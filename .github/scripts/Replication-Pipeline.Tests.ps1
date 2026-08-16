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
        $script:Pipeline | Should -Match 'gh api -X POST repos/dotnet/maui/forks'
        $script:Pipeline | Should -Match 'newly created MauiBot fork did not become writable within 60 seconds'
        $script:Pipeline | Should -Match "(?s)job: ValidateReplicationPublisher.*?GH_TOKEN: \$\(GH_COMMENT_TOKEN\)"
        $migrationIndex = $script:Pipeline.IndexOf("displayName: 'Move existing reproduction PRs and comments to testing fork'")
        $copilotJobIndex = $script:Pipeline.IndexOf("- job: CopilotReview")
        $migrationIndex | Should -BeGreaterThan -1
        $migrationIndex | Should -BeLessThan $copilotJobIndex
        $script:Pipeline | Should -Match 'sparseCheckoutDirectories: \.github/scripts/shared'
        $script:Pipeline | Should -Match 'Move-ReplicationPRsToTestingFork\.ps1'
        $script:Pipeline | Should -Match 'Move-ReplicationPRCommentsToTestingFork\.ps1'
        $script:Pipeline | Should -Match 'Export-ReplicationPRFeedback\.ps1'
        $script:Pipeline | Should -Match "artifact: 'ReplicationFeedback'"
        $script:Pipeline | Should -Match "(?s)- job: CopilotReview.*?condition: or\(eq\('\$\{\{ parameters\.Mode \}\}', 'review'\), eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\)\)"
        $script:Pipeline | Should -Match 'git checkout --detach origin/main'
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
        $script:Pipeline | Should -Match "(?s)- stage: PublishReplication.*?condition: and\(eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\), in\(dependencies\.ReviewPR\.result, 'Succeeded', 'SucceededWithIssues', 'Failed'\)\)"
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
}

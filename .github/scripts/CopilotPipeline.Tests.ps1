#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $repoRoot = Join-Path $PSScriptRoot '..' '..' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $pipelinePath = Join-Path $PSScriptRoot '..' '..' 'eng' 'pipelines' 'ci-copilot.yml' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $pipeline = Get-Content -Raw -LiteralPath $pipelinePath
    $provisionPath = Join-Path $PSScriptRoot '..' '..' 'eng' 'pipelines' 'common' 'provision.yml' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $provision = Get-Content -Raw -LiteralPath $provisionPath

    $script:BaseBranchPatterns = @(
        $pipeline -split '\r?\n' |
            Where-Object { $_ -match 'BASE_REF.*=~ \^' } |
            ForEach-Object {
                if ($_ -notmatch '=~ \^(.+?)\$ \]\]') {
                    throw "Could not extract the base-branch allowlist from: $_"
                }

                "^$($Matches[1])$"
            }
    )

    $script:AutomationFiles = @(
        Get-ChildItem -LiteralPath (Join-Path $repoRoot '.github') -Recurse -File |
            Where-Object { $_.Extension -in @('.json', '.md', '.ps1', '.rb', '.sh', '.yaml', '.yml') }
        Get-Item -LiteralPath $pipelinePath
    )
}

Describe 'Copilot reviewer base branch allowlist' {
    It 'keeps every pipeline validator synchronized' {
        $script:BaseBranchPatterns.Count | Should -Be 5
        @($script:BaseBranchPatterns | Select-Object -Unique).Count | Should -Be 1
    }

    Describe 'Copilot model policy' {
        It 'contains no Anthropic model identifiers in repository automation' {
            $violations = foreach ($file in $script:AutomationFiles) {
                Select-String `
                    -LiteralPath $file.FullName `
                    -Pattern '(?i)\b(?:claude|anthropic)-[a-z0-9][a-z0-9.-]*\b' `
                    -AllMatches |
                    ForEach-Object { "$($file.FullName):$($_.LineNumber):$($_.Line.Trim())" }
            }

            @($violations) | Should -BeNullOrEmpty
        }

        It 'pins the primary reviewer instead of accepting an environment override' {
            $reviewScript = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'Review-PR.ps1')

            $reviewScript | Should -Match ([regex]::Escape("`$copilotModel = 'gpt-5.6-sol'"))
            $reviewScript | Should -Not -Match 'COPILOT_REVIEW_MODEL'
        }

        It 'pins the local test reviewer instead of accepting an environment override' {
            $reviewTestsScript = Get-Content -Raw -LiteralPath (
                Join-Path $PSScriptRoot 'Review-Tests.ps1')

            $reviewTestsScript | Should -Match ([regex]::Escape('$model = "gpt-5.6-sol"'))
            $reviewTestsScript | Should -Not -Match 'COPILOT_REVIEW_TESTS_MODEL'
        }
    }

    It 'accepts every supported base branch shape' {
        $pattern = $script:BaseBranchPatterns[0]

        @(
            'main'
            'net12.0'
            'inflight/current'
            'inflight/10.0.101'
            'inflight/11.0.100-preview.7'
            'release/10.0.1xx-sr10'
            'release/11.0.1xx-preview7.1'
        ) | ForEach-Object {
            [regex]::IsMatch($_, $pattern) | Should -BeTrue -Because "'$_' is supported"
        }
    }

    It 'rejects unsupported or unsafe base branch shapes' {
        $pattern = $script:BaseBranchPatterns[0]

        @(
            'improved-reviewer'
            'inflight/.hidden'
            'inflight/Future'
            'inflight/foo/bar'
            'inflight/../main'
            'refs/heads/inflight/current'
            'release/custom'
        ) | ForEach-Object {
            [regex]::IsMatch($_, $pattern) | Should -BeFalse -Because "'$_' is unsupported or unsafe"
        }
    }
}

Describe 'Copilot review artifact publication' {
    It 'uses the trusted fixed-path bounded exporter' {
        $pipeline | Should -Match ([regex]::Escape(
            '$exportScript = Join-Path $trusted "scripts/shared/Export-ExpectedPRAgentArtifact.ps1"'))
        $pipeline | Should -Match 'Export-ExpectedPRAgentArtifact\s+`'
        $pipeline | Should -Match ([regex]::Escape(
            "targetPath: '`$(Build.ArtifactStagingDirectory)/copilot-logs-publish'"))
        $pipeline | Should -Match ([regex]::Escape(
            "condition: and(succeededOrFailed(), eq(variables['BoundedCopilotLogsReady'], 'true'))"))
    }

    It 'does not recursively publish agent-controlled workspaces' {
        $pipeline | Should -Not -Match (
            'Copy-Item\s+-Path\s+"CustomAgentLogsTmp"\s+-Destination\s+\$logsDir\s+-Recurse')
        $pipeline | Should -Not -Match (
            'Get-ChildItem\s+-Path\s+\.\s+-Filter\s+"Review_Feedback_\*\.md"\s+-Recurse')
        $pipeline | Should -Not -Match (
            'Copy-Item\s+-Path\s+"\.github/agent-pr-session".*-Recurse')
    }
}

Describe 'Android provisioning branch compatibility' {
    It 'selects the SDK query supported by the checked-out branch tool manifest' {
        $provision | Should -Match ([regex]::Escape(
            '$toolIds = @($toolManifest.tools.PSObject.Properties.Name)'))
        $provision | Should -Match ([regex]::Escape(
            "if (`$toolIds -contains 'microsoft.maui.cli')"))
        $provision | Should -Match ([regex]::Escape(
            '& dotnet maui android sdk check --ci --json'))
        $provision | Should -Match ([regex]::Escape(
            "elseif (`$toolIds -contains 'androidsdk.tool')"))
        $provision | Should -Match ([regex]::Escape(
            '& dotnet android sdk info --format=Json'))
    }

    It 'isolates legacy JDK apt provisioning from the unrelated Chrome feed' {
        $provision | Should -Match ([regex]::Escape(
            "if (`$toolIds -contains 'androidsdk.tool')"))
        $provision | Should -Match ([regex]::Escape(
            "'dl.google.com/linux/chrome'"))
        $provision | Should -Match ([regex]::Escape(
            "'.maui-jdk-disabled'"))
        $provision | Should -Match ([regex]::Escape(
            "displayName: 'Restore Chrome apt feed after legacy JDK provisioning'"))
        $provision | Should -Match ([regex]::Escape(
            "condition: and(always(), eq(variables['Agent.OS'], 'Linux'))"))
    }
}

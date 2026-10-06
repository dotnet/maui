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
    $vallySetupPath = Join-Path $PSScriptRoot 'SetupVallyRuntime.sh' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $vallySetup = Get-Content -Raw -LiteralPath $vallySetupPath

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

}

Describe 'Copilot review artifact publication' {
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

Describe 'Vally runtime dependency pinning' {
    It 'pins the Copilot SDK that supplies the security-argument-compatible CLI' {
        $vallySetup | Should -Match ([regex]::Escape(
            'copilot_sdk_version=1.0.7'))
        $vallySetup | Should -Match ([regex]::Escape(
            '"@github/copilot-sdk@${copilot_sdk_version}"'))
        $vallySetup | Should -Match ([regex]::Escape(
            'Expected Copilot SDK $copilot_sdk_version, found $installed_copilot_sdk_version'))
    }
}

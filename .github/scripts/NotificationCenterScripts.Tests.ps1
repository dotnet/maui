#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $disableScript = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'disable-notification-center.sh'
    $enableScript = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'enable-notification-center.sh'
    $lifecycleScripts = @(
        $disableScript
        $enableScript
        Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'dismiss-apple-account-dialog.sh'
        Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'dismiss-maccatalyst-app-recovery-dialog.sh'
    )
    $helper = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'run-as-console-user.sh'
    $uiTestsPipeline = Join-Path $PSScriptRoot '..' '..' 'eng' 'pipelines' 'common' 'ui-tests-steps.yml'
    $pesterWorkflow = Join-Path $PSScriptRoot '..' 'workflows' 'powershell-script-tests.yml'
    $shellCommand = Get-Command sh -ErrorAction SilentlyContinue
    $shell = if ($shellCommand) { $shellCommand.Path } else { $null }
}

Describe 'Notification Center script safety' {
    It 'keeps every desktop setup script best-effort and non-interactive' {
        foreach ($script in $lifecycleScripts) {
            $content = Get-Content -Raw -LiteralPath $script

            $content | Should -Match ([regex]::Escape('. "$scriptDir/run-as-console-user.sh"'))
            $content | Should -Match '\brun_as_console_user\b'
            $content | Should -Not -Match '\blaunchctl\s+asuser\b'
            $content | Should -Match ([regex]::Escape('scriptDir=$(CDPATH= cd "$(dirname "$0")" && pwd)'))
            $content | Should -Not -Match '\b(?:cd|dirname)\s+--'
            $content | Should -Not -Match '\bexit\s+1\b'
            $content.TrimEnd() | Should -Match 'exit 0$'
        }

        $helperContent = Get-Content -Raw -LiteralPath $helper
        $helperContent | Should -Match '\bsudo\s+-n(?:\s|$)'
        $helperContent | Should -Match ([regex]::Escape('if [ "$caller_uid" = "$target_uid" ]; then'))
    }

    It 'uses modern launchctl controls and verifies the plist-defined service state' {
        $disableContent = Get-Content -Raw -LiteralPath $disableScript
        $enableContent = Get-Content -Raw -LiteralPath $enableScript

        foreach ($content in @($disableContent, $enableContent)) {
            $content | Should -Match 'diagnosticLogStatus=\$\?'
            $content | Should -Match '\[ "\$diagnosticLogStatus" -ne 0 \]'
            $content | Should -Match '\[ ! -f "\$diagnosticLog" \]'
        }

        $disableContent | Should -Match 'PlistBuddy.*Print :Label'
        $disableContent | Should -Match 'PlistBuddy.*Print :Program'
        $disableContent | Should -Match '\[ ! -r "\$servicePlist" \]'
        $disableContent | Should -Match '\$serviceLabelStatus" -ne 0'
        $disableContent | Should -Match '\$serviceProgramStatus" -ne 0'
        $disableContent | Should -Match 'launchctl disable "\$serviceTarget"'
        $disableContent | Should -Match 'launchctl bootout "\$serviceTarget"'
        $disableContent | Should -Match 'launchctl print-disabled "\$serviceDomain"'
        $disableContent | Should -Match '/usr/bin/pgrep -u "\$uid" -x "\$serviceProcess"'
        $disableContent | Should -Match '\$processCheckStatus" -le 1'
        $disableContent | Should -Not -Match 'pgrep .*2>/dev/null \|\| true'
        $disableContent | Should -Match 'kill "\$pid"'
        $disableContent | Should -Match 'kill -CONT "\$pid"'
        $disableContent | Should -Match 'kill -STOP "\$pid"'
        $disableContent | Should -Match '/bin/ps -o state= -p "\$pid"'
        $disableContent | Should -Match '\[ "\$runningPids" = "\$verifiedPids" \]'
        $disableContent | Should -Match '(?s)\[ -z "\$runningPids" \].*launchctl print-disabled' -Because 'a process that exits before suspension must be re-verified through launchd'
        $disableContent | Should -Match '\$NF == "disabled"'
        $disableContent | Should -Match 'Notification Center disabled.*\(verified\)'
        $disableContent | Should -Match 'Notification Center suspended.*verified SIP fallback'
        $disableContent | Should -Not -Match 'run_as_console_user.*launchctl unload'
        $disableContent.IndexOf('kill -CONT "$pid"') | Should -BeLessThan $disableContent.IndexOf('launchctl disable "$serviceTarget"')

        $enableContent | Should -Match 'PlistBuddy.*Print :Label'
        $enableContent | Should -Match 'PlistBuddy.*Print :Program'
        $enableContent | Should -Match '\[ ! -r "\$servicePlist" \]'
        $enableContent | Should -Match '\$serviceLabelStatus" -ne 0'
        $enableContent | Should -Match '\$serviceProgramStatus" -ne 0'
        $enableContent | Should -Match 'kill -CONT "\$pid"'
        $enableContent | Should -Match '/bin/ps -o state= -p "\$1"'
        $enableContent | Should -Match 'launchctl enable "\$serviceTarget"'
        $enableContent | Should -Match 'launchctl bootstrap "\$serviceDomain" "\$servicePlist"'
        $enableContent | Should -Match 'launchctl print-disabled "\$serviceDomain"'
        $enableContent | Should -Match 'launchctl print "\$serviceTarget"'
        $enableContent | Should -Match '\$NF == "disabled"'
        $enableContent | Should -Match 'Notification Center enabled.*\(verified\)'
        $enableContent | Should -Not -Match 'run_as_console_user.*launchctl load'
        $enableContent.IndexOf('kill -CONT "$pid"') | Should -BeLessThan $enableContent.IndexOf('launchctl enable "$serviceTarget"')
    }

    It 'always restores Notification Center after shared Catalyst UI tests' {
        $pipelineContent = Get-Content -Raw -LiteralPath $uiTestsPipeline
        $enableStart = $pipelineContent.LastIndexOf("- bash:", $pipelineContent.IndexOf("displayName: 'Enable Notification Center'"))
        $enableEnd = $pipelineContent.IndexOf("timeoutInMinutes:", $enableStart)
        $enableBlock = $pipelineContent.Substring($enableStart, $enableEnd - $enableStart)

        $enableBlock | Should -Match 'condition:\s+always\(\)'
    }

    It 'rejects incomplete console-user invocations before shifting arguments' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $output = & $shell -c '. "$1"; run_as_console_user alice 501' sh $helper 2>&1

        $LASTEXITCODE | Should -Be 64
        ($output -join "`n") | Should -Match 'requires a user, uid, and command'
        ($output -join "`n") | Should -Not -Match 'shift'
    }

    It 'runs the Pester workflow when any coupled trusted asset changes' {
        $workflowContent = Get-Content -Raw -LiteralPath $pesterWorkflow

        foreach ($path in @(
            ".github/workflows/**",
            "eng/scripts/**",
            "eng/pipelines/common/provision.yml",
            "eng/pipelines/common/ui-tests-steps.yml"
        )) {
            $workflowContent | Should -Match ([regex]::Escape("- '$path'"))
        }
    }

    It 'runs commands directly when the agent already is the console user' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $sandbox = Join-Path ([System.IO.Path]::GetTempPath()) "run-as-console-user-$([System.IO.Path]::GetRandomFileName())"
        New-Item -ItemType Directory -Path $sandbox | Out-Null
        try {
            $fakeId = Join-Path $sandbox 'id'
            "#!/bin/sh`nprintf '501\n'" | Set-Content -LiteralPath $fakeId -Encoding utf8 -NoNewline
            & chmod +x $fakeId

            $output = & $shell -c 'PATH="$2:$PATH"; export PATH; . "$1"; run_as_console_user alice 501 printf "%s" direct' sh $helper $sandbox

            $LASTEXITCODE | Should -Be 0
            $output | Should -Be 'direct'
        } finally {
            Remove-Item -LiteralPath $sandbox -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'elevates launchctl before entering a different console-user session' -Skip:(-not (Get-Command sh -ErrorAction SilentlyContinue)) {
        $sandbox = Join-Path ([System.IO.Path]::GetTempPath()) "run-as-console-user-$([System.IO.Path]::GetRandomFileName())"
        New-Item -ItemType Directory -Path $sandbox | Out-Null
        try {
            $fakeId = Join-Path $sandbox 'id'
            "#!/bin/sh`nprintf '501\n'" | Set-Content -LiteralPath $fakeId -Encoding utf8 -NoNewline
            $fakeSudo = Join-Path $sandbox 'sudo'
            "#!/bin/sh`nprintf 'sudo:%s\n' `"`$*`"" | Set-Content -LiteralPath $fakeSudo -Encoding utf8 -NoNewline
            & chmod +x $fakeId $fakeSudo

            $output = & $shell -c 'PATH="$2:$PATH"; export PATH; . "$1"; run_as_console_user alice 502 printf "%s" fallback' sh $helper $sandbox

            $LASTEXITCODE | Should -Be 0
            $output | Should -Be 'sudo:-n launchctl asuser 502 sudo -n -u alice printf %s fallback'
        } finally {
            Remove-Item -LiteralPath $sandbox -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

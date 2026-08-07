#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $lifecycleScripts = @(
        Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'disable-notification-center.sh'
        Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'enable-notification-center.sh'
        Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'dismiss-apple-account-dialog.sh'
    )
    $helper = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'run-as-console-user.sh'
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
            $content | Should -Not -Match '\bexit\s+1\b'
            $content.TrimEnd() | Should -Match 'exit 0$'
        }

        $helperContent = Get-Content -Raw -LiteralPath $helper
        $helperContent | Should -Match '\bsudo\s+-n(?:\s|$)'
        $helperContent | Should -Match ([regex]::Escape('if [ "$caller_uid" = "$target_uid" ]; then'))
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

function Get-EnvErrorPatterns {
    <#
    .SYNOPSIS
        Single source of truth for environment-error patterns that trigger retry.
    .DESCRIPTION
        Returns an array of regex patterns that identify transient environment
        errors (as opposed to real test failures). Used by Invoke-UITestWithRetry,
        Review-PR.ps1 STEP 3, and the Gate (verify-tests-fail.ps1) to make
        identical retry decisions.
    #>
    return @(
        'error ADB0010.*InstallFailedException',
        'InstallFailedException',
        'Failure calling service package',
        'Broken pipe',
        'XHarness exit code:\s*83',
        'Application test run crashed',
        'SIGABRT.*load_aot_module',
        'AppiumServerHasNotBeenStartedLocally',
        'no such element.*could not be located',
        'no devices/emulators found',
        'device offline',
        'Could not connect to device',
        'Failed to launch the application',
        'cmd: Failure',
        # Wholesale HostApp launch/render failure. When the app installs but its
        # first page never renders, EVERY test in a fixture fails at OneTimeSetup
        # with "Timed out waiting for Go To Test button to appear (the app did not
        # recover after crash-recovery attempts)" (UtilExtensions.NavigateToGallery
        # -> WaitForGoToTestButtonWithRecovery). This is USUALLY an intermittent infra
        # flake (emulator/app cold-start slowness), NOT a code failure — proven by the
        # same HostApp head passing on a different agent (e.g. #36575 IndicatorView 41/41
        # while #34637 Shape / #30875 / #35640 Material3 hit all-setup-failed). The
        # test's own crash-recovery only force-stops+relaunches the app; a pipeline
        # retry additionally `adb reboot`s and rebuilds/reinstalls the app fresh,
        # which clears the stuck emulator state. Without these patterns the category
        # returned "N marked failed (setup failed)" after ONE attempt with no retry.
        #
        # ⚠️ AMBIGUOUS: the SAME text is emitted when the PR itself deterministically
        # breaks HostApp startup. They are therefore also listed in
        # Get-AmbiguousStartupPatterns, which callers use to allow exactly ONE recovery
        # retry and then treat a recurrence as a deterministic (PR-caused) failure
        # instead of burning the whole retry budget and reporting INCONCLUSIVE.
        'did not recover after crash-recovery attempts',
        'Timed out waiting for Go To Test button'
    )
}

function Get-AmbiguousStartupPatterns {
    <#
    .SYNOPSIS
        Env-error patterns whose producer emits identical text for a transient
        emulator problem AND for a deterministic PR-caused HostApp startup crash.
    .DESCRIPTION
        These are a SUBSET of Get-EnvErrorPatterns. Because the signature alone cannot
        tell the two causes apart, callers grant exactly one recovery attempt (device
        reboot + fresh rebuild/reinstall). If the very same signature reappears after
        that recovery, the failure is reproducible across a clean device state and must
        be reported as a real failure rather than retried as infrastructure — otherwise a
        PR that breaks HostApp startup consumes every category's retry budget and lands
        as INCONCLUSIVE instead of surfacing the regression.
    #>
    return @(
        'did not recover after crash-recovery attempts',
        'Timed out waiting for Go To Test button'
    )
}

function Get-VerifiedCrashStartupEnvErrorPatterns {
    <#
    .SYNOPSIS
        Retry-history patterns that specifically prove an app crash or startup failure.
    .DESCRIPTION
        EnvErrorHistory contains every matched retryable infrastructure pattern, including
        package-install, device-connectivity, and Appium failures. Only this strict subset
        is safe to use when deciding that a final timeout was crash-driven.
    #>
    $patterns = @(
        'Application test run crashed',
        'SIGABRT.*load_aot_module',
        'Failed to launch the application'
    )

    return @($patterns + @(Get-AmbiguousStartupPatterns))
}

function Test-EnvErrorHistoryHasVerifiedCrashStartup {
    param(
        [AllowNull()]
        [AllowEmptyCollection()]
        [string[]] $EnvErrorHistory
    )

    $verifiedPatterns = @(Get-VerifiedCrashStartupEnvErrorPatterns)
    foreach ($entry in @($EnvErrorHistory)) {
        if ([string]::IsNullOrWhiteSpace($entry)) {
            continue
        }

        $pattern = $entry.Trim()
        if ($pattern -ne 'timeout' -and $verifiedPatterns -contains $pattern) {
            return $true
        }
    }

    return $false
}

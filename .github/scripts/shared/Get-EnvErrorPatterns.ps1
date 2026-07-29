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
        # -> WaitForGoToTestButtonWithRecovery). This is an intermittent infra flake
        # (emulator/app cold-start slowness), NOT a code failure — proven by the same
        # HostApp head passing on a different agent (e.g. #36575 IndicatorView 41/41
        # while #34637 Shape / #30875 / #35640 Material3 hit all-setup-failed). The
        # test's own crash-recovery only force-stops+relaunches the app; a pipeline
        # retry additionally `adb reboot`s and rebuilds/reinstalls the app fresh,
        # which clears the stuck emulator state. Without this pattern the category
        # returned "N marked failed (setup failed)" after ONE attempt with no retry.
        'did not recover after crash-recovery attempts',
        'Timed out waiting for Go To Test button'
    )
}

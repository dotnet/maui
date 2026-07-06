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
        'cmd: Failure'
    )
}

#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for the PURE functions in Gather-TestFailureContext.ps1 — the deterministic
    /review tests gather script that gates merge verdicts.

.DESCRIPTION
    Gather-TestFailureContext.ps1 is a param'd, self-executing script (it does real GitHub/AzDO/
    Helix I/O on load), so these tests AST-extract only the pure, side-effect-free functions and
    dot-source them — the same pattern Run-DeviceTests.Tests.ps1 uses. No network, no auth.

    Functions covered:
      - Get-HelixWorkItemCounts      (Phase 1: anonymous /workitems completeness + fail counting)
      - Get-XUnitFailures            (Phase 2: parse xUnit v2 TestResults XML)
      - Get-ConsoleFailureReason     (Phase 2: extract crash/timeout reason from console log)
      - New-DeviceWorkItemFailureRecords (Phase 2: classify ONE failed work item into records)

    CORE INVARIANT under test: NEVER A FALSE GREEN. A non-zero-ExitCode device-test work item must
    ALWAYS yield at least one capping record, and any incompleteness (running items, short counts,
    unreadable result files, a killed/hung/crashed run, or a non-zero exit with zero named failures)
    must keep the verdict at "needs human investigation".

.EXAMPLE
    Invoke-Pester ./Gather-TestFailureContext.Tests.ps1
    Invoke-Pester ./Gather-TestFailureContext.Tests.ps1 -Output Detailed
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Gather-TestFailureContext.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Get-ObjectValue',
            'Get-HelixWorkItemCounts',
            'Get-XUnitFailures',
            'Get-ConsoleFailureReason',
            'New-DeviceWorkItemFailureRecords',
            'Get-AggregatedBaseLegMap',
            'Get-PlatformFromText',
            'Get-ErrorFingerprint',
            'Get-BuildErrorSignature',
            'Test-IsTransientBuildErrorCode',
            'Get-BuildErrorsFromLog'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Get-HelixWorkItemCounts (anonymous /workitems completeness + fail counting)' {
    It 'confirms a finished, full, all-pass job as NOT unverified, 0 failures' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount 10 -JobFinished '2026-06-27T10:00:00Z'
        $r.sawCount | Should -BeTrue
        $r.totalFail | Should -Be 0
        $r.unverified | Should -BeFalse
    }

    It 'counts non-zero ExitCodes as failures and surfaces their names' {
        $mixed = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "ok$_"; State = 'Finished'; ExitCode = 0 } })
        $mixed += @(1..3 | ForEach-Object { [pscustomobject]@{ Name = "bad$_"; State = 'Finished'; ExitCode = 1 } })
        $r = Get-HelixWorkItemCounts -WorkItems $mixed -InitialWorkItemCount 13 -JobFinished '2026-06-27T10:00:00Z'
        $r.totalFail | Should -Be 3
        @($r.failedNames).Count | Should -Be 3
        (@($r.failedNames) | Where-Object { $_ -like 'bad*' }).Count | Should -Be 3
        $r.unverified | Should -BeFalse
    }

    It 'marks a still-running job unverified (items not Finished + no job finish)' {
        $running = @(1..2 | ForEach-Object { [pscustomobject]@{ Name = "run$_"; State = 'Running'; ExitCode = $null } })
        $running += @(1..5 | ForEach-Object { [pscustomobject]@{ Name = "done$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $running -InitialWorkItemCount 7 -JobFinished $null
        $r.unverified | Should -BeTrue
        $r.totalFail | Should -Be 0
    }

    It 'counts a hidden fail AND stays unverified when a sibling item is still running' {
        $mix2 = @(
            [pscustomobject]@{ Name = 'f1'; State = 'Finished'; ExitCode = 2 },
            [pscustomobject]@{ Name = 'r1'; State = 'Running'; ExitCode = $null }
        )
        $r = Get-HelixWorkItemCounts -WorkItems $mix2 -InitialWorkItemCount 2 -JobFinished '2026-06-27T10:00:00Z'
        $r.totalFail | Should -Be 1
        $r.unverified | Should -BeTrue
    }

    It 'marks a short set unverified (fewer returned than InitialWorkItemCount) even when all pass' {
        $short = @(1..3 | ForEach-Object { [pscustomobject]@{ Name = "s$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $short -InitialWorkItemCount 5 -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
    }

    It 'treats an empty array as unverified with no observed count' {
        $r = Get-HelixWorkItemCounts -WorkItems @() -InitialWorkItemCount 0 -JobFinished '2026-06-27T10:00:00Z'
        $r.sawCount | Should -BeFalse
        $r.unverified | Should -BeTrue
    }

    It 'treats $null work items as unverified with no observed count' {
        $r = Get-HelixWorkItemCounts -WorkItems $null -InitialWorkItemCount $null -JobFinished $null
        $r.sawCount | Should -BeFalse
        $r.unverified | Should -BeTrue
    }

    It 'treats a string error body as unverified' {
        $r = Get-HelixWorkItemCounts -WorkItems '404 Not Found' -InitialWorkItemCount $null -JobFinished $null
        $r.unverified | Should -BeTrue
    }

    It 'treats a non-integer ExitCode as unverified (cannot interpret), not a silent pass' {
        $weird = @([pscustomobject]@{ Name = 'w1'; State = 'Finished'; ExitCode = 'abc' })
        $r = Get-HelixWorkItemCounts -WorkItems $weird -InitialWorkItemCount 1 -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
        $r.totalFail | Should -Be 0
    }

    It 'marks unverified when the job-finished timestamp is blank' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount 10 -JobFinished '   '
        $r.unverified | Should -BeTrue
    }

    It 'handles IDictionary work items via Get-ObjectValue (member access alone would miss them)' {
        $dictItems = @(
            ([ordered]@{ Name = 'd1'; State = 'Finished'; ExitCode = 0 }),
            ([ordered]@{ Name = 'd2'; State = 'Finished'; ExitCode = 1 })
        )
        $r = Get-HelixWorkItemCounts -WorkItems $dictItems -InitialWorkItemCount 2 -JobFinished '2026-06-27T10:00:00Z'
        $r.totalFail | Should -Be 1
        $r.unverified | Should -BeFalse
    }

    It 'marks unverified when InitialWorkItemCount is null (completeness unknowable)' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount $null -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
        $r.totalFail | Should -Be 0
        $r.sawCount | Should -BeTrue
    }

    It 'marks unverified when InitialWorkItemCount is non-integer' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount 'Unknown' -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
    }

    It 'confirms when actual count EXCEEDS planned (retry republished more items, not a shortfall)' {
        $seven = @(1..7 | ForEach-Object { [pscustomobject]@{ Name = "x$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $seven -InitialWorkItemCount 6 -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeFalse
        $r.totalFail | Should -Be 0
    }
}

Describe 'Get-XUnitFailures (parse xUnit v2 TestResults XML)' {
    It 'parses mixed Pass/Fail/Skip and captures the failure message' {
        $xmlMixed = @'
<assemblies>
  <assembly name="A.dll" total="4" passed="2" failed="1" skipped="1">
    <collection>
      <test name="N.T.Pass1" type="N.T" method="Pass1" result="Pass" />
      <test name="N.T.Pass2" type="N.T" method="Pass2" result="Pass" />
      <test name="N.T.Skipped" type="N.T" method="Skipped" result="Skip" />
      <test name="N.T.Boom" type="N.T" method="Boom" result="Fail">
        <failure exception-type="Xunit.Sdk.EqualException">
          <message><![CDATA[Assert.Equal() Failure]]></message>
          <stack-trace><![CDATA[at N.T.Boom()]]></stack-trace>
        </failure>
      </test>
    </collection>
  </assembly>
</assemblies>
'@
        $x = Get-XUnitFailures -Xml $xmlMixed
        $x.parsed | Should -BeTrue
        $x.total | Should -Be 4
        $x.passed | Should -Be 2
        $x.skipped | Should -Be 1
        $x.failed | Should -Be 1
        @($x.failedTests).Count | Should -Be 1
        $x.failedTests[0].name | Should -Be 'N.T.Boom'
        $x.failedTests[0].message | Should -Match 'Assert.Equal'
    }

    It 'parses an all-pass run with zero failed tests' {
        $xmlClean = '<assemblies><assembly total="2" passed="2" failed="0" skipped="0"><collection><test name="a" result="Pass"/><test name="b" result="Pass"/></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlClean
        $x.parsed | Should -BeTrue
        $x.failed | Should -Be 0
        @($x.failedTests).Count | Should -Be 0
    }

    It 'returns parsed=$false on malformed XML (caller must NOT trust a zero count)' {
        $x = Get-XUnitFailures -Xml '<assemblies><assembly broken'
        $x.parsed | Should -BeFalse
        $x.failed | Should -Be 0
    }

    It 'returns parsed=$false on empty/whitespace input' {
        $x = Get-XUnitFailures -Xml '   '
        $x.parsed | Should -BeFalse
    }

    It 'aggregates failures across multiple assemblies' {
        $xmlMulti = '<assemblies><assembly><collection><test name="f1" result="Fail"><failure><message>boom1</message></failure></test></collection></assembly><assembly><collection><test name="f2" result="Fail"><failure><message>boom2</message></failure></test><test name="p" result="Pass"/></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlMulti
        $x.failed | Should -Be 2
        (@($x.failedTests | ForEach-Object { $_.name }) -join ',') | Should -Be 'f1,f2'
    }

    It 'tolerates a UTF-8 BOM + xml declaration (live Helix per-category files are served this way)' {
        # Regression guard for the byte[]/BOM decode bug found in live validation: Azure blob
        # serves TestResults-*.xml as application/octet-stream, so the downloaded text can carry
        # a leading BOM that would otherwise make XmlDocument.LoadXml throw.
        $xmlBom = ([char]0xFEFF) + '<?xml version="1.0" encoding="utf-8"?>' + "`n" + '<assemblies><assembly total="1" passed="1" failed="0" skipped="0"><collection><test name="ok" result="Pass"/></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlBom
        $x.parsed | Should -BeTrue
        $x.passed | Should -Be 1
    }

    It 'counts an assembly-level error node (fixture/cleanup crash) with no failed-test element as a named failure' {
        # xUnit v2 records class/collection-fixture failures and unhandled cleanup exceptions as
        # <errors><error> nodes that carry no <test> element. Missing these would let a PR-introduced
        # fixture crash vanish while a sibling test dismisses on base -> a false green.
        $xmlErr = @'
<assemblies>
  <assembly name="A.dll" total="1" passed="1" failed="0" skipped="0" errors="1">
    <errors>
      <error type="assembly-cleanup" name="A.dll">
        <failure exception-type="System.InvalidOperationException">
          <message><![CDATA[fixture teardown threw]]></message>
        </failure>
      </error>
    </errors>
    <collection>
      <test name="A.Ok" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@
        $x = Get-XUnitFailures -Xml $xmlErr
        $x.parsed | Should -BeTrue
        $x.failed | Should -Be 1
        @($x.failedTests).Count | Should -Be 1
        $x.failedTests[0].name | Should -Be 'A.dll'
        $x.failedTests[0].message | Should -Match 'teardown'
    }

    It 'exposes declaredFailed > failed when the file asserts more failures than we could extract (truncation/schema drift)' {
        # The assembly attribute claims 3 failures but only one <test result="Fail"> is present -> the
        # file is truncated/partial. declaredFailed must exceed the extracted count so the caller can
        # flag the read as incomplete and cap to NHI instead of trusting the single name.
        $xmlShort = '<assemblies><assembly total="5" passed="2" failed="3" skipped="0"><collection><test name="only.one" result="Fail"><failure><message>boom</message></failure></test></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlShort
        $x.parsed | Should -BeTrue
        $x.failed | Should -Be 1
        $x.declaredFailed | Should -Be 3
        ($x.declaredFailed -gt $x.failed) | Should -BeTrue
    }

    It 'SECURITY: returns parsed=$false on a DOCTYPE (DtdProcessing=Prohibit blocks billion-laughs / XXE)' {
        $xmlDtd = @'
<?xml version="1.0"?>
<!DOCTYPE assemblies [
  <!ENTITY a "aaaaaaaaaa">
  <!ENTITY b "&a;&a;&a;&a;&a;&a;&a;&a;&a;&a;">
]>
<assemblies><assembly total="1" passed="0" failed="1"><collection><test name="x" result="Fail"><failure><message>&b;</message></failure></test></collection></assembly></assemblies>
'@
        $x = Get-XUnitFailures -Xml $xmlDtd
        # A blocked DTD parse caps to NHI (parsed=$false -> opaque fallback), never a false green.
        $x.parsed | Should -BeFalse
        $x.failed | Should -Be 0
    }
}

Describe 'Get-ConsoleFailureReason (extract crash/timeout reason from console log)' {
    It 'flags a timeout/hang as incomplete and captures the [FAIL] line' {
        $conTimeout = @'
Starting app for category discovery...
Waiting for category discovery (timeout: 60 seconds)...
[FAIL] Timeout waiting for HybridWebView test results after 480 seconds
Test execution completed with exit code: 1
'@
        $c = Get-ConsoleFailureReason -Console $conTimeout
        $c.isIncomplete | Should -BeTrue
        $c.isTimeout | Should -BeTrue
        $c.reason | Should -Match 'HybridWebView'
    }

    It 'flags an unhandled-exception crash as incomplete' {
        $conCrash = "Running tests...`nUnhandled exception. System.AccessViolationException`nProcess was killed"
        $c = Get-ConsoleFailureReason -Console $conCrash
        $c.isCrash | Should -BeTrue
        $c.isIncomplete | Should -BeTrue
    }

    It 'does NOT flag a clean run as incomplete' {
        $conClean = "Running tests...`nAll tests passed`nDone"
        $c = Get-ConsoleFailureReason -Console $conClean
        $c.isIncomplete | Should -BeFalse
        [string]::IsNullOrEmpty($c.reason) | Should -BeTrue
    }

    It 'does NOT flag empty console output as incomplete' {
        $c = Get-ConsoleFailureReason -Console ''
        $c.isIncomplete | Should -BeFalse
    }

    It 'does NOT over-cap a cleanly-completed Windows run that merely had failing tests' {
        # run-windows-devicetests.cmd:481 echoes "Test execution completed with exit code: 1"
        # UNCONDITIONALLY for any non-zero run, and "[FAIL] <test>" is a NAMED failure, not an
        # incomplete run. Neither may set isIncomplete, or every failed Windows work item would cap to
        # NHI and a clean named failure could never flow through to base/known-issue attribution.
        $conNamed = @'
Running Microsoft.Maui.DeviceTests...
[FAIL] Microsoft.Maui.DeviceTests.ButtonTests.SomeRealTest
ERROR: At least 1 test(s) failed
Test execution completed with exit code: 1
'@
        $c = Get-ConsoleFailureReason -Console $conNamed
        $c.isIncomplete | Should -BeFalse
        # the [FAIL]/exit-code lines are still captured for the human-readable reason
        $c.reason | Should -Match 'SomeRealTest'
    }

    It 'still flags a genuine timeout even when the unconditional exit-code line is present' {
        # The category-timeout marker (:wait_for_result) and the exit-code tail co-occur; the timeout
        # marker -- not the exit-code line -- must be what trips isIncomplete.
        $conTo = @'
[FAIL] Timeout waiting for HybridWebView test results after 480 seconds
Test execution completed with exit code: 1
'@
        $c = Get-ConsoleFailureReason -Console $conTo
        $c.isIncomplete | Should -BeTrue
    }

    It 'flags a total wipeout ("All test processes may have crashed") as incomplete' {
        $c = Get-ConsoleFailureReason -Console "Launching categories...`nAll test processes may have crashed`nTest execution completed with exit code: 1"
        $c.isIncomplete | Should -BeTrue
    }
}

Describe 'New-DeviceWorkItemFailureRecords (classify ONE failed work item — never a false green)' {
    BeforeEach {
        $script:ctx = @{ platform = 'windows'; buildId = 1; buildDefinition = 'maui-pr-devicetests'; helixJobId = 'job1'; helixWorkItem = 'WI' }
        $script:conOk = [ordered]@{ reason = ''; isTimeout = $false; isCrash = $false; isIncomplete = $false }
        $script:trxFail = [ordered]@{ parsed = $true; total = 3; passed = 2; failed = 1; skipped = 0; failedTests = @([ordered]@{ name = 'Ns.Real.Test'; type = 'Ns.Real'; method = 'Test'; message = 'boom' }) }
    }

    It 'A. emits only NAMED records for a cleanly-completed run with real failures' {
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
        $recs[0]['testName'] | Should -Be 'Ns.Real.Test'
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 0
    }

    It 'B. emits ONE incomplete capping record for an infra hang (0 failed + timeout + dump)' {
        $trxHang = [ordered]@{ parsed = $true; total = 713; passed = 713; failed = 0; skipped = 30; failedTests = @() }
        $conHang = [ordered]@{ reason = '[FAIL] Timeout waiting for HybridWebView'; isTimeout = $true; isCrash = $false; isIncomplete = $true }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $trxHang -Console $conHang -HasDump $true -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-workitem-incomplete'
        $recs[0]['crashDump'] | Should -BeTrue
        $recs[0]['message'] | Should -Match 'HybridWebView'
        $recs[0]['message'] | Should -Match '713 passed'
    }

    It 'C. emits BOTH named AND incomplete records when named failures coincide with a mid-run crash' {
        $conCrash2 = [ordered]@{ reason = 'crash'; isTimeout = $false; isCrash = $true; isIncomplete = $true }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $conCrash2 -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 1
    }

    It 'D. emits an incomplete record when no result file was readable' {
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $null -Console $null -HasDump $false -AnyResultFile $false -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-workitem-incomplete'
        $recs[0]['message'] | Should -Match 'no test-results file'
    }

    It 'E. SAFETY: a non-zero-exit work item with ZERO named failures STILL caps (never trust 0-fail as clean)' {
        $trxZero = [ordered]@{ parsed = $true; total = 5; passed = 5; failed = 0; skipped = 0; failedTests = @() }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $trxZero -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-workitem-incomplete'
    }

    It 'F. SAFETY: a crash DUMP alongside named failures STILL caps, even with a clean console' {
        # A SIGSEGV/abort can flush a partial result file then kill the run, so named failures + a dump
        # means the PR regression may be among the tests that never ran. The dump alone must force the
        # incomplete cap regardless of what the console says.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $true -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['crashDump'] | Should -BeTrue
        $inc[0]['message'] | Should -Match 'crash dump'
    }

    It 'G. SAFETY: a crash SIGNAL (isCrash) with isIncomplete=$false STILL caps alongside named failures' {
        # Console may detect a crash token (core dumped / .dmp / segfault) without setting isIncomplete;
        # isCrash alone must force the cap so a mid-run native crash can never be greened.
        $conCrashOnly = [ordered]@{ reason = ''; isTimeout = $false; isCrash = $true; isIncomplete = $false }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $conCrashOnly -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 1
    }

    It 'H. SAFETY: ResultReadIncomplete (a result file overflowed/failed to read) STILL caps alongside named failures' {
        # Even with a clean console, no dump, and real named failures, a partial result-file read means
        # an unseen failure could be the PR's -> cap to NHI.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -ResultReadIncomplete $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['message'] | Should -Match 'could not be fully read'
    }

    It 'I. a fully-clean completed run (no dump, no crash, complete read) emits ONLY named records' {
        # Confirms the new guards did not over-cap: a clean run with real failures and no incompleteness
        # signal still dismisses through the normal named path (so base/known-issue dismissal still works).
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -ResultReadIncomplete $false -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 0
    }

    It 'J. SAFETY: a NEGATIVE work-item ExitCode (WorkItemCrashed) STILL caps alongside named failures' {
        # A signal-killed work item (e.g. Helix ExitCode -4 on PR #36161) can flush a partial TRX with
        # real named failures before dying. A clean console + complete-looking read must NOT let it
        # green -- the negative exit code alone forces the incomplete cap.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -WorkItemCrashed $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['message'] | Should -Match 'terminated abnormally'
    }

    It 'K. WorkItemCrashed defaults to $false so a clean named run is unaffected' {
        # The crash defense must be OPT-IN: omitting -WorkItemCrashed (a normal positive-exit failure)
        # leaves the clean named-failure flow-through intact.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
    }

    It 'L. SAFETY: a TRUNCATED console (ConsoleReadIncomplete) STILL caps alongside named failures' {
        # The timeout/crash markers print at the TAIL, so a truncated read can drop them while the head
        # still shows named '[FAIL]' lines. A truncated console on a failed work item must therefore cap
        # to NHI -- the unread tail could hold the marker proving the run never finished.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -ConsoleReadIncomplete $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['message'] | Should -Match 'console log was truncated'
    }

    It 'M. ConsoleReadIncomplete defaults to $false so a clean named run is unaffected' {
        # The truncation guard must be OPT-IN: omitting -ConsoleReadIncomplete (a console that fit under
        # the read cap) leaves the clean named-failure flow-through intact.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
    }
}

Describe 'Get-AggregatedBaseLegMap (multi-build base leg diff — network-free via pre-seeded cache)' {
    # The aggregator only calls Get-TimelineRecordResultMap on a CACHE MISS, so pre-seeding $Cache with
    # entries keyed "org|project|buildId" is a fully network-free seam: each case supplies its own base
    # single-build leg maps and asserts the green/red tallies that decide whether a PR leg is a clean
    # 'regressed-vs-base' or a base flake. A cache MISS would call the (here-undefined)
    # Get-TimelineRecordResultMap and throw, so a clean return also proves no network was attempted.
    # Single-build cache entry shape mirrors Get-TimelineRecordResultMap: { accessible; records:
    # normName -> { name; hasFailed; hasSucceeded } }.

    It 'counts a leg GREEN across all sampled base builds (all-green -> greenCount=N, failedCount=0)' {
        $cache = @{
            'o|p|101' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
            'o|p|102' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
        }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 101 }, [ordered]@{ id = 102 }) -Cache $cache
        $agg.accessible | Should -BeTrue
        $agg.sampledBuilds | Should -Be 2
        $agg.records['leg a'].greenCount | Should -Be 2
        $agg.records['leg a'].failedCount | Should -Be 0
    }

    It 'tallies a leg red on some base builds and green on others (green-plus-red)' {
        $cache = @{
            'o|p|201' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $true; hasSucceeded = $false } } }
            'o|p|202' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
            'o|p|203' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
        }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 201 }, [ordered]@{ id = 202 }, [ordered]@{ id = 203 }) -Cache $cache
        $agg.sampledBuilds | Should -Be 3
        $agg.records['leg a'].greenCount | Should -Be 2
        $agg.records['leg a'].failedCount | Should -Be 1
    }

    It 'counts a retry-then-pass base build as RED for that build (hasFailed wins over hasSucceeded)' {
        # A base build where the leg failed one attempt but a retry later passed still carries a
        # base-branch flake -> it must count RED, never GREEN, so it cannot mask a base flake and let a
        # matching PR-red occurrence be read as a clean regression.
        $cache = @{ 'o|p|301' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $true; hasSucceeded = $true } } } }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 301 }) -Cache $cache
        $agg.records['leg a'].failedCount | Should -Be 1
        $agg.records['leg a'].greenCount | Should -Be 0
    }

    It 'skips an INACCESSIBLE base build (not counted, not sampled)' {
        $cache = @{
            'o|p|401' = [ordered]@{ accessible = $false; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
            'o|p|402' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
        }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 401 }, [ordered]@{ id = 402 }) -Cache $cache
        $agg.sampledBuilds | Should -Be 1
        $agg.records['leg a'].greenCount | Should -Be 1
        $agg.baseBuildIds.Count | Should -Be 1
        $agg.baseBuildIds[0] | Should -Be 402
    }

    It 'returns accessible=$false when NO base build was readable' {
        $cache = @{ 'o|p|501' = [ordered]@{ accessible = $false; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } } }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 501 }) -Cache $cache
        $agg.accessible | Should -BeFalse
        $agg.sampledBuilds | Should -Be 0
    }

    It 'REUSES the shared cache across calls (a base id fetched once serves later PR builds — network-free)' {
        # The outer loop shares ONE $baseRecordMapCache across PR builds; a base id read for one PR
        # build must be reused for the next without a second fetch (a cache MISS would call the
        # undefined Get-TimelineRecordResultMap and throw).
        $cache = @{ 'o|p|601' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } } }
        $agg1 = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 601 }) -Cache $cache
        $agg2 = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 601 }) -Cache $cache
        $agg1.records['leg a'].greenCount | Should -Be 1
        $agg2.records['leg a'].greenCount | Should -Be 1
        $cache.Keys.Count | Should -Be 1
    }

    It 'ignores base builds with a non-positive id' {
        $cache = @{ 'o|p|701' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } } }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 0 }, [ordered]@{ id = 701 }) -Cache $cache
        $agg.sampledBuilds | Should -Be 1
        $agg.baseBuildIds.Count | Should -Be 1
        $agg.baseBuildIds[0] | Should -Be 701
    }
}

Describe 'Test-IsTransientBuildErrorCode (transient infra vs deterministic toolchain boundary)' {
    It 'classifies restore/network + file-lock codes as transient' {
        Test-IsTransientBuildErrorCode -Signature 'NU1301'  | Should -BeTrue
        Test-IsTransientBuildErrorCode -Signature 'MSB3021' | Should -BeTrue
        Test-IsTransientBuildErrorCode -Signature 'MSB3027' | Should -BeTrue
    }

    It 'classifies deterministic compiler/toolchain codes as NOT transient' {
        Test-IsTransientBuildErrorCode -Signature 'CS0246'  | Should -BeFalse   # C# compile error
        Test-IsTransientBuildErrorCode -Signature 'NU1101'  | Should -BeFalse   # package not found (deterministic)
        Test-IsTransientBuildErrorCode -Signature 'MSB4018' | Should -BeFalse   # task failed unexpectedly (deterministic)
        Test-IsTransientBuildErrorCode -Signature 'Failed to load assembly' | Should -BeFalse
        Test-IsTransientBuildErrorCode -Signature 'CrossGen/R2R'            | Should -BeFalse
    }

    It 'treats an empty/whitespace signature as NOT transient' {
        Test-IsTransientBuildErrorCode -Signature ''    | Should -BeFalse
        Test-IsTransientBuildErrorCode -Signature '   ' | Should -BeFalse
    }
}

Describe 'Get-BuildErrorsFromLog (deterministicBuildError boundary — one-green-base shortcut gate)' {
    It 'flags a transient NuGet restore/network code (NU1301) as NON-deterministic' {
        $r = @(Get-BuildErrorsFromLog -Lines @('##[error]error NU1301: Unable to load the service index for source https://pkgs.dev.azure.com/x/index.json') -LogId 10 -RecordName 'Build_iOS')
        $r.Count | Should -Be 1
        $r[0].deterministicBuildError | Should -BeFalse
    }

    It 'flags transient MSBuild file-lock codes (MSB3021 / MSB3027) as NON-deterministic' {
        $r1 = @(Get-BuildErrorsFromLog -Lines @('error MSB3021: Unable to copy file "a.dll" to "b.dll". The process cannot access the file because it is being used by another process.') -LogId 11 -RecordName 'Build_Android')
        $r1[0].deterministicBuildError | Should -BeFalse
        $r2 = @(Get-BuildErrorsFromLog -Lines @('error MSB3027: Could not copy "a.dll" to "b.dll". Exceeded retry count of 10. Failed. The file is locked by: "dotnet".') -LogId 12 -RecordName 'Build_Android')
        $r2[0].deterministicBuildError | Should -BeFalse
    }

    It 'keeps a genuine deterministic compile break (CS0246) as deterministic' {
        $r = @(Get-BuildErrorsFromLog -Lines @('Foo.cs(12,5): error CS0246: The type or namespace name ''Bar'' could not be found') -LogId 13 -RecordName 'Build_Windows')
        $r.Count | Should -Be 1
        $r[0].deterministicBuildError | Should -BeTrue
    }

    It 'does NOT blanket-exclude the NU/MSB prefixes (NU1101, MSB4018 stay deterministic)' {
        $rNu = @(Get-BuildErrorsFromLog -Lines @('error NU1101: Unable to find package Foo. No packages exist with this id.') -LogId 14 -RecordName 'Build_iOS')
        $rNu[0].deterministicBuildError | Should -BeTrue
        $rMsb = @(Get-BuildErrorsFromLog -Lines @('error MSB4018: The "GenerateResource" task failed unexpectedly.') -LogId 15 -RecordName 'Build_iOS')
        $rMsb[0].deterministicBuildError | Should -BeTrue
    }

    It 'keeps a native crash NON-deterministic (unchanged behavior)' {
        $r = @(Get-BuildErrorsFromLog -Lines @('Process terminated. Segmentation fault (core dumped)') -LogId 16 -RecordName 'Run_iOS')
        $r.Count | Should -Be 1
        $r[0].deterministicBuildError | Should -BeFalse
    }
}

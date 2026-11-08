# NUnit/Appium on Microsoft.Testing.Platform

The four `TestCases.*.Tests` Appium projects remain **NUnit** tests. They are desktop
test executables driving the target app through Appium, not native device-test
hosts. They opt into Microsoft.Testing.Platform (MTP) using `EnableNUnitRunner`.
They explicitly declare `IsTestingPlatformApplication` so SDK discovery also
works before the adapter's restored MSBuild targets are available.
`UITest.NUnit` is a non-executable support library and must not register an adapter.

## Dependencies

The test hosts use NUnit **4.0.1**, NUnit3TestAdapter **6.1.0**, and
Microsoft.Testing.Extensions.TrxReport **2.5.1**. The support library uses the same
NUnit framework version. Adapter 4.5.0 does not provide the required MTP host;
adapter 5.x targets MTP 1.x. Adapter 6.x targets MTP 2.x.

These are intentional explicit project versions, not an assumption that the old
VSTest adapter also works under the root MTP runner. The released
[6.1.0 package manifest](https://api.nuget.org/v3-flatcontainer/nunit3testadapter/6.1.0/nunit3testadapter.nuspec)
depends on Microsoft.Testing.Extensions.VSTestBridge and
Microsoft.Testing.Platform.MSBuild **2.0.2** as its minimum. The repository pins
both modules and the reporter to its shared MTP **2.5.1** version so NUnit and
xUnit hosts use compatible infrastructure. See also the
[NUnit adapter documentation](https://docs.nunit.org/articles/vs-test-adapter/Index.html).

## Commands, settings, and results

Continue using `.github/scripts/BuildAndRunHostApp.ps1` for device-backed Appium
testing. Its existing `-Category` and `-TestFilter` parameters are unchanged.
The launcher and Cake now construct root-MTP commands with `--project`,
`--configuration`, `--results-directory`, `--report-trx`, and
`--report-trx-filename`. They do not use VSTest's `--logger`, positional project
argument, or the VSTest `-- NUnit.Property=value` separator.

The trusted local launcher also supports retained target checkouts whose
`global.json` still selects VSTest. Both initial and retry attempts then retain
the positional project, `--logger trx`, and unchanged NUnit filter contract.
Those attempts validate genuine TRX and propagate process/run failures without
requiring MTP-only XML reconciliation. Migrated targets keep the stricter MTP
TRX/NUnit XML reconciliation described below. Script overlays alone never change
the target checkout's SDK protocol.

Attachment imports are restricted to the attempt's result directory and
explicitly configured test-output, diagnostic and application-log directories.
Report-provided paths cannot expand those roots: traversal, foreign absolute
paths, symbolic links and junctions fail before their contents are copied.
Legitimate absolute screenshot/log paths remain supported within those roots.
Archive destination ancestry is checked before directory creation and again
before copying, so a test-produced symbolic link or junction cannot redirect
the preserved evidence outside its result archive.

NUnit's MTP bridge accepts `--filter` with the existing category/FQN expressions,
including `TestCategory=Issues`, `FullyQualifiedName~Issue123`, exact FQNs, and
combined expressions. It accepts runsettings using `--settings`. Narrowed CI
retries continue to use NUnit's `Where` setting (including parameterized test
names and whole feature-matrix fixtures), without an additional original CLI
filter replacing that selection. Other runsettings entries are retained.

`NUnit.TestOutputXml` is set in an attempt-specific runsettings file. **Do not
remove NUnit XML when publishing TRX.** A fixture teardown error can produce
passing TRX test cases and a successful raw MTP exit code. The orchestration must
read NUnit XML, mark the TRX summary failed, and fail the job. Missing or
unreadable XML is not success.

Each attempt has an immutable `raw` directory containing its TRX, NUnit XML, and
attachment snapshots. MTP's TRX contains absolute paths to the original
screenshots/logs; unlike VSTest it need not copy them into a deployment directory.
The orchestration snapshots those files and rewrites TRX references before
another attempt can overwrite them. Final retry reconciliation retains passing
first-attempt coverage and unresolved fixture failures, including previously
ignored test cases. The original attempt evidence remains available.
Recovery checks retain each case's outcome: explicitly ignored siblings stay
`NotExecuted`, and a failed nonignored test cannot recover merely by skipping
its retry. Shared executable helper changes also trigger the UI pipeline's
branch and pull-request validation paths.

## CPU-only compatibility checks

`src/TestUtils/tests/UITest.NUnit.Tests` runs real child NUnit/MTP hosts without
Appium, devices, or a built HostApp. It checks category/FQN filters, runsettings,
TRX/NUnit XML, screenshot/log attachments, assertion/setup/teardown failures, and
retry reconciliation.

```shell
dotnet test --project src/TestUtils/tests/UITest.NUnit.Tests/UITest.NUnit.Tests.csproj --report-trx
```

For isolated validation without the repository build infrastructure, build with
`-p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false` and the
NuGet.org source, then pass the same properties to `dotnet test --project`.
Remove that project's local `bin` and `obj` directories before returning to the
normal repository build, which uses the centralized `artifacts` directories.
Production Appium projects must use the normal repository build.

The local Appium launcher also has focused PowerShell regression coverage:

```powershell
Invoke-Pester .github/scripts/shared/NUnitMtp.Tests.ps1, .github/scripts/BuildAndRunHostApp.Tests.ps1
```

These checks do not replace device-backed screenshot comparisons or a provisioned
Appium run on each platform.

## Other Cake test execution

The common desktop Cake helper selects framework-specific MTP options. NUnit
keeps `--filter` and `--report-trx`; xUnit uses its supported
`--filter-method`/`--filter-not-method`, `--filter-trait`/`--filter-not-trait`,
and built-in `--report-xunit-trx` options. Simple legacy `FullyQualifiedName`
and category filters are translated. Grouped method alternatives can be ANDed
with a category constraint. Expressions that cannot be represented faithfully
fail with guidance instead of silently running a different selection.
An xUnit query beginning with `/`, such as `/[Category=Core]`, is forwarded using
`--filter-query`. No no-tests exit code is suppressed.
Distributed MSBuild loggers stay on the build commands, not on the MTP test
command: forwarding `-dl` to the test host causes an invalid-arguments handshake
failure. Test binlogs, genuine TRX and NUnit XML remain enabled.

Native `testOnly`, `test`, and `buildAndTest` tasks in the Android, iOS,
Mac Catalyst and Windows Cake scripts now invoke `dotnet test --project`, not the
desktop Appium helper or XHarness. Their native `--test-filter` accepts
`Category=<name>` or `SkipCategories=<name>,<name>`. The value becomes the
**build property** `MauiTestFilter`; commas are MSBuild-escaped so every excluded
category reaches the app. A build is allowed because changing that property
must update the app's embedded metadata.

The selected Android/iOS device is forwarded with `--device` when available.
Android requests real MTP TRX output using `--report-trx`. iOS leaves the native
report in the app's `Documents/TestResults`; Cake copies that genuine report from
the selected simulator into the requested host result directory and validates it
before cleanup. Missing, unchanged, empty and failed reports fail the task;
stdout is never converted into TRX. Mac Catalyst sets `MauiTestResultsDirectory`
to the requested host result directory. Nonzero process exits fail the task even
when a passing report exists.
Windows delegates to the shared owned native runner with its explicit project,
TFM and packaging mode. Packaged runs use the existing signed-MSIX activation
path; unpackaged runs remain distinct. Neither mode discovers numeric category
indexes or expects legacy XML files. Both require genuine TRX and retain the
existing signed-package provisioning prerequisite.

Appium preparation, configuration, and NUnit retries remain separate from native
test execution. UI installation/discovery primitives use `adb`, `simctl`, and
`devicectl` rather than the removed XHarness tool. Explicit simulator selection
is retained; only a simulator created by this invocation is erased for reset.
No device provisioning or installation runs during the Cake dry-run checks.

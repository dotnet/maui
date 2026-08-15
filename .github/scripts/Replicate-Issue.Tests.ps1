#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
    $script:Source = Get-Content -LiteralPath $scriptPath -Raw
    $script:BuildSandboxPath = Join-Path $PSScriptRoot 'BuildAndRunSandbox.ps1'
    $script:BuildSandboxSource = Get-Content `
        -LiteralPath $script:BuildSandboxPath `
        -Raw
    . (Join-Path $PSScriptRoot 'shared/Assert-ReplicationTestGuard.ps1')
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$errors)
    if ($errors) {
        throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
    }

    foreach ($name in @(
        'ConvertTo-ReplicationSafeLog',
        'Test-PathInsideRoot',
        'Assert-NoReparsePointInParentPath',
        'Assert-BoundedGeneratedFile',
        'Assert-GeneratedSandboxXaml',
        'Assert-GeneratedSandboxSources',
        'Assert-NoDuplicateJsonProperties',
        'Read-GeneratedAppiumPlan',
        'ConvertTo-BoundedAgentLine',
        'Get-ProposedTestFiles',
        'Assert-TestProposalMatchesPlan',
        'Get-VerifierTestType',
        'Assert-GeneratedTestContent'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $name
        }, $true)
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Replication orchestrator security boundary' {
    It 'removes Azure logging directives from untrusted output' {
        ConvertTo-ReplicationSafeLog -Value 'x ##vso[task.setvariable variable=Y]bad ##[error]fake' |
            Should -BeExactly 'x bad fake'
    }

    It 'recognizes only paths inside the requested root' {
        $root = Join-Path $TestDrive 'root'
        New-Item -ItemType Directory -Path $root | Out-Null
        Test-PathInsideRoot -Path (Join-Path $root 'child/file.txt') -Root $root | Should -BeTrue
        Test-PathInsideRoot -Path (Join-Path $root '../outside.txt') -Root $root | Should -BeFalse
    }

    It 'accepts only a bounded declarative Appium plan' {
        $IssueNumber = 37440
        $appiumPlanPath = Join-Path $TestDrive 'appium-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "steps": [
    {
      "action": "tap",
      "description": "Tap the reproduction button",
      "locator": { "strategy": "accessibilityId", "value": "ReproduceButton" },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "setOrientation",
      "description": "Rotate the selected device",
      "locator": null,
      "value": "landscape",
      "timeoutSeconds": 10
    },
    {
      "action": "assertTextContains",
      "description": "Verify the reported incorrect result",
      "locator": { "strategy": "accessibilityId", "value": "ResultLabel" },
      "value": "Incorrect",
      "timeoutSeconds": 10
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        $invalid = Get-Content -LiteralPath $appiumPlanPath -Raw |
            ConvertFrom-Json -Depth 10
        $invalid.steps[-1].action = 'waitFor'
        $invalid.steps[-1].value = $null
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*must end with a deterministic assertion*'
    }

    It 'rejects dangerous capabilities in generated Sandbox source' {
        $repoRoot = $TestDrive
        $sandboxXamlPath = Join-Path $TestDrive 'MainPage.xaml'
        $sandboxCodePath = Join-Path $TestDrive 'MainPage.xaml.cs'
        @'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage">
    <Button AutomationId="ReproduceButton" />
</ContentPage>
'@ | Set-Content -LiteralPath $sandboxXamlPath
        @'
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath

        { Assert-GeneratedSandboxSources } | Should -Not -Throw

        @'
using P = System.Diagnostics.Process;
namespace Maui.Controls.Sample;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        P.Start("sh");
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath
        { Assert-GeneratedSandboxSources } | Should -Throw '*prohibited*'
    }

    It 'requires the exact bounded Sandbox XAML schema' {
$repoRoot = $TestDrive
$sandboxXamlPath = Join-Path $TestDrive 'MainPage.xaml'
$sandboxCodePath = Join-Path $TestDrive 'MainPage.xaml.cs'
@'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
     xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
     xmlns:system="clr-namespace:System"
     x:Class="Maui.Controls.Sample.MainPage">
    <system:String>unexpected namespace</system:String>
</ContentPage>
'@ | Set-Content -LiteralPath $sandboxXamlPath
@'
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
InitializeComponent();
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath

{ Assert-GeneratedSandboxSources } |
    Should -Throw '*bounded MainPage contract*'
    }

    It 'uses a trusted Appium interpreter instead of agent-authored host code' {
        $script:Source | Should -Match 'appium-plan\.json'
        $script:Source | Should -Match 'RunReplicationAppiumPlan\.cs'
        $script:Source | Should -Match 'Copy-Item[\s\S]*trustedAppiumRunnerPath'
        $script:Source | Should -Not -Match 'Create "\$appiumScriptPath"'
        $script:BuildSandboxSource | Should -Match 'REPLICATION_PLATFORM'
        $script:BuildSandboxSource | Should -Match 'REPLICATION_WINDOWS_APP_PATH'
        $script:BuildSandboxSource | Should -Match 'shell pidof -s com\.microsoft\.maui\.sandbox'
    }

    It 'stores verifier wrapper logs outside the strict verification contract' {
        $script:Source |
            Should -Match 'sandboxArtifactDir "verification-wrapper-attempt-\$attempt\.log"'
        $script:Source |
            Should -Not -Match 'verificationDir "wrapper-attempt-\$attempt\.log"'
    }

    It 'rejects duplicate Appium plan properties' {
        $IssueNumber = 37440
        $appiumPlanPath = Join-Path $TestDrive 'duplicate-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "issueNumber": 37441,
  "steps": []
}
'@ | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*duplicate JSON property*'
    }

    It 'maps bounded proposal types to existing verifier types' {
        Get-VerifierTestType unit | Should -BeExactly 'UnitTest'
        Get-VerifierTestType xaml | Should -BeExactly 'XamlUnitTest'
        Get-VerifierTestType device | Should -BeExactly 'DeviceTest'
        Get-VerifierTestType ui | Should -BeExactly 'UITest'
    }

    It 'gives Copilot no shell, URL, MCP, broad write, Azure, or GitHub publication capability' {
        $script:Source | Should -Match "'--available-tools', 'view', 'rg', 'glob', 'apply_patch'"
        $script:Source | Should -Match '--disable-builtin-mcps'
        $script:Source | Should -Match '--allow-tool.*write\(\$fullPath\)'
        $script:Source | Should -Match 'permissions must target exact regular files'
        $script:Source | Should -Not -Match 'WriteRoots'
        $script:Source | Should -Not -Match '--allow-all-tools|--allow-all-paths|--allow-all-urls|--yolo'
        $script:Source | Should -Match 'GH_REPLICATION_TOKEN'
        $script:Source | Should -Match 'Invoke-WithoutReplicationSecrets'
    }

    It 'plans exact new issue-specific test files before granting write access' {
        $repoRoot = Join-Path $TestDrive 'repo'
        $approvedTestRoots = @('tests/')
        $IssueNumber = 37440
        New-Item -ItemType Directory -Path (Join-Path $repoRoot 'tests/Issues') -Force |
            Out-Null
        $proposal = [pscustomobject]@{
            testType = 'unit'
            testFilter = 'Issue37440'
            files = @('tests/Issues/Issue37440Tests.cs')
        }

        Get-ProposedTestFiles -Proposal $proposal -ValidateNewTargets |
            Should -BeExactly 'tests/Issues/Issue37440Tests.cs'

        $proposal.files = @('tests/Issues/OtherTests.cs')
        { Get-ProposedTestFiles -Proposal $proposal -ValidateNewTargets } |
            Should -Throw '*issue-specific*'

        $proposal.files = @('tests/Issues/Issue37440Tests.cs')
        Set-Content -LiteralPath (Join-Path $repoRoot $proposal.files[0]) -Value 'existing'
        { Get-ProposedTestFiles -Proposal $proposal -ValidateNewTargets } |
            Should -Throw '*already exists*'
    }

    It 'uses stable host identifiers instead of unresolved device variables' {
        $script:Source | Should -Match 'DeviceUdid contains an unresolved pipeline variable'
        $script:Source | Should -Match "'mac-catalyst-host'"
        $script:Source | Should -Match "'windows-host'"
        $script:Source | Should -Match 'device = \$selectedDeviceId'
        $script:Source | Should -Match 'id = \$selectedDeviceId'
    }

    It 'preserves current attempt counts in blocked candidate manifests' {
        $script:Source | Should -Match 'sandbox = \$sandboxAttempts'
        $script:Source | Should -Match 'automatedTest = \$testAttempts'
        $script:Source | Should -Not -Match 'attempts = \[ordered\]@\{ sandbox = 0; automatedTest = 0 \}'
    }

    It 'prepares the app before starting a bounded recording-only run' {
        $script:Source | Should -Match "'-PrepareOnly'"
        $script:Source | Should -Match "'-SkipBuildDeploy'"
        $script:Source | Should -Match ([regex]::Escape("'-RepoRoot', `$repoRoot"))
        $script:BuildSandboxSource | Should -Match '\[string\]\$RepoRoot'
        $script:BuildSandboxSource | Should -Match 'Repository root does not exist'
        $script:Source | Should -Match "'-MaxDurationSeconds', '180'"
        $script:Source | Should -Match 'Record-Reproduction\.ps1'
    }

    It 'uses the explicit repository root when run from a trusted copy' {
        $trustedScripts = Join-Path $TestDrive 'trusted/scripts'
        $trustedShared = Join-Path $trustedScripts 'shared'
        New-Item -ItemType Directory -Path $trustedShared -Force | Out-Null
        Copy-Item `
            -LiteralPath $script:BuildSandboxPath `
            -Destination (Join-Path $trustedScripts 'BuildAndRunSandbox.ps1')
        Copy-Item `
            -LiteralPath (Join-Path $PSScriptRoot 'shared/shared-utils.ps1') `
            -Destination (Join-Path $trustedShared 'shared-utils.ps1')
        @'
param(
    [string]$Platform,
    [string]$ProjectPath,
    [string]$TargetFramework,
    [string]$Configuration,
    [string]$DeviceUdid,
    [string]$BundleId,
    [switch]$Rebuild
)

$expected = [IO.Path]::GetFullPath(
    (Join-Path $env:EXPECTED_REPLICATION_REPO 'src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj'))
if ([IO.Path]::GetFullPath($ProjectPath) -cne $expected) {
    throw "Unexpected project path: $ProjectPath"
}
exit 0
'@ | Set-Content -LiteralPath (Join-Path $trustedShared 'Build-AndDeploy.ps1')

        $repo = Join-Path $TestDrive 'worktree'
        $project = Join-Path $repo 'src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj'
        $appiumScript = Join-Path $repo 'CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs'
        New-Item -ItemType Directory -Path (Split-Path -Parent $project) -Force |
            Out-Null
        New-Item -ItemType Directory -Path (Split-Path -Parent $appiumScript) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath $project
        'return;' | Set-Content -LiteralPath $appiumScript

        $previousRepo = $env:EXPECTED_REPLICATION_REPO
        try {
            $env:EXPECTED_REPLICATION_REPO = $repo
            $output = @(& pwsh -NoLogo -NoProfile -NonInteractive `
                -File (Join-Path $trustedScripts 'BuildAndRunSandbox.ps1') `
                -Platform catalyst `
                -RepoRoot $repo `
                -PrepareOnly 2>&1)
            $LASTEXITCODE | Should -Be 0 -Because ($output -join [Environment]::NewLine)
            $output -join [Environment]::NewLine |
                Should -Match 'Sandbox build and deployment preparation completed'
        }
        finally {
            $env:EXPECTED_REPLICATION_REPO = $previousRepo
        }
    }

    It 'requires new add-only guarded tests and literal expected failure verification' {
        $script:Source | Should -Match ([regex]::Escape("`$entry.Status -ne '??'"))
        $script:Source | Should -Match 'Assert-ReplicationTestGuard'
        $script:Source | Should -Match 'ExpectedFailureSignature'
        $script:Source | Should -Match 'verificationPassed'
    }

    It 'accepts the canonical platform-aware device guard before verification' {
        $repoRoot = $TestDrive
        $relativePath = 'src/Controls/tests/DeviceTests/Issues/Issue37440Tests.cs'
        $path = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force |
            Out-Null
        @'
using Xunit;

public class Issue37440Tests
{
    const string IssueNumber = "37440";

    static string? GetReplicationIssue()
    {
#if ANDROID
        return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
        return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
        return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
    }

    [Fact]
    public void ReproducesIssue()
    {
        if (!string.Equals(GetReplicationIssue(), IssueNumber, StringComparison.Ordinal))
        {
            return;
        }

        Assert.True(false, "Expected failure");
    }
}
'@ | Set-Content -LiteralPath $path

        {
            Assert-GeneratedTestContent `
                -Files @($relativePath) `
                -Issue 37440 `
                -TestType DeviceTest
        } | Should -Not -Throw
    }

    It 'rejects a noncanonical device guard before verification' {
        $repoRoot = $TestDrive
        $relativePath = 'src/Controls/tests/DeviceTests/Issues/Issue37440AlternativeTests.cs'
        $path = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force |
            Out-Null
        @'
using Xunit;

public class Issue37440AlternativeTests
{
    [Fact]
    public void ReproducesIssue()
    {
        if (!string.Equals(
            Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
            "37440",
            StringComparison.Ordinal))
        {
            return;
        }

        Assert.True(false, "Expected failure");
    }
}
'@ | Set-Content -LiteralPath $path

        {
            Assert-GeneratedTestContent `
                -Files @($relativePath) `
                -Issue 37440 `
                -TestType DeviceTest
        } | Should -Throw '*missing the exact issue-keyed*'
    }

    It 'rejects commented or late reproduction guards' {
        $commentedGuard = @'
[Fact]
public void ReproducesIssue()
{
    // if (!string.Equals(Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"), "37440", StringComparison.Ordinal)) { return; }
    Assert.True(false);
}
'@
        {
            Assert-ReplicationTestGuard `
                -Content $commentedGuard `
                -Path 'CommentedGuard.cs' `
                -IssueNumber 37440 `
                -TestType UnitTest
        } | Should -Throw '*missing the exact issue-keyed*'

        $lateGuard = @'
[Fact]
public void ReproducesIssue()
{
    Assert.True(false);
    if (!string.Equals(Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"), "37440", StringComparison.Ordinal))
    {
        return;
    }
}
'@
        {
            Assert-ReplicationTestGuard `
                -Content $lateGuard `
                -Path 'LateGuard.cs' `
                -IssueNumber 37440 `
                -TestType UnitTest
        } | Should -Throw '*missing the exact issue-keyed*'
    }

    It 'rejects test lifecycle code that can run before the guard' {
        $source = @'
public class Issue37440
{
    public Issue37440()
    {
        throw new Exception("Runs before the test body");
    }

    [Fact]
    public void ReproducesIssue()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"), "37440", StringComparison.Ordinal))
        {
            return;
        }
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $source `
                -Path 'Issue37440.cs'
        } | Should -Throw '*unguarded test-class constructor*'
    }
}

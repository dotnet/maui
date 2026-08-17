#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
    $script:Source = Get-Content -LiteralPath $scriptPath -Raw
    $script:BuildSandboxPath = Join-Path $PSScriptRoot 'BuildAndRunSandbox.ps1'
    $script:BuildSandboxSource = Get-Content `
        -LiteralPath $script:BuildSandboxPath `
        -Raw
    $script:TrustedAppiumSource = Get-Content `
        -LiteralPath (Join-Path $PSScriptRoot 'templates/RunReplicationAppiumPlan.cs') `
        -Raw
    $buildTokens = $null
    $buildErrors = $null
    $buildAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:BuildSandboxPath,
        [ref]$buildTokens,
        [ref]$buildErrors)
    if ($buildErrors) {
        throw ($buildErrors | ForEach-Object Message) -join [Environment]::NewLine
    }
    $resolveCatalystApp = $buildAst.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Resolve-CatalystSandboxAppPath'
    }, $true)
    Invoke-Expression $resolveCatalystApp.Extent.Text
    . (Join-Path $PSScriptRoot 'shared/Assert-ReplicationTestGuard.ps1')
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$errors)
    if ($errors) {
        throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
    }

    foreach ($name in @(
        'ConvertTo-ReplicationSafeLog',
        'Get-ReplicationFailureDetails',
        'Test-PathInsideRoot',
        'Assert-NoReparsePointInParentPath',
        'Assert-BoundedGeneratedFile',
        'Assert-GeneratedSandboxXaml',
        'Assert-GeneratedSandboxSources',
        'Assert-NoDuplicateJsonProperties',
        'Read-GeneratedAppiumPlan',
        'ConvertTo-BoundedAgentLine',
        'Read-SandboxProposal',
        'Assert-LighterTestRejections',
        'Get-ProposedTestFiles',
        'Assert-TestProposalMatchesPlan',
        'Read-TestProposal',
        'Get-VerifierTestType',
        'Get-ReplicationTargetTestDeclarations',
        'Resolve-ReplicationVerifierMetadata',
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
    It 'requires unconditional reproduction tests during generation and repair' {
        $script:Source |
            Should -Match 'must run normally and fail without an environment variable'
        $script:Source | Should -Match 'Do not reference MAUI_REPRODUCTION_ISSUE'
    }

    It 'requires a single-line expected failure signature during planning' {
        $script:Source |
            Should -Match 'expectedFailureSignature must be a trimmed single-line string'
        $script:Source |
            Should -Match 'not an Expected/Actual multi-line rendering'
    }

    It 'uses native Appium class-name locators instead of Selenium CSS locators' {
        $script:TrustedAppiumSource |
            Should -Match '"className"\s*=>\s*MobileBy\.ClassName\(locator\.Value\)'
        $script:TrustedAppiumSource |
            Should -Not -Match '"className"\s*=>\s*By\.ClassName\(locator\.Value\)'
    }

    It 'maps constrained Android text locators to native UiAutomator selectors' {
        $script:TrustedAppiumSource |
            Should -Match '"androidText"\s*=>\s*MobileBy\.AndroidUIAutomator'
        $script:TrustedAppiumSource |
            Should -Match 'UiSelector\(\)\.text'
        $script:Source |
            Should -Match "'androidText'"
        $script:Source |
            Should -Match 'androidText value is unsafe'
    }

    It 'captures Catalyst evidence frames through the trusted Appium session' {
        $script:TrustedAppiumSource |
            Should -Match 'MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY'
        $script:TrustedAppiumSource |
            Should -Match '\(\(ITakesScreenshot\)driver\)\.GetScreenshot\(\)'
        $script:TrustedAppiumSource |
            Should -Match 'frame-\{frameIndex:D4\}\.png'
    }

    It 'polls semantic text assertions until the expected state or timeout' {
        $script:TrustedAppiumSource |
            Should -Match 'static void AssertElementText[\s\S]*new WebDriverWait\(driver, timeout\)'
        $script:TrustedAppiumSource |
            Should -Match 'wait\.Until\(current =>'
        $script:TrustedAppiumSource |
            Should -Match 'catch \(WebDriverTimeoutException exception\)'
    }

    It 'uses the supported macOS unified-log debug flag' {
        $script:BuildSandboxSource | Should -Match 'log show --debug --predicate'
        $script:BuildSandboxSource | Should -Not -Match 'log show --level'
    }

    It 'removes terminal controls and Azure logging directives from untrusted output' {
        ConvertTo-ReplicationSafeLog `
            -Value "x`e[31;1m red`e[0m`n##vso[task.setvariable variable=Y]bad ##[error]fake" |
            Should -BeExactly 'x red bad fake'
    }

    It 'preserves primary failure lines when long stack traces displace them from the tail' {
        $output = @(
            'System.TimeoutException: Expected REPRODUCED but actual text was NOT REPRODUCED.'
            1..40 | ForEach-Object { "   at Example.Stack.Frame$_()" }
            'final cleanup line'
        )

        $details = Get-ReplicationFailureDetails -Output $output

        $details |
            Should -Match ([regex]::Escape(
                'System.TimeoutException: Expected REPRODUCED but actual text was NOT REPRODUCED.'))
        $details | Should -Match 'at Example\.Stack\.Frame40\(\)'
        $details | Should -Match 'final cleanup line'
        $details | Should -Not -Match 'at Example\.Stack\.Frame1\(\)'
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
      "action": "assertTextEquals",
      "description": "Verify the reported incorrect result",
      "locator": { "strategy": "accessibilityId", "value": "ResultLabel" },
      "value": "BUG REPRODUCED: Incorrect",
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
            Should -Throw '*must end with an exact semantic text assertion*'

        $invalid.steps[-1].action = 'assertNotExists'
        $invalid.steps[-1].locator = [pscustomobject]@{
            strategy = 'accessibilityId'
            value = 'MissingItem'
        }
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw "*uses unsupported action 'assertNotExists'*"
    }

    It 'accepts safe Android literal text locators and rejects other platforms or expressions' {
        $IssueNumber = 37440
        $Platform = 'android'
        $appiumPlanPath = Join-Path $TestDrive 'android-text-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Verify the native Android result",
      "locator": { "strategy": "androidText", "value": "BUG REPRODUCED: Native Android result" },
      "value": "BUG REPRODUCED: Native Android result",
      "timeoutSeconds": 30
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        $Platform = 'ios'
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*androidText outside Android*'

        $Platform = 'android'
        $invalid = Get-Content -LiteralPath $appiumPlanPath -Raw |
            ConvertFrom-Json -Depth 10
        $invalid.steps[0].locator.value = 'new UiSelector().text("BUG REPRODUCED")'
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*androidText value is unsafe*'
    }

    It 'requires exact semantic trigger equivalence in the Sandbox proposal' {
        $sandboxProposalPath = Join-Path $TestDrive 'sandbox-proposal.json'
        @'
{
  "reproductionSteps": ["Move the pointer outside the stationary SwipeView item."],
  "expectedBehavior": "The active gesture continues tracking.",
  "observedBehaviorCheck": "The semantic result reports tracking stopped.",
  "reportedTrigger": "A pointer leaves the bounds of a stationary SwipeView item during an active gesture.",
  "sandboxTrigger": "A pointer leaves the bounds of a stationary SwipeView item during an active gesture.",
  "scenarioDifferences": [],
  "files": [
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml",
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs",
    "CustomAgentLogsTmp/Sandbox/appium-plan.json"
  ]
}
'@ | Set-Content -LiteralPath $sandboxProposalPath

        { Read-SandboxProposal | Out-Null } | Should -Not -Throw

        $invalid = Get-Content -LiteralPath $sandboxProposalPath -Raw |
            ConvertFrom-Json -Depth 10
        $invalid.sandboxTrigger =
            'Move the SwipeView outside a stationary pointer during the active gesture.'
        $invalid.scenarioDifferences = @('The control moves instead of the pointer.')
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $sandboxProposalPath
        { Read-SandboxProposal | Out-Null } |
            Should -Throw '*scenarioDifferences must be empty*'
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
using ShellRenderer = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
namespace Maui.Controls.Sample;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        _ = typeof(ShellRenderer);
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath
        { Assert-GeneratedSandboxSources } | Should -Not -Throw

        @'
using System.Diagnostics.CodeAnalysis;
namespace Maui.Controls.Sample;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    [RequiresUnreferencedCode("Loads bounded inline XAML.")]
    void LoadInlineXaml()
    {
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

    It 'allows ordinary preference variables while rejecting the Preferences API' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var preferences = new UIWindowSceneGeometryPreferencesIOS();' `
                -Path 'Issue37264.iOS.cs'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'Preferences.Set("theme", "dark");' `
                -Path 'Issue37264.iOS.cs'
        } | Should -Throw '*device-external-access*'
    }

    It 'allows standard XAML schema URIs in LoadFromXaml strings only' {
        $source = @'
var xaml = """
<Label xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    <Label.FormattedText><FormattedString /></Label.FormattedText>
</Label>
""";
new Label().LoadFromXaml(xaml);
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $source `
                -Path 'MainPage.xaml.cs'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content ($source + "`nvar endpoint = `"https://example.invalid`";") `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'remote-url'*"
    }

    It 'accepts assembly-qualified XAML namespace declarations without allowing reflection' {
        $xaml = @'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:maps="clr-namespace:Microsoft.Maui.Controls.Maps;assembly=Microsoft.Maui.Controls.Maps"
             x:Class="Maui.Controls.Sample.MainPage">
    <maps:Map />
</ContentPage>
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $xaml `
                -Path 'MainPage.xaml'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var assembly = typeof(string).Assembly;' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'reflection'*"
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
    Should -Throw "*XAML namespace 'system' is not allowed*"
    }

    It 'uses a trusted Appium interpreter instead of agent-authored host code' {
        $script:Source | Should -Match 'appium-plan\.json'
        $script:Source | Should -Match 'RunReplicationAppiumPlan\.cs'
        $script:Source | Should -Match 'Copy-Item[\s\S]*trustedAppiumRunnerPath'
        $script:Source | Should -Not -Match 'Create "\$appiumScriptPath"'
        $script:BuildSandboxSource | Should -Match 'REPLICATION_PLATFORM'
        $script:BuildSandboxSource | Should -Match 'REPLICATION_WINDOWS_APP_PATH'
        $script:BuildSandboxSource | Should -Match 'shell pidof -s com\.microsoft\.maui\.sandbox'
        $script:TrustedAppiumSource |
            Should -Match '"appium:uiautomator2ServerInstallTimeout",\s*120_000'
        $script:TrustedAppiumSource | Should -Match 'case "restartApp"'
        $script:TrustedAppiumSource | Should -Match 'driver\.TerminateApp\(appId\)'
        $script:TrustedAppiumSource | Should -Match 'driver\.ActivateApp\(appId\)'
    }

    It 'uses typed Appium properties for reserved Windows capabilities' {
        $script:TrustedAppiumSource |
            Should -Match '#:property WindowsAppSdkBootstrapInitialize=false'
        $script:TrustedAppiumSource |
            Should -Match '#:property WindowsAppSdkDeploymentManagerInitialize=false'
        $script:TrustedAppiumSource |
            Should -Match 'options\.DeviceName\s*=\s*"WindowsPC"'
        $script:BuildSandboxSource |
            Should -Match 'dotnet run --file \$AppiumTestScript'
        $script:BuildSandboxSource |
            Should -Not -Match 'dotnet run RunWithAppiumTest\.cs'
        $script:TrustedAppiumSource |
            Should -Match 'Process\.Start\(new ProcessStartInfo\(appPath\)'
        $script:TrustedAppiumSource |
            Should -Match 'MainWindowHandle\s*!=\s*IntPtr\.Zero'
        $script:TrustedAppiumSource |
            Should -Match '"appTopLevelWindow"'
        $script:TrustedAppiumSource |
            Should -Match 'launchedWindowsApp\.Kill\(entireProcessTree:\s*true\)'
        $script:TrustedAppiumSource |
            Should -Not -Match 'AddAdditionalAppiumOption\("appium:(?:deviceName|app)"'
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
        $script:Source | Should -Match "'GH_TOKEN'"
        $script:Source | Should -Not -Match 'GH_REPLICATION_TOKEN'
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
        $script:Source | Should -Match '''-DeviceUdid'', \$selectedDeviceId'
    }

    It 'preserves current attempt counts in blocked candidate manifests' {
        $script:Source | Should -Match 'sandbox = \$sandboxAttempts'
        $script:Source | Should -Match 'automatedTest = \$testAttempts'
        $script:Source | Should -Not -Match 'attempts = \[ordered\]@\{ sandbox = 0; automatedTest = 0 \}'
    }

    It 'prepares the app before starting a bounded recording-only run' {
        $script:Source | Should -Match "'-PrepareOnly'"
        $script:Source | Should -Match "'-LaunchOnly'"
        $script:Source | Should -Match "'-SkipBuildDeploy'"
        $script:Source | Should -Match 'Launching the Sandbox before evidence recording'
        $script:Source | Should -Match ([regex]::Escape("'-RepoRoot', `$repoRoot"))
        $script:BuildSandboxSource | Should -Match '\[string\]\$RepoRoot'
        $script:BuildSandboxSource | Should -Match 'Repository root does not exist'
        $script:BuildSandboxSource | Should -Match 'Prepared Sandbox launch settled before evidence recording'
        $script:TrustedAppiumSource | Should -Match '"appium:forceAppLaunch", false'
        $script:TrustedAppiumSource | Should -Match '"appium:dontStopAppOnReset", true'
        $script:TrustedAppiumSource | Should -Match 'MAUI_REPLICATION_RECORDING_START_MARKER'
        $script:TrustedAppiumSource | Should -Match 'WriteRecordingStartMarker\(\)'
        $script:TrustedAppiumSource | Should -Match 'REPLICATION_NOT_REPRODUCED'
        $script:Source | Should -Match "'sandbox_inconclusive'"
        $script:Source | Should -Match "'sandbox_not_reproduced'"
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

    It 'resolves the single built Mac Catalyst app without assuming its bundle name' {
        $repo = Join-Path $TestDrive 'catalyst-repo'
        $output = Join-Path $repo 'artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-maccatalyst/maccatalyst-arm64'
        $app = Join-Path $output 'Maui.Controls.Sample.Sandbox.app'
        New-Item -ItemType Directory -Path $app -Force | Out-Null

        Resolve-CatalystSandboxAppPath `
            -RepositoryRoot $repo `
            -BuildConfiguration Debug `
            -Framework net10.0-maccatalyst `
            -RuntimeIdentifier maccatalyst-arm64 |
            Should -BeExactly $app

        New-Item -ItemType Directory -Path (Join-Path $output 'Unexpected.app') |
            Out-Null
        {
            Resolve-CatalystSandboxAppPath `
                -RepositoryRoot $repo `
                -BuildConfiguration Debug `
                -Framework net10.0-maccatalyst `
                -RuntimeIdentifier maccatalyst-arm64
        } | Should -Throw '*Expected exactly one*'
    }

    It 'restores every tracked file to the pinned baseline between Sandbox attempts' {
        $script:Source |
            Should -Match 'git restore --source \$BaseSha --staged --worktree -- \.'
        $script:Source |
            Should -Not -Match 'git restore --worktree -- \$sandboxXamlPath \$sandboxCodePath'
    }

    It 'restores tracked verifier build side effects while preserving generated tests' {
        $script:Source |
            Should -Match 'function Restore-TrackedVerificationSideEffects'
        $script:Source |
            Should -Match '\$preserved\.Contains\(\$entry\.Path\)'
        $script:Source |
            Should -Match 'git restore --source \$BaseSha --staged --worktree -- @restorePaths'
        $script:Source |
            Should -Match 'Restore-TrackedVerificationSideEffects -PreservedFiles \$generatedFiles'
    }

    It 'preserves bounded device verification diagnostics before cleanup' {
        $script:Source | Should -Match 'function Copy-VerificationDiagnostics'
        $script:Source |
            Should -Match '\$ArtifactRoot "verification-diagnostics/attempt-\$Attempt"'
        $script:Source |
            Should -Not -Match '\$verificationDir "diagnostics/attempt-\$Attempt"'
        $script:Source | Should -Match '\$files\.Count -gt 64'
        $script:Source | Should -Match '\$totalBytes -gt 8MB'
        $script:Source |
            Should -Match 'finally\s*\{\s*Copy-VerificationDiagnostics -Attempt \$attempt\s*Restore-TrackedVerificationSideEffects'
    }

    It 'allows source-safety, host, compile, and empirical repairs within the bounded Sandbox loop' {
        $script:Source |
            Should -Match '\[int\]\$MaxSandboxAttempts\s*=\s*5'
        $script:Source |
            Should -Match 'throw "\$Description failed with exit code \$exitCode\.`n\$failureDetails"'
        $script:Source |
            Should -Match 'Get-ReplicationFailureDetails -Output \$output'
        $script:Source |
            Should -Match 'Do not add maps or other assembly-qualified XAML namespaces'
        $script:Source |
            Should -Match 'Fully qualify ambiguous framework type names'
        $script:Source |
            Should -Match ([regex]::Escape(
                "XAML namespace '`$prefix' is not allowed or has the wrong value"))
        $script:Source |
            Should -Match 'Use Console\.WriteLine rather than importing System\.Diagnostics'
        $script:Source |
            Should -Match 'Sandbox source must not use Task\.Delay'
        $script:Source |
            Should -Match 'Every XAML element referenced from code-behind must have x:Name'
        $script:Source |
            Should -Match 'Every string must be non-empty and already trimmed'
        $script:Source |
            Should -Match 'back, restartApp, swipe, and setOrientation require `"locator": null`'
        $script:Source |
            Should -Match 'Do not use assertNotExists or any intermediate assertion'
        $script:Source |
            Should -Match 'rather than moving the control when the report moves the pointer'
        $script:Source |
            Should -Match 'prior tracked Sandbox files were restored to baseline'
        $script:Source |
            Should -Match 'Do not use Task\.Delay, Thread\.Sleep, timers, Task\.Run'
        $script:Source |
            Should -Match 'Never repeat a fully qualified type after CS0234 or CS0246'
        $script:Source |
            Should -Match 'do not guess namespaces'
        $script:Source |
            Should -Match 'evidence that starts with the failure already latched is invalid'
        $script:Source |
            Should -Match 'event-driven completion such as a TaskCompletionSource'
    }

    It 'requires new add-only unconditional tests and literal expected failure verification' {
        $script:Source | Should -Match ([regex]::Escape("`$entry.Status -ne '??'"))
        $script:Source | Should -Match 'Assert-ReplicationGeneratedSourceSafety'
        $script:Source | Should -Match 'ExpectedFailureSignature'
        $script:Source | Should -Match 'verificationPassed'
    }

    It 'requires exact semantic trigger equivalence before test authoring' {
        $script:Source | Should -Match "'reportedTrigger'"
        $script:Source | Should -Match "'testTrigger'"
        $script:Source | Should -Match "'scenarioDifferences'"
        $script:Source | Should -Match 'scenarioDifferences must be empty'
        $script:Source | Should -Match 'replacing platform-default styling with an explicit Style'
        $script:Source | Should -Match 'replacing a gesture with a programmatic API'
        $script:Source | Should -Match 'adding a layout ancestor absent from the issue'
        $script:Source | Should -Match 'replacing the reported public source/service with a custom test type or service'
        $script:Source | Should -Match 'prove the same virtual or native view instance was reused'
        $script:Source | Should -Match 'FIFO completion order are not proof'
        $script:Source | Should -Match 'every environmental prerequisite such as locale/culture'
        $script:Source | Should -Match 'hard-coding locale-specific output without arranging and verifying'
        $script:Source | Should -Match 'derive the expected value from the active environment'
        $script:Source | Should -Match 'Do not repair an environment-sensitive test by hard-coding'
        $script:Source | Should -Match 'pure new-API/feature request is not an empirically reproducible baseline defect'
        $script:Source | Should -Match 'sentinel outside the passing domain'
        $script:Source | Should -Match 'replacing a real orientation change with WidthRequest or Arrange'
        $script:Source | Should -Match 'substitutes Arrange for a real device orientation change'
    }

    It 'rejects synthetic Arrange as a device-orientation trigger' {
        $repoRoot = $TestDrive
        $IssueNumber = 31059
        $approvedTestRoots = @('src/Controls/tests/DeviceTests/')
        $relativePath = 'src/Controls/tests/DeviceTests/Issue31059.iOS.cs'
        $fullPath = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fullPath) -Force |
            Out-Null
        @'
public class Issue31059
{
    public void ReproducesIssue()
    {
        view.Arrange(new Rect(0, 0, 844, 220));
    }
}
'@ | Set-Content -LiteralPath $fullPath

        $testProposalPath = Join-Path $TestDrive 'test-proposal.json'
        [ordered]@{
            testType = 'device'
            testFilter = 'Issue31059'
            expectedFailureSignature = 'Item should remain centered after rotation.'
            files = @($relativePath)
            reproductionSteps = @('Rotate the device from portrait to landscape.')
            expectedBehavior = 'The last item remains centered.'
            observedBehavior = 'The previous item becomes centered.'
            reportedTrigger = 'Rotate the iOS device from portrait to landscape.'
            testTrigger = 'Change width with Arrange to imitate landscape.'
            scenarioDifferences = @()
            lighterTypesRejected = [ordered]@{
                unit = 'Requires the native iOS CollectionView.'
                xaml = 'Requires runtime scrolling and orientation.'
            }
        } | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $testProposalPath

        { Read-TestProposal -ActualFiles @($relativePath) | Out-Null } |
            Should -Throw '*substitutes Arrange for a real device orientation change*'
    }

    It 'repairs generated tests that fail trusted source validation before verification' {
        $script:Source |
            Should -Match 'try\s*\{\s*\$generatedFiles = @\(Get-GeneratedTestFiles\)'
        $script:Source |
            Should -Match '\$repairFailureSummary = ConvertTo-ReplicationSafeLog \$_.Exception.Message 4000'
        $script:Source |
            Should -Match 'if \(\$intentToAddApplied\)\s*\{\s*& git reset'
    }

    It 'allows source repairs, a compile repair, and final empirical verification' {
        $script:Source | Should -Match '\[int\]\$MaxTestAttempts = 5'
        $script:Source |
            Should -Match 'Fix all compiler diagnostics shown by the trusted verifier'
        $script:Source |
            Should -Match 'read existing tests in the same project and platform'
    }

    It 'requires exact reasons for every rejected lighter test type' {
        {
            Assert-LighterTestRejections `
                -Value ([pscustomobject]@{}) `
                -SelectedType unit
        } | Should -Not -Throw

        $deviceReasons = [pscustomobject]@{
            unit = 'Requires the native control.'
            xaml = 'Requires a runtime property update.'
        }
        {
            Assert-LighterTestRejections -Value $deviceReasons -SelectedType device
        } | Should -Not -Throw

        {
            Assert-LighterTestRejections `
                -Value ([pscustomobject]@{ unit = 'Only unit was considered.' }) `
                -SelectedType device
        } | Should -Throw '*exactly the rejected lighter test types*'

        {
            Assert-LighterTestRejections `
                -Value ([pscustomobject]@{
                    unit = 'Requires native state.'
                    xaml = [pscustomobject]@{ reason = 'Not a string.' }
                }) `
                -SelectedType device
        } | Should -Throw "*reason for 'xaml' must be a string*"
    }

    It 'accepts an unconditional device reproduction test before verification' {
        $repoRoot = $TestDrive
        $relativePath = 'src/Controls/tests/DeviceTests/Issues/Issue37440Tests.cs'
        $path = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force |
            Out-Null
        @'
using Xunit;

public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue()
    {
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

    It 'rejects an opt-in device reproduction guard before verification' {
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
        } | Should -Throw '*environment-secrets*'
    }

    It 'rejects reproduction guards even when the assertion is unconditional' {
        $guardedSource = @'
[Fact]
public void ReproducesIssue()
{
    Assert.True(false);
    _ = Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $guardedSource `
                -Path 'Guarded.cs'
        } | Should -Throw '*environment-secrets*'
    }

    It 'rejects framework behavior switches that manufacture the failure' {
        $source = @'
[Fact]
public void ReproducesIssue()
{
    VisualElement.SkipMeasureInvalidatedPropagation = true;
    Assert.True(false);
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $source `
                -Path 'Issue20722.cs'
        } | Should -Throw "*prohibited 'framework-behavior-switch'*"
    }

    It 'rejects unsafe MacCatalyst filenames and mismatched platform APIs' {
        {
            Assert-ReplicationPlatformSourceSafety `
                -Content 'using UIKit;' `
                -Path 'src/Controls/tests/DeviceTests/Issue35516.MacCatalyst.cs' `
                -Platform 'catalyst'
        } | Should -Throw '*unsafe MacCatalyst filename*'

        {
            Assert-ReplicationPlatformSourceSafety `
                -Content 'using UIKit;' `
                -Path 'src/Controls/tests/DeviceTests/Issue35516.cs' `
                -Platform 'catalyst'
        } | Should -Throw '*without a matching platform-specific path*'

        {
            Assert-ReplicationPlatformSourceSafety `
                -Content 'using UIKit;' `
                -Path 'src/Controls/tests/DeviceTests/Issue35516.iOS.cs' `
                -Platform 'catalyst'
        } | Should -Not -Throw
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
        Assert.True(false);
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $source `
                -Path 'Issue37440.cs'
        } | Should -Throw '*unguarded test-class constructor*'
    }

    It 'allows only the canonical empty UI-test device constructor' {
        $canonicalConstructor = @'
public class Issue36826 : _IssuesUITest
{
    public Issue36826 /* NUnit provides the selected test device. */ (TestDevice device) : base(device)
    {
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $canonicalConstructor `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36826.cs'
        } | Should -Not -Throw

        $descriptiveParameter = @'
public class Issue36800 : _IssuesUITest
{
    public Issue36800(TestDevice testDevice) : base(testDevice)
    {
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $descriptiveParameter `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36800.cs'
        } | Should -Not -Throw

        $constructorWithCode = @'
public class Issue36826 : _IssuesUITest
{
    public Issue36826(TestDevice device) : base(device)
    {
        App.Tap("unsafe");
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $constructorWithCode `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36826.cs'
        } | Should -Throw '*unguarded test-class constructor*'
    }

    It 'allows expression-bodied helper properties but rejects field initializers' {
        $helperProperties = @'
sealed class BindingSource
{
    public string Property1 => "First value";
    public string Property2 => "Second value";
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $helperProperties `
                -Path 'Issue10792.cs'
        } | Should -Not -Throw

        $fieldInitializer = @'
public class Issue10792
{
    private string value = "runs before the guard";
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $fieldInitializer `
                -Path 'Issue10792.cs'
        } | Should -Throw '*unguarded test lifecycle hook*'
    }

    It 'starts Appium from a resolved executable with explicit inherited environment' {
        $script:BuildSandboxSource |
            Should -Match '\(Get-Command appium -ErrorAction Stop\)\.Source'
        $script:BuildSandboxSource |
            Should -Match '\$env:PATH = \$pathValue'
        $script:BuildSandboxSource |
            Should -Match '\$env:APPIUM_HOME = \$homeValue'
        ([regex]::Matches(
            $script:BuildSandboxSource,
            'Invoke-WebRequest -Uri "http://127\.0\.0\.1:\$AppiumPort/status" -NoProxy'
        ).Count) | Should -Be 2
        $script:BuildSandboxSource |
            Should -Match 'Appium startup log:'
    }

    It 'rejects pre-execution code in a generated helper file without a test attribute' {
        $repoRoot = $TestDrive
        $testFile = 'src/Controls/tests/Core.UnitTests/Issues/Issue37440Tests.cs'
        $helperFile = 'src/Controls/tests/Core.UnitTests/Issues/Issue37440Bootstrap.cs'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $testFile)) -Force |
            Out-Null
        @'
using Xunit;

public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue()
    {
        Assert.True(false, "Issue37440");
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $testFile)
        @'
using System.Runtime.CompilerServices;

public static class Issue37440Bootstrap
{
    [ModuleInitializer]
    public static void Initialize() => throw new Exception("Runs before the guarded test");
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $helperFile)

        {
            Assert-GeneratedTestContent `
                -Files @($testFile, $helperFile) `
                -Issue 37440 `
                -TestType UnitTest
        } | Should -Throw '*unguarded test lifecycle hook*'
    }

    It 'allows a UI HostApp companion constructor while guarding the UI test assembly' {
        $repoRoot = $TestDrive
        $testFile = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issue37440Tests.cs'
        $hostFile = 'src/Controls/tests/TestCases.HostApp/Issues/Issue37440Page.xaml.cs'
        foreach ($file in @($testFile, $hostFile)) {
            New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
                Out-Null
        }
        @'
using NUnit.Framework;

public class Issue37440Tests
{
    [Test]
    public void ReproducesIssue()
    {
        Assert.Fail("Issue37440");
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $testFile)
        @'
public partial class Issue37440Page : ContentPage
{
    public Issue37440Page()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $hostFile)

        {
            Assert-GeneratedTestContent `
                -Files @($testFile, $hostFile) `
                -Issue 37440 `
                -TestType UITest
        } | Should -Not -Throw
    }
}

Describe 'Replication verifier metadata resolution' {
    BeforeEach {
        $repoRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $repoRoot -Force | Out-Null
        $script:DetectorPath = Join-Path $PSScriptRoot 'shared/Detect-TestsInDiff.ps1'
    }

    It 'resolves the exact Controls unit-test project, class, and method' {
        $file = 'src/Controls/tests/Core.UnitTests/Issues/Issue37440Tests.cs'
        $project = 'src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath (Join-Path $repoRoot $project)
        @'
namespace Microsoft.Maui.Controls.Tests;

public class Issue37440Tests
{
    private class Recorder
    {
    }

    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)

        $metadata = Resolve-ReplicationVerifierMetadata `
            -Files @($file) `
            -TestType UnitTest `
            -TestFilter Issue37440 `
            -Platform android `
            -DetectorPath $script:DetectorPath

        $metadata.Project | Should -BeExactly 'Controls.Core.UnitTests'
        $metadata.ProjectPath | Should -BeExactly $project
        $metadata.ClassName |
            Should -BeExactly 'Microsoft.Maui.Controls.Tests.Issue37440Tests'
        $metadata.MethodName | Should -BeExactly 'ReproducesIssue37440'
    }

    It 'resolves a non-Controls unit-test project instead of defaulting to Controls' {
        $file = 'src/Core/tests/UnitTests/Issue37440Tests.cs'
        $project = 'src/Core/tests/UnitTests/Core.UnitTests.csproj'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath (Join-Path $repoRoot $project)
        @'
namespace Microsoft.Maui.UnitTests;

public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)

        $metadata = Resolve-ReplicationVerifierMetadata `
            -Files @($file) `
            -TestType UnitTest `
            -TestFilter Issue37440 `
            -Platform android `
            -DetectorPath $script:DetectorPath

        $metadata.Project | Should -BeExactly 'Core.UnitTests'
        $metadata.ProjectPath | Should -BeExactly $project
    }

    It 'resolves a non-Controls device project with exact class isolation' {
        $file = 'src/Essentials/test/DeviceTests/Tests/Issue37440Tests.cs'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
            Out-Null
        @'
namespace Microsoft.Maui.Essentials.DeviceTests;

[Category(TestCategory.Essentials)]
public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)

        $metadata = Resolve-ReplicationVerifierMetadata `
            -Files @($file) `
            -TestType DeviceTest `
            -TestFilter 'Category=Essentials' `
            -Platform android `
            -DetectorPath $script:DetectorPath

        $metadata.Project | Should -BeExactly 'Essentials'
        $metadata.ClassName |
            Should -BeExactly 'Microsoft.Maui.Essentials.DeviceTests.Issue37440Tests'
        $metadata.MethodName | Should -BeExactly 'ReproducesIssue37440'
    }

    It 'rejects ambiguous planned files instead of broadening the verifier run' {
        $files = @(
            'src/Core/tests/UnitTests/Issue37440FirstTests.cs',
            'src/Core/tests/UnitTests/Issue37440SecondTests.cs'
        )
        $project = 'src/Core/tests/UnitTests/Core.UnitTests.csproj'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $files[0])) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath (Join-Path $repoRoot $project)
        foreach ($file in $files) {
            $className = [IO.Path]::GetFileNameWithoutExtension($file)
            @"
public class $className
{
    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
"@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)
        }

        {
            Resolve-ReplicationVerifierMetadata `
                -Files $files `
                -TestType UnitTest `
                -TestFilter Issue37440 `
                -Platform android `
                -DetectorPath $script:DetectorPath
        } | Should -Throw '*exactly one targeted test method*'
    }
}

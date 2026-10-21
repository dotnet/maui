using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17389 : _IssuesUITest
{
	string? tapLogPath;
	int? targetPid;

	protected override bool ResetAfterEachTest => true;

	public Issue17389(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "InputTransparent should not affect background color on Windows layouts";

	public override IConfig GetTestConfig()
	{
		var config = base.GetTestConfig();
		if (_testDevice == TestDevice.Mac)
			ConfigureTapLog(config);
		return config;
	}

	public override void LaunchAppWithTest()
	{
		if (App is AppiumCatalystApp catalystApp)
			ConfigureTapLog(catalystApp.Config);
		base.LaunchAppWithTest();
	}

	void ConfigureTapLog(IConfig config)
	{
		tapLogPath ??= Path.Combine(
			Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY") ?? TestContext.CurrentContext.WorkDirectory,
			$"Issue17389-{Guid.NewGuid():N}.log");
		config.SetTestConfigurationArg("ISSUE17389_TAP_LOG", tapLogPath);
	}

	[TearDown]
	public void AttachTapDiagnostics()
	{
		if (tapLogPath is not null)
		{
			Assert.That(File.Exists(tapLogPath), Is.True, "App-side receiver log is missing.");
			TestContext.WriteLine(File.ReadAllText(tapLogPath));
			TestContext.AddTestAttachment(tapLogPath, "Issue17389 native ended touches and gesture callbacks");
			var completedPid = targetPid ?? throw new InvalidOperationException("Target process identity was not established.");
			var logDirectory = Path.GetDirectoryName(tapLogPath) ?? throw new InvalidOperationException("Tap log has no directory.");
			var pidPath = Path.Combine(logDirectory, "Issue17389-completed-target-pids.txt");
			var pidText = completedPid.ToString(System.Globalization.CultureInfo.InvariantCulture);
			var priorPids = File.Exists(pidPath) ? File.ReadAllLines(pidPath) : Array.Empty<string>();
			Assert.That(priorPids, Does.Not.Contain(pidText), "Target process was reused across cold cases.");
			File.AppendAllText(pidPath, pidText + Environment.NewLine);
		}
	}

	[Test, Order(1)]
	[Category(UITestCategories.Layout)]
	public void ValidateBackgroundColorDoesNotAffectInputTransparent()
	{
		App.WaitForElement("ToggleInputTransparentButton");

		if (App is AppiumCatalystApp)
		{
			var processes = System.Diagnostics.Process.GetProcessesByName("Controls.TestCases.HostApp");
			var processIds = processes.Select(process => process.Id).ToArray();
			foreach (var process in processes)
				process.Dispose();
			Assert.That(processIds, Has.Length.EqualTo(1), "Cannot establish target process identity.");
			targetPid = processIds[0];
			TestContext.WriteLine($"TargetAppPid: {processIds[0]}");
		}

		string[] layouts = new[]
		{
			 "RedGrid",
			 "GreenGrid",
			 "BlueBorder",
			 "PurpleContent"
		};

		foreach (var layout in layouts)
		{
			App.Tap(layout);
			TestContext.WriteLine($"{DateTime.UtcNow:O} Enabled tap {layout}: {App.FindElement("TapCountLabel").GetText()}");
		}

		App.WaitForElement("Tap count: 4");
		App.Click("ToggleInputTransparentButton");

		foreach (var layout in layouts)
		{
			App.Tap(layout);
			TestContext.WriteLine($"{DateTime.UtcNow:O} Transparent tap {layout}: {App.FindElement("TapCountLabel").GetText()}");
		}

		App.WaitForElement("Tap count: 0");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Layout)]
	public void ValidateInputTransparentBackgroundColorToggle()
	{
		Exception? exception = null;

		App.WaitForElement("ToggleInputTransparentButton");
		App.Tap("ToggleInputTransparentButton");
		Assert.That(App.WaitForTextToBePresentInElement("RedGrid", "InputTransparent=True"), Is.True);
		Assert.That(App.WaitForTextToBePresentInElement("TapCountLabel", "Tap count: 0"), Is.True);

		App.WaitForElement("ToggleBackgroundColorsButton");
		VerifyScreenshotOrSetException(ref exception, "BeforeToggleBackgroundColors");
		App.WaitForElement("ToggleBackgroundColorsButton");
		App.Tap("ToggleBackgroundColorsButton");
		VerifyScreenshotOrSetException(ref exception, "AfterToggleBackgroundColors");

		if (exception != null)
		{
			throw exception;
		}
	}
}
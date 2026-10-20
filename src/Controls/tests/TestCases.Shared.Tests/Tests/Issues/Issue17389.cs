using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17389 : _IssuesUITest
{
	string? tapLogPath;

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
			Assert.That(File.Exists(tapLogPath), Is.True, $"App-side tap log was not created: {tapLogPath}");
			TestContext.WriteLine(File.ReadAllText(tapLogPath));
			TestContext.AddTestAttachment(tapLogPath, "Issue17389 app-side tap receivers and counter transitions");
		}
	}

	[TestCase("baseline")]
	[TestCase("macos-center")]
	[TestCase("activated")]
	[TestCase("primer")]
	[Order(1)]
	[Category(UITestCategories.Layout)]
	public void ValidateBackgroundColorDoesNotAffectInputTransparent(string control)
	{
		App.WaitForElement("ToggleInputTransparentButton");

		if (App is AppiumCatalystApp catalystApp)
		{
			var processes = System.Diagnostics.Process.GetProcessesByName("Controls.TestCases.HostApp");
			var processIds = processes.Select(process => process.Id).ToArray();
			foreach (var process in processes)
				process.Dispose();
			TestContext.WriteLine($"TargetAppPids: {string.Join(",", processIds)}");
			Assert.That(processIds, Has.Length.EqualTo(1), "Cannot establish the target app process identity.");
			TestContext.WriteLine($"TargetAppPid: {processIds[0]}");

			if (control == "activated")
			{
				catalystApp.Driver.ExecuteScript("macos: activateApp", new Dictionary<string, object>
				{
					{ "bundleId", "com.microsoft.maui.uitests" }
				});
			}
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
			TapLayout(layout, control);
			TestContext.WriteLine($"{DateTime.UtcNow:O} Enabled tap {layout}: {App.FindElement("TapCountLabel").GetText()}");
		}

		App.WaitForElement("Tap count: 4");
		App.Click("ToggleInputTransparentButton");

		foreach (var layout in layouts)
		{
			TapLayout(layout, control);
			TestContext.WriteLine($"{DateTime.UtcNow:O} Transparent tap {layout}: {App.FindElement("TapCountLabel").GetText()}");
		}

		App.WaitForElement("Tap count: 0");
	}

	void TapLayout(string layout, string control)
	{
		if ((control == "macos-center" || control == "primer") && App is AppiumCatalystApp)
		{
			var rect = App.WaitForElementAndGetRect(layout);
			if (control == "primer")
			{
				App.TapCoordinates(rect.CenterX(), rect.CenterY());
				return;
			}

			var response = App.CommandExecutor.Execute("tapCoordinates", new Dictionary<string, object>
			{
				{ "x", rect.CenterX() },
				{ "y", rect.CenterY() }
			});
			Assert.That(response.Result, Is.EqualTo(CommandResponseResult.Success));
		}
		else
		{
			App.Tap(layout);
		}
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
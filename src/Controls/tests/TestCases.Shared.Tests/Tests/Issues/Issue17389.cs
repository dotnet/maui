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

	public override void LaunchAppWithTest()
	{
		if (App is AppiumCatalystApp catalystApp)
		{
			tapLogPath = Path.Combine(
				Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY") ?? TestContext.CurrentContext.WorkDirectory,
				$"Issue17389-{Guid.NewGuid():N}.log");
			catalystApp.Config.SetTestConfigurationArg("ISSUE17389_TAP_LOG", tapLogPath);
		}

		base.LaunchAppWithTest();
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
	[TestCase("green-first")]
	[TestCase("activate")]
	[TestCase("physical-center")]
	[TestCase("physical-lower")]
	[Order(1)]
	[Category(UITestCategories.Layout)]
	public void ValidateBackgroundColorDoesNotAffectInputTransparent(string control)
	{
		App.WaitForElement("ToggleInputTransparentButton");

		string[] layouts = new[]
		{
			 "RedGrid",
			 "GreenGrid",
			 "BlueBorder",
			 "PurpleContent"
		};

		if (control == "green-first")
			(layouts[0], layouts[1]) = (layouts[1], layouts[0]);

		if (control == "activate")
		{
			Assert.That(App, Is.InstanceOf<AppiumCatalystApp>());
			((AppiumCatalystApp)App).Driver.ExecuteScript("macos: activateApp", new Dictionary<string, object>
			{
				{ "bundleId", "com.microsoft.maui.uitests" }
			});
		}

		foreach (var layout in layouts)
		{
			if (layout == "RedGrid" && (control == "physical-center" || control == "physical-lower"))
			{
				var rect = App.FindElement(layout).GetRect();
				var x = rect.X + rect.Width / 2;
				var y = rect.Y + rect.Height * (control == "physical-center" ? 0.5 : 0.75);
				TestContext.WriteLine($"Physical tap {layout}: ({x}, {y}); rect={rect}");
				App.TapCoordinates((float)x, (float)y);
			}
			else
			{
				App.Tap(layout);
			}
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
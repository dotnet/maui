using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26598 : _IssuesUITest
{
	public override string Issue => "Tabbar disappears when navigating back from page with hidden TabBar in iOS";

	public Issue26598(TestDevice device) : base(device)
	{
	}

#if MACCATALYST
	readonly string _nativeLogPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"Issue26598-{Guid.NewGuid():N}.log");
	Dictionary<string, string>? _loggingArguments;
	string? _previousLogPath;
	bool _hadPreviousLogPath;

	public override IConfig GetTestConfig()
	{
		var config = base.GetTestConfig();
		if (UITestContext is null)
			ConfigureNativeLogging(config);
		return config;
	}

	public override void LaunchAppWithTest()
	{
		ConfigureNativeLogging(((AppiumApp)App).Config);
		base.LaunchAppWithTest();
	}

	void ConfigureNativeLogging(IConfig config)
	{
		if (_loggingArguments is not null)
			return;

		var arguments = config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs") ?? new Dictionary<string, string>();
		_hadPreviousLogPath = arguments.TryGetValue("MAUI_LOG_FILE", out _previousLogPath);
		arguments["MAUI_LOG_FILE"] = _nativeLogPath;
		config.SetProperty("TestConfigurationArgs", arguments);
		_loggingArguments = arguments;
	}

	public override void TestTearDown()
	{
		try
		{
			if (File.Exists(_nativeLogPath))
			{
				var attachment = Path.ChangeExtension(_nativeLogPath, "captured.log");
				File.Copy(_nativeLogPath, attachment, overwrite: true);
				TestContext.WriteLine(File.ReadAllText(attachment));
				TestContext.AddTestAttachment(attachment, "Issue26598 native tab bar state");
			}
			else
			{
				TestContext.Error.WriteLine($"Issue26598 native diagnostics were not produced: {_nativeLogPath}");
			}
		}
		finally
		{
			base.TestTearDown();
		}
	}

	protected override void FixtureOneTimeTearDown()
	{
		try
		{
			base.FixtureOneTimeTearDown();
		}
		finally
		{
			if (_loggingArguments is not null)
			{
				if (_hadPreviousLogPath)
					_loggingArguments["MAUI_LOG_FILE"] = _previousLogPath!;
				else
					_loggingArguments.Remove("MAUI_LOG_FILE");
				_loggingArguments = null;
			}
		}
	}
#endif

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldbeVisibleNavigatingBackFromNonTabbedPage()
	{
		// Is a iOS issue; see https://github.com/dotnet/maui/issues/26598
		// Initially TabBar for Issue26598Home is hidden 
		App.WaitForElement("NavigateToInnerTab");
		App.Click("NavigateToInnerTab");

		// Case 1 - After navigating to Inner Page ,  the TabBar should be visible
		ShellFeatureTestActions.WaitForBottomTab(App, "RecentTab");

		// Case 2 - Navigate to the InnerTabPage where the TabBar is hidden
		App.WaitForElement("NavigateToTabBarPage");
		App.Click("NavigateToTabBarPage");
		App.WaitForElement("Issue26589NonTab");
		ShellFeatureTestActions.WaitForNoBottomTab(App, "RecentTab");

		// Case 3 - Navigate back to the HomeTab, the TabBar should be visible
		ShellFeatureTestActions.TapPageBack(App, "InnerTab");
		App.WaitForElement("NavigateToTabBarPage");
		ShellFeatureTestActions.WaitForBottomTab(App, "RecentTab").Tap();
		App.WaitForElement("RecentTabContent");
		App.WaitForNoElement("NavigateToTabBarPage");
		ShellFeatureTestActions.WaitForBottomTab(App, "HomeTab").Tap();
		App.WaitForElement("NavigateToTabBarPage");
	}
}

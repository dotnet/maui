using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26598 : _IssuesUITest
{
	public override string Issue => "Tabbar disappears when navigating back from page with hidden TabBar in iOS";

#if ANDROID
	const string back = "";
#else
	const string back = "InnerTab";
#endif

	public Issue26598(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldbeVisibleNavigatingBackFromNonTabbedPage()
	{
		// Is a iOS issue; see https://github.com/dotnet/maui/issues/26598
		// Initially TabBar for Issue26598Home is hidden 
		App.WaitForElement("NavigateToInnerTab");
		App.Click("NavigateToInnerTab");

		// Case 1 - After navigating to Inner Page ,  the TabBar should be visible
		App.WaitForElement("RecentTab");

		// Case 2 - Navigate to the InnerTabPage where the TabBar is hidden
		App.WaitForElement("NavigateToTabBarPage");
		App.Click("NavigateToTabBarPage");
		App.WaitForElement("Issue26589NonTab");

		// Case 3 - Navigate back to the HomeTab, the TabBar should be visible
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			App.TapBackArrow(); // In iOS 26, the previous page title is not shown along with the back arrow, so we use the default back arrow
		}
		else
		{
			App.TapBackArrow(back);
		}
		App.WaitForElement("RecentTab");
		App.Click("RecentTab");
		App.WaitForElement("HomeTab");
		App.Click("HomeTab");
		App.WaitForElement("RecentTab");
	}

	[TearDown]
	public void RecordCompletedTargetIdentity()
	{
		if (App is not AppiumCatalystApp)
			return;

		// Capture after the test, before fixture teardown closes the Mac app.
		var processes = System.Diagnostics.Process.GetProcessesByName("Controls.TestCases.HostApp");
		var processIds = processes.Select(process => process.Id).ToArray();
		foreach (var process in processes)
			process.Dispose();
		Assert.That(processIds, Has.Length.EqualTo(1), "Cannot establish target process identity after interactions.");
		var completedPid = processIds[0];
		TestContext.WriteLine($"TargetAppPid: {completedPid}");

		var outputDirectory = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY")
			?? TestContext.CurrentContext.WorkDirectory;
		var pidPath = Path.Combine(outputDirectory, "Issue26598-completed-target-pids.txt");
		var pidText = completedPid.ToString(System.Globalization.CultureInfo.InvariantCulture);
		var priorPids = File.Exists(pidPath) ? File.ReadAllLines(pidPath) : Array.Empty<string>();
		Assert.That(priorPids, Does.Not.Contain(pidText), "Target process was reused across measured cases.");
		File.AppendAllText(pidPath, pidText + Environment.NewLine);
	}
}


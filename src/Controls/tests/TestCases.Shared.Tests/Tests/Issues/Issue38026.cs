#if ANDROID     // This issue is reproduced only on Android API 36, so added an Android-specific test and restricted it from running on other platforms, also categorized the test under SafeAreaEdges.                                                                                                                                                                                                  
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38026 : _IssuesUITest
{
	public Issue38026(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Navigation stops working after changing Window.Page from Shell";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void BackWorksAfterReplacingShellWithNavigationPage()
	{
		App.WaitForElement("InstallNavigationPageButton");
		App.Tap("InstallNavigationPageButton");

		App.WaitForElement("ShowShellButton");
		App.Tap("ShowShellButton");

		App.WaitForElement("OpenShellFlyoutButton");
		App.Tap("OpenShellFlyoutButton");

		App.WaitForElement("ReplaceShellButton");
		App.Tap("ReplaceShellButton");

		App.WaitForElement("ReplacementPageLabel");

		App.Back();

		App.WaitForNoElement("ReplacementPageLabel");
	}
}
#endif
#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // The back button text is partially visible on Android More Information:https://github.com/dotnet/maui/issues/19747 and the back button text is not overridden in Windows More information:https://github.com/dotnet/maui/issues/1625
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11244 : _IssuesUITest
{
	public Issue11244(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] BackButtonBehavior no longer displays on the first routed page in 4.7";

	[Test]
	[Category(UITestCategories.Shell)]
	public void LeftToolbarItemTextDisplaysWhenFlyoutIsDisabled()
	{
		App.WaitForElement("Logout");
	}
}
#endif
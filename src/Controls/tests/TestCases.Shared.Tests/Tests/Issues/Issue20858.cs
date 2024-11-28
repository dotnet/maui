#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20858 : _IssuesUITest
{
	public Issue20858(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "FlyoutPage Android app crashing on orientation change when flyout is open";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void NoExceptionShouldBeThrown()
	{
		App.SetOrientationPortrait();
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
		App.SetOrientationLandscape();

		//The test passes if no exception is thrown
	}
}
#endif
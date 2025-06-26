#if ANDROID || IOS //This test case verifies "SetOrientationPotrait and Landscape works" exclusively on the Android and IOS platforms
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20858 : _IssuesUITest
{
	public Issue20858(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "FlyoutPage Android app crashing on orientation change when flyout is open";

	[Fact]
	[Trait("Category", UITestCategories.FlyoutPage)]
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
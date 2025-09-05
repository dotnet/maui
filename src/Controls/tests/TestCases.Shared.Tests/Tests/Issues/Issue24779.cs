#if ANDROID || IOS //This test case verifies "SetOrientationPotrait and Landscape works" exclusively on the Android and IOS platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24779 : _IssuesUITest
{
	public Issue24779(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Android crashing when rotating a flyout page";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void NoExceptionShouldBeThrown()
	{
		App.SetOrientationPortrait();
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
		
		// Rotate multiple times to try to trigger the crash
		App.SetOrientationLandscape();
		App.SetOrientationPortrait();
		App.SetOrientationLandscape();
		App.SetOrientationPortrait();

		//The test passes if no exception is thrown
	}
}
#endif
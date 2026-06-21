#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS    //FlyoutLayoutBehavior changes are not supported on Android and iOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18158 : _IssuesUITest
{
	public Issue18158(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "FlyoutPage does not respond to changes in the FlyoutLayoutBehavior property";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutLayoutBehaviorChanges()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		App.WaitForElement("Label");
		App.WaitForNoElement("Button");
	}
}

#endif
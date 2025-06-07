#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29655 : _IssuesUITest
{
	public Issue29655(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "BackButtonBehavior Clicked event does not exist";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void BackButtonBehaviorClickEventShouldWork()
	{
		App.WaitForElement("GoToDetailPage");
		App.Tap("GoToDetailPage");
		App.TapBackArrow("Click");
		App.WaitForElement("SuccessLabel");
	}
}
#endif
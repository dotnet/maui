#if TEST_FAILS_ON_WINDOWS // Cannot open programatically a SwipeView on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17204 : _IssuesUITest
{
	public override string Issue => "SwipeView does not work correctly on iOS when opened programmatically";

	public Issue17204(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void ProgrammaticallyOpenedSwipeViewShouldBeVisible()
	{
		App.WaitForElement("OpenSwipeViewButton");
		App.Tap("OpenSwipeViewButton");
		VerifyScreenshot();
	}
}
#endif
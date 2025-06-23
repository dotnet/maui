#if TEST_FAILS_ON_WINDOWS //Carousel view tests fail on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30118 : _IssuesUITest
{
	public Issue30118(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "IndicatorView does not visually update dot indicators when the ItemsSource count changes";

	[Test]
	[Category(UITestCategories.IndicatorView)]
	public void IndicatorViewShouldUpdate()
	{
		App.WaitForElement("AddButton");
		App.Tap("AddButton");
		App.Tap("AddButton");
		App.Tap("AddButton");
		App.Tap("AddButton");
		App.Tap("AddButton");
		VerifyScreenshot();
	}
}
#endif
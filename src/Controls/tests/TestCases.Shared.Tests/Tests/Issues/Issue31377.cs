#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID //The more page works differently on these platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31377 : _IssuesUITest
{
	public Issue31377(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "TabbedPage overflow 'More' button does not work";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Issue31377TestsOverflowMoreButton()
	{
		App.WaitForElement("More");
		App.Tap("More");
		App.WaitForElement("Tab 10");
	}
}
#endif
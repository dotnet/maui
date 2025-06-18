#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // Test ignored on Windows and Android due to rendering issues. The documentation specifies that TabbedPage should contain NavigationPage or ContentPage, but this sample uses nested TabbedPages.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4973 : _IssuesUITest
{
	public Issue4973(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage nav tests";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Issue4973Test()
	{
		App.WaitForElement("Tab5");
		App.Tap("Tab5");
		App.WaitForElement("Test");
		GC.Collect();
		App.Tap("Tab1");
		App.Tap("Tab2");
	}
}
#endif
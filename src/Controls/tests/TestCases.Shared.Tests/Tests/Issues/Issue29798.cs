#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29798 : _IssuesUITest
{
	public Issue29798(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Tab becomes blank after specific navigation pattern";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabShouldNotBeBlackAfterTabNavigation()
	{
		App.WaitForElement("GotoPage2");
		App.Tap("GotoPage2");
		App.WaitForElement("Page2Label");
		App.Tap("Tab1");
		App.WaitForElement("GotoPage2");
		App.Tap("GotoPage2");
		App.WaitForElement("Page2Label");
		App.Tap("Tab1");
		App.WaitForElement("GotoPage2");
	}
}
#endif
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30728 : _IssuesUITest
{
	public override string Issue => "Android image sources with the same source ref fail to load";

	public Issue30728(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyTabBarIconIsLoaded()
	{
		App.WaitForElement("Tab 1");
		App.Tap("Tab 1");
		App.WaitForElement("Tab 2");
		App.Tap("Tab 2");
		App.WaitForElement("Tab 3");
		App.Tap("Tab 3");
		App.WaitForElement("Tab 1");
		App.Tap("Tab 1");

		VerifyScreenshot();
	}
}
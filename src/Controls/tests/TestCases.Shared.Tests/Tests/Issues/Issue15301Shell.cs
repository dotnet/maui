#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15301Shell : _IssuesUITest
{
	public Issue15301Shell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell.TabActiveTapped";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ClickingCurrentTabShouldActivateEvent()
	{
		App.WaitForElement("Deep0Button");
		App.Click("Deep0Button");
		App.WaitForElement("SubpageLabel");
		App.Click("Tab 1");
		App.WaitForElement("Deep0Button");
	}
}

#endif
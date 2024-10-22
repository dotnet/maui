using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1323 : _IssuesUITest
{
	const string Success = "Success";

	public Issue1323(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "tabbed page BarTextColor is not pervasive and can't be applied after instantiation";

	// [Test]
	// [Category(UITestCategories.TabbedPage)]
	// [FailsOnIOS]
	// public void Issue1323Test()
	// {
	// 	RunningApp.WaitForElement(X => X.Marked("Page 1"));
	// 	RunningApp.WaitForElement(X => X.Marked("Page5"));
	// 	RunningApp.Screenshot("All tab bar items text should be white");
	// }
}

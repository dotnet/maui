using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1323 : _IssuesUITest
{
	const string Tab1 = "Page 1";
	const string Tab2 = "Page5";


	public Issue1323(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "tabbed page BarTextColor is not pervasive and can't be applied after instantiation";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Issue1323Test()
	{
		App.WaitForTabElement(Tab1);
		App.WaitForTabElement(Tab2);
		VerifyScreenshot();
	}
}

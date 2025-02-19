using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1323 : _IssuesUITest
{

#if ANDROID
	const string Tab1 = "PAGE 1";
	const string Tab2 = "PAGE5";
#else
	const string Tab1 = "Page 1";
	const string Tab2 = "Page5";
#endif

	public Issue1323(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "tabbed page BarTextColor is not pervasive and can't be applied after instantiation";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Issue1323Test()
	{
		App.WaitForElement(Tab1);
		App.WaitForElement(Tab2);
		VerifyScreenshot();
	}
}

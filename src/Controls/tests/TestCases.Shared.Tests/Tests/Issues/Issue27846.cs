#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27846 : _IssuesUITest
{
	public Issue27846(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] More tab doesn't respect shell nav bar customization";

	[Test]
	[Category(UITestCategories.Shell)]
	[Category(UITestCategories.TabbedPage)]
	public void MoreTabShouldRespectNavBarCustomization()
	{
		App.WaitForElement("More");
		App.Click("More");
		VerifyScreenshot();
	}
}
#endif

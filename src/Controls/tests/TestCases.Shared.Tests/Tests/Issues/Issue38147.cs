#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38147 : _IssuesUITest
{
	public override string Issue => "iOS Glass UI bottom tabs are not properly aligned";

	public Issue38147(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void VerifyTabBarItemsAreProperlyAligned()
	{
		App.WaitForElement("HomeTabLabel");
		VerifyScreenshot();
	}
}
#endif
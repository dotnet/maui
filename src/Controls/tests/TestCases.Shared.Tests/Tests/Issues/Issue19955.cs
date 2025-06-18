using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue19955 : _IssuesUITest
{
	public Issue19955(TestDevice device) : base(device) { }

	public override string Issue => "Navigating Back to FlyoutPage Renders Blank Page";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void NavigatingBackToFlyoutPageRendersBlankPage()
	{
		App.WaitForElement("NavigateToSecondPageButton");
		App.Tap("NavigateToSecondPageButton");
		App.WaitForElement("NavigateBackToFirstPageButton");
		App.Tap("NavigateBackToFirstPageButton");
		App.WaitForElement("NavigateToSecondPageButton");
	}
}
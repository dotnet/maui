using NUnit.Framework;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7823 : _IssuesUITest
	{
		public Issue7823(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "In a ToolbarItems, if an item has no icon but just text, MAUI uses the icon from the previous page in the Navigation";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void UpdateToolbarItemAfterNavigate()
		{
			// 1. Navigate from Page with a ToolbarItem using an Icon. 
			App.WaitForElement("WaitForStubControl");
			App.Tap("WaitForStubControl");

			App.WaitForElement("SecondPageLoaded");

			// 2. Verify that the second page with a ToolbarItem without an icon does not show the icon of the previous page.
			VerifyScreenshot();
		}
	}
}

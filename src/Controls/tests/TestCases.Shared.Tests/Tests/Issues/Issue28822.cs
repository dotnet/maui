using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28822 : _IssuesUITest
	{
		public Issue28822(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ToolbarItem behavior with ImageSource iOS";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemShouldBeCorrectlyRendered()
		{
			App.WaitForElement("HelloWorldLabel");
			VerifyScreenshot();
		}
	}
}
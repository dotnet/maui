#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25709 : _IssuesUITest
	{
		public Issue25709(TestDevice device) : base(device)
		{
		}

		public override string Issue => "MenuBarItem foreground was not updated";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void MenuBarItemForegroundUpdated()
		{
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}
#endif
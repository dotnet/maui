using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22937 : _IssuesUITest
	{
		public override string Issue => "ToolbarItem font color not updating properly after changing the available state at runtime";

		public Issue22937(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemFontColorDynamicUpdate()
		{
			App.WaitForElement("ChangeState");
			App.Tap("ChangeState");

			VerifyScreenshot();
		}
	}
}
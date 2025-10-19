using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7156 : _IssuesUITest
	{
		public Issue7156(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Fix for wrong secondary ToolbarItem size on Windows";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemCorrectSizeTest()
		{
			App.WaitForMoreButton();
			App.TapMoreButton();
			App.Tap("Secondary Text Item");
			VerifyScreenshot();
		}
	}
}
#if WINDOWS
using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.ToolbarItem)]
		public void ToolbarItemCorrectSizeTest()
		{
			App.ToggleSecondaryToolbarItems();
			App.Tap("Secondary Text Item");
			VerifyScreenshot();
		}
	}
}
#endif
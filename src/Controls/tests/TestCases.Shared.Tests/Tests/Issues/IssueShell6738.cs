using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueShell6738 : _IssuesUITest
	{
		public override string Issue => "The color of the custom icon in Shell always resets to the default blue";

		public IssueShell6738(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void EnsureCustomFlyoutIconColor()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}

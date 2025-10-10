using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26726 : _IssuesUITest
	{
		public override string Issue => "Flyout Icon Positioned Incorrectly in RTL mode";

		public Issue26726(TestDevice testDevice) : base(testDevice) { }

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void FlyoutIconShouldBeCorrectlyPositioned()
		{
			App.WaitForElement("ShowRightToLeft");
			App.Click("ShowRightToLeft");
			VerifyScreenshot();
		}
	}
}
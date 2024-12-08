using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26392 : _IssuesUITest
	{
		public Issue26392(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Click on flyout clicks on page behind";

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void ClickThroughFlyoutShouldBeImpossible()
		{
			App.WaitForElement("ButtonInFlyout");
			App.Click("ButtonInFlyout");

			var actionLabelText = App.WaitForElement("ActionLabel").GetText();
			Assert.That(actionLabelText, Is.EqualTo("Button behind flyout was not clicked"));
		}
	}
}
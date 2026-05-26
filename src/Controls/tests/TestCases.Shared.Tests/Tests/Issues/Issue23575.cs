using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23575 : _IssuesUITest
	{
		public Issue23575(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Gradient never returns to the correct colour";

		[Test]
		[Category(UITestCategories.Border)]
		public void LinearGradientShouldHaveTheSameTopColorAsBackground()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}
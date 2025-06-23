using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13390 : _IssuesUITest
	{
		public Issue13390(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Custom SlideFlyoutTransition is not working";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void CustomSlideFlyoutTransitionCausesCrash()
		{
			// If this hasn't already crashed, the test is passing
			App.WaitForElement("Success");
		}
	}
}

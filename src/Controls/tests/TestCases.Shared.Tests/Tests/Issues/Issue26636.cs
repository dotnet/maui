using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26636 : _IssuesUITest
	{
		public Issue26636(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Shadows & Gradients don't work with a list view's header/footer";

		[Test]
		[Category(UITestCategories.ListView)]
		public void GradientAndShadowShouldWork()
		{
			App.WaitForElement("HeaderLabel");
			VerifyScreenshot();
		}
	}
}
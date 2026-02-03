using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11404 : _IssuesUITest
	{
		public Issue11404(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Line coordinates not computed correctly";

		[Test]
		[Category(UITestCategories.Shape)]
		public void LineWithReversedCoordinatesShouldRenderSymmetrically()
		{
			// Wait for the test grid to be ready
			App.WaitForElement("DescriptionLabel");

			VerifyScreenshot();
		}
	}
}

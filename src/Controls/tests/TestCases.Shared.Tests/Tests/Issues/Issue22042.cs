using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22042 : _IssuesUITest
	{
		public Issue22042(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] Border Stroke GradiantBrush can only switch to another gradiantbrush";

		[Test]
		[Category(UITestCategories.Border)]
		public void BorderColorShouldChange()
		{
			App.WaitForElement("label");

			//Applies a gradient
			App.Click("label");

			//Applies a solid color
			App.Click("label");

			VerifyScreenshot();
		}
	}
}

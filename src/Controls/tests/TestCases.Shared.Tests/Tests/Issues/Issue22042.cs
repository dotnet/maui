using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue22042 : _IssuesUITest
	{
		public Issue22042(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] Border Stroke GradiantBrush can only switch to another gradiantbrush";

		[Test]
		public void BorderCollorShouldChange()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement("border");

			//Applies a gradient
			App.Click("border");

			//Applies a solid color
			App.Click("border");

			VerifyScreenshot();
		}
	}
}

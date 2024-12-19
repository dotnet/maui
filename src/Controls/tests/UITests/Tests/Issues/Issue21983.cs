using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21983 : _IssuesUITest
	{
		public override string Issue => "GradientBrushes are not supported on Shape.Stroke";

		public Issue21983(TestDevice device) : base(device)
		{
		}

		[Test]
		public void GradientShouldBeAppliedToStrokes()
		{
			//It is not supported on Windows yet
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Windows });

			_ = App.WaitForElement("path");

			VerifyScreenshot();
		}
	}
}

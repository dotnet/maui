using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23119 : _IssuesUITest
	{
		public Issue23119(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Assigning a Brush of type RadialGradientBrush to the Background property of an ImageButton causes the BG to show a solid color";

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void GradientBackgroundShouldWorkWithImageButton()
		{
			App.WaitForElement("imageButton");
			VerifyScreenshot();
		}
	}
}
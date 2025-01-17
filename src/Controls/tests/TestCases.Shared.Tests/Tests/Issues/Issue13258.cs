using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13258 : _IssuesUITest
	{
		public Issue13258(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "MAUI Slider thumb image is big on android";

		[Test]
		[Category(UITestCategories.Slider)]
		public void SliderThumbImageShouldBeScaled()
		{
			App.WaitForElement("slider");
			VerifyScreenshot();
		}
	}
}
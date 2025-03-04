using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue18131 : _IssuesUITest
	{
		public override string Issue => "Color changes are not reflected in the Rectangle shapes";

		public Issue18131(TestDevice testDevice) : base(testDevice) { }

		[Test]
		[Category(UITestCategories.Shape)]
		public async Task UpdateShapeBackgroundColor()
		{
			App.WaitForElement("ChangeColorButton");
			App.Click("ChangeColorButton");
			await Task.Delay(500); // Avoid incorrect snapshot comparison from Ripple or tap effects
			VerifyScreenshot();
		}
	}
}
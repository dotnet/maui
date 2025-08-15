using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25385 : _IssuesUITest
	{
		public Issue25385(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "DataTrigger doesn't change shape background color";

		[Test]
		[Category(UITestCategories.Shape)]
		public void ShapeBackgroundColorChanges()
		{
			App.WaitForElement("Button");
			App.Click("Button");

			//Test passes if rectangle and ellipse are Red
			VerifyScreenshot();
		}
	}
}
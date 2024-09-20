using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22032 : _IssuesUITest
	{
		public Issue22032(TestDevice device) : base(device) { }

		public override string Issue => "Shell FlyoutItem Tab Selected Icon Color not changing if using Font icons";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void SelectedTabIconShouldChangeColor()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.WaitForElement("label");

			// The test passes if tab1 icon is green and tab2 red
			VerifyScreenshot();
		}
	}
}

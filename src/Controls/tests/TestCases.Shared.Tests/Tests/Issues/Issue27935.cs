using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27935 : _IssuesUITest
	{
		public Issue27935(TestDevice device) : base(device) { }

		public override string Issue => "iOS: Rectangle that is invisible when page loads can never be made visible";

		[Test, Order(1)]
		[Category(UITestCategories.Shape)]
		public void ShapesShouldAppearCorrectlyWhenIsVisibleChangesToTrue()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.Shape)]
		public void ShapesShouldHideCorrectlyWhenIsVisibleChangesToFalse()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}
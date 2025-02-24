using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27935 : _IssuesUITest
	{
		public Issue27935(TestDevice device) : base(device) { }

		public override string Issue => "iOS: Rectangle that is invisible when page loads can never be made visible";

		[Test]
		[Category(UITestCategories.Shape)]
		public void ShapesShouldAppearCorrectlyWhenIsVisibleChanges()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}
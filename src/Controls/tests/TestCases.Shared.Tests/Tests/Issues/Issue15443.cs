using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue15443 : _IssuesUITest
	{
		public override string Issue => "Programmatic position set should work as expected";

		public Issue15443(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselShouldWorkProperOnBinding()
		{
			var initialRect = App.WaitForElement("One").GetRect();
			App.WaitForElement("15443Button3");
			App.Click("15443Button3");
			var rect = App.WaitForElement("Three").GetRect();
			Assert.That(rect.X, Is.GreaterThan(0));
			var labelText = App.WaitForElement("15443PositionLabel").GetText();
			Assert.That(labelText, Is.EqualTo("Position: 2"));
			App.Click("15443Button2");
			var rect1 = App.WaitForElement("Two").GetRect();
			Assert.That(rect1.X, Is.GreaterThan(0));
			var labelText1 = App.WaitForElement("15443PositionLabel").GetText();
			Assert.That(labelText1, Is.EqualTo("Position: 1"));
		}
	}
}

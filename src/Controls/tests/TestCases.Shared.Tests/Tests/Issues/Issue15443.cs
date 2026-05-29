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
			App.WaitForElement("15443Button3");
			App.Tap("15443Button3");
			App.WaitForTextToBePresentInElement("15443CurrentItemLabel", "Current: Three");
			Assert.That(App.FindElement("15443CurrentItemLabel").GetText(), Is.EqualTo("Current: Three"),
			"Tapping button 3 should navigate to item Three");
			Assert.That(App.FindElement("15443PositionLabel").GetText(), Is.EqualTo("Position: 2"),
			"Tapping button 3 should navigate to position 2");
			App.Tap("15443Button2");
			App.WaitForTextToBePresentInElement("15443CurrentItemLabel", "Current: Two");
			Assert.That(App.FindElement("15443CurrentItemLabel").GetText(), Is.EqualTo("Current: Two"),
			"Tapping button 2 should navigate to item Two");
			Assert.That(App.FindElement("15443PositionLabel").GetText(), Is.EqualTo("Position: 1"),
			"Tapping button 2 should navigate to position 1");
			App.Tap("15443Button1");
			App.WaitForTextToBePresentInElement("15443CurrentItemLabel", "Current: One");
			Assert.That(App.FindElement("15443CurrentItemLabel").GetText(), Is.EqualTo("Current: One"),
			"Tapping button 1 should navigate to item One");
			Assert.That(App.FindElement("15443PositionLabel").GetText(), Is.EqualTo("Position: 0"),
			"Tapping button 1 should navigate to position 0");
		}
	}
}

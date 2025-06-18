using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8845 : _IssuesUITest
	{
		public Issue8845(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Picker on windows shows \"Microsoft.Maui.Controls.Picker\" if ItemsSource has an empty string";

		[Test]
		[Category(UITestCategories.Picker)]
		public void PickerShouldDisplayValueFromItemDisplayBinding()
		{
			App.WaitForElement("UpdateButton");
			App.Tap("UpdateButton");

			var picker1Text = App.WaitForElement("MauiPicker1").GetText();
			var picker2Text = App.WaitForElement("MauiPicker2").GetText();

			Assert.That(picker1Text, Is.Empty);
			Assert.That(picker2Text, Is.EqualTo("Not null"));
		}
	}
}

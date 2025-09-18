using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27519 : _IssuesUITest
	{
		public Issue27519(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "When opening the Picker, the first item is selected instead of the currently selected item";

		[Test]
		[Category(UITestCategories.Picker)]
		public void CorrectItemShouldBeSelectedWhenOpeningPicker()
		{
			App.WaitForElement("Picker");
			App.Click("Picker");
			VerifyScreenshot();
		}
	}
}
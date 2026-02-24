using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6101 : _IssuesUITest
	{
		public override string Issue => "Picker items do not appear when tapping on the picker while using PushModalAsync for navigation";

		public Issue6101(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void ShouldAppearItemsWhenTappingOnPickerUsingPushModalAsync()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			App.WaitForElement("Picker");
			App.Click("Picker");
			VerifyScreenshot();
		}
	}
}
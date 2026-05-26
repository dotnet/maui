#if TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/27999
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27474 : _IssuesUITest
	{
		public Issue27474(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Picker view items are not displayed when setting the title on the picker control";

		[Test]
		[Category(UITestCategories.Picker)]
		public void ShouldDisplayPickerItemsWhenOpeningPicker()
		{
			App.WaitForElement("Picker");
			App.Click("Picker");
			VerifyScreenshot();
		}
	}
}
#endif
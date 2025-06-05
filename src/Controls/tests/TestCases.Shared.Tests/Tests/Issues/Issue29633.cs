#if TEST_FAILS_ON_CATALYST // Highlighting Not Working Properly on Mac Catalyst, https://github.com/dotnet/maui/issues/27519
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29633 : _IssuesUITest
{
	public Issue29633(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Picker Does Not Show Selected Item Highlight";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerShouldShowSelectedItemHighlight()
	{
		App.WaitForElement("HighlightPickerItem");
		App.Click("HighlightPickerItem");
		VerifyScreenshot();
	}
}
#endif
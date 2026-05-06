#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34899 : _IssuesUITest
{
	public Issue34899(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => ".Net 10 Picker item not centered and wrong focus outline of Entry on Mac";

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryFocusOutlineShouldAlignWithBorder()
	{
		App.WaitForElement("Issue34899Entry");
		App.Tap("Issue34899Entry");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerFocusOutlineShouldAlignWithBorder()
	{
		App.WaitForElement("Issue34899Picker");
		App.Tap("Issue34899Picker");
		VerifyScreenshot();
	}
}
#endif

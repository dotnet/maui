#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34899 : _IssuesUITest
{
	public Issue34899(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => ".Net 10 Picker item not centered and wrong focus outline of Entry/Picker on Mac";

	// On macOS 26+ (Tahoe / Liquid Glass), UITextField with BorderStyle=RoundedRect
	// renders a pill/capsule-shaped focus ring that doesn't match the visible border.
	// These tests verify the FocusEffect override correctly rounds the focus ring
	// to match the visible rectangular border shape.

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryFocusOutlineShouldAlignWithBorder()
	{
		App.WaitForElement("Issue34899Entry");
		App.Tap("Issue34899Entry");
		VerifyScreenshot();
	}
}
#endif

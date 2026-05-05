#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class Issue35279 : _IssuesUITest
{
	public override string Issue =>
		"KeyboardAccelerator with Cmd+Shift modifiers breaks entire MenuBarItem on Mac Catalyst";

	public Issue35279(TestDevice testDevice) : base(testDevice) { }

	// Regression test for https://github.com/dotnet/maui/issues/35279
	// A MenuFlyoutItem with a Cmd+Shift keyboard accelerator (e.g. Cmd+Shift+S for "Save As")
	// caused Mac Catalyst to silently reject the UIKeyCommand, making the entire parent
	// UIMenu non-functional (the File menu would not open at all).
	// Fix: alphabetic keys are normalised to lowercase before being passed to UIKeyCommand.Create.
	[Test]
	public void FileMenuIsOpenableWithCmdShiftAccelerator()
	{
		App.WaitForElement("ResultLabel");

		// Open the File menu — this fails entirely without the fix
		App.WaitForElement("File");
		App.Tap("File");

		// Both items must be visible inside the now-working File menu
		App.WaitForElement("Save As");
		App.Tap("Save As");

		Assert.That(
			App.WaitForElement("ResultLabel").GetText(),
			Is.EqualTo("Save As executed"));
	}
}
#endif

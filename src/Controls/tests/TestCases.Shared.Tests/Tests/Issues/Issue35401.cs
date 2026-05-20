#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35401 : _IssuesUITest
{
	public Issue35401(TestDevice device) : base(device) { }

	public override string Issue => "Modal page keyboard dismissal causes parent Shell page layout to offset on iOS";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ParentPageShouldNotOffsetAfterModalKeyboardDismissal()
	{
		// Navigate from Level1 to Level2
		App.WaitForElement("GoToLevel2Button");
		App.Tap("GoToLevel2Button");

		// Capture the Y position of the Level2 title before opening the modal
		App.WaitForElement("Level2Title");
		var titleRectBefore = App.WaitForElement("Level2Title").GetRect();

		// Open the transparent modal page
		App.WaitForElement("OpenModalButton");
		App.Tap("OpenModalButton");

		// Focus the Editor inside the modal to trigger the software keyboard
		App.WaitForElement("ModalEditor");
		App.Tap("ModalEditor");

		// The bug only manifests when UIKeyboardWillHideNotification fires.
		// On simulators with a hardware keyboard connected, the software keyboard
		// does not appear and no notification is sent — skip rather than false-pass.
		if (!App.WaitForKeyboardToShow(TimeSpan.FromSeconds(5)))
		{
			Assert.Ignore("Software keyboard did not appear in this test environment. " +
				"Run on a device (or simulator with software keyboard enabled) to verify " +
				"the regression from PR #33958 is fixed.");
			return;
		}

		// Dismiss the keyboard — this fires UIKeyboardWillHideNotification, which
		// previously triggered an incorrect frame adjustment on the underlying Shell page.
		App.Tap("Done");
		App.WaitForKeyboardToHide();

		// Close the modal
		App.WaitForElement("CloseModalButton");
		App.Tap("CloseModalButton");

		// Level2 page should be visible again with its layout intact
		App.WaitForElement("Level2Title");
		var titleRectAfter = App.WaitForElement("Level2Title").GetRect();

		// The title Y position must not have shifted — regression from PR #33958 caused
		// OnKeyboardWillHide to reposition the underlying Shell page when the modal's
		// keyboard was dismissed.
		Assert.That(titleRectAfter.Y, Is.EqualTo(titleRectBefore.Y),
			"Level2 page title shifted vertically after modal keyboard was dismissed — layout offset regression (PR #33958)");
	}
}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Only Android lets a read-only input get focus
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6250 : _IssuesUITest
{
	public Issue6250(TestDevice device) : base(device) { }

	public override string Issue => "[Android] IsReadOnly=true EditText controls are not focusable";

	[TestCase("ReadOnlyEntry")]
	[TestCase("ReadOnlyEditor")]
	[TestCase("ReadOnlySearchBar")]
	[Category(UITestCategories.Entry)]
	public void FocusingReadOnlyInputHidesKeyboard(string readOnlyInput)
	{
		App.WaitForElement("EditableEntry");
		App.Tap("EditableEntry");
		Assert.That(App.WaitForKeyboardToShow(), Is.True, "Tapping the editable Entry should show the keyboard");

		// The read-only input gets focus but can't be typed into, so the keyboard must not stay up
		App.Tap(readOnlyInput);
		Assert.That(App.WaitForKeyboardToHide(TimeSpan.FromSeconds(5)), Is.True, $"Focusing {readOnlyInput} should hide the keyboard");
	}
}
#endif

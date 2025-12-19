using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33038 : _IssuesUITest
{
	public Issue33038(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Layout breaks on first navigation until soft keyboard appears/disappears";

	[Test]
	[Category(UITestCategories.Shell)]
	public void LayoutShouldBeCorrectOnFirstNavigation()
	{
		App.WaitForElement("StartPageLabel");
		App.Tap("GoToSignInButton");

		var labelRect = App.WaitForElement("SignInLabel").GetRect();
		var safeAreaTop = int.Parse(App.WaitForElement("SafeAreaTopLabel").GetText() ?? "0");

		// Verify label is below safe area (not overlapped by status bar/display cutout)
		Assert.That(labelRect.Y, Is.GreaterThanOrEqualTo(safeAreaTop),
			$"SignInLabel Y ({labelRect.Y}) should be >= safe area top ({safeAreaTop})");

		// Verify position remains consistent after keyboard show/hide
		App.Tap("EmailEntry");
		App.DismissKeyboard();
		var afterKeyboard = App.WaitForElement("SignInLabel").GetRect();

		Assert.That(labelRect.Y, Is.EqualTo(afterKeyboard.Y).Within(5),
			"SignInLabel Y position should remain consistent after keyboard show/hide");
	}
}

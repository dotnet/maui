using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25728 : _IssuesUITest
{
	public Issue25728(TestDevice device) : base(device) { }

	public override string Issue => "Java.Lang.IllegalArgumentException when clearing Entry text with StringFormat binding on Android";

	[Test]
	[Category(UITestCategories.Entry)]
	public void ClearingEntryWithStringFormatBindingShouldNotCrash()
	{
		// Wait for the Entry with StringFormat binding to appear (shows "0.00" initially)
		App.WaitForElement("FloatEntry");

		// Clear all characters from the Entry
		App.ClearText("FloatEntry");

		// Enter a number — this triggered Java.Lang.IllegalArgumentException on Android
		App.EnterText("FloatEntry", "42");

		// If we reach here without a crash, the issue is fixed
		App.WaitForElement("FloatEntry");
	}
}

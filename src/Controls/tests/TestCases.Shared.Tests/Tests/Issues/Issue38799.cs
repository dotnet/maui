// The focus-loss ordering being observed is specific to Android.
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38799 : _IssuesUITest
{
	public Issue38799(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Replacing focused Entry content on a Shell page";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell)]
	public void ReplacingTextEntryRelinquishesFocusBeforeRemoval()
	{
		App.WaitForElement("TextCell");
		App.Tap("TextCell");
		App.WaitForElement("TextEntry");
		// Explicitly show the text keyboard before switching, as in the issue.
		App.Tap("TextEntry");
		Assert.That(App.WaitForKeyboardToShow(), Is.True, "The text keyboard must be visible before switching cells.");
		App.WaitForElement("NumericCell");
		App.Tap("NumericCell");
		App.WaitForElement("NumericEntry");
		Assert.That(App.WaitForElement("FocusResult").GetText(), Is.EqualTo("Focus relinquished before removal"));
	}
}
#endif

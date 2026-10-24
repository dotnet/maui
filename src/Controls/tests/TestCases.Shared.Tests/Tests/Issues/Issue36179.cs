#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36179 : _IssuesUITest
{
	public override string Issue => "Entry.Completed accidentally fired on Enter key press in IME candidate window";

	public Issue36179(TestDevice device) : base(device) { }

	// Verifies that sending only KeyUp(Enter) — the same sequence produced by IME candidate
	// confirmation — does NOT fire Entry.Completed.
	// Without the fix, Completed fires on any KeyUp(Enter) regardless of KeyDown.
	// With the fix, Completed requires a preceding KeyDown(Enter).
	[Test]
	[Category(UITestCategories.Entry)]
	public void CompletedDoesNotFireOnIMECandidateEnter()
	{
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");

		// Send ONLY KeyUp(Enter) with no KeyDown — simulates IME candidate confirmation.
		// The IME swallows KeyDown entirely; only KeyUp(Enter) leaks through.
		var app = App as AppiumApp;
		Assert.That(app, Is.Not.Null, "This test requires AppiumApp");

		app!.Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
		{
			["actions"] = new[]
			{
				new Dictionary<string, object> { ["virtualKeyCode"] = 0x0D, ["down"] = false }
			}
		});

		// Completed should NOT have fired
		Assert.That(App.WaitForElement("CompletedCountLabel").GetText(), Is.EqualTo("Completed: 0"));

		// Now send a real Enter (KeyDown + KeyUp) — Completed SHOULD fire
		App.PressEnter();

		Assert.That(App.WaitForElement("CompletedCountLabel").GetText(), Is.EqualTo("Completed: 1"));
	}
}
#endif

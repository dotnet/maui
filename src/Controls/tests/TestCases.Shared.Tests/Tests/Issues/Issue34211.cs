using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34211 : _IssuesUITest
{
	public Issue34211(TestDevice device) : base(device) { }

	public override string Issue => "Android display-size change causes parent and drawable children mismatch in .NET MAUI";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void DrawableDirtyRectMatchesGraphicsViewSizeAfterDisplayDensityChange()
	{
		// Wait for the GraphicsView to appear
		App.WaitForElement("Issue34211_GraphicsView");

		// Wait until the drawable has drawn at least once.
		// The label auto-updates on every draw, so poll until it leaves "Waiting..." state.
		App.WaitForElement(() =>
		{
			var text = App.FindElement("Issue34211_StatusLabel")?.GetText() ?? string.Empty;
			return text.StartsWith("PASS", StringComparison.Ordinal) ||
				   text.StartsWith("FAIL", StringComparison.Ordinal)
				? App.FindElement("Issue34211_StatusLabel")
				: null;
		}, "Timed out waiting for GraphicsView initial draw");

		// Verify initial draw is correct on all platforms
		var initialStatus = App.FindElement("Issue34211_StatusLabel").GetText() ?? string.Empty;
		Assert.That(initialStatus, Does.StartWith("PASS"),
			$"Initial draw: GraphicsView and drawable sizes do not match. Actual: {initialStatus}");

#if ANDROID
		// Android-only: trigger a real display-density change via adb.
		// Background the app first — exactly as the user does: go to Settings, change
		// Display size, come back. The Activity may be recreated (density is not in MAUI's
		// configChanges), so we re-navigate to the issue page after foregrounding.
		string originalDensity = ShellHelper.ExecuteShellCommandWithOutput("adb shell wm density").Trim();
		try
		{
			App.BackgroundApp();
			string newDensity = originalDensity.Contains("320", StringComparison.Ordinal) ? "280" : "320";
			ShellHelper.ExecuteShellCommand($"adb shell wm density {newDensity}");
			App.ForegroundApp();

			// Activity may have been recreated — re-navigate to the issue page
			App.WaitForElement("SearchBar");
			App.ClearText("SearchBar");
			App.EnterText("SearchBar", Issue);
			App.WaitForElement("GoToTestButton");
			App.Tap("GoToTestButton");

			// Wait for the label to reflect the post-density-change draw
			App.WaitForElement(() =>
			{
				var text = App.FindElement("Issue34211_StatusLabel")?.GetText() ?? string.Empty;
				return text.StartsWith("PASS", StringComparison.Ordinal) ||
					   text.StartsWith("FAIL", StringComparison.Ordinal)
					? App.FindElement("Issue34211_StatusLabel")
					: null;
			}, "Timed out waiting for draw after density change");

			var statusAfterChange = App.FindElement("Issue34211_StatusLabel").GetText() ?? string.Empty;
			Assert.That(statusAfterChange, Does.StartWith("PASS"),
				$"GraphicsView and drawable sizes diverged after Android display-density change (issue #34211). " +
				$"Actual: {statusAfterChange}");
		}
		finally
		{
			ShellHelper.ExecuteShellCommand("adb shell wm density reset");
		}
#endif
	}
}

#if ANDROID // Android-only: display-density change can only be triggered via adb shell wm density.
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
		App.WaitForElement("Issue34211_GraphicsView");

		string originalDensity = ShellHelper.ExecuteShellCommandWithOutput("adb shell wm density").Trim();
		try
		{
			App.BackgroundApp();
			ShellHelper.ExecuteShellCommand($"adb shell wm density {(originalDensity.Contains("320", StringComparison.Ordinal) ? "280" : "320")}");
			App.ForegroundApp();

			// adb shell wm density triggers an Activity recreation because density is not listed
			// in MAUI's android:configChanges. The app restarts at the list page, so re-navigate.
			App.WaitForElement("SearchBar");
			App.ClearText("SearchBar");
			App.EnterText("SearchBar", Issue);
			App.WaitForElement("GoToTestButton");
			App.Tap("GoToTestButton");

			var el = App.WaitForElement(() =>
			{
				var e = App.FindElement("Issue34211_StatusLabel");
				var text = e?.GetText() ?? string.Empty;
				return text.StartsWith("PASS", StringComparison.Ordinal) || text.StartsWith("FAIL", StringComparison.Ordinal) ? e : null;
			}, "Timed out waiting for draw after density change");
			Assert.That(el.GetText(), Does.StartWith("PASS"), "GraphicsView and drawable sizes diverged after display-density change");
		}
		finally
		{
			ShellHelper.ExecuteShellCommand("adb shell wm density reset");
		}
	}
}
#endif

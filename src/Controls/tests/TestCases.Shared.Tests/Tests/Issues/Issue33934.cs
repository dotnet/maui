#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33934 : _IssuesUITest
{
	public override string Issue => "[iOS] TranslateToAsync causes spurious SizeChanged events after animation completion";

	public Issue33934(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void BottomSheetAnimationShouldComplete()
	{
		// Tester reported: "Due to an infinite layout loop, the bottom sheet animation
		// does not stop and continues indefinitely."
		//
		// This test verifies the animation completes by checking that:
		// 1. The IterationCountLabel becomes visible (only happens when loop exits)
		// 2. The iteration count is reasonable (≤ 2)
		//
		// If the layout loop is infinite, the label never becomes visible and
		// the WaitForElement will time out after 15 seconds — a clear failure signal.

		// Step 1: Open the dialog which triggers the bottom sheet animation
		App.WaitForElement("ShowDialogBtn", timeout: TimeSpan.FromSeconds(10));
		App.Tap("ShowDialogBtn");

		// Step 2: Wait for the iteration count label to become visible.
		// This label is ONLY made visible when the animation loop exits successfully.
		// If the loop is infinite, this will time out — proving the bug exists.
		var iterationLabel = App.WaitForElement("IterationCountLabel", timeout: TimeSpan.FromSeconds(15));
		Assert.That(iterationLabel, Is.Not.Null,
			"IterationCountLabel never became visible — the bottom sheet animation is stuck " +
			"in an infinite layout loop and never completed.");

		// Step 3: Verify the iteration count is reasonable
		var labelText = iterationLabel.GetText();
		Assert.That(labelText, Is.Not.Null, "Label text should not be null");

		var parts = labelText!.Split(':');
		Assert.That(parts.Length, Is.GreaterThanOrEqualTo(2), $"Unexpected label format: '{labelText}'");

		var countStr = parts[1].Trim();
		Assert.That(int.TryParse(countStr, out int iterationCount), Is.True,
			$"Failed to parse iteration count from: '{labelText}'");

		Assert.That(iterationCount, Is.LessThanOrEqualTo(2),
			$"Animation completed but took {iterationCount} iterations (expected ≤ 2). " +
			"This indicates spurious SizeChanged events are still triggering unnecessary restarts.");
	}
}
#endif

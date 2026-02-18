#if IOS
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33934 : _IssuesUITest
{
	public override string Issue => "[iOS] TranslateToAsync causes spurious SizeChanged events after animation completion";

	public Issue33934(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Navigation)]
	public async Task AnimationIterationsShouldNotExceedTwo()
	{
		// Step 1: Wait for and locate the Show Dialog button
		var showDialogButton = App.WaitForElement("ShowDialogBtn", timeout: TimeSpan.FromSeconds(10));
		Assert.That(showDialogButton, Is.Not.Null, "Show Dialog button should be present on screen");

		// Step 2: Tap the button to trigger dialog and animation
		App.Tap("ShowDialogBtn");


		// Step 3: Give animation time to complete
		await Task.Delay(3000);

		// Step 4: Read the iteration count from the label text
		var iterationLabel = App.WaitForElement("IterationCountLabel", timeout: TimeSpan.FromSeconds(10));
		Assert.That(iterationLabel, Is.Not.Null, "Iteration count label should be visible after animation");

		var labelText = iterationLabel.GetText();
		Assert.That(labelText, Is.Not.Null.And.Contains(":"), "Label should contain iteration count in format 'Animation Iterations: X'");

		// Extract the count value from "Animation Iterations: X"
		var countStr = labelText!.Split(':')[1].Trim();
		var isValidCount = int.TryParse(countStr, out int iterationCount);
		Assert.That(isValidCount, Is.True, $"Failed to parse iteration count from label: '{labelText}'");

		// Step 6: Verify the iteration count is 1 or 2 (not excessive)
		Assert.That(iterationCount, Is.LessThanOrEqualTo(2), 
			$"Animation should complete in 2 or fewer iterations, but had {iterationCount}. This indicates spurious SizeChanged events.");
	}
}
#endif




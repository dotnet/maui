using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2894 : _IssuesUITest
{
	const string kGesture1 = "Sentence 1: ";
	const string kGesture2 = "Sentence 2: ";
	const string kLabelAutomationId = "kLabelAutomationId";

	public Issue2894(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Gesture Recognizers added to Span after it's been set to FormattedText don't work and can cause an NRE";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void VariousSpanGesturePermutation()
	{
		App.WaitForElement($"{kGesture1}0");
		App.WaitForElement($"{kGesture2}0");
		var labelId = App.WaitForElement(kLabelAutomationId);
		var target = labelId.GetRect();

		for (int i = 1; i < 5; i++)
		{
			App.Tap($"TestSpan{i}");
			App.WaitForElement($"{kGesture1}{i - 1}");
			App.WaitForElement(kLabelAutomationId);
			PerformGestureActionForFirstSpan(target);
			PerformGestureActionForSecondSpan(target);
		}

		App.Tap($"TestSpan5");
		PerformGestureActionForFirstSpan(target);
		PerformGestureActionForSecondSpan(target);
		App.WaitForElement($"{kGesture1}4");
		App.WaitForElement($"{kGesture2}4");
	}

	void PerformGestureAction(float x, float y)
	{
#if MACCATALYST // TapCoordinates is not working on MacCatalyst Issue: https://github.com/dotnet/maui/issues/19754
		App.ClickCoordinates(x, y);
#else
		App.TapCoordinates(x, y);
#endif
	}

	void PerformGestureActionForFirstSpan(Rectangle target)
	{
		PerformGestureAction(target.X + 5, target.Y + 5);
	}

	void PerformGestureActionForSecondSpan(Rectangle target)
	{
#if ANDROID // Calculate points vary on Android and other platforms.
		App.TapCoordinates(target.X + target.Width / 2, target.Y + 2);
#else
		PerformGestureAction(target.X + target.Width - 10, target.Y + 2);
#endif

	}
}
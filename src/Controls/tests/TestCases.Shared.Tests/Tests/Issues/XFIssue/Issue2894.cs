using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2894 : _IssuesUITest
{
	public Issue2894(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Gesture Recognizers added to Span after it's been set to FormattedText don't work and can cause an NRE";

// 	[FailsOnAndroid]
// 	[FailsOnIOS]
// 	[Test]
// 	[Category(UITestCategories.Gestures)]
// 	public void VariousSpanGesturePermutation()
// 	{
// 		RunningApp.WaitForElement($"{kGesture1}0");
// 		RunningApp.WaitForElement($"{kGesture2}0");
// 		var labelId = RunningApp.WaitForElement(kLabelAutomationId);
// 		var target = labelId.First().Rect;


// 		for (int i = 1; i < 5; i++)
// 		{
// 			RunningApp.Tap($"TestSpan{i}");

// 			// These tap retries work around a Tap Coordinate bug
// 			// with Xamarin.UITest >= 3.0.7
// 			int tapAttempts = 0;
// 			do
// 			{
// 				RunningApp.TapCoordinates(target.X + 5, target.Y + 5);
// 				if (tapAttempts == 4)
// 					RunningApp.WaitForElement($"{kGesture1}{i}");

// 				tapAttempts++;
// 			} while (RunningApp.Query($"{kGesture1}{i}").Length == 0);

// 			tapAttempts = 0;

// 			do
// 			{
// #if WINDOWS
// 				RunningApp.TapCoordinates(target.X + target.Width - 10, target.Y + 2);
// #else
// 				RunningApp.TapCoordinates(target.X + target.CenterX, target.Y + 2);
// #endif
// 				if (tapAttempts == 4)
// 					RunningApp.WaitForElement($"{kGesture1}{i}");

// 				tapAttempts++;

// 			} while (RunningApp.Query($"{kGesture2}{i}").Length == 0);
// 		}


// 		RunningApp.Tap($"TestSpan5");
// 		RunningApp.TapCoordinates(target.X + 5, target.Y + 5);

// #if WINDOWS
// 		RunningApp.TapCoordinates(target.X + target.Width - 10, target.Y + 2);
// #else
// 		RunningApp.TapCoordinates(target.X + target.CenterX, target.Y + 2);
// #endif

// 		RunningApp.WaitForElement($"{kGesture1}4");
// 		RunningApp.WaitForElement($"{kGesture2}4");
// 	}
}
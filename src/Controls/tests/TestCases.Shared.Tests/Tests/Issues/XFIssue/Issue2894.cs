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
	// 		App.WaitForElement($"{kGesture1}0");
	// 		App.WaitForElement($"{kGesture2}0");
	// 		var labelId = App.WaitForElement(kLabelAutomationId);
	// 		var target = labelId.First().Rect;


	// 		for (int i = 1; i < 5; i++)
	// 		{
	// 			App.Tap($"TestSpan{i}");

	// 			// These tap retries work around a Tap Coordinate bug
	// 			// with Xamarin.UITest >= 3.0.7
	// 			int tapAttempts = 0;
	// 			do
	// 			{
	// 				App.TapCoordinates(target.X + 5, target.Y + 5);
	// 				if (tapAttempts == 4)
	// 					App.WaitForElement($"{kGesture1}{i}");

	// 				tapAttempts++;
	// 			} while (App.Query($"{kGesture1}{i}").Length == 0);

	// 			tapAttempts = 0;

	// 			do
	// 			{
	// #if WINDOWS
	// 				App.TapCoordinates(target.X + target.Width - 10, target.Y + 2);
	// #else
	// 				App.TapCoordinates(target.X + target.CenterX, target.Y + 2);
	// #endif
	// 				if (tapAttempts == 4)
	// 					App.WaitForElement($"{kGesture1}{i}");

	// 				tapAttempts++;

	// 			} while (App.Query($"{kGesture2}{i}").Length == 0);
	// 		}


	// 		App.Tap($"TestSpan5");
	// 		App.TapCoordinates(target.X + 5, target.Y + 5);

	// #if WINDOWS
	// 		App.TapCoordinates(target.X + target.Width - 10, target.Y + 2);
	// #else
	// 		App.TapCoordinates(target.X + target.CenterX, target.Y + 2);
	// #endif

	// 		App.WaitForElement($"{kGesture1}4");
	// 		App.WaitForElement($"{kGesture2}4");
	// 	}
}
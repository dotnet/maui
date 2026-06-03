using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29740 : _IssuesUITest
{
	public Issue29740(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Windows] Stepper control fails to reach maximum value when increment exceeds remaining threshold";

	[Test]
	[Category(UITestCategories.Stepper)]
	public void GestureRecognizersOnLabelSpanShouldWork()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to Stepper Increment issue in iOS 26."); // Issue Link: https://github.com/dotnet/maui/issues/33769
		}
		var initialvalue = App.WaitForElement("29740StepperValueLabel").GetText();
		Assert.That(initialvalue, Is.EqualTo("Stepper Value: 0"));

		// Stepper: min=0, max=10, increment=3 → 4 taps to reach 10 (0→3→6→9→10, clamped to max).
		// Each tap is verified individually to handle intermittent tap failures.
		string[] expectedValues = ["Stepper Value: 3", "Stepper Value: 6", "Stepper Value: 9", "Stepper Value: 10"];
		for (int i = 0; i < 4; i++)
		{
			var expected = expectedValues[i];
			App.RetryAssert(() =>
			{
				var currentValue = App.WaitForElement("29740StepperValueLabel").GetText();
				if (currentValue != expected)
				{
					// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
					// See https://github.com/appium/appium/issues/22272
#if MACCATALYST
					App.DecreaseStepper("29740Stepper");
#else
					App.IncreaseStepper("29740Stepper");
#endif
					currentValue = App.WaitForElement("29740StepperValueLabel").GetText();
				}
				Assert.That(currentValue, Is.EqualTo(expected));
			});
		}
	}
}
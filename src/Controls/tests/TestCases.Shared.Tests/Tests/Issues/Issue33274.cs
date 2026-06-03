using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33274 : _IssuesUITest
{
	public Issue33274(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Windows Maui Stepper is not clamped to minimum or maximum internally";

	[Test]
	[Category(UITestCategories.Stepper)]
	public void Issue33274CheckInAndDeCrementation()
	{
		App.WaitForElement("Maximumlabel");

		// We are already at maximum and the increment schould not increase the internal value
		// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
		// See https://github.com/appium/appium/issues/22272

		// retry tap if it didn't register
		App.RetryAssert(() =>
		{
			var currentValue = App.FindElement("Maximumlabel").GetText();
			if (currentValue != "0")
			{
#if MACCATALYST
				App.DecreaseStepper("Maximumstepper");
				App.IncreaseStepper("Maximumstepper");
#else
				App.IncreaseStepper("Maximumstepper");
				App.DecreaseStepper("Maximumstepper");

#endif
				currentValue = App.FindElement("Maximumlabel").GetText();
			}
			Assert.That(currentValue, Is.EqualTo("0"));
		});

		// We are already at minimum and the decrement schould not decrease the internal value
		// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
		// See https://github.com/appium/appium/issues/22272

		//retry tap if it didn't register
		App.RetryAssert(() =>
		{
			var currentValue = App.FindElement("Minimumlabel").GetText();
			if (currentValue != "1")
			{
#if MACCATALYST
				App.IncreaseStepper("Minimumstepper");
				App.DecreaseStepper("Minimumstepper");
#else
	            App.DecreaseStepper("Minimumstepper");
				App.IncreaseStepper("Minimumstepper");
#endif
				currentValue = App.FindElement("Minimumlabel").GetText();
			}
			Assert.That(currentValue, Is.EqualTo("1"));
		});
	}
}
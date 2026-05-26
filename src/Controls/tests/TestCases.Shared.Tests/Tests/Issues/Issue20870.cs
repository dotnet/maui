using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20870 : _IssuesUITest
{
	public Issue20870(TestDevice device) : base(device) { }

	public override string Issue => "Single-tap and double-tap tests";

	[Test]
	[Category(UITestCategories.Gestures)]
	public async Task TapsAndDoubleTaps()
	{
		// Initialize.
		App.WaitForElement("Results");

		// Verify a button with a single-tap gesture recognizer.
		App.Tap("ButtonSingleTap");
		await Task.Delay(200); // Do not artificially simulate a double-tap.
		App.Tap("ButtonSingleTap");

		await Task.Delay(1000);

		string? text = App.FindElement("Results").GetText()?.Trim();
		ClassicAssert.AreEqual("OnTap called|OnTap called", text);

		App.Tap("Clear");

		// Verify a button with a double-tap gesture recognizer.
		App.Tap("ButtonDoubleTap");
		await Task.Delay(200); // Do not artificially simulate a triple-tap.
		App.DoubleTap("ButtonDoubleTap");

		text = App.FindElement("Results").GetText()?.Trim();
		ClassicAssert.AreEqual("OnDoubleTap called", text);

		App.Tap("Clear");

		// Verify a button with single-tap and double-tap gesture recognizers. Note the order of recognizer registrations.
		App.Tap("ButtonSingleAndDoubleTap");
		await Task.Delay(200);
		App.DoubleTap("ButtonSingleAndDoubleTap");

		text = App.FindElement("Results").GetText()?.Trim();
		ClassicAssert.AreEqual("OnTap called|OnTap called|OnDoubleTap called", text);

		App.Tap("Clear");

		// Verify a button with double-tap and single-tap gesture recognizers. Note the order of recognizer registrations.
		App.Tap("ButtonDoubleAndSingleTap");
		await Task.Delay(200);
		App.DoubleTap("ButtonDoubleAndSingleTap");

		text = App.FindElement("Results").GetText()?.Trim();
		ClassicAssert.AreEqual("OnTap called|OnTap called|OnDoubleTap called", text);
	}
}
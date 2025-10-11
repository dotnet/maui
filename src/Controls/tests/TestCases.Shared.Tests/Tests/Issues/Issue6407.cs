using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Gestures)]
public class Issue6407 : _IssuesUITest
{
	public Issue6407(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NumberOfTapsRequired fails when greater than 2";

	[Test]
	public void SingleTapGestureShouldWork()
	{
		App.WaitForElement("SingleTapBox");
		App.Tap("SingleTapBox");
		App.WaitForElement("SingleTapCounter");
		
		var counterText = App.FindElement("SingleTapCounter").GetText();
		Assert.That(counterText, Is.EqualTo("Single taps: 1"));
	}

	[Test]
	public void DoubleTapGestureShouldWork()
	{
		App.WaitForElement("DoubleTapBox");
		App.DoubleTap("DoubleTapBox");
		App.WaitForElement("DoubleTapCounter");
		
		var counterText = App.FindElement("DoubleTapCounter").GetText();
		Assert.That(counterText, Is.EqualTo("Double taps: 1"));
	}

	[Test]
	public void TripleTapGestureShouldWork()
	{
		App.WaitForElement("TripleTapBox");
		
		// Perform three quick taps
		App.Tap("TripleTapBox");
		App.Tap("TripleTapBox");
		App.Tap("TripleTapBox");
		
		// Wait a moment for the gesture to be processed
		Task.Delay(500).Wait();
		
		App.WaitForElement("TripleTapCounter");
		var counterText = App.FindElement("TripleTapCounter").GetText();
		Assert.That(counterText, Is.EqualTo("Triple taps: 1"));
	}

	[Test]
	public void QuadrupleTapGestureShouldWork()
	{
		App.WaitForElement("QuadrupleTapBox");
		
		// Perform four quick taps
		App.Tap("QuadrupleTapBox");
		App.Tap("QuadrupleTapBox");
		App.Tap("QuadrupleTapBox");
		App.Tap("QuadrupleTapBox");
		
		// Wait a moment for the gesture to be processed
		Task.Delay(500).Wait();
		
		App.WaitForElement("QuadrupleTapCounter");
		var counterText = App.FindElement("QuadrupleTapCounter").GetText();
		Assert.That(counterText, Is.EqualTo("Quadruple taps: 1"));
	}

	[Test]
	public void QuintupleTapGestureShouldWork()
	{
		App.WaitForElement("QuintupleTapBox");
		
		// Perform five quick taps
		App.Tap("QuintupleTapBox");
		App.Tap("QuintupleTapBox");
		App.Tap("QuintupleTapBox");
		App.Tap("QuintupleTapBox");
		App.Tap("QuintupleTapBox");
		
		// Wait a moment for the gesture to be processed
		Task.Delay(500).Wait();
		
		App.WaitForElement("QuintupleTapCounter");
		var counterText = App.FindElement("QuintupleTapCounter").GetText();
		Assert.That(counterText, Is.EqualTo("Quintuple taps: 1"));
	}

	[Test]
	public void ResetCountersShouldWork()
	{
		// First, trigger some taps
		App.WaitForElement("SingleTapBox");
		App.Tap("SingleTapBox");
		App.DoubleTap("DoubleTapBox");
		
		// Reset counters
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		
		// Verify counters are reset
		var singleCounter = App.FindElement("SingleTapCounter").GetText();
		var doubleCounter = App.FindElement("DoubleTapCounter").GetText();
		
		Assert.That(singleCounter, Is.EqualTo("Single taps: 0"));
		Assert.That(doubleCounter, Is.EqualTo("Double taps: 0"));
	}
}
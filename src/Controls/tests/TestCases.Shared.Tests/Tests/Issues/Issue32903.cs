using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32903 : _IssuesUITest
	{
		public Issue32903(TestDevice device) : base(device) { }

		public override string Issue => "Slider Binding Initialization Order Causes Incorrect Value Assignment in XAML";

		[Test]
		[Category(UITestCategories.Slider)]
		public void SliderValueShouldInitializeCorrectlyWithBindings()
		{
			// Wait for the page to load
			App.WaitForElement("ValueLabel");
			
			// Get the actual slider value displayed
			var actualValueText = App.FindElement("ActualValueLabel").GetText();
			
			// The bug: Value should be 50, but due to binding order it becomes 10 (or 1)
			// Extract the numeric value from "Slider.Value: X"
			if (actualValueText == null)
			{
				Assert.Fail("ActualValueLabel text is null");
				return;
			}
			
			var valueStr = actualValueText.Replace("Slider.Value:", string.Empty, StringComparison.Ordinal).Trim();
			var actualValue = double.Parse(valueStr);
			
			// The bug manifests as Value being clamped to Minimum (10) or default Maximum (1)
			// Expected: 50
			// Actual (with bug): 10 or 1
			Console.WriteLine($"[Test] Slider.Value is: {actualValue}");
			
			// This test will FAIL with the bug (Value will be 10 or 1 instead of 50)
			// After the fix, this test should PASS (Value will be 50)
			Assert.That(actualValue, Is.EqualTo(50).Within(0.1), 
				$"Slider Value should initialize to 50, but was {actualValue}. This indicates the binding initialization order bug.");
		}
	}
}

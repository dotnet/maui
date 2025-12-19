using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6813 : _IssuesUITest
{
	public override string Issue => "Reusing the same page for formsheet Modal causes measuring issues";

	public Issue6813(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Modal)]
	public void ReusingModalPageShouldNotCauseSizingIssues()
	{
		// Start from root page
		App.WaitForElement("ShowModalButton");
		
		// Show the modal for the first time
		App.Tap("ShowModalButton");
		App.WaitForElement("StatusLabel");
		
		// Get initial modal size
		var initialSizeText = App.FindElement("SizeLabel").GetText();
		Console.WriteLine($"Initial modal size: {initialSizeText}");
		
		// Extract width and height from "Size: 393 x 851" format
		var initialSize = ParseSize(initialSizeText);
		Assert.That(initialSize.Width, Is.GreaterThan(0), "Initial width should be positive");
		Assert.That(initialSize.Height, Is.GreaterThan(0), "Initial height should be positive");
		
		// Show the modal again (reusing the same instance)
		App.Tap("ShowModalButton"); // This is "Show This Modal Again" button in the modal
		App.WaitForElement("StatusLabel");
		
		// Wait for layout to complete
		System.Threading.Thread.Sleep(1000);
		
		// Get size after second presentation
		var secondSizeText = App.FindElement("SizeLabel").GetText();
		Console.WriteLine($"Second modal size: {secondSizeText}");
		
		var secondSize = ParseSize(secondSizeText);
		
		// Show it a third time to verify consistency
		App.Tap("ShowModalButton");
		App.WaitForElement("StatusLabel");
		System.Threading.Thread.Sleep(1000);
		
		var thirdSizeText = App.FindElement("SizeLabel").GetText();
		Console.WriteLine($"Third modal size: {thirdSizeText}");
		
		var thirdSize = ParseSize(thirdSizeText);
		
		// Verify that the modal size doesn't shrink significantly
		// Allow for small variations due to animation/layout timing (±10 pixels)
		var tolerance = 10.0;
		
		Assert.That(secondSize.Width, Is.EqualTo(initialSize.Width).Within(tolerance),
			$"Second presentation width should not shrink. Initial: {initialSize.Width}, Second: {secondSize.Width}");
		Assert.That(secondSize.Height, Is.EqualTo(initialSize.Height).Within(tolerance),
			$"Second presentation height should not shrink. Initial: {initialSize.Height}, Second: {secondSize.Height}");
		
		Assert.That(thirdSize.Width, Is.EqualTo(initialSize.Width).Within(tolerance),
			$"Third presentation width should not shrink. Initial: {initialSize.Width}, Third: {thirdSize.Width}");
		Assert.That(thirdSize.Height, Is.EqualTo(initialSize.Height).Within(tolerance),
			$"Third presentation height should not shrink. Initial: {initialSize.Height}, Third: {thirdSize.Height}");
	}
	
	private (double Width, double Height) ParseSize(string sizeText)
	{
		// Parse "Size: 393 x 851" format
		var parts = sizeText.Replace("Size:", "").Trim().Split('x');
		if (parts.Length == 2 &&
		    double.TryParse(parts[0].Trim(), out var width) &&
		    double.TryParse(parts[1].Trim(), out var height))
		{
			return (width, height);
		}
		
		return (0, 0);
	}
}

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29544 : _IssuesUITest
{
	public override string Issue =>
		"PreviousItem and PreviousPosition not updating correctly on ScrollTo or Position set";

	public Issue29544(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void PreviousPositionUpdatesCorrectlyOnScrollTo()
	{
		App.WaitForElement("ScrollTo3Button");
		App.Tap("ScrollTo3Button");

		App.RetryAssert(() =>
		{
			var currentPos = App.FindElement("CurrentPositionLabel").GetText();
			Assert.That(currentPos, Does.Contain("3"),
				"CarouselView should reach position 3 after ScrollTo");
		});

		var previousPositionText = App.FindElement("PreviousPositionLabel").GetText();
		Assert.That(previousPositionText, Does.Contain("0"),
			"After scrolling from position 0 to 3, PreviousPosition should be 0");

		var previousItemText = App.FindElement("PreviousItemLabel").GetText();
		Assert.That(previousItemText, Does.Contain("Item 1"),
			"After scrolling from position 0 to 3, PreviousItem should be 'Item 1'");

		App.Tap("ScrollTo1Button");

		App.RetryAssert(() =>
		{
			var currentPos = App.FindElement("CurrentPositionLabel").GetText();
			Assert.That(currentPos, Does.Contain("1"),
				"CarouselView should reach position 1 after second ScrollTo");
		});

		var secondPreviousPosition = App.FindElement("PreviousPositionLabel").GetText();
		Assert.That(secondPreviousPosition, Does.Contain("3"),
			"After scrolling from position 3 to 1, PreviousPosition should be 3");

		var secondPreviousItem = App.FindElement("PreviousItemLabel").GetText();
		Assert.That(secondPreviousItem, Does.Contain("Item 4"),
			"After scrolling from position 3 to 1, PreviousItem should be 'Item 4'");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void PreviousPositionUpdatesCorrectlyOnSetPosition()
	{
		App.WaitForElement("SetPosition0Button");
		App.Tap("SetPosition0Button");

		App.RetryAssert(() =>
		{
			var pos = App.FindElement("CurrentPositionLabel").GetText();
			Assert.That(pos, Does.Contain("0"), "CarouselView should be at position 0");
		});

		App.Tap("SetPosition3Button");

		App.RetryAssert(() =>
		{
			var currentPos = App.FindElement("CurrentPositionLabel").GetText();
			Assert.That(currentPos, Does.Contain("3"),
				"CarouselView should reach position 3 after setting Position = 3");
		});

		var previousPositionText = App.FindElement("PreviousPositionLabel").GetText();
		Assert.That(previousPositionText, Does.Contain("0"),
			"After setting Position from 0 to 3, PreviousPosition should be 0");

		var previousItemText = App.FindElement("PreviousItemLabel").GetText();
		Assert.That(previousItemText, Does.Contain("Item 1"),
			"After setting Position from 0 to 3, PreviousItem should be 'Item 1'");
	}
}

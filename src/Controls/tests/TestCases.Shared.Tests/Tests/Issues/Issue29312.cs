using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29312 : _IssuesUITest
{
	public Issue29312(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "CarouselView2 position label and IndicatorView do not update when navigating";

	private void ResetCarouselViewToInitialState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		WaitForLabelText("PositionLabel", "1/5");
	}

	private void WaitForLabelText(string automationId, string expectedText, int timeoutMs = 3000)
	{
		var startTime = DateTime.UtcNow;
		while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
		{
			try
			{
				var element = App.FindElement(automationId);
				if (element?.GetText() == expectedText)
					return;
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is System.ArgumentException)
			{
				// Element not found yet, continue waiting
			}
			Thread.Sleep(100);
		}

		// One final check before failing
		try
		{
			var finalElement = App.FindElement(automationId);
			var actualText = finalElement?.GetText() ?? "null";
			if (actualText != expectedText)
			{
				throw new TimeoutException($"Timeout waiting for {automationId} to show '{expectedText}'. Actual: '{actualText}'");
			}
		}
		catch (Exception ex) when (!(ex is TimeoutException))
		{
			throw new TimeoutException($"Timeout waiting for {automationId} to show '{expectedText}'. Element not found.", ex);
		}
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(1)]
	public void CarouselViewInitialStateShouldBeCorrect()
	{
		App.WaitForElement("TestCarouselView");
		App.WaitForElement("PositionLabel");
		App.WaitForElement("DebugLabel");

		ResetCarouselViewToInitialState();

		var initialPosition = App.FindElement("PositionLabel");
		Assert.That(initialPosition.GetText(), Is.EqualTo("1/5"));

		var initialDebugInfo = App.FindElement("DebugLabel");
		Assert.That(initialDebugInfo.GetText(), Does.Contain("Position=0"));
		Assert.That(initialDebugInfo.GetText(), Does.Contain("CurrentItem=Item 1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(2)]
	public void CarouselViewPositionLabelShouldUpdateOnNavigation()
	{
		App.WaitForElement("TestCarouselView");
		App.WaitForElement("NextButton");

		ResetCarouselViewToInitialState();

		var initialPosition = App.FindElement("PositionLabel");
		Assert.That(initialPosition.GetText(), Is.EqualTo("1/5"));

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "2/5");

		var updatedPosition = App.FindElement("PositionLabel");
		Assert.That(updatedPosition.GetText(), Is.EqualTo("2/5"));

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "3/5");

		var thirdPosition = App.FindElement("PositionLabel");
		Assert.That(thirdPosition.GetText(), Is.EqualTo("3/5"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(3)]
	public void CarouselViewShouldNavigateBackwardCorrectly()
	{
		App.WaitForElement("TestCarouselView");
		App.WaitForElement("PreviousButton");
		App.WaitForElement("NextButton");

		ResetCarouselViewToInitialState();

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "2/5");
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "3/5");

		var positionAfterForward = App.FindElement("PositionLabel");
		Assert.That(positionAfterForward.GetText(), Is.EqualTo("3/5"));

		App.Tap("PreviousButton");
		WaitForLabelText("PositionLabel", "2/5");

		var positionAfterBackward = App.FindElement("PositionLabel");
		Assert.That(positionAfterBackward.GetText(), Is.EqualTo("2/5"));

		App.Tap("PreviousButton");
		WaitForLabelText("PositionLabel", "1/5");

		var backToStart = App.FindElement("PositionLabel");
		Assert.That(backToStart.GetText(), Is.EqualTo("1/5"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(4)]
	public void CarouselViewIndicatorViewShouldUpdateOnNavigation()
	{
		App.WaitForElement("TestCarouselView");
		App.WaitForElement("TestIndicatorView");
		App.WaitForElement("NextButton");

		ResetCarouselViewToInitialState();

		var indicatorViewInitial = App.FindElement("TestIndicatorView");
		Assert.That(indicatorViewInitial, Is.Not.Null);

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "2/5");

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "3/5");

		var indicatorViewAfterNavigation = App.FindElement("TestIndicatorView");
		Assert.That(indicatorViewAfterNavigation, Is.Not.Null);
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(5)]
	public void CarouselViewDebugInfoShouldReflectCurrentState()
	{
		App.WaitForElement("TestCarouselView");
		App.WaitForElement("DebugLabel");

		ResetCarouselViewToInitialState();

		var initialDebugInfo = App.FindElement("DebugLabel");
		Assert.That(initialDebugInfo.GetText(), Does.Contain("Position=0"));
		Assert.That(initialDebugInfo.GetText(), Does.Contain("CurrentItem=Item 1"));

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "2/5");

		var updatedDebugInfo = App.FindElement("DebugLabel");
		Assert.That(updatedDebugInfo.GetText(), Does.Contain("Position=1"));
		Assert.That(updatedDebugInfo.GetText(), Does.Contain("CurrentItem=Item 2"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(6)]
	public void CarouselViewSetCurrentItemShouldUpdatePosition()
	{
		App.WaitForElement("TestCarouselView");
		App.WaitForElement("SetCurrentItemButton");

		ResetCarouselViewToInitialState();

		var initialPosition = App.FindElement("PositionLabel");
		Assert.That(initialPosition.GetText(), Is.EqualTo("1/5"));

		App.Tap("SetCurrentItemButton");
		WaitForLabelText("PositionLabel", "3/5");

		var updatedPosition = App.FindElement("PositionLabel");
		Assert.That(updatedPosition.GetText(), Is.EqualTo("3/5"));

		var debugInfo = App.FindElement("DebugLabel");
		Assert.That(debugInfo.GetText(), Does.Contain("Position=2"));
		Assert.That(debugInfo.GetText(), Does.Contain("CurrentItem=Item 3"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(7)]
	public void CarouselViewResetButtonShouldWorkCorrectly()
	{
		App.WaitForElement("TestCarouselView");
		App.WaitForElement("ResetButton");

		ResetCarouselViewToInitialState();

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "2/5");
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "3/5");

		var positionBeforeReset = App.FindElement("PositionLabel");
		Assert.That(positionBeforeReset.GetText(), Is.EqualTo("3/5"));

		App.Tap("ResetButton");
		WaitForLabelText("PositionLabel", "1/5");

		var positionAfterReset = App.FindElement("PositionLabel");
		Assert.That(positionAfterReset.GetText(), Is.EqualTo("1/5"));

		var debugInfoAfterReset = App.FindElement("DebugLabel");
		Assert.That(debugInfoAfterReset.GetText(), Does.Contain("Position=0"));
		Assert.That(debugInfoAfterReset.GetText(), Does.Contain("CurrentItem=Item 1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(8)]
	public void CarouselViewShouldHandleBoundaryConditions()
	{
		App.WaitForElement("TestCarouselView");

		ResetCarouselViewToInitialState();

		var initialPosition = App.FindElement("PositionLabel");
		Assert.That(initialPosition.GetText(), Is.EqualTo("1/5"));

		App.Tap("PreviousButton");
		// Position should remain at 1/5 when going back from the first item
		Thread.Sleep(300); // Brief delay to allow any potential change to occur

		var positionAfterPreviousAtStart = App.FindElement("PositionLabel");
		Assert.That(positionAfterPreviousAtStart.GetText(), Is.EqualTo("1/5"));

		// Navigate to the end
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "2/5");
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "3/5");
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "4/5");
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "5/5");

		var positionAtEnd = App.FindElement("PositionLabel");
		Assert.That(positionAtEnd.GetText(), Is.EqualTo("5/5"));

		App.Tap("NextButton");
		// Position should remain at 5/5 when going forward from the last item
		Thread.Sleep(300); // Brief delay to allow any potential change to occur

		var positionAfterNextAtEnd = App.FindElement("PositionLabel");
		Assert.That(positionAfterNextAtEnd.GetText(), Is.EqualTo("5/5"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(9)]
	public void CarouselViewRapidNavigationShouldMaintainConsistency()
	{
		App.WaitForElement("TestCarouselView");

		ResetCarouselViewToInitialState();

		// Rapid navigation forward
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "2/5");
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "3/5");
		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "4/5");

		var positionAfterRapidNext = App.FindElement("PositionLabel");
		Assert.That(positionAfterRapidNext.GetText(), Is.EqualTo("4/5"));

		var debugInfoAfterRapid = App.FindElement("DebugLabel");
		Assert.That(debugInfoAfterRapid.GetText(), Does.Contain("Position=3"));

		// Rapid navigation backward
		App.Tap("PreviousButton");
		WaitForLabelText("PositionLabel", "3/5");
		App.Tap("PreviousButton");
		WaitForLabelText("PositionLabel", "2/5");

		var positionAfterRapidPrevious = App.FindElement("PositionLabel");
		Assert.That(positionAfterRapidPrevious.GetText(), Is.EqualTo("2/5"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Order(10)]
	public void CarouselViewPositionAndCurrentItemShouldStayInSync()
	{
		App.WaitForElement("TestCarouselView");

		ResetCarouselViewToInitialState();

		App.Tap("SetCurrentItemButton");
		WaitForLabelText("PositionLabel", "3/5");

		var positionLabel = App.FindElement("PositionLabel");
		var debugLabel = App.FindElement("DebugLabel");

		Assert.That(positionLabel.GetText(), Is.EqualTo("3/5"));
		Assert.That(debugLabel.GetText(), Does.Contain("Position=2"));
		Assert.That(debugLabel.GetText(), Does.Contain("CurrentItem=Item 3"));

		App.Tap("NextButton");
		WaitForLabelText("PositionLabel", "4/5");

		var updatedPositionLabel = App.FindElement("PositionLabel");
		var updatedDebugLabel = App.FindElement("DebugLabel");

		Assert.That(updatedPositionLabel.GetText(), Is.EqualTo("4/5"));
		Assert.That(updatedDebugLabel.GetText(), Does.Contain("Position=3"));
		Assert.That(updatedDebugLabel.GetText(), Does.Contain("CurrentItem=Item 4"));
	}
}
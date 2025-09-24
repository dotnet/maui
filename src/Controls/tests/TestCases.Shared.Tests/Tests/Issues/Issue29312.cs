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
		Thread.Sleep(300);
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
		Thread.Sleep(500);

		var updatedPosition = App.FindElement("PositionLabel");
		Assert.That(updatedPosition.GetText(), Is.EqualTo("2/5"));

		App.Tap("NextButton");
		Thread.Sleep(500);

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
		Thread.Sleep(500);
		App.Tap("NextButton");
		Thread.Sleep(500);

		var positionAfterForward = App.FindElement("PositionLabel");
		Assert.That(positionAfterForward.GetText(), Is.EqualTo("3/5"));

		App.Tap("PreviousButton");
		Thread.Sleep(500);

		var positionAfterBackward = App.FindElement("PositionLabel");
		Assert.That(positionAfterBackward.GetText(), Is.EqualTo("2/5"));

		App.Tap("PreviousButton");
		Thread.Sleep(500);

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
		Thread.Sleep(500);

		App.Tap("NextButton");
		Thread.Sleep(500);

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
		Thread.Sleep(500);

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
		Thread.Sleep(500);

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
		Thread.Sleep(500);
		App.Tap("NextButton");
		Thread.Sleep(500);

		var positionBeforeReset = App.FindElement("PositionLabel");
		Assert.That(positionBeforeReset.GetText(), Is.EqualTo("3/5"));

		App.Tap("ResetButton");
		Thread.Sleep(500);

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
		Thread.Sleep(500);

		var positionAfterPreviousAtStart = App.FindElement("PositionLabel");
		Assert.That(positionAfterPreviousAtStart.GetText(), Is.EqualTo("1/5"));

		for (int i = 0; i < 4; i++)
		{
			App.Tap("NextButton");
			Thread.Sleep(300);
		}

		var positionAtEnd = App.FindElement("PositionLabel");
		Assert.That(positionAtEnd.GetText(), Is.EqualTo("5/5"));

		App.Tap("NextButton");
		Thread.Sleep(500);

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

		for (int i = 0; i < 3; i++)
		{
			App.Tap("NextButton");
			Thread.Sleep(200);
		}

		var positionAfterRapidNext = App.FindElement("PositionLabel");
		Assert.That(positionAfterRapidNext.GetText(), Is.EqualTo("4/5"));

		var debugInfoAfterRapid = App.FindElement("DebugLabel");
		Assert.That(debugInfoAfterRapid.GetText(), Does.Contain("Position=3"));

		for (int i = 0; i < 2; i++)
		{
			App.Tap("PreviousButton");
			Thread.Sleep(200);
		}

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
		Thread.Sleep(500);

		var positionLabel = App.FindElement("PositionLabel");
		var debugLabel = App.FindElement("DebugLabel");

		Assert.That(positionLabel.GetText(), Is.EqualTo("3/5"));
		Assert.That(debugLabel.GetText(), Does.Contain("Position=2"));
		Assert.That(debugLabel.GetText(), Does.Contain("CurrentItem=Item 3"));

		App.Tap("NextButton");
		Thread.Sleep(500);

		var updatedPositionLabel = App.FindElement("PositionLabel");
		var updatedDebugLabel = App.FindElement("DebugLabel");

		Assert.That(updatedPositionLabel.GetText(), Is.EqualTo("4/5"));
		Assert.That(updatedDebugLabel.GetText(), Does.Contain("Position=3"));
		Assert.That(updatedDebugLabel.GetText(), Does.Contain("CurrentItem=Item 4"));
	}
}
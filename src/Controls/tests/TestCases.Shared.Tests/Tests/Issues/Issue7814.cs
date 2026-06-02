using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7814 : _IssuesUITest
{
	const string CarouselViewId = "Issue7814CarouselView";
	const string HorizontalScrollViewId = "Issue7814HorizontalScrollView";
	const string OuterScrollViewId = "Issue7814OuterScrollView";
	const string VerticalOffsetLabelId = "Issue7814VerticalScrollYLabel";
	const string HorizontalOffsetLabelId = "Issue7814HorizontalScrollXLabel";

	public Issue7814(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Vertical scrolling not working for CarouselView and CustomLayouts";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerticalScrollFromCarouselWorksAfterHorizontalScrollViewGesture()
	{
		if (App is not AppiumAndroidApp)
		{
			Assert.Ignore("Issue 7814 is Android-specific.");
		}

		App.WaitForElement(CarouselViewId);
		App.WaitForElement(HorizontalScrollViewId);

		var initialVerticalOffset = GetVerticalOffset();
		var initialCarouselRect = GetVisibleRect(CarouselViewId);
		DragWithinRect(initialCarouselRect, 0.5, 0.65, 0.5, 0.35);

		App.RetryAssert(() =>
		{
			Assert.That(GetVerticalOffset(), Is.GreaterThan(initialVerticalOffset + 20));
		});

		var carouselRectForReturnGesture = GetVisibleRect(CarouselViewId);
		var horizontalOffsetBeforeGesture = GetHorizontalOffset();
		DragWithinVisibleArea(HorizontalScrollViewId, 0.85, 0.5, 0.15, 0.5);

		App.RetryAssert(() =>
		{
			Assert.That(GetHorizontalOffset(), Is.GreaterThan(horizontalOffsetBeforeGesture + 20));
		});

		var verticalOffsetBeforeReturnGesture = GetVerticalOffset();
		DragWithinRect(carouselRectForReturnGesture, 0.5, 0.15, 0.5, 0.85);

		App.RetryAssert(() =>
		{
			Assert.That(GetVerticalOffset(), Is.LessThan(verticalOffsetBeforeReturnGesture - 20));
		});
	}

	void DragWithinVisibleArea(string automationId, double fromXRatio, double fromYRatio, double toXRatio, double toYRatio)
	{
		var visibleRect = GetVisibleRect(automationId);
		DragWithinRect(visibleRect, fromXRatio, fromYRatio, toXRatio, toYRatio);
	}

	void DragWithinRect(Rectangle visibleRect, double fromXRatio, double fromYRatio, double toXRatio, double toYRatio)
	{
		var fromX = GetCoordinate(visibleRect.Left, visibleRect.Width, fromXRatio);
		var fromY = GetCoordinate(visibleRect.Top, visibleRect.Height, fromYRatio);
		var toX = GetCoordinate(visibleRect.Left, visibleRect.Width, toXRatio);
		var toY = GetCoordinate(visibleRect.Top, visibleRect.Height, toYRatio);

		App.DragCoordinates(fromX, fromY, toX, toY);
	}

	Rectangle GetVisibleRect(string automationId)
	{
		var elementRect = App.WaitForElement(automationId).GetRect();
		var viewportRect = App.WaitForElement(OuterScrollViewId).GetRect();
		var visibleRect = Rectangle.Intersect(elementRect, viewportRect);

		Assert.That(visibleRect.Width, Is.GreaterThan(40), $"{automationId} should be horizontally visible.");
		Assert.That(visibleRect.Height, Is.GreaterThan(40), $"{automationId} should be vertically visible.");

		return visibleRect;
	}

	static float GetCoordinate(int start, int length, double ratio)
	{
		return (float)(start + (length * ratio));
	}

	int GetVerticalOffset() => GetOffset(VerticalOffsetLabelId);

	int GetHorizontalOffset() => GetOffset(HorizontalOffsetLabelId);

	int GetOffset(string automationId)
	{
		var text = App.FindElement(automationId).GetText();
		var separatorIndex = text?.LastIndexOf(':') ?? -1;

		Assert.That(separatorIndex, Is.GreaterThanOrEqualTo(0), $"Offset label {automationId} should contain a ':' separator.");

		return int.Parse(text![(separatorIndex + 1)..].Trim(), CultureInfo.InvariantCulture);
	}
}

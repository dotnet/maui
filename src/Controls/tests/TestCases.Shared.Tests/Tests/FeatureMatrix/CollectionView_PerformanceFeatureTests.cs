using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class CollectionView_PerformanceFeatureTests : _GalleryUITest
{
	public const string PerformanceMatrix = "CollectionView Performance Matrix";

	public override string GalleryPageName => PerformanceMatrix;

	public CollectionView_PerformanceFeatureTests(TestDevice device)
		: base(device)
	{
	}

	#region Simple Template Performance Tests

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void VerifySimpleTemplateLoad100Items()
	{
		App.WaitForElement("SimpleTemplateButton");
		App.Tap("SimpleTemplateButton");
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("StatusLabel");
		App.WaitForElement("PerformanceCollectionView");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySimpleTemplateLoad1000Items()
	{
		App.WaitForElement("Load1000Button");
		App.Tap("Load1000Button");
		App.WaitForElement("StatusLabel");
		App.WaitForElement("PerformanceCollectionView");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySimpleTemplateClearItems()
	{
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("ClearButton");
		App.Tap("ClearButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySimpleTemplateScrollToEnd()
	{
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("ScrollEndButton");
		App.Tap("ScrollEndButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	#endregion

	#region Complex Template Performance Tests

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyComplexTemplateLoad100Items()
	{
		App.WaitForElement("Back");
		App.Tap("Back");
		App.WaitForElement("ComplexTemplateButton");
		App.Tap("ComplexTemplateButton");
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("PerformanceCollectionView");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyComplexTemplateLoad1000Items()
	{
		App.WaitForElement("Load1000Button");
		App.Tap("Load1000Button");
		App.WaitForElement("StatusLabel");
		App.WaitForElement("PerformanceCollectionView");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyComplexTemplateClearItems()
	{
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("ClearButton");
		App.Tap("ClearButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyComplexTemplateScrollToEnd()
	{
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("ScrollEndButton");
		App.Tap("ScrollEndButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	#endregion

	#region Scrolling Performance Tests

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollingPerformanceSlowScroll()
	{
		App.WaitForElement("Back");
		App.Tap("Back");
		App.WaitForElement("ScrollingPerformanceButton");
		App.Tap("ScrollingPerformanceButton");
		App.WaitForElement("SlowScrollButton");
		App.Tap("SlowScrollButton");
		App.WaitForElement("FpsLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollingPerformanceFastScroll()
	{
		App.WaitForElement("FastScrollButton");
		App.Tap("FastScrollButton");
		App.WaitForElement("FpsLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollingPerformanceJumpScroll()
	{
		App.WaitForElement("JumpScrollButton");
		App.Tap("JumpScrollButton");
		App.WaitForElement("FpsLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollingPerformanceLoad1000()
	{
		App.WaitForElement("Load1000Button");
		App.Tap("Load1000Button");
		App.WaitForElement("StatusLabel");
		App.WaitForElement("PerformanceCollectionView");
		VerifyScreenshot();
	}

	#endregion

	#region Grid Span Performance Tests

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGridSpanPerformanceWithSpan2()
	{
		App.WaitForElement("Back");
		App.Tap("Back");
		App.WaitForElement("GridSpanButton");
		App.Tap("GridSpanButton");
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("PerformanceCollectionView");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGridSpanPerformanceWithSpan3()
	{
		App.WaitForElement("Span3Button");
		App.Tap("Span3Button");
		App.WaitForElement("SpanLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGridSpanPerformanceWithSpan5()
	{
		App.WaitForElement("Span5Button");
		App.Tap("Span5Button");
		App.WaitForElement("SpanLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGridSpanPerformanceWith1000Items()
	{
		App.WaitForElement("Span2Button");
		App.Tap("Span2Button");
		App.WaitForElement("Load1000Button");
		App.Tap("Load1000Button");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	#endregion

	#region Grouping Orientation Performance Tests

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupingPerformanceVertical5Groups()
	{
		App.WaitForElement("Back");
		App.Tap("Back");
		App.WaitForElement("GroupingOrientationButton");
		App.Tap("GroupingOrientationButton");
		App.WaitForElement("Load5GroupsButton");
		App.Tap("Load5GroupsButton");
		App.WaitForElement("PerformanceCollectionView");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupingPerformanceVertical20Groups()
	{
		App.WaitForElement("Load20GroupsButton");
		App.Tap("Load20GroupsButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupingPerformanceHorizontalOrientation()
	{
		App.WaitForElement("Load5GroupsButton");
		App.Tap("Load5GroupsButton");
		App.WaitForElement("HorizontalButton");
		App.Tap("HorizontalButton");
		App.WaitForElement("OrientationLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupingPerformanceToggleGroupingOff()
	{
		App.WaitForElement("VerticalButton");
		App.Tap("VerticalButton");
		App.WaitForElement("Load5GroupsButton");
		App.Tap("Load5GroupsButton");
		App.WaitForElement("GroupOffButton");
		App.Tap("GroupOffButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupingPerformanceLoad1000Items()
	{
		App.WaitForElement("GroupOnButton");
		App.Tap("GroupOnButton");
		App.WaitForElement("Load1000Button");
		App.Tap("Load1000Button");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupingPerformanceClearItems()
	{
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("ClearButton");
		App.Tap("ClearButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupingPerformanceScrollToEnd()
	{
		App.WaitForElement("Load100Button");
		App.Tap("Load100Button");
		App.WaitForElement("ScrollEndButton");
		App.Tap("ScrollEndButton");
		App.WaitForElement("StatusLabel");
		VerifyScreenshot();
	}

	#endregion
}

using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ScrollViewFeatureTests : UITest
{
	public const string ScrollViewFeatureMatrix = "ScrollView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string ContentLabel = "ContentLabel";
	public const string ContentImage = "ContentImage";
	public const string ContentEditor = "ContentEditor";
	public const string ContentVerticalStackLayout = "ContentVerticalStackLayout";
	public const string ContentHorizontalStackLayout = "ContentHorizontalStackLayout";
	public const string ContentGrid = "ContentGrid";
	public const string ContentAbsoluteLayout = "ContentAbsoluteLayout";
	public const string OrientationVertical = "OrientationVertical";
	public const string OrientationHorizontal = "OrientationHorizontal";
	public const string OrientationBoth = "OrientationBoth";
	public const string OrientationNeither = "OrientationNeither";
	public const string FlowDirectionRTL = "FlowDirectionRTL";
	public const string ShadowTrue = "ShadowTrue";
	public const string IsVisibleFalse = "IsVisibleFalse";
	public const string IsEnabledFalse = "IsEnabledFalse";
	public const string ScrollToPositionEntry = "ScrollToPositionEntry";
	public const string ScrollToPositionButton = "ScrollToPositionButton";

	public ScrollViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ScrollViewFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEnd()
	{
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStart()
	{
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenter()
	{
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisible()
	{
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(5)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(6)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndEditorContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndEditorContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndEditorContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(12)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndEditorContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(13)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(21)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndAbsoluteLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndAbsoluteLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(23)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndAbsoluteLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(24)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndAbsoluteContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(25)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndVerticalLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentVerticalStackLayout);
		App.Tap(ContentVerticalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(26)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndVerticalLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentVerticalStackLayout);
		App.Tap(ContentVerticalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(27)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndVerticalLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentVerticalStackLayout);
		App.Tap(ContentVerticalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndVerticalLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentVerticalStackLayout);
		App.Tap(ContentVerticalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(29)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndEditorContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(30)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndEditorContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(31)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndEditorContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(32)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndEditorContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(33)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndImageContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(34)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndImageContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(35)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndImageContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(36)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndImageContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(37)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndGridContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(38)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndGridContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(39)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndGridContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(40)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndGridContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(41)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndAbsoluteLayoutContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(42)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndAbsoluteLayoutContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test , Order(43)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndAbsoluteLayoutContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test , Order(44)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndAbsoluteContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(45)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionEndAndHorizontalLayoutContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "end");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(46)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionStartAndHorizontalLayoutContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "start");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(47)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionCenterAndHorizontalLayoutontentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "center");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(48)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollToPositionMakeVisibleAndHorizontalLayoutContentWhenOrientationHorizontal()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ScrollToPositionEntry);
		App.ClearText(ScrollToPositionEntry);
		App.EnterText(ScrollToPositionEntry, "makevisible");
		App.WaitForElement(ScrollToPositionButton);
		App.Tap(ScrollToPositionButton);
		App.WaitForElement("ScrollViewContent");
		VerifyScreenshot();
	}

	[Test, Order(49)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollX()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.Not.EqualTo("0"), "ScrollX should not be zero after scrolling right");
	}

	[Test, Order(50)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithScrollY()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentVerticalStackLayout);
		App.Tap(ContentVerticalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.Not.EqualTo("0"), "ScrollY should not be zero after scrolling down");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithContentLabel()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentLabel);
		App.Tap(ContentLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewContent");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.GreaterThan("0"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithContentGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewContent");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.GreaterThan("0"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithContentEditor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewContent");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.GreaterThan("0"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithContentVerticalStackLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentVerticalStackLayout);
		App.Tap(ContentVerticalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewContent");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.GreaterThan("0"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewContentAbsoluteLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewContent");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.GreaterThan("0"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewContent");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.GreaterThan("0"));
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //Issue Link: https://github.com/dotnet/maui/issues/30070
	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithHorizontalOrientationAndLabelContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"));
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithHorizontalOrientationAndImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.WaitForElement("ScrollViewContent");
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Image content");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithHorizontalOrientationAndEditorContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.WaitForElement("ScrollViewContent");
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Editor content");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithHorizontalOrientationAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.WaitForElement("ScrollViewContent");
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Grid content");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithHorizontalOrientationAndHorizontalStackLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.WaitForElement("ScrollViewContent");
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with HorizontalStackLayout content");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithHorizontalOrientationAndAbsoluteContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.WaitForElement("ScrollViewContent");
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with AbsoluteLayout content");
	}
#endif

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationBothAndAbsoluteLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(OrientationBoth);
		App.Tap(OrientationBoth);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Orientation Both");
		Assert.That(scrollY, Is.GreaterThan("0"), "ScrollY should not be zero after scrolling down with Orientation Both");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationBothAndLabelContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentLabel);
		App.Tap(ContentLabel);
		App.WaitForElement(OrientationBoth);
		App.Tap(OrientationBoth);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Orientation Both");
		Assert.That(scrollY, Is.GreaterThan("0"), "ScrollY should not be zero after scrolling down with Orientation Both");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationBothAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(OrientationBoth);
		App.Tap(OrientationBoth);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Orientation Both");
		Assert.That(scrollY, Is.GreaterThan("0"), "ScrollY should not be zero after scrolling down with Orientation Both");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationBothImage()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(OrientationBoth);
		App.Tap(OrientationBoth);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Orientation Both");
		Assert.That(scrollY, Is.GreaterThan("0"), "ScrollY should not be zero after scrolling down with Orientation Both");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationBothWhenContentEditor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(OrientationBoth);
		App.Tap(OrientationBoth);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollX, Is.GreaterThan("0"), "ScrollX should not be zero after scrolling right with Orientation Both");
		Assert.That(scrollY, Is.GreaterThan("0"), "ScrollY should not be zero after scrolling down with Orientation Both");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationNeitherAndContentLabel()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentLabel);
		App.Tap(ContentLabel);
		App.WaitForElement(OrientationNeither);
		App.Tap(OrientationNeither);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
        App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationNeitherAndContentImage()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(OrientationNeither);
		App.Tap(OrientationNeither);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
        App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationNeitherAndAbsoluteLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(OrientationNeither);
		App.Tap(OrientationNeither);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationNeitherAndEditor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(OrientationNeither);
		App.Tap(OrientationNeither);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationNeitherAndVerticalLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentVerticalStackLayout);
		App.Tap(ContentVerticalStackLayout);
		App.WaitForElement(OrientationNeither);
		App.Tap(OrientationNeither);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationNeitherAndHorizontalLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(OrientationNeither);
		App.Tap(OrientationNeither);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithOrientationNeitherAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(OrientationNeither);
		App.Tap(OrientationNeither);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		App.ScrollRight("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		var scrollX = App.WaitForElement("ScrollXLabel").GetText();
		Assert.That(scrollX, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when Orientation is Neither");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ShadowTrue);
		App.Tap(ShadowTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithIsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsVisibleFalse);
		App.Tap(IsVisibleFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement("ScrollViewControl");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithIsEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsEnabledFalse);
		App.Tap(IsEnabledFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewContent");
		App.ScrollDown("ScrollViewControl", ScrollStrategy.Gesture, 0.99);
		var scrollY = App.WaitForElement("ScrollYLabel").GetText();
		Assert.That(scrollY, Is.EqualTo("0"), "Scrolling Does not work when IsEnabled is false");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLAndImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLAndEditor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLAndHorizontalLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLAndAbsoluteLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS// Issue Link for Android: https://github.com/dotnet/maui/issues/30081 and Issue Link For iOS: https://github.com/dotnet/maui/issues/30070
	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLWhenHorizontalOrientation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLWhenHorizontalOrientationAndImageContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentImage);
		App.Tap(ContentImage);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLWhenHorizontalOrientationAndGridContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentGrid);
		App.Tap(ContentGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLWhenHorizontalOrientationAndContentEditor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentEditor);
		App.Tap(ContentEditor);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLWhenHorizontalOrientationAndHorizontalLayoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentHorizontalStackLayout);
		App.Tap(ContentHorizontalStackLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewWithRTLWhenHorizontalOrientationLAndAbsoluteLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(OrientationHorizontal);
		App.Tap(OrientationHorizontal);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ContentAbsoluteLayout);
		App.Tap(ContentAbsoluteLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ScrollViewControl");
		VerifyScreenshot();
	}
#endif
}
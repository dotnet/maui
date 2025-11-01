using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Layout)]
public class FlexLayoutFeatureTests : UITest
{
	public const string FlexLayoutFeatureMatrix = "FlexLayout Feature Matrix";

	public FlexLayoutFeatureTests(TestDevice device) : base(device) { }

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(FlexLayoutFeatureMatrix);
	}

	[Test, Order(1)]
	public void FlexLayout_ValidateDefaultValues()
	{
		App.WaitForElement("AlignContentLabel");
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("Stretch"));
		Assert.That(App.FindElement("AlignItemsLabel").GetText(), Is.EqualTo("Stretch"));
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Row"));
		Assert.That(App.FindElement("JustifyContentLabel").GetText(), Is.EqualTo("Start"));
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("NoWrap"));
		Assert.That(App.FindElement("Child1AlignSelfLabel").GetText(), Is.EqualTo("Auto"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void FlexLayout_SetWrapAlignContentStretch()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("AlignContentStretchButton");
		App.Tap("AlignContentStretchButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("AddChildrenButton");
		App.Tap("AddChildrenButton");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("Stretch"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void FlexLayout_SetWrapAlignContentCenter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("AlignContentCenterButton");
		App.Tap("AlignContentCenterButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("Center"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void FlexLayout_SetWrapAlignContentStart()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("AlignContentStartButton");
		App.Tap("AlignContentStartButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("Start"));
		VerifyScreenshot();
	}

	[Test, Order(5)]
	public void FlexLayout_SetWrapAlignContentEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("AlignContentEndButton");
		App.Tap("AlignContentEndButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("End"));
		VerifyScreenshot();
	}

	[Test, Order(6)]
	public void FlexLayout_SetWrapAlignContentSpaceAround()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("AlignContentSpaceAroundButton");
		App.Tap("AlignContentSpaceAroundButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("SpaceAround"));
		VerifyScreenshot();
	}

	[Test, Order(7)]
	public void FlexLayout_SetWrapAlignContentSpaceBetween()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("AlignContentSpaceBetweenButton");
		App.Tap("AlignContentSpaceBetweenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("SpaceBetween"));
		VerifyScreenshot();
	}

	[Test, Order(8)]
	public void FlexLayout_SetWrapAlignContentSpaceEvenly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("AlignContentSpaceEvenlyButton");
		App.Tap("AlignContentSpaceEvenlyButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("SpaceEvenly"));
		VerifyScreenshot();
	}

	[Test, Order(9)]
	public void FlexLayout_SetWrapReverseAlignContentStretch()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapReverseButton");
		App.Tap("WrapReverseButton");
		App.WaitForElement("AlignContentStretchButton");
		App.Tap("AlignContentStretchButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Reverse"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("Stretch"));
		VerifyScreenshot();
	}

	[Test, Order(10)]
	public void FlexLayout_SetWrapReverseAlignContentCenter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapReverseButton");
		App.Tap("WrapReverseButton");
		App.WaitForElement("AlignContentCenterButton");
		App.Tap("AlignContentCenterButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Reverse"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("Center"));
		VerifyScreenshot();
	}

	[Test, Order(11)]
	public void FlexLayout_SetWrapReverseAlignContentStart()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapReverseButton");
		App.Tap("WrapReverseButton");
		App.WaitForElement("AlignContentStartButton");
		App.Tap("AlignContentStartButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Reverse"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("Start"));
		VerifyScreenshot();
	}

	[Test, Order(12)]
	public void FlexLayout_SetWrapReverseAlignContentEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WrapReverseButton");
		App.Tap("WrapReverseButton");
		App.WaitForElement("AlignContentEndButton");
		App.Tap("AlignContentEndButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Reverse"));
		Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("End"));
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/31565

    [Test, Order(13)]
    public void FlexLayout_SetWrapReverseAlignContentSpaceAround()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("WrapReverseButton");
        App.Tap("WrapReverseButton");
        App.WaitForElement("AlignContentSpaceAroundButton");
        App.Tap("AlignContentSpaceAroundButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
        Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Reverse"));
        Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("SpaceAround"));
        VerifyScreenshot();
    }

    [Test, Order(14)]
    public void FlexLayout_SetWrapReverseAlignContentSpaceBetween()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("WrapReverseButton");
        App.Tap("WrapReverseButton");
        App.WaitForElement("AlignContentSpaceBetweenButton");
        App.Tap("AlignContentSpaceBetweenButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
        Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Reverse"));
        Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("SpaceBetween"));
        VerifyScreenshot();
    }

    [Test, Order(15)]
    public void FlexLayout_SetWrapReverseAlignContentSpaceEvenly()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("WrapReverseButton");
        App.Tap("WrapReverseButton");
        App.WaitForElement("AlignContentSpaceEvenlyButton");
        App.Tap("AlignContentSpaceEvenlyButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("AlignContentLabel");
        Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Reverse"));
        Assert.That(App.FindElement("AlignContentLabel").GetText(), Is.EqualTo("SpaceEvenly"));
        VerifyScreenshot();
    }
#endif

	[Test, Order(16)]
	public void FlexLayout_AlignItemsCenter()
	{
		App.WaitForElement("RemoveChildrenButton");
		App.Tap("RemoveChildrenButton");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AlignItemsCenterButton");
		App.Tap("AlignItemsCenterButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignItemsLabel");
		Assert.That(App.FindElement("AlignItemsLabel").GetText(), Is.EqualTo("Center"));
		VerifyScreenshot();
	}

	[Test, Order(17)]
	public void FlexLayout_AlignItemsEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AlignItemsEndButton");
		App.Tap("AlignItemsEndButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignItemsLabel");
		Assert.That(App.FindElement("AlignItemsLabel").GetText(), Is.EqualTo("End"));
		VerifyScreenshot();
	}

	[Test, Order(18)]
	public void FlexLayout_AlignItemsStretch()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AlignItemsStretchButton");
		App.Tap("AlignItemsStretchButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignItemsLabel");
		Assert.That(App.FindElement("AlignItemsLabel").GetText(), Is.EqualTo("Stretch"));
		VerifyScreenshot();
	}

	[Test, Order(19)]
	public void FlexLayout_DirectionRowReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionRowReverseButton");
		App.Tap("DirectionRowReverseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DirectionLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("RowReverse"));
		VerifyScreenshot();
	}

	[Test, Order(20)]
	public void FlexLayout_DirectionColumn()
	{
#if ANDROID || IOS
		App.WaitForElement("RemoveChildrenButton");
		App.Tap("RemoveChildrenButton");
#endif
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnButton");
		App.Tap("DirectionColumnButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DirectionLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Column"));
		VerifyScreenshot();
	}

	[Test, Order(21)]
	public void FlexLayout_DirectionColumnReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnReverseButton");
		App.Tap("DirectionColumnReverseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DirectionLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("ColumnReverse"));
		VerifyScreenshot();
	}

	[Test, Order(22)]
	public void FlexLayout_JustifyContentCenter()
	{
#if ANDROID || IOS
		App.WaitForElement("AddChildrenButton");
		App.Tap("AddChildrenButton");
#endif
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("JustifyContentCenterButton");
		App.Tap("JustifyContentCenterButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("JustifyContentLabel");
		Assert.That(App.FindElement("JustifyContentLabel").GetText(), Is.EqualTo("Center"));
		VerifyScreenshot();
	}

	[Test, Order(23)]
	public void FlexLayout_JustifyContentEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("JustifyContentEndButton");
		App.Tap("JustifyContentEndButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("JustifyContentLabel");
		Assert.That(App.FindElement("JustifyContentLabel").GetText(), Is.EqualTo("End"));
		VerifyScreenshot();
	}

	[Test, Order(24)]
	public void FlexLayout_JustifyContentSpaceBetween()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("JustifyContentSpaceBetweenButton");
		App.Tap("JustifyContentSpaceBetweenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("JustifyContentLabel");
		Assert.That(App.FindElement("JustifyContentLabel").GetText(), Is.EqualTo("SpaceBetween"));
		VerifyScreenshot();
	}

	[Test, Order(25)]
	public void FlexLayout_JustifyContentSpaceAround()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("JustifyContentSpaceAroundButton");
		App.Tap("JustifyContentSpaceAroundButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("JustifyContentLabel");
		Assert.That(App.FindElement("JustifyContentLabel").GetText(), Is.EqualTo("SpaceAround"));
		VerifyScreenshot();
	}

	[Test, Order(26)]
	public void FlexLayout_JustifyContentSpaceEvenly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("JustifyContentSpaceEvenlyButton");
		App.Tap("JustifyContentSpaceEvenlyButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("JustifyContentLabel");
		Assert.That(App.FindElement("JustifyContentLabel").GetText(), Is.EqualTo("SpaceEvenly"));
		VerifyScreenshot();
	}

	[Test, Order(27)]
	public void FlexLayout_AlignSelfStart()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Child1AlignSelfStartButton");
		App.Tap("Child1AlignSelfStartButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1AlignSelfLabel");
		Assert.That(App.FindElement("Child1AlignSelfLabel").GetText(), Is.EqualTo("Start"));
		VerifyScreenshot();
	}

	[Test, Order(28)]
	public void FlexLayout_AlignSelfCenter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Child1AlignSelfCenterButton");
		App.Tap("Child1AlignSelfCenterButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1AlignSelfLabel");
		Assert.That(App.FindElement("Child1AlignSelfLabel").GetText(), Is.EqualTo("Center"));
		VerifyScreenshot();
	}

	[Test, Order(29)]
	public void FlexLayout_AlignSelfEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Child1AlignSelfEndButton");
		App.Tap("Child1AlignSelfEndButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1AlignSelfLabel");
		Assert.That(App.FindElement("Child1AlignSelfLabel").GetText(), Is.EqualTo("End"));
		VerifyScreenshot();
	}

	[Test, Order(30)]
	public void FlexLayout_AlignSelfStretch()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Child1AlignSelfStretchButton");
		App.Tap("Child1AlignSelfStretchButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1AlignSelfLabel");
		Assert.That(App.FindElement("Child1AlignSelfLabel").GetText(), Is.EqualTo("Stretch"));
		VerifyScreenshot();
	}

	[Test, Order(31)]
	public void FlexLayout_Child1Grow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Child1GrowEntry");
		App.ClearText("Child1GrowEntry");
		App.EnterText("Child1GrowEntry", "100");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1GrowLabel");
		Assert.That(App.FindElement("Child1GrowLabel").GetText(), Is.EqualTo("100"));
		VerifyScreenshot();
	}

	[Test, Order(32)]
	public void FlexLayout_Child1Shrink()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ChildShrinkEntry");
		App.ClearText("ChildShrinkEntry");
		App.EnterText("ChildShrinkEntry", "10");
		App.WaitForElement("HeightAndWidthRequestButton");
		App.Tap("HeightAndWidthRequestButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
#if MACCATALYST || WINDOWS
        App.WaitForElement("AddChildrenButton");
        App.Tap("AddChildrenButton");
#endif
		App.WaitForElementTillPageNavigationSettled("ChildShrinkLabel");
		Assert.That(App.FindElement("ChildShrinkLabel").GetText(), Is.EqualTo("10"));
		VerifyScreenshot();
	}

	[Test, Order(33)]
	public void FlexLayout_Child1Order()
	{
#if MACCATALYST || WINDOWS
        App.WaitForElement("RemoveChildrenButton");
        App.Tap("RemoveChildrenButton");
#endif
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Child1OrderEntry");
		App.ClearText("Child1OrderEntry");
		App.EnterText("Child1OrderEntry", "4");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1OrderLabel");
		Assert.That(App.FindElement("Child1OrderLabel").GetText(), Is.EqualTo("4"));
		VerifyScreenshot();
	}

	[Test, Order(34)]
	public void FlexLayout_BasisAuto_DirectionRow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionRowButton");
		App.Tap("DirectionRowButton");
		App.WaitForElement("Child1BasisAutoButton");
		App.Tap("Child1BasisAutoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Row"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Auto"));
		VerifyScreenshot();
	}

	[Test, Order(35)]
	public void FlexLayout_BasisFixed_DirectionRow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionRowButton");
		App.Tap("DirectionRowButton");
		App.WaitForElement("Child1BasisFixed100Button");
		App.Tap("Child1BasisFixed100Button");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Row"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Fixed100"));
		VerifyScreenshot();
	}

	[Test, Order(36)]
	public void FlexLayout_BasisPercentage_DirectionRow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionRowButton");
		App.Tap("DirectionRowButton");
		App.WaitForElement("Child1BasisPercent50Button");
		App.Tap("Child1BasisPercent50Button");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Row"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Percent50"));
		VerifyScreenshot();
	}

	[Test, Order(37)]
	public void FlexLayout_BasisAuto_DirectionRowReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionRowReverseButton");
		App.Tap("DirectionRowReverseButton");
		App.WaitForElement("Child1BasisAutoButton");
		App.Tap("Child1BasisAutoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("RowReverse"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Auto"));
		VerifyScreenshot();
	}

	[Test, Order(38)]
	public void FlexLayout_BasisFixed_DirectionRowReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionRowReverseButton");
		App.Tap("DirectionRowReverseButton");
		App.WaitForElement("Child1BasisFixed100Button");
		App.Tap("Child1BasisFixed100Button");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("RowReverse"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Fixed100"));
		VerifyScreenshot();
	}

	[Test, Order(39)]
	public void FlexLayout_BasisPercentage_DirectionRowReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionRowReverseButton");
		App.Tap("DirectionRowReverseButton");
		App.WaitForElement("Child1BasisPercent50Button");
		App.Tap("Child1BasisPercent50Button");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("RowReverse"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Percent50"));
		VerifyScreenshot();
	}

	[Test, Order(40)]
	public void FlexLayout_BasisAuto_DirectionColumn()
	{
#if ANDROID || IOS
		App.WaitForElement("RemoveChildrenButton");
		App.Tap("RemoveChildrenButton");
#endif
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnButton");
		App.Tap("DirectionColumnButton");
		App.WaitForElement("Child1BasisAutoButton");
		App.Tap("Child1BasisAutoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Column"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Auto"));
		VerifyScreenshot();
	}

	[Test, Order(41)]
	public void FlexLayout_BasisFixed_DirectionColumn()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnButton");
		App.Tap("DirectionColumnButton");
		App.WaitForElement("Child1BasisFixed100Button");
		App.Tap("Child1BasisFixed100Button");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Column"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Fixed100"));
		VerifyScreenshot();
	}

	[Test, Order(42)]
	public void FlexLayout_BasisPercentage_DirectionColumn()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnButton");
		App.Tap("DirectionColumnButton");
		App.WaitForElement("Child1BasisPercent50Button");
		App.Tap("Child1BasisPercent50Button");
#if MACCATALYST || WINDOWS
        App.WaitForElement("SpecificHeightButton");
        App.Tap("SpecificHeightButton");
#endif
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("Column"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Percent50"));
		VerifyScreenshot();
	}

	[Test, Order(43)]
	public void FlexLayout_BasisAuto_DirectionColumnReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnReverseButton");
		App.Tap("DirectionColumnReverseButton");
		App.WaitForElement("Child1BasisAutoButton");
		App.Tap("Child1BasisAutoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("ColumnReverse"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Auto"));
		VerifyScreenshot();
	}

	[Test, Order(44)]
	public void FlexLayout_BasisFixed_DirectionColumnReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnReverseButton");
		App.Tap("DirectionColumnReverseButton");
		App.WaitForElement("Child1BasisFixed100Button");
		App.Tap("Child1BasisFixed100Button");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("ColumnReverse"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Fixed100"));
		VerifyScreenshot();
	}

	[Test, Order(45)]
	public void FlexLayout_BasisPercentage_DirectionColumnReverse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DirectionColumnReverseButton");
		App.Tap("DirectionColumnReverseButton");
		App.WaitForElement("Child1BasisPercent50Button");
		App.Tap("Child1BasisPercent50Button");
#if MACCATALYST || WINDOWS
        App.WaitForElement("SpecificHeightButton");
        App.Tap("SpecificHeightButton");
#endif
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Child1BasisLabel");
		Assert.That(App.FindElement("DirectionLabel").GetText(), Is.EqualTo("ColumnReverse"));
		Assert.That(App.FindElement("Child1BasisLabel").GetText(), Is.EqualTo("Percent50"));
		VerifyScreenshot();
	}

	[Test, Order(46)]
	public void FlexLayout_SetAlignItemsCenterWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AlignItemsCenterButton");
		App.Tap("AlignItemsCenterButton");
		App.WaitForElement("HeightAndWidthRequestButton");
		App.Tap("HeightAndWidthRequestButton");
		App.WaitForElement("SpecificHeightButton");
		App.Tap("SpecificHeightButton");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignItemsLabel");
		Assert.That(App.FindElement("AlignItemsLabel").GetText(), Is.EqualTo("Center"));
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		VerifyScreenshot();
	}

	[Test, Order(47)]
	public void FlexLayout_SetAlignItemsEndWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AlignItemsEndButton");
		App.Tap("AlignItemsEndButton");
		App.WaitForElement("HeightAndWidthRequestButton");
		App.Tap("HeightAndWidthRequestButton");
		App.WaitForElement("SpecificHeightButton");
		App.Tap("SpecificHeightButton");
		App.WaitForElement("WrapWrapButton");
		App.Tap("WrapWrapButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("AlignItemsLabel");
		Assert.That(App.FindElement("AlignItemsLabel").GetText(), Is.EqualTo("End"));
		Assert.That(App.FindElement("WrapLabel").GetText(), Is.EqualTo("Wrap"));
		VerifyScreenshot();
	}
}
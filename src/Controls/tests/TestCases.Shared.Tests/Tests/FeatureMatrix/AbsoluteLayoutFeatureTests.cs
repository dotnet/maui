using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class AbsoluteLayoutFeatureTests : UITest
{
	public const string AbsoluteLayoutFeatureMatrix = "AbsoluteLayout Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string XEntry = "XEntry";
	public const string YEntry = "YEntry";
	public const string WidthEntry = "WidthEntry";
	public const string HeightEntry = "HeightEntry";
	public const string LayoutFlagNoneCheckBox = "LayoutFlagNoneCheckBox";
	public const string LayoutFlagXProportionalCheckBox = "LayoutFlagXProportionalCheckBox";
	public const string LayoutFlagYProportionalCheckBox = "LayoutFlagYProportionalCheckBox";
	public const string LayoutFlagWidthProportionalCheckBox = "LayoutFlagWidthProportionalCheckBox";
	public const string LayoutFlagHeightProportionalCheckBox = "LayoutFlagHeightProportionalCheckBox";
	public const string LayoutFlagPositionProportionalCheckBox = "LayoutFlagPositionProportionalCheckBox";
	public const string LayoutFlagSizeProportionalCheckBox = "LayoutFlagSizeProportionalCheckBox";
	public const string LayoutFlagAllCheckBox = "LayoutFlagAllCheckBox";

	public AbsoluteLayoutFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(AbsoluteLayoutFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_LayoutBounds()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.ClearText(XEntry);
		App.EnterText(XEntry, "100");
		App.WaitForElement(YEntry);
		App.ClearText(YEntry);
		App.EnterText(YEntry, "100");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement(LayoutFlagNoneCheckBox);
		App.Tap(LayoutFlagNoneCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_WidthAndHeight()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "500");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "250");
		App.WaitForElement(LayoutFlagNoneCheckBox);
		App.Tap(LayoutFlagNoneCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_XProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.ClearText(XEntry);
		App.EnterText(XEntry, "0.5");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement(LayoutFlagXProportionalCheckBox);
		App.Tap(LayoutFlagXProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_YProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(YEntry);
		App.ClearText(YEntry);
		App.EnterText(YEntry, "0.5");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement(LayoutFlagYProportionalCheckBox);
		App.Tap(LayoutFlagYProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_XProportionalAndYProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.ClearText(XEntry);
		App.EnterText(XEntry, "0.5");
		App.WaitForElement(YEntry);
		App.ClearText(YEntry);
		App.EnterText(YEntry, "0.5");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement(LayoutFlagXProportionalCheckBox);
		App.Tap(LayoutFlagXProportionalCheckBox);
		App.WaitForElement(LayoutFlagYProportionalCheckBox);
		App.Tap(LayoutFlagYProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_PositionProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.ClearText(XEntry);
		App.EnterText(XEntry, "0.5");
		App.WaitForElement(YEntry);
		App.ClearText(YEntry);
		App.EnterText(YEntry, "0.5");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement(LayoutFlagPositionProportionalCheckBox);
		App.Tap(LayoutFlagPositionProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_WidthProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "0.5");
		App.WaitForElement(LayoutFlagWidthProportionalCheckBox);
		App.Tap(LayoutFlagWidthProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_HeightProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "0.5");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement(LayoutFlagHeightProportionalCheckBox);
		App.Tap(LayoutFlagHeightProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_WidthProportionalAndHeightProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "0.5");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "0.5");
		App.WaitForElement(LayoutFlagHeightProportionalCheckBox);
		App.Tap(LayoutFlagHeightProportionalCheckBox);
		App.WaitForElement(LayoutFlagWidthProportionalCheckBox);
		App.Tap(LayoutFlagWidthProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_SizeProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "0.5");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "0.5");
		App.WaitForElement(LayoutFlagSizeProportionalCheckBox);
		App.Tap(LayoutFlagSizeProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_SizeProportionalWithMaximumValue()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "1");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "1");
		App.WaitForElement(LayoutFlagSizeProportionalCheckBox);
		App.Tap(LayoutFlagSizeProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_AllProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.ClearText(XEntry);
		App.EnterText(XEntry, "0.5");
		App.WaitForElement(YEntry);
		App.ClearText(YEntry);
		App.EnterText(YEntry, "0.5");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "0.5");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "0.5");
		App.WaitForElement(LayoutFlagAllCheckBox);
		App.Tap(LayoutFlagAllCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_SizeProportionalAndPositionProportional()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.ClearText(XEntry);
		App.EnterText(XEntry, "0.5");
		App.WaitForElement(YEntry);
		App.ClearText(YEntry);
		App.EnterText(YEntry, "0.5");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "0.5");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "0.5");
		App.WaitForElement(LayoutFlagSizeProportionalCheckBox);
		App.Tap(LayoutFlagSizeProportionalCheckBox);
		App.WaitForElement(LayoutFlagPositionProportionalCheckBox);
		App.Tap(LayoutFlagPositionProportionalCheckBox);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_FlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_Visibility()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/31496
    [Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_BackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("BackgroundColorGrayButton");
		App.Tap("BackgroundColorGrayButton");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_Reset_LayoutBounds()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.ClearText(XEntry);
		App.EnterText(XEntry, "100");
		App.WaitForElement(YEntry);
		App.ClearText(YEntry);
		App.EnterText(YEntry, "100");
		App.WaitForElement(HeightEntry);
		App.ClearText(HeightEntry);
		App.EnterText(HeightEntry, "100");
		App.WaitForElement(WidthEntry);
		App.ClearText(WidthEntry);
		App.EnterText(WidthEntry, "100");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(XEntry);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif	
}
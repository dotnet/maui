using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class AbsoluteLayoutFeatureTests : _GalleryUITest
{
	public const string AbsoluteLayoutFeatureMatrix = "AbsoluteLayout Feature Matrix";
	public override string GalleryPageName => AbsoluteLayoutFeatureMatrix;
	protected override bool ResetAfterEachTest => true;
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
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
		AssertBoxBounds(250, 500);
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
		AssertBoxBounds(100, 100, xProportional: true);
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
		AssertBoxBounds(100, 100, yProportional: true);
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
		AssertBoxBounds(100, 100, xProportional: true, yProportional: true);
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
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
		AssertBoxBounds(0.5, 100, widthProportional: true);
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
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
		AssertBoxBounds(0.5, 0.5, widthProportional: true, heightProportional: true);
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
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
		AssertBoxBounds(1, 1, widthProportional: true, heightProportional: true);
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void VerifyAbsoluteLayout_Visibility()
	{
		App.WaitForElement("FixedLabel");
		App.WaitForElement("ClickMeButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForNoElement(Apply);
		App.WaitForElement(Options);
		App.WaitForNoElement("MainLayout");
		App.WaitForNoElement("FixedLabel");
		App.WaitForNoElement("ClickMeButton");
	}

	void AssertBoxBounds(double width, double height, bool widthProportional = false,
		bool heightProportional = false, bool xProportional = false, bool yProportional = false)
	{
		App.WaitForNoElement(Apply);
		App.WaitForElement(Options);
		App.WaitForElement("MainLayout");
		if (Device != TestDevice.iOS)
		{
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
			return;
		}

		App.WaitForElement("BlueBox");
		string frameDetails = "Native frames have not been read.";
		Assert.That(() =>
		{
			var layout = App.FindElement("MainLayout").GetRect();
			var box = App.FindElement("BlueBox").GetRect();
			var expectedWidth = widthProportional ? layout.Width * width : width;
			var expectedHeight = heightProportional ? layout.Height * height : height;
			var expectedX = layout.X + (xProportional ? (layout.Width - expectedWidth) / 2 : 0);
			var expectedY = layout.Y + (yProportional ? (layout.Height - expectedHeight) / 2 : 0);
			frameDetails = $"Native MainLayout={layout}; actual BlueBox={box}; "
				+ $"expected BlueBox=(X={expectedX}, Y={expectedY}, Width={expectedWidth}, Height={expectedHeight}); "
				+ $"proportional flags: X={xProportional}, Y={yProportional}, Width={widthProportional}, Height={heightProportional}.";
			return layout.Width > 0 && layout.Height > 0
				&& Math.Abs(box.Width - expectedWidth) <= 1
				&& Math.Abs(box.Height - expectedHeight) <= 1
				&& Math.Abs(box.X - expectedX) <= 1
				&& Math.Abs(box.Y - expectedY) <= 1;
		}, Is.True.After(5000, 100), () => frameDetails);
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
}
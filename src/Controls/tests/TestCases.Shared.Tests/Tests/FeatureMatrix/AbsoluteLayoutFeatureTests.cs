using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class AbsoluteLayoutFeatureTests : _GalleryUITest
{
	public const string AbsoluteLayoutFeatureMatrix = "AbsoluteLayout Feature Matrix";
	public override string GalleryPageName => AbsoluteLayoutFeatureMatrix;
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
		App.WaitForElement(Options);
		if (Device is TestDevice.Android or TestDevice.iOS)
		{
			Assert.That(App.WaitForKeyboardToHide(), Is.True);
		}

		var layout = App.WaitForElement("MainLayout");
		App.WaitForElement("BlueBox");
		App.WaitForElement("FixedLabel");
		App.WaitForElement("ClickMeButton");
		Assert.That(layout.GetRect().Width, Is.GreaterThan(0));
		Assert.That(layout.GetRect().Height, Is.GreaterThan(0));
		Assert.That(() =>
		{
			var parent = App.FindElement("MainLayout").GetRect();
			var child = App.FindElement("BlueBox").GetRect();
			var top = App.FindElement("FixedLabel").GetRect();
			var bottom = App.FindElement("ClickMeButton").GetRect();
			// The native parent's bounds include system insets. These y=0 and y=0.99
			// children locate the usable content height without assuming an inset size.
			var contentHeight = (bottom.Y - top.Y) / 0.99 + bottom.Height;
			return new[]
			{
				child.Width - parent.Width * 0.5,
				child.Height - contentHeight * 0.5,
				child.X - (parent.X + (parent.Width - child.Width) * 0.5),
				child.Y - (top.Y + (contentHeight - child.Height) * 0.5)
			};
		}, Is.All.InRange(-2.0, 2.0).After(5000, 100),
			"The native box should occupy half the layout's width and height and be centered.");
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
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.WaitForNoElement("MainLayout");
		App.WaitForNoElement("BlueBox");
		App.WaitForNoElement("FixedLabel");
		App.WaitForNoElement("ClickMeButton");

		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("MainLayout");
		App.WaitForElement("FixedLabel");
		App.WaitForElement("ClickMeButton");
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
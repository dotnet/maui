using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Layout)]
public class StackLayoutFeatureTests : UITest
{
	public const string StackLayoutFeatureMatrix = "StackLayout Feature Matrix";

	public StackLayoutFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(StackLayoutFeatureMatrix);
	}

	[Test]
	public void HorizontalStackLayout_IsVisible()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("CheckBoxIsVisible");
		App.Tap("CheckBoxIsVisible");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForNoElement("LabelSpacing");
	}

	[Test]
	public void VerticalStackLayout_IsVisible()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("CheckBoxIsVisible");
		App.Tap("CheckBoxIsVisible");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForNoElement("LabelSpacing");
	}

	[Test]
	public void HorizontalStackLayout_RTLFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_RTLFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_Spacing_With_RTL()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "20");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void HorizontalStackLayout_Spacing_With_RTL()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "20");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void HorizontalStackLayout_Spacing_With_Height()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void HorizontalStackLayout_Spacing_With_Width()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void HorizontalStackLayout_RTLFlowDirection_With_Height()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void HorizontalStackLayout_RTLFlowDirection_With_Width()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_Spacing_With_Width()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_Spacing_With_Height()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_RTLFlowDirection_With_Width()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_RTLFlowDirection_With_Height()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_RTLFlowDirection_With_HeightAndWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void HorizontalStackLayout_RTLFlowDirection_With_HeightAndWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("CheckBoxIsRtl");
		App.Tap("CheckBoxIsRtl");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void VerticalStackLayout_Spacing_With_HeightAndWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	public void HorizontalStackLayout_Spacing_With_HeightAndWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.WaitForElement("EntryHeight");
		App.ClearText("EntryHeight");
		App.EnterText("EntryHeight", "50");

		App.WaitForElement("EntryWidth");
		App.ClearText("EntryWidth");
		App.EnterText("EntryWidth", "50");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

#if ANDROID || IOS
	[Test]
	public void HorizontalStackLayout_Spacing_WithLandscape()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.SetOrientationLandscape();

		App.WaitForElement("Apply");
		App.Tap("Apply");

#if ANDROID
		VerifyScreenshot(cropLeft: 125);
#else
		VerifyScreenshot();
#endif
	}

	[Test]
	public void VerticalStackLayout_Spacing_WithLandscape()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("RadioVertical");
		App.Tap("RadioVertical");

		App.WaitForElement("EntrySpacing");
		App.ClearText("EntrySpacing");
		App.EnterText("EntrySpacing", "30");

		App.SetOrientationLandscape();

		App.WaitForElement("Apply");
		App.Tap("Apply");

#if ANDROID
		VerifyScreenshot(cropLeft: 125);
#else
		VerifyScreenshot();
#endif
	}
#endif
}
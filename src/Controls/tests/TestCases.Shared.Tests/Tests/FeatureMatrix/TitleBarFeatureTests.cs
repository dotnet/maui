#if ANDROID || WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class TitleBarFeatureTests : UITest
{
	public const string TitleBarFeatureMatrix = "TitleBar Feature Matrix";

	public TitleBarFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(TitleBarFeatureMatrix);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_Window()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");

		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");

		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");

		App.WaitForElement("ShowBackgroundColorCheckBox");
		App.Tap("ShowBackgroundColorCheckBox");

		App.WaitForElement("OrangeRadioButton");
		App.Tap("OrangeRadioButton");

		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");

		App.WaitForElement("ProgressBarRadioButton");
		App.Tap("ProgressBarRadioButton");

		VerifyScreenshot();

	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TrailingContentAndLeadingContent_WithHorizontalStackLayout()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");
		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");

		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("HorizontalStackLayoutRadioButton");
		App.Tap("HorizontalStackLayoutRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TrailingContentAndLeadingContent_WithGrid()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");
		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");

		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("ProgressBarRadioButton");
		App.Tap("ProgressBarRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TrailingContentAndLeadingContent_WithTitleAndSubtitle()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");
		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TrailingContentAndLeadingContent_WithBackgroundColor()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");
		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");

		App.WaitForElement("ShowBackgroundColorCheckBox");
		App.Tap("ShowBackgroundColorCheckBox");
		App.WaitForElement("OrangeRadioButton");
		App.Tap("OrangeRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TrailingContentAndLeadingContent_WithSearchBar()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");
		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");

		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("SearchBarRadioButton");
		App.Tap("SearchBarRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TitleAndSubTitle_WithBackgroundColor()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");

		App.WaitForElement("ShowBackgroundColorCheckBox");
		App.Tap("ShowBackgroundColorCheckBox");
		App.WaitForElement("OrangeRadioButton");
		App.Tap("OrangeRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TitleAndSubTitle_WithHorizontalStackLayout()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");
		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("HorizontalStackLayoutRadioButton");
		App.Tap("HorizontalStackLayoutRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TitleAndSubTitle_WithGrid()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");
		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("ProgressBarRadioButton");
		App.Tap("ProgressBarRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TitleAndSubTitle_WithSearchBar()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");
		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("SearchBarRadioButton");
		App.Tap("SearchBarRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_BackgroundColor_WithGrid()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowBackgroundColorCheckBox");
		App.Tap("ShowBackgroundColorCheckBox");
		App.WaitForElement("RedRadioButton");
		App.Tap("RedRadioButton");
		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("ProgressBarRadioButton");
		App.Tap("ProgressBarRadioButton");

		VerifyScreenshot();
	}
#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_MACCATALYST //For more information see:
	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_RTL_WithTrailingContentAndLeadingContent()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");
		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");
		App.WaitForElement("FlowDirectionRTLCheckBox");
		App.Tap("FlowDirectionRTLCheckBox");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_RTL_WithTitleAndSubTitle()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("FlowDirectionRTLCheckBox");
		App.Tap("FlowDirectionRTLCheckBox");
		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_RTL_WithSearchBar()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("FlowDirectionRTLCheckBox");
		App.Tap("FlowDirectionRTLCheckBox");
		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("SearchBarRadioButton");
		App.Tap("SearchBarRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_RTL_WithHorizontalStackLayout()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("FlowDirectionRTLCheckBox");
		App.Tap("FlowDirectionRTLCheckBox");
		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("HorizontalStackLayoutRadioButton");
		App.Tap("HorizontalStackLayoutRadioButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_RTL_WithGridWithProgressBar()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("FlowDirectionRTLCheckBox");
		App.Tap("FlowDirectionRTLCheckBox");
		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("ProgressBarRadioButton");
		App.Tap("ProgressBarRadioButton");

		VerifyScreenshot();
	}
#endif
	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_TitleAndSubTitle_Entry()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("TitleEntry");
		App.Tap("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Custom Title");

		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");
		App.WaitForElement("SubtitleEntry");
		App.Tap("SubtitleEntry");
		App.ClearText("SubtitleEntry");
		App.EnterText("SubtitleEntry", "Custom Subtitle");

		VerifyScreenshot();
	}
}
#endif
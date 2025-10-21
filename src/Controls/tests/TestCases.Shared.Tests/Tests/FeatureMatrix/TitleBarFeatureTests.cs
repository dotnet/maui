// This feature test is applicable only on desktop platforms (Windows and Mac).
#if MACCATALYST || WINDOWS
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

		App.WaitForElement("ShowIconCheckBox");
		App.Tap("ShowIconCheckBox");

		App.WaitForElement("ShowBackgroundColorCheckBox");
		App.Tap("ShowBackgroundColorCheckBox");

		App.WaitForElement("OrangeRadioButton");
		App.Tap("OrangeRadioButton");

		App.WaitForElement("ShowForegroundColorCheckBox");
		App.Tap("ShowForegroundColorCheckBox");
		App.WaitForElement("WhiteForegroundRadioButton");
		App.Tap("WhiteForegroundRadioButton");

		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");

		App.WaitForElement("ProgressBarRadioButton");
		App.Tap("ProgressBarRadioButton");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_Icon_WithTrailingContentAndLeadingContent()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowIconCheckBox");
		App.Tap("ShowIconCheckBox");

		App.WaitForElement("ShowLeadingContentCheckBox");
		App.Tap("ShowLeadingContentCheckBox");
		App.WaitForElement("ShowTrailingContentCheckBox");
		App.Tap("ShowTrailingContentCheckBox");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_Icon_WithBackgroundColor()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowIconCheckBox");
		App.Tap("ShowIconCheckBox");

		App.WaitForElement("ShowBackgroundColorCheckBox");
		App.Tap("ShowBackgroundColorCheckBox");
		App.WaitForElement("RedRadioButton");
		App.Tap("RedRadioButton");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_Icon_WithSearchBar()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowIconCheckBox");
		App.Tap("ShowIconCheckBox");

		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("SearchBarRadioButton");
		App.Tap("SearchBarRadioButton");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_Icon_WithForegroundColor()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowIconCheckBox");
		App.Tap("ShowIconCheckBox");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");

		App.WaitForElement("ShowForegroundColorCheckBox");
		App.Tap("ShowForegroundColorCheckBox");
		App.WaitForElement("WhiteForegroundRadioButton");
		App.Tap("WhiteForegroundRadioButton");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_ForegroundColor_WithBackgroundColor()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");

		App.WaitForElement("ShowForegroundColorCheckBox");
		App.Tap("ShowForegroundColorCheckBox");
		App.WaitForElement("WhiteForegroundRadioButton");
		App.Tap("WhiteForegroundRadioButton");

		App.WaitForElement("ShowBackgroundColorCheckBox");
		App.Tap("ShowBackgroundColorCheckBox");
		App.WaitForElement("RedRadioButton");
		App.Tap("RedRadioButton");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_ForegroundColor_WithGrid()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleCheckBox");
		App.Tap("ShowTitleCheckBox");
		App.WaitForElement("ShowSubtitleCheckBox");
		App.Tap("ShowSubtitleCheckBox");

		App.WaitForElement("ShowForegroundColorCheckBox");
		App.Tap("ShowForegroundColorCheckBox");
		App.WaitForElement("WhiteForegroundRadioButton");
		App.Tap("WhiteForegroundRadioButton");

		App.WaitForElement("IsTitleBarContentVisibleCheckBox");
		App.Tap("IsTitleBarContentVisibleCheckBox");
		App.WaitForElement("ProgressBarRadioButton");
		App.Tap("ProgressBarRadioButton");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleBar_IsVisible()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("ShowTitleBarCheckBox");
		App.Tap("ShowTitleBarCheckBox");

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_MACCATALYST //For more information see: https://github.com/dotnet/maui/issues/30399
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar:true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar:true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar:true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar:true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar:true);
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

		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");

		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif
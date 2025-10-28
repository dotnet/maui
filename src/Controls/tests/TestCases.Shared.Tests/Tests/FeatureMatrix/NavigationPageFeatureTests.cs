using System.Linq;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Navigation)]
	public class NavigationPageFeatureTests : UITest
	{
		public const string NavigationPageFeatureMatrix = "NavigationPage Feature Matrix";

		public NavigationPageFeatureTests(TestDevice device) : base(device) { }

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(NavigationPageFeatureMatrix);
		}

		[Test, Order(1)]
		public void EntryPoints_AreVisible()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("ApplyButton");
			App.WaitForElement("PushPageButton");
			App.WaitForElement("PopToRootButton");

			App.WaitForElement("HasNavigationBarCheckBox");
			App.WaitForElement("HasBackButtonCheckBox");
			App.WaitForElement("BackButtonTitleEntry");

			App.WaitForElement("BarBackgroundColorBlueButton");
			App.WaitForElement("BarTextColorWhiteButton");
			App.WaitForElement("BarBackgroundLinearButton");
			App.WaitForElement("BarBackgroundRadialButton");
			App.WaitForElement("BarBackgroundClearButton");
			App.WaitForElement("IconColorRedButton");
			App.WaitForElement("IconColorPurpleButton");
			App.WaitForElement("IconColorDefaultButton");
			App.WaitForElement("TitleIconButton");
			App.WaitForElement("TitleViewButton");
		}

		[Test, Order(0)]
		public void Defaults_AreCorrect_OnLoad()
		{
			App.WaitForElement("CurrentPageLabel");
			Assert.That(App.FindElement("CurrentPageLabel").GetText(), Is.EqualTo("Sample Page"));
			App.WaitForElement("RootPageLabel");
			Assert.That(App.FindElement("RootPageLabel").GetText(), Is.EqualTo("Sample Page"));

			// Last event should be the initial NavigatedTo on root
			App.WaitForElement("LastNavigationEventLabel");
			Assert.That(App.FindElement("LastNavigationEventLabel").GetText(), Does.Contain("NavigatedTo: Sample Page"));

			// From/PreFrom not yet
			App.WaitForElement("NavigatedFromStatusLabel");
			Assert.That(App.FindElement("NavigatedFromStatusLabel").GetText(), Is.EqualTo("Not yet"));
			App.WaitForElement("NavigatingFromStatusLabel");
			Assert.That(App.FindElement("NavigatingFromStatusLabel").GetText(), Is.EqualTo("Not yet"));

			// To params should indicate a single-item stack on load
			App.WaitForElement("NavigatedToParamsLabel");
			var toParams = App.FindElement("NavigatedToParamsLabel").GetText();
			Assert.That(toParams, Does.Contain("StackCount=1"));

			// Default back button title
			App.WaitForElement("BackButtonTitleEntry");
			Assert.That(App.FindElement("BackButtonTitleEntry").GetText(), Is.EqualTo("Back"));
		}

		[Test, Order(2)]
		public void Events_AreRaised_WithExpectedPages_On_Push_And_Pop()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			App.WaitForElement("PopPageButton");

			// Pop back to root
			App.Tap("PopPageButton");
			App.WaitForElement("CurrentPageLabel");

			// Verify statuses reflect raised pages
			App.WaitForElement("NavigatedFromStatusLabel");
			Assert.That(App.FindElement("NavigatedFromStatusLabel").GetText(), Does.Contain("Page 2"));
			App.WaitForElement("NavigatingFromStatusLabel");
			Assert.That(App.FindElement("NavigatingFromStatusLabel").GetText(), Does.Contain("Page 2"));
			App.WaitForElement("NavigatedToStatusLabel");
			Assert.That(App.FindElement("NavigatedToStatusLabel").GetText(), Does.Contain("Sample Page"));

			// Verify parameter labels include the expected titles and keys
			var toParams = App.FindElement("NavigatedToParamsLabel").GetText();
			Assert.That(toParams, Does.Contain("PreviousPageTitle=Page 2"));
			var fromParams = App.FindElement("NavigatedFromParamsLabel").GetText();
			Assert.That(fromParams, Does.Contain("DestinationTitle=Sample Page"));
			var preParams = App.FindElement("NavigatingFromParamsLabel").GetText();
			Assert.That(preParams, Does.Contain("Requested=Pop"));
		}

		[Test, Order(3)]
		public void PopToRoot_FromPage3_ReturnsToRoot()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton"); // to Page 2
			App.WaitForElement("PushPage3Button");
			App.Tap("PushPage3Button"); // to Page 3

			// Pop to root using the in-page button
			App.WaitForElement("PopToRootPageButton");
			App.Tap("PopToRootPageButton");
			App.WaitForElement("CurrentPageLabel");
			Assert.That(App.FindElement("CurrentPageLabel").GetText(), Is.EqualTo("Sample Page"));
		}

		// BackButtonTitle does not applicable for Android and Windows Platforms
#if TEST_FAILS_ON_IOS && TESTS_FAIL_ON_MACCATALYST// Issue Link: https://github.com/dotnet/maui/issues/31539
#if IOS || MACCATALYST
		[Test,Order(19)]
		public void BackButtonTitle_AppliesOnNextPage_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("BackButtonTitleEntry");
			App.ClearText("BackButtonTitleEntry");
			App.EnterText("BackButtonTitleEntry", "My Back");
			App.PressEnter();
			App.Tap("ApplyButton");
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Validate visually on page 2 (back button title)
			VerifyScreenshot();
		}
#endif
#endif

		[Test, Order(5)]
		public void ToggleHasBackButton_HidesBackArrow_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			Exception? exception = null;
			App.WaitForElement("HasBackButtonCheckBox");
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Screenshot: Back arrow visible
			VerifyScreenshotOrSetException(ref exception, "BackButtonVisible");
			App.WaitForElement("PopPageButton");
			App.Tap("PopPageButton");
			App.WaitForElement("HasBackButtonCheckBox");
			App.Tap("HasBackButtonCheckBox");
			App.WaitForElement("ApplyButton");
			App.Tap("ApplyButton");
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Screenshot: Back arrow hidden
			VerifyScreenshotOrSetException(ref exception, "BackButtonHidden");
			if (exception != null)
				throw exception;
		}

		[Test, Order(6)]
		public void ToggleHasNavigationBar_HidesBar_Visual()
		{
			App.WaitForElement("PopToRootPageButton");
			App.Tap("PopToRootPageButton");

			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			Exception? exception = null;
			App.WaitForElement("HasNavigationBarCheckBox");
			App.Tap("HasNavigationBarCheckBox");
			// Screenshot: Navigation bar hidden
			VerifyScreenshotOrSetException(ref exception, "NavBarHidden");
			App.Tap("HasNavigationBarCheckBox");
			// Screenshot: Navigation bar visible again
			VerifyScreenshotOrSetException(ref exception, "NavBarVisible");
			if (exception != null)
				throw exception;
		}

		[Test, Order(7)]
		public void SetBarBackgroundColor_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			Exception? exception = null;
			App.WaitForElement("BarBackgroundColorBlueButton");
			App.Tap("BarBackgroundColorBlueButton");
			// Screenshot: Blue bar background
			VerifyScreenshotOrSetException(ref exception, "BarBgBlue");
			App.WaitForElement("BarBackgroundColorRedButton");
			App.Tap("BarBackgroundColorRedButton");
			// Screenshot: Red bar background
			VerifyScreenshotOrSetException(ref exception, "BarBgRed");
			if (exception != null)
				throw exception;
		}

		[Test, Order(8)]
		public void SetBarTextColor_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			Exception? exception = null;
			App.WaitForElement("BarTextColorWhiteButton");
			App.Tap("BarTextColorWhiteButton");
			// Screenshot: White bar text color
			VerifyScreenshotOrSetException(ref exception, "BarTextWhite");
			App.WaitForElement("BarTextColorBlackButton");
			App.Tap("BarTextColorBlackButton");
			// Screenshot: Black bar text color
			VerifyScreenshotOrSetException(ref exception, "BarTextBlack");
			if (exception != null)
				throw exception;
		}

		[Test, Order(9)]
		public void SetBarBackground_Linear_Radial_Clear_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			Exception? exception = null;
			App.WaitForElement("BarBackgroundLinearButton");
			App.Tap("BarBackgroundLinearButton");
			// Screenshot: Linear gradient applied
			VerifyScreenshotOrSetException(ref exception, "BarBgLinear");

			App.WaitForElement("BarBackgroundRadialButton");
			App.Tap("BarBackgroundRadialButton");
			// Screenshot: Radial gradient applied
			VerifyScreenshotOrSetException(ref exception, "BarBgRadial");

			App.WaitForElement("BarBackgroundClearButton");
			App.Tap("BarBackgroundClearButton");
			if (exception != null)
				throw exception;
		}

		[Test, Order(10)]
		public void SetIconColor_Red_Purple_Default_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			Exception? exception = null;
			App.WaitForElement("IconColorRedButton");
			App.Tap("IconColorRedButton");
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Screenshot: Red icon color on pushed page
			VerifyScreenshotOrSetException(ref exception, "IconColorRed");
			App.WaitForElement("PopPageButton");
			App.Tap("PopPageButton");

			App.WaitForElement("IconColorDefaultButton");
			App.Tap("IconColorDefaultButton");
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Screenshot: Default icon color on pushed page
			VerifyScreenshotOrSetException(ref exception, "IconColorDefault");
			if (exception != null)
				throw exception;
		}

		[Test, Order(11)]
		public void TitleIcon_Add_Visual()
		{
			App.WaitForElement("PopToRootPageButton");
			App.Tap("PopToRootPageButton");

			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			// Screenshot: Title icon applied on current page
			VerifyScreenshot();
		}

#if TEST_FAILS_ON_ANDROID      //Issue Link: https://github.com/dotnet/maui/issues/31445                                                                
		[Test, Order(12)]
		public void TitleIcon_AddingTwice_DoesNotDuplicate()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			VerifyScreenshot();
		}
#endif

		[Test, Order(13)]
		public void Combine_BarBackgroundColor_TextColor_IconColor_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			// Set bar background color and text color
			App.WaitForElement("BarBackgroundColorBlueButton");
			App.Tap("BarBackgroundColorBlueButton");
			App.WaitForElement("BarTextColorWhiteButton");
			App.Tap("BarTextColorWhiteButton");
			// Set icon color
			App.WaitForElement("IconColorRedButton");
			App.Tap("IconColorRedButton");
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Screenshot: Combined bar background, text color and icon color on pushed page
			VerifyScreenshot();
		}

		[Test, Order(14)]
		public void TitleIcon_And_TitleView_Persist_On_Push_Then_Clear()
		{
			App.WaitForElement("PopToRootPageButton");
			App.Tap("PopToRootPageButton");

			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			// Add both title icon and custom title view
			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			App.WaitForElement("TitleViewButton");
			App.Tap("TitleViewButton");
			App.WaitForElement("BackButtonTitleEntry");
			App.ClearText("BackButtonTitleEntry");
			App.EnterText("BackButtonTitleEntry", "My Back");
			App.PressEnter();
			App.WaitForElement("ApplyButton");
			App.Tap("ApplyButton");
			// Push and verify they persist
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Screenshot: Title icon and custom title view present on pushed page
			VerifyScreenshot();
		}

		[Test, Order(15)]
		public void EventParams_Update_On_Push_And_Pop()
		{
			App.WaitForElement("PopToRootPageButton");
			App.Tap("PopToRootPageButton");

			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			// Push to Page 2
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			// Pop back to root (labels live on root page)
			App.WaitForElement("PopPageButton");
			App.Tap("PopPageButton");
			// Validate param label contains expected keys after navigation sequence
			App.WaitForElement("NavigationEventParamsLabel");
			var text = App.FindElement("NavigationEventParamsLabel").GetText();
			Assert.That(text, Does.Match("PreviousPageTitle|BeforeCount"));
		}

		[Test, Order(16)]
		public void Combine_AllFeatures_Then_Push_Pop_Verify_AllEventsAndParams()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			// Combine several features
			App.WaitForElement("BarBackgroundColorBlueButton");
			App.Tap("BarBackgroundColorBlueButton");
			App.WaitForElement("BarTextColorWhiteButton");
			App.Tap("BarTextColorWhiteButton");
			App.WaitForElement("BarBackgroundLinearButton");
			App.Tap("BarBackgroundLinearButton");
			App.WaitForElement("IconColorPurpleButton");
			App.Tap("IconColorPurpleButton");
			App.WaitForElement("BackButtonTitleEntry");
			App.ClearText("BackButtonTitleEntry");
			App.EnterText("BackButtonTitleEntry", "TestBack");
			App.PressEnter();
			App.WaitForElement("ApplyButton");
			App.Tap("ApplyButton");
			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			App.WaitForElement("TitleViewButton");
			App.Tap("TitleViewButton");
			App.WaitForElement("ApplyButton");
			App.Tap("ApplyButton");

			// Push to Page 2
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton");
			App.WaitForElement("PopPageButton"); // we are on Page 2

			// Pop back to root so we can read root labels
			App.Tap("PopPageButton");
			App.WaitForElement("CurrentPageLabel");

			// Ensure all three events reported and include expected page names/keys
			App.WaitForElement("NavigatedFromStatusLabel");
			var fromStatus = App.FindElement("NavigatedFromStatusLabel").GetText();
			Assert.That(fromStatus, Does.Contain("Page 2"), "NavigatedFrom should be raised on Page 2");

			App.WaitForElement("NavigatingFromStatusLabel");
			var preFromStatus = App.FindElement("NavigatingFromStatusLabel").GetText();
			Assert.That(preFromStatus, Does.Contain("Page 2"), "NavigatingFrom should be raised on Page 2");

			App.WaitForElement("NavigatedToStatusLabel");
			var toStatus = App.FindElement("NavigatedToStatusLabel").GetText();
			Assert.That(toStatus, Does.Contain("Sample Page"), "NavigatedTo should be raised on Sample Page after pop");

			// Params validations
			var toParams = App.FindElement("NavigatedToParamsLabel").GetText();
			Assert.That(toParams, Does.Contain("PreviousPageTitle=Page 2"));

			var fromParams = App.FindElement("NavigatedFromParamsLabel").GetText();
			Assert.That(fromParams, Does.Contain("DestinationTitle=Sample Page"));
			Assert.That(fromParams, Does.Contain("Type=Pop"));

			var preParams = App.FindElement("NavigatingFromParamsLabel").GetText();
			Assert.That(preParams, Does.Contain("Requested=Pop"));
		}

		[Test, Order(17)]
		public void Combine_AllFeatures_PushToPage3_PopToRoot_Verify_AllEventsAndParams()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");
			// Ensure some non-defaults are set
			App.WaitForElement("BarBackgroundColorRedButton");
			App.Tap("BarBackgroundColorRedButton");
			App.WaitForElement("BarTextColorBlackButton");
			App.Tap("BarTextColorBlackButton");
			App.WaitForElement("IconColorRedButton");
			App.Tap("IconColorRedButton");

			// Go to Page 3
			App.WaitForElement("PushPageButton");
			App.Tap("PushPageButton"); // Page 2
			App.WaitForElement("PushPage3Button");
			App.Tap("PushPage3Button"); // Page 3
			App.WaitForElement("PopToRootPageButton");

			// Pop to root
			App.Tap("PopToRootPageButton");
			App.WaitForElement("CurrentPageLabel");

			// Validate event labels reference Page 3 and Sample Page appropriately
			var fromStatus = App.FindElement("NavigatedFromStatusLabel").GetText();
			Assert.That(fromStatus, Does.Contain("Page 3"), "NavigatedFrom should be raised on Page 3");

			var preFromStatus = App.FindElement("NavigatingFromStatusLabel").GetText();
			Assert.That(preFromStatus, Does.Contain("Page 3"), "NavigatingFrom should be raised on Page 3");

			var toStatus = App.FindElement("NavigatedToStatusLabel").GetText();
			Assert.That(toStatus, Does.Contain("Sample Page"), "NavigatedTo should be raised on Sample Page after PopToRoot");

			// Params validations for PopToRoot flow
			var toParams = App.FindElement("NavigatedToParamsLabel").GetText();
			Assert.That(toParams, Does.Contain("PreviousPageTitle=Page 3"));

			var fromParams = App.FindElement("NavigatedFromParamsLabel").GetText();
			Assert.That(fromParams, Does.Contain("DestinationTitle=Sample Page"));
			Assert.That(fromParams, Does.Contain("Type=PopToRoot"));

			var preParams = App.FindElement("NavigatingFromParamsLabel").GetText();
			Assert.That(preParams, Does.Contain("Requested=PopToRoot"));
		}

		[Test, Order(18)]
		public void TitleView_Add_Visual()
		{
			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			Exception? exception = null;
			App.WaitForElement("TitleViewButton");
			App.Tap("TitleViewButton");
			// Screenshot: TitleView applied
			VerifyScreenshotOrSetException(ref exception, "TitleViewApplied");
			App.WaitForElement("TitleViewClearButton");
			App.Tap("TitleViewClearButton");
			// Screenshot: TitleView cleared
			VerifyScreenshotOrSetException(ref exception, "TitleViewCleared");
			if (exception != null)
				throw exception;
		}
	}
}
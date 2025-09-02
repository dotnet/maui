using NUnit.Framework;
using System.Linq;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class NavigationPageFeatureTests : UITest
	{
		public const string NavigationPageFeatureMatrix = "NavigationPage Feature Matrix";

		public NavigationPageFeatureTests(TestDevice device) : base(device) { }

		void ResetState()
		{
			bool Exists(string id) => App.Query.ById(id).Any();

			// Walk back to root (where ResetButton lives) via PopToRoot or Pop
			for (int i = 0; i < 5; i++)
			{
				if (Exists("ResetButton"))
					break; // root reached

				if (Exists("PopToRootPageButton"))
				{
					App.Tap("PopToRootPageButton");
					App.WaitForElement("CurrentPageLabel");
					continue;
				}

				if (Exists("PopPageButton"))
				{
					App.Tap("PopPageButton");
					App.WaitForElement("CurrentPageLabel");
					continue;
				}

				break; // nothing else to do
			}

			if (Exists("ResetButton"))
			{
				App.Tap("ResetButton");
				App.WaitForElement("BackButtonTitleEntry");
			}
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(NavigationPageFeatureMatrix);
		}

		[Test, Order(1)]
		[Category(UITestCategories.Navigation)]
		public void EntryPoints_AreVisible()
		{
			//ResetState();
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
		[Category(UITestCategories.Navigation)]
		public void Defaults_AreCorrect_OnLoad()
		{
			//ResetState();
			// Root page should be Sample Page
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

		[Test, Order(10)]
		[Category(UITestCategories.Navigation)]
		public void Events_AreRaised_WithExpectedPages_On_Push_And_Pop()
		{
			ResetState();
			// Push to Page 2
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

		[Test]
		[Category(UITestCategories.Navigation)]
		public void PopToRoot_FromPage3_ReturnsToRoot()
		{
			ResetState();
			// Go to Page 3
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

#if TEST_FAILS_ON_IOS && !ANDROID && !WINDOWS	//Issue
		[Test]
		[Category(UITestCategories.Navigation)]
		public void BackButtonTitle_AppliesOnNextPage_Visual()
		{
			App.WaitForElement("BackButtonTitleEntry");
			App.ClearText("BackButtonTitleEntry");
			App.EnterText("BackButtonTitleEntry", "My Back");
			App.PressEnter();
			App.Tap("ApplyButton");
			App.Tap("PushPageButton");
			// Validate visually on page 2 (back button title)
			VerifyScreenshot();
			App.WaitForElement("PopToRootPageButton");
			App.Tap("PopToRootPageButton");
		}
#endif

		[Test]
		[Category(UITestCategories.Navigation)]
		public void ToggleHasBackButton_HidesBackArrow_Visual()
		{
			ResetState();
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
			if (exception != null) throw exception;
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void ToggleHasNavigationBar_HidesBar_Visual()
		{
			ResetState();
			Exception? exception = null;
			App.WaitForElement("HasNavigationBarCheckBox");
			App.Tap("HasNavigationBarCheckBox");
			// Screenshot: Navigation bar hidden
			VerifyScreenshotOrSetException(ref exception, "NavBarHidden");
			App.Tap("HasNavigationBarCheckBox");
			// Screenshot: Navigation bar visible again
			VerifyScreenshotOrSetException(ref exception, "NavBarVisible");
			if (exception != null) throw exception;
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void SetBarBackgroundColor_Visual()
		{
			ResetState();
			Exception? exception = null;
			App.WaitForElement("BarBackgroundColorBlueButton");
			App.Tap("BarBackgroundColorBlueButton");
			// Screenshot: Blue bar background
			VerifyScreenshotOrSetException(ref exception, "BarBgBlue");
			App.WaitForElement("BarBackgroundColorRedButton");
			App.Tap("BarBackgroundColorRedButton");
			// Screenshot: Red bar background
			VerifyScreenshotOrSetException(ref exception, "BarBgRed");
			if (exception != null) throw exception;
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void SetBarTextColor_Visual()
		{
			ResetState();
			Exception? exception = null;
			App.WaitForElement("BarTextColorWhiteButton");
			App.Tap("BarTextColorWhiteButton");
			// Screenshot: White bar text color
			VerifyScreenshotOrSetException(ref exception, "BarTextWhite");
			App.WaitForElement("BarTextColorBlackButton");
			App.Tap("BarTextColorBlackButton");
			// Screenshot: Black bar text color
			VerifyScreenshotOrSetException(ref exception, "BarTextBlack");
			if (exception != null) throw exception;
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Brush)]
		public void SetBarBackground_Linear_Radial_Clear_Visual()
		{
			ResetState();
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
			if (exception != null) throw exception;
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void SetIconColor_Red_Purple_Default_Visual()
		{
			ResetState();
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
			if (exception != null) throw exception;
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.TitleView)]
		public void TitleIcon_Add_Visual()
		{
			ResetState();
			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			// Screenshot: Title icon applied on current page
			VerifyScreenshot();
		}

#if TEST_FAILS_ON_ANDROID		//Issue Link: https://github.com/dotnet/maui/issues/31445
		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.TitleView)]
		public void TitleIcon_AddingTwice_DoesNotDuplicate()
		{
			// Repro steps for bug: tapping Add Icon multiple times produces multiple icons rendered.
			ResetState();
			App.WaitForElement("TitleIconButton");
			App.Tap("TitleIconButton");
			App.Tap("TitleIconButton");
			App.Tap("TitleIconButton");
			VerifyScreenshot();
		}
#endif

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Combine_BarBackgroundColor_TextColor_IconColor_Visual()
		{
			ResetState();
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
			// App.WaitForElement("PopPageButton");
			// App.Tap("PopPageButton");
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.TitleView)]
		public void TitleIcon_And_TitleView_Persist_On_Push_Then_Clear()
		{
			ResetState();
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

		[Test]
		[Category(UITestCategories.Navigation)]
		public void EventParams_Update_On_Push_And_Pop()
		{
			ResetState();
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

		[Test]
		[Order(100)]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Brush)]
		[Category(UITestCategories.TitleView)]
		public void Combine_AllFeatures_Then_Push_Pop_Verify_AllEventsAndParams()
		{
			ResetState();
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
			Assert.That(preParams, Does.Contain("DestinationTitle=Sample Page"));
		}

		[Test]
		[Order(110)]
		[Category(UITestCategories.Navigation)]
		public void Combine_AllFeatures_PushToPage3_PopToRoot_Verify_AllEventsAndParams()
		{
			ResetState();
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
			Assert.That(preParams, Does.Contain("DestinationTitle=Sample Page"));
		}


		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.TitleView)]
		public void TitleView_Add_Visual()
		{
			ResetState();
			Exception? exception = null;
			App.WaitForElement("TitleViewButton");
			App.Tap("TitleViewButton");
			// Screenshot: TitleView applied
			VerifyScreenshotOrSetException(ref exception, "TitleViewApplied");
			App.WaitForElement("TitleViewClearButton");
			App.Tap("TitleViewClearButton");
			// Screenshot: TitleView cleared
			VerifyScreenshotOrSetException(ref exception, "TitleViewCleared");
			if (exception != null) throw exception;
		}
	}
}


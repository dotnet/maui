using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class FlyoutPageFeatureTests : UITest
{
	public const string FlyoutPageFeatureMatrix = "Flyout Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string FlowDirectionRTL = "FlowDirectionRTL";
	public const string IsEnabledFalse = "IsEnabledFalse";
	public const string IsVisibleFalse = "IsVisibleFalse";
	public const string IsPresentedTrue = "IsPresentedTrue";
	public const string IsGestureEnabledTrue = "IsGestureEnabledTrue";
	public const string IsGestureEnabledFalse = "IsGestureEnabledFalse";
	public const string TitleEntry = "TitleEntry";
	public const string FlyoutLayoutBehaviorSplit = "FlyoutLayoutBehaviorSplit";
	public const string FlyoutLayoutBehaviorSplitOnPortrait = "FlyoutLayoutBehaviorSplitOnPortrait";
	public const string SetDetail1Button = "SetDetail1Button";
	public const string SetFlyout1Button = "SetFlyout1Button";
	public const string FlyoutLayoutBehaviorPopover = "FlyoutLayoutBehaviorPopover";

	public FlyoutPageFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(FlyoutPageFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_NavigationEvents()
	{
		App.WaitForElement("SetDetail1Button");
		App.Tap("SetDetail1Button");
		App.WaitForElement("BackToOriginalDetailButton1");
		App.Tap("BackToOriginalDetailButton1");
		App.WaitForElement("NavigatedToLabel");
		Assert.That(App.FindElement("NavigatedToLabel").GetText(), Is.EqualTo("NavigatedTo: Detail 1"));
		Assert.That(App.FindElement("NavigatingFromLabel").GetText(), Is.EqualTo("NavigatingFrom: Detail 1"));
		Assert.That(App.FindElement("NavigatedFromLabel").GetText(), Is.EqualTo("NavigatedFrom: Detail 1"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/31374, https://github.com/dotnet/maui/issues/31372
	[Test, Order(2)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsPresented()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsPresentedTrue);
		App.Tap(IsPresentedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Flyout Item 1");
		App.WaitForElement("Flyout Item 2");
		App.Tap("CloseFlyoutButton");
	}
#endif

#if TEST_FAILS_ON_WINDOWS // In windows default set as Split FlyoutLayoutBehavior
	[Test, Order(3)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsGestureEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGestureEnabledTrue);
		App.Tap(IsGestureEnabledTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		var detail = App.FindElement("DetailPageLabel").GetRect();
		var startX = detail.X + 5;
		var endX = detail.X + (detail.Width * 0.5);
		var y = detail.Y + (detail.Height / 2);
		bool flyoutOpened = false;
		for (int i = 0; i < 3 && !flyoutOpened; i++)
		{
			App.DragCoordinates((float)startX, (float)y, (float)endX, (float)y);
			try
			{
				App.WaitForElement("Flyout Item 1", timeout: TimeSpan.FromSeconds(2));
				flyoutOpened = true;
			}
			catch (Exception)
			{
				// not found yet, will retry
			}
		}
		Assert.That(flyoutOpened, Is.True, "Flyout did not open after multiple drag attempts");
		App.Tap("CloseFlyoutButton");
	}

	[Test, Order(4)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsGestureEnabledFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGestureEnabledFalse);
		App.Tap(IsGestureEnabledFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		var detail = App.FindElement("DetailPageLabel").GetRect();
		var startX = detail.X + 5;
		var endX = detail.X + (detail.Width * 0.5);
		var y = detail.Y + (detail.Height / 2);
		App.DragCoordinates((float)startX, (float)y, (float)endX, (float)y);
		App.WaitForNoElement("Flyout Item 1");
		App.WaitForNoElement("Flyout Item 2");
	}
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/31390
	[Test, Order(5)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsGestureEnabled_FlyoutBehaviorPopover()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGestureEnabledTrue);
		App.Tap(IsGestureEnabledTrue);
		App.WaitForElement(FlyoutLayoutBehaviorPopover);
		App.Tap(FlyoutLayoutBehaviorPopover);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		var detail = App.FindElement("DetailPageLabel").GetRect();
		var startX = detail.X + 5;
		var endX = detail.X + (detail.Width * 0.5);
		var y = detail.Y + (detail.Height / 2);
		bool flyoutOpened = false;
		for (int i = 0; i < 3 && !flyoutOpened; i++)
		{
			App.DragCoordinates((float)startX, (float)y, (float)endX, (float)y);
			try
			{
				App.WaitForElement("Flyout Item 1", timeout: TimeSpan.FromSeconds(2));
				flyoutOpened = true;
			}
			catch (Exception)
			{
				// not found yet, will retry
			}
		}
		Assert.That(flyoutOpened, Is.True, "Flyout did not open after multiple drag attempts");
		App.Tap("CloseFlyoutButton");
	}

	[Test, Order(6)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsGestureEnabledFalse_FlyoutBehaviorPopover()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGestureEnabledFalse);
		App.Tap(IsGestureEnabledFalse);
		App.WaitForElement(FlyoutLayoutBehaviorPopover);
		App.Tap(FlyoutLayoutBehaviorPopover);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		var detail = App.FindElement("DetailPageLabel").GetRect();
		var startX = detail.X + 5;
		var endX = detail.X + (detail.Width * 0.5);
		var y = detail.Y + (detail.Height / 2);
		App.DragCoordinates((float)startX, (float)y, (float)endX, (float)y);
		App.WaitForNoElement("Flyout Item 1");
		App.WaitForNoElement("Flyout Item 2");
	}
#endif

	[Test, Order(7)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsEnabledFalse);
		App.Tap(IsEnabledFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/26726
	[Test, Order(8)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsFlowDirectionRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link:  https://github.com/dotnet/maui/issues/31374, https://github.com/dotnet/maui/issues/31372
	[Test, Order(9)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsFlowDirectionRTLWithIsPresented()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(IsPresentedTrue);
		App.Tap(IsPresentedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //  Orientation not supported in Catalyst and Windows , Android Issue Link: https://github.com/dotnet/maui/issues/31374
	[Test, Order(10)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsPresented_OrientationLandscape()
	{
		App.SetOrientationLandscape();
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsPresentedTrue);
		App.Tap(IsPresentedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Flyout Item 1");
		App.WaitForElement("Flyout Item 2");
		App.Tap("CloseFlyoutButton");
		App.SetOrientationPortrait();
	}
#endif

	[Test, Order(11)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_Title()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TitleEntry);
		App.EnterText(TitleEntry, "New Title");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST// FlyoutLayoutBehavior is not changed in mobile platforms,  Issue Link: https://github.com/dotnet/maui/issues/16245
	[Test, Order(12)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_FlyoutLayoutBehavior_Split()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlyoutLayoutBehaviorSplit);
		App.Tap(FlyoutLayoutBehaviorSplit);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/31390
	[Test, Order(13)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_FlyoutLayoutBehavior_SplitOnPortrait()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlyoutLayoutBehaviorSplitOnPortrait);
		App.Tap(FlyoutLayoutBehaviorSplitOnPortrait);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/31390
	[Test, Order(14)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_FlyoutLayoutBehaviorPopover()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlyoutLayoutBehaviorPopover);
		App.Tap(FlyoutLayoutBehaviorPopover);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/20088
	[Test, Order(15)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_Detail()
	{
		App.WaitForElement(SetDetail1Button);
		App.Tap(SetDetail1Button);
		App.WaitForElement("Detail 1 - Content");
		App.WaitForElement("BackToOriginalDetailButton1");
		App.Tap("BackToOriginalDetailButton1");
	}

	[Test, Order(16)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_Flyout()
	{
		App.WaitForElement(SetFlyout1Button);
		App.Tap(SetFlyout1Button);
		App.WaitForElement("OpenFlyoutButton");
		App.Tap("OpenFlyoutButton");
		App.WaitForElement("Flyout 1 - Item 1");
		App.WaitForElement("Flyout 1 - Item 2");
		App.WaitForElement("BackToOriginalFlyoutButton1");
		App.Tap("BackToOriginalFlyoutButton1");
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Android Issue Link: https://github.com/dotnet/maui/issues/22116 , https://github.com/dotnet/maui/issues/15211
	// Windows Issue Link: https://github.com/dotnet/maui/issues/15211#issuecomment-1562587775 , https://github.com/dotnet/maui/issues/31390
	[Test, Order(17)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_DetailPageIconImageSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IconFontIconButton");
		App.Tap("IconFontIconButton");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_DetailPageIconImageSource_FlyoutLayoutBehavior()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IconFontIconButton");
		App.Tap("IconFontIconButton");
		App.WaitForElement(FlyoutLayoutBehaviorPopover);
		App.Tap(FlyoutLayoutBehaviorPopover);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/31390, https://github.com/dotnet/maui/issues/31374
	[Test, Order(19)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_FlyoutLayoutBehaviorPopover_IsPresented()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlyoutLayoutBehaviorPopover);
		App.Tap(FlyoutLayoutBehaviorPopover);
		App.WaitForElement(IsPresentedTrue);
		App.Tap(IsPresentedTrue);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Flyout Item 1");
		App.WaitForElement("Flyout Item 2");
		App.WaitForElement("CloseFlyoutButton");
		App.Tap("CloseFlyoutButton");
	}
#endif

	[Test, Order(20)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyDynamicFlyoutPage()
	{
		App.WaitForElement("GoToNewPageButton");
		App.Tap("GoToNewPageButton");
		App.WaitForElement("This is the detail of the new FlyoutPage");
		App.WaitForElement("CloseNewFlyoutPageButton");
		App.Tap("CloseNewFlyoutPageButton");
	}

	[Test, Order(21)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_BackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("BackgroundColorLightYellowButton");
		App.Tap("BackgroundColorLightYellowButton");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPage_IsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsVisibleFalse);
		App.Tap(IsVisibleFalse);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
}
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class AppThemeFeatureTests : UITest
{
	public const string AppThemeFeatureMatrix = "App Theme";
	public AppThemeFeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(AppThemeFeatureMatrix);
	}

	public void VerifyScreenshotWithPlatformCropping()
	{
#if IOS
        VerifyScreenshot(cropBottom: 1200); 
#else
		VerifyScreenshot();
#endif
	}

	[Test, Order(1)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_VerifyVisualState()
	{
		App.WaitForElement("DefaultLightThemeButton");
		App.Tap("DefaultLightThemeButton");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_VerifyVisualState()
	{
		App.WaitForElement("DefaultDarkThemeButton");
		App.Tap("DefaultDarkThemeButton");
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_CheckBox_VerifyVisualState()
	{
		App.WaitForElement("DefaultLightThemeButton");
		App.Tap("DefaultLightThemeButton");
		App.WaitForElement("AppThemePage");
		App.Tap("AppThemePage");
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("CheckBox");
		App.Tap("CheckBox");
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_CheckBox_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("CheckBox");
		App.Tap("CheckBox");
		App.Tap("CheckBox");
		VerifyScreenshot();
	}

	[Test, Order(5)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_DatePicker_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("DatePicker");
#if ANDROID
		App.Tap("DatePicker");
		App.Tap("22");
		App.WaitForElement("OK");
		App.Tap("OK");
#elif WINDOWS
		App.Tap("DatePicker");
		App.Tap("22");
#endif
		VerifyScreenshot();
	}

	[Test, Order(6)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_DatePicker_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("DatePicker");
#if ANDROID
		App.Tap("DatePicker");
		App.Tap("23");
		App.WaitForElement("OK");
		App.Tap("OK");
#elif WINDOWS
		App.Tap("DatePicker");
		App.Tap("23");
#endif
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_RadioButton_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_RadioButton_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("RadioButton_Cat");
		App.Tap("RadioButton_Cat");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST

	[Test, Order(9)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_Picker_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("Picker");
		App.Tap("Picker");
		App.WaitForElement("Blue Monkey");
		App.Tap("Blue Monkey");
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_Picker_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("Picker");
		App.Tap("Picker");
		App.WaitForElement("Howler Monkey");
		App.Tap("Howler Monkey");
		VerifyScreenshot();
	}
#endif

	[Test, Order(11)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_Slider_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("Slider");
		VerifyScreenshot();
	}

	[Test, Order(12)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_Slider_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("Slider");
		VerifyScreenshot();
	}

	[Test, Order(13)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_Switch_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("Switch");
		App.Tap("Switch");
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_Switch_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("Switch");
		App.Tap("Switch");
		App.Tap("Switch");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Issue Link - https://github.com/dotnet/maui/issues/30837

	[Test, Order(15)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_TimePicker_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("TimePicker");
#if ANDROID
		App.Tap("TimePicker");
		App.WaitForElement(AppiumQuery.ByAccessibilityId("6"));
		App.Tap(AppiumQuery.ByAccessibilityId("6"));
		App.WaitForElement("OK");
		App.Tap("OK");
#elif WINDOWS
		App.Tap("TimePicker");
		App.WaitForElement("5");
		App.Tap("5");
		App.WaitForElement("AcceptButton");
		App.Tap("AcceptButton");
#endif
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_TimePicker_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("TimePicker");
#if ANDROID
		App.Tap("TimePicker");
		App.WaitForElement(AppiumQuery.ByAccessibilityId("6"));
		App.Tap(AppiumQuery.ByAccessibilityId("6"));
		App.WaitForElement("OK");
		App.Tap("OK");
#elif WINDOWS
		App.Tap("TimePicker");
		App.WaitForElement("8");
		App.Tap("8");
		App.WaitForElement("AcceptButton");
		App.Tap("AcceptButton");
#endif
		VerifyScreenshot();
	}
#endif

	[Test, Order(17)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_SearchBar_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("SearchBar");
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_SearchBar_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("SearchBar");
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_Editor_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("Editor");
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_Editor_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("Editor");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //Issue Link - https://github.com/dotnet/maui/issues/19997

	[Test, Order(21)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_Entry_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("Entry");
		App.Tap("Entry");
		VerifyScreenshotWithPlatformCropping();
#if ANDROID
		if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
		}
#endif
	}

	[Test, Order(22)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_Entry_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("Entry");
		VerifyScreenshotWithPlatformCropping();
#if ANDROID
		if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
		}
#endif
	}
#endif

	[Test, Order(23)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_EntryAndPlaceholderColor_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("Entry");
		App.ClearText("Entry");
#if ANDROID
		if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
		}
#endif
		VerifyScreenshotWithPlatformCropping();
	}

	[Test, Order(24)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_EntryAndPlaceholderColor_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("Entry");
		VerifyScreenshotWithPlatformCropping();
	}

	[Test, Order(25)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_EditorAndPlaceholderColor_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("Editor");
		App.ClearText("Editor");
#if ANDROID
		if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
		}
#endif
		VerifyScreenshotWithPlatformCropping();
	}

	[Test, Order(26)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_EditorAndPlaceholderColor_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("Editor");
		VerifyScreenshotWithPlatformCropping();
	}

	[Test, Order(27)]
	[Category(UITestCategories.ViewBaseTests)]
	public void LightTheme_SearchBarAndPlaceholderColor_VerifyVisualState()
	{
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		App.WaitForElement("SearchBar");
		App.ClearText("SearchBar");
#if ANDROID
		if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
		}
#endif
		VerifyScreenshotWithPlatformCropping();
	}

	[Test, Order(28)]
	[Category(UITestCategories.ViewBaseTests)]
	public void DarkTheme_SearchBarAndPlaceholderColor_VerifyVisualState()
	{
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		App.WaitForElement("SearchBar");
		VerifyScreenshotWithPlatformCropping();
	}
}
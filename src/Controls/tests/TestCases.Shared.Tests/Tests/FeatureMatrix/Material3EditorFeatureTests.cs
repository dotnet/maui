// Material3 Editor tests use a separate HostApp page (EditorMaterial3ControlPage) that creates
// a plain Editor control instead of UITestEditor. This ensures the Material3 EditorHandler2
// is used instead of the M2 UITestEditorHandler.
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3EditorFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Editor Material3 Feature Matrix";

	const int CropBottomValue = 1750;

	public Material3EditorFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenAlingnedHorizontally()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenAlingnedVertically()
	{
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenFontFamilySetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenCharacterSpacingSetValues()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorHorizontalTextAlignmentBasedOnCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorVerticalTextAlignmentBasedOnCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorCharacterSpacingWhenFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorCharacterSpacingWhenMaxLengthSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLenght");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorHorizontalTextAlignmentWhenVerticalTextAlignmentSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenTextColorSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("Editor Control"); // Add an additional tap to make the Editor control unfocus.
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenFontSizeSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_ANDROID //related issue link: https://github.com/dotnet/maui/issues/29833
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenIsSpellCheckEnabledTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SpellCheckTrue");
		App.Tap("SpellCheckTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.ClearText("TestEditor");
		App.EnterText("TestEditor", "Testig");
		App.EnterText("TestEditor", " ");
		VerifyScreenshotWithKeyboardHandling();
	}
#endif

#if TEST_FAILS_ON_ANDROID //keybord type is not supported on Android, related issue: https://github.com/dotnet/maui/issues/26968
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenKeyboardTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Numeric");
		App.Tap("Numeric");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenReturnTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Search");
		App.Tap("Search");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorControlWhenFlowDirectionSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWhenFlowDirectionSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorControlWhenPlaceholderTextSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorControlWhenPlaceholderColorSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorTextWhenFontAttributesSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyzEditorTextWhenAutoSizeTextChangesSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AutoSizeTextChanges");
		App.Tap("AutoSizeTextChanges");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "When auto-resizing is enabled, the height of the Editor will increase when the user fills it with text, and the height will decrease as the user deletes text. This can be used to ensure that Editor objects in a DataTemplate.");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshotWithKeyboardHandling();
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyzEditorTextWhenAutoSizeDisabled()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "When auto-resizing is enabled, the height of the Editor will increase when the user fills it with text, and the height will decrease as the user deletes text. This can be used to ensure that Editor objects in a DataTemplate.");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshotWithKeyboardHandling();
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditor_WithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWithHorizontalAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWithVerticalAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VStart");
		App.Tap("VStart");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWithCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWithFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWithFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyEditorPlaceholderWithFontAttributes()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyzEditorPlaceholderWithAutoSizeDiabled()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "When auto-resizing is enabled, the height of the Editor will increase when the user fills it with text, and the height will decrease as the user deletes text. This can be used to ensure that Editor objects in a DataTemplate.");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Editor_VerifyzEditorPlaceholderWithAutoSizeTextChanges()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("AutoSizeTextChanges");
		App.Tap("AutoSizeTextChanges");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "When auto-resizing is enabled, the height of the Editor will increase when the user fills it with text, and the height will decrease as the user deletes text. This can be used to ensure that Editor objects in a DataTemplate.");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	/// <summary>
	/// Helper method to handle keyboard visibility and take a screenshot with appropriate cropping
	/// </summary>
	/// <param name="screenshotName">Optional name for the screenshot</param>
	void VerifyScreenshotWithKeyboardHandling(string? screenshotName = null)
	{
		if (App.IsKeyboardShown())
			App.DismissKeyboard();

		if (string.IsNullOrEmpty(screenshotName))
			VerifyScreenshot(cropBottom: CropBottomValue);
		else
			VerifyScreenshot(screenshotName, cropBottom: CropBottomValue);
	}
}
#endif

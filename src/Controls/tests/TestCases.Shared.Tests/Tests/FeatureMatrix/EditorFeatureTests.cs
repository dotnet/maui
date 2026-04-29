using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Editor)]
public class EditorFeatureTests : _GalleryUITest
{
	public const string EditorFeatureMatrix = "Editor Feature Matrix";

	public override string GalleryPageName => EditorFeatureMatrix;

#if IOS
	private const int CropBottomValue = 1550;
#elif ANDROID
	private const int CropBottomValue = 1150;
#elif WINDOWS
	private const int CropBottomValue = 400;
#else
	private const int CropBottomValue = 360;		
#endif

	public EditorFeatureTests(TestDevice device)
		: base(device)
	{
	}
	// Note: FontAutoScaling states cannot currently be reliably covered in CI environments, as system font scaling settings are not consistently supported or controllable in automated runs.
	[Test, Order(1)]
	public void VerifyEditorInitialEventStates()
	{
		App.WaitForElement("TestEditor");
		Assert.That(App.WaitForElement("FocusedLabel").GetText(), Is.EqualTo("Focused: Not triggered"));
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Not triggered"));
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Not triggered"));
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='', New='Test Editor'"));
	}

	[Test, Order(2)]
	public async Task VerifyEditorFocusedEvent()
	{
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		await Task.Delay(100);
#if ANDROID || IOS
		if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
		}
#endif
		Assert.That(App.WaitForElement("FocusedLabel").GetText(), Is.EqualTo("Focused: Event Triggered"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //when using App.EnterText() in a multiline field like an Editor, it types the text and then presses the Return key — which adds a new line.
	[Test, Order(3)]
	public void VerifyEditorTextChangedEvent()
	{

		App.WaitForElement("TestEditor");
		App.ClearText("TestEditor");
		App.EnterText("TestEditor", "New Text");
		App.DismissKeyboard();
#if !ANDROID
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='New Tex', New='New Text'"));
#else
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='', New='New Text'"));
#endif
	}
#endif

	[Test, Order(4)]
	public async Task VerifyEditorUnfocusedEvent()
	{
		App.WaitForElement("TestEditor");
		App.WaitForElement("SelectionLengthEntry");
		App.Tap("SelectionLengthEntry");
		await Task.Delay(100);
#if ANDROID
		App.DismissKeyboard();
#else
		App.WaitForElement("EditorControlTitleLabel");
		App.Tap("EditorControlTitleLabel");
#endif
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Event Triggered"));
	}

	[Test, Order(5)]
	public void VerifyEditorCompletedEvent()
	{
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		App.PressEnter();
#if ANDROID
		App.DismissKeyboard();
#else
		App.WaitForElement("EditorControlTitleLabel");
		App.Tap("EditorControlTitleLabel");
#endif
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Event Triggered"));
	}

	[Test, Order(6)]
	public void VerifyEditorTextWhenAlignedHorizontally()
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

	[Test, Order(7)]
	public void VerifyEditorTextWhenAlignedVertically()
	{
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Editor control even if its IsReadOnly property is set to true.
	[Test, Order(8)]
	public void VerifyEditorWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.EnterText("TestEditor", "123");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("Test Editor"));
	}
#endif

	[Test, Order(9)]
	public void VerifyEditorTextWhenFontFamilySetValue()
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

	[Test, Order(10)]
	public void VerifyEditorTextWhenCharacterSpacingSetValues()
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

	[Test, Order(11)]
	public void VerifyEditorHorizontalTextAlignmentBasedOnCharacterSpacing()
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

	[Test, Order(12)]
	public void VerifyEditorVerticalTextAlignmentBasedOnCharacterSpacing()
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

	[Test, Order(13)]
	public void VerifyEditorCharacterSpacingWhenFontFamily()
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

	[Test, Order(14)]
	public void VerifyEditorCharacterSpacingWhenMaxLengthSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLength");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(15)]
	public void VerifyEditorTextWhenMaxLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLength");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("Test E"));
		App.ClearText("TestEditor");
		App.EnterText("TestEditor", "1234567890");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("123456"));
	}

#if TEST_FAILS_ON_ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Editor control even if its IsReadOnly property is set to true.
	[Test, Order(16)]
	public void VerifyEditorMaxLengthWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLength");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.EnterText("TestEditor", "123");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("Test E"));
	}
#endif

	[Test, Order(17)]
	public void VerifyEditorHorizontalTextAlignmentWhenVerticalTextAlignmentSet()
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

	[Test, Order(18)]
	public void VerifyEditorTextWhenTextColorSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("EditorControlTitleLabel"); // Add an additional tap to make the Editor control unfocus.
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(19)]
	public void VerifyEditorTextColorSetDefaultValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorBlue");
		App.Tap("TextColorBlue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("EditorControlTitleLabel"); // Add an additional tap to make the Editor control unfocus.
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorDefault");
		App.Tap("TextColorDefault");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("EditorControlTitleLabel"); // Add an additional tap to make the Editor control unfocus.
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(20)]
	public void VerifyEditorTextWhenFontSizeSetCorrectly()
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

	[Test, Order(21)]
	[Ignore("Fails on all platforms, related issue link: https://github.com/dotnet/maui/issues/29833")]
	public void VerifyEditorTextWhenIsTextPredictionEnabledTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextPredictionTrue");
		App.Tap("TextPredictionTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.ClearText("TestEditor");
		App.EnterText("TestEditor", "Testig");
		App.EnterText("TestEditor", " ");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("Testing "));
	}

	[Test, Order(22)]
	[Ignore("Fails on all platforms, related issue link: https://github.com/dotnet/maui/issues/29833")]
	public void VerifyEditorTextWhenIsSpellCheckEnabledTrue()
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

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID  // On iOS and Maccatalyst While updating CursorPosition and SelectionLength, the Editor text gets deleted. & On Android, changing CursorPosition keeps the cursor visible even when IsCursorVisible is set to false, which is unexpected.
	[Test, Order(23)]
	public void VerifyEditorTextWhenSelectionLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SelectionLengthEntry");
		App.ClearText("SelectionLengthEntry");
		App.EnterText("SelectionLengthEntry", "5");
		App.DismissKeyboard();
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("0"));
	}

	[Test, Order(24)]
	public void VerifyEditorTextWhenCursorPositionValueSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CursorPositionEntry");
		App.ClearText("CursorPositionEntry");
		App.EnterText("CursorPositionEntry", "5");
		App.DismissKeyboard();
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("5"));
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("11"));
	}

	[Test, Order(25)]
	public void VerifyEditorCursorPositionWhenSelectionLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CursorPositionEntry");
		App.ClearText("CursorPositionEntry");
		App.EnterText("CursorPositionEntry", "3");
		App.WaitForElement("SelectionLengthEntry");
		App.ClearText("SelectionLengthEntry");
		App.EnterText("SelectionLengthEntry", "5");
		App.DismissKeyboard();
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("3"));
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("5"));
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("11"));
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("0"));
	}
#endif

#if TEST_FAILS_ON_WINDOWS // On Windows, cursor position and selection length still work when the Entry is set to read-only.
	[Test, Order(26)]
	public void VerifyEditorCursorPositionWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("0"));
	}

	[Test, Order(27)]
	public void VerifyEditorSelectionLengthWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SelectionLengthEntry");
		App.ClearText("SelectionLengthEntry");
		App.EnterText("SelectionLengthEntry", "3");
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("3"));
	}
#endif

	[Test, Order(28)]
	[Ignore("Fails on all platforms, the keybord type is not supported on Windows and Maccatalyst platforms & On Android & IOS related issue:https://github.com/dotnet/maui/issues/26968")]
	public void VerifyEditorTextWhenKeyboardTypeSet()
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

#if TEST_FAILS_ON_ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsEnabled property is set to false.
	[Test, Order(29)]
	public void VerifyEditorControlWhenIsEnabledFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("EnabledFalse");
		App.Tap("EnabledFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.EnterText("TestEditor", "123");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("Test Editor"));
	}
#endif

	[Test, Order(30)]
	public void VerifyEditorControlWhenIsVisibleFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VisibleFalse");
		App.Tap("VisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("TestEditor");
	}

	[Test, Order(31)]
	public void VerifyEditorControlWhenFlowDirectionSet()
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

#if TEST_FAILS_ON_WINDOWS //On Windows, the placeholder is not visible because its text alignment is reset to default values when navigating to the page. This issue occurs only in the Host App.
	[Test, Order(32)]
	public void VerifyEditorPlaceholderWhenFlowDirectionSet()
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


	[Test, Order(33)]
	public void VerifyEditorControlWhenPlaceholderTextSet()
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

	[Test, Order(34)]
	public void VerifyEditorControlWhenPlaceholderColorSet()
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

	[Test, Order(35)]
	public void VerifyEditorControlWhenPlaceholderColorSetDefaultValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorDefault");
		App.Tap("PlaceholderColorDefault");
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
#endif

	[Test, Order(36)]
	public void VerifyEditorTextDynamicChange()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "New Text Changed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("New Text Changed"));
	}

	[Test, Order(37)]
	public void VerifyEditorTextWhenFontAttributesSet()
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

	[Test, Order(38)]
	public void VerifyEditorTextWhenTextTransformUppercase()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextTransformUppercase");
		App.Tap("TextTransformUppercase");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("TEST EDITOR"));
	}

	[Test, Order(39)]
	public void VerifyEditorTextWhenTextTransformLowercase()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextTransformLowercase");
		App.Tap("TextTransformLowercase");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("test editor"));
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29812
	[Test, Order(40)]
	public void VerifyEditorWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(41)]
	public void VerifyEditorPlaceholderWithShadow()
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
#endif

#if TEST_FAILS_ON_WINDOWS //On Windows, the placeholder is not visible because its text alignment is reset to default values when navigating to the page. This issue occurs only in the Host App.
	[Test, Order(42)]
	public void VerifyEditorPlaceholderWithHorizontalAlignment()
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

	[Test, Order(43)]
	public void VerifyEditorPlaceholderWithVerticalAlignment()
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

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30071
	[Test, Order(44)]
	public void VerifyEditorPlaceholderWithCharacterSpacing()
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
#endif

	[Test, Order(45)]
	public void VerifyEditorPlaceholderWithFontFamily()
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

	[Test, Order(46)]
	public void VerifyEditorPlaceholderWithFontSize()
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

	[Test, Order(47)]
	public void VerifyEditorPlaceholderWithFontAttributes()
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
#endif

	[Test, Order(48)]
	public void VerifyEditorWhenHeightRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "100");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(49)]
	public void VerifyEditorWhenWidthRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "100");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(50)]
	public void VerifyEditorWhenHeightAndWidthRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "100");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "80");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30571 and the placeholder is not visible because its text alignment is reset to default values when navigating to the page. This issue occurs only in the Host App on windows.
	[Test, Order(51)]
	public void VerifyEditorPlaceholderWithAutoSizeDisabled()
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
		App.ClearText("TestEditor");
	}
#endif

	[Test, Order(52)]
	public void VerifyEditorTextWhenAutoSizeDisabled()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "When auto-resizing is enabled, the height of the Editor will increase when the user fills it with text, and the height will decrease as the user deletes text. This can be used to ensure that Editor objects in a DataTemplate.");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
		App.ClearText("TestEditor");
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30571 and the placeholder is not visible because its text alignment is reset to default values when navigating to the page. This issue occurs only in the Host App on windows.
	[Test, Order(53)]
	public void VerifyEditorPlaceholderWithAutoSizeTextChanges()
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
		App.ClearText("TestEditor");
	}
#endif

	[Test, Order(54)]
	public void VerifyEditorTextWhenAutoSizeTextChangesSet()
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
		VerifyScreenshot(cropBottom: CropBottomValue);
		App.ClearText("TestEditor");
	}

	[Test, Order(55)]
	public void VerifyEditorTextWhenAutoSizeTextChangesSetWithShortShrinkText()
	{
		Exception? exception = null;
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
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "VerifyEditorTextWhenAutoSizeTextChangesSetWithShortShrinkText_LongText");
		App.ClearText("TestEditor");
		App.EnterText("TestEditor", "Short text");
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "VerifyEditorTextWhenAutoSizeTextChangesSetWithShortShrinkText_ShortText");
		if (exception != null)
		{
			throw exception;
		}
	}

	[Test, Order(56)]
	public void VerifyEditorTextWhenAutoSizeTextChangesSetWithHeightRequest()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "100");
		App.WaitForElement("AutoSizeTextChanges");
		App.Tap("AutoSizeTextChanges");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(57)]
	public void VerifyEditorTextWhenFontAttributesBoldAndItalicSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(58)]
	public void VerifyEditorPlaceholderTextWhenFontAttributesBoldAndItalicSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
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

	[Test, Order(59)]
	public void VerifyEditorWhenOpacitySet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(60)]
	public void VerifyEditorWhenOpacityResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "1.0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(61)]
	public void VerifyEditorWhenOpacitySetToZero()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EditorControlTitleLabel");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(62)]
	public void VerifyEditorWhenBackgroundColorSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(63)]
	public void VerifyEditorBackgroundColorWithTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorLightBlue");
		App.Tap("BackgroundColorLightBlue");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(64)]
	public void VerifyEditorBackgroundColorWithPlaceholder()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorLightBlue");
		App.Tap("BackgroundColorLightBlue");
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

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //related issue link: https://github.com/dotnet/maui/issues/34611
	[Test, Order(65)]
	public void VerifyEditorBackgroundColorResetToNone()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorNone");
		App.Tap("BackgroundColorNone");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
#endif

	/// <summary>
	/// Helper method to handle keyboard visibility and take a screenshot with appropriate cropping
	/// </summary>
	/// <param name="screenshotName">Optional name for the screenshot</param>
	private void VerifyScreenshotWithKeyboardHandling(string? screenshotName = null)
	{
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif

		// Using cropping instead of DismissKeyboard() on iOS to maintain focus during testing
		if (string.IsNullOrEmpty(screenshotName))
			VerifyScreenshot(cropBottom: CropBottomValue);
		else
			VerifyScreenshot(screenshotName, cropBottom: CropBottomValue);
	}

	/// <summary>
	/// Helper method to handle keyboard visibility and set exception if screenshot verification fails
	/// </summary>
	/// <param name="exception">Reference to exception variable</param>
	/// <param name="screenshotName">Name for the screenshot</param>
	private void VerifyScreenshotWithKeyboardHandlingOrSetException(ref Exception? exception, string screenshotName)
	{
#if ANDROID
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif
		VerifyScreenshotOrSetException(ref exception, screenshotName, cropBottom: CropBottomValue);

	}
}

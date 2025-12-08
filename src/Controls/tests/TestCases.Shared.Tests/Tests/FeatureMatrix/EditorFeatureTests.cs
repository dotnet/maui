using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Editor)]
public class EditorFeatureTests : UITest
{
	public const string EditorFeatureMatrix = "Editor Feature Matrix";

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

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(EditorFeatureMatrix);
	}

	[Test, Order(0)]
	public void VerifyEditorInitialEventStates()
	{
		App.WaitForElement("TestEditor");
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Not triggered"));
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Not triggered"));
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='', New='Test Editor'"));
	}

	[Test, Order(4)]
	public void VerifyEditorCompletedEvent()
	{
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		App.PressEnter();
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Event Triggered"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //when using App.EnterText() in a multiline field like an Editor, it types the text and then presses the Return key — which adds a new line.
	[Test, Order(2)]
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

	[Test, Order(1)]
	public async Task VerifyEditorFocusedEvent()
	{
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		await Task.Delay(100);
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("FocusedLabel").GetText(), Is.EqualTo("Focused: Event Triggered"));
	}

	[Test, Order(3)]
	public async Task VerifyEditorUnfocusedEvent()
	{
		App.WaitForElement("TestEditor");
		App.WaitForElement("SelectionLengthEntry");
		App.Tap("SelectionLengthEntry");
		await Task.Delay(100);
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Event Triggered"));
	}

	[Test]
	public void VerifyEditorTextWhenAlingnedHorizontally()
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
	public void VerifyEditorTextWhenAlingnedVertically()
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
	[Test]
	public void VerifyTextEditorWhenSetAsReadOnly()
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

	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
	public void VerifyEditorCharacterSpacingWhenMaxLengthSet()
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
	public void VerifyEditorTextWhenMaxLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLenght");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		Assert.That(App.WaitForElement("TestEditor").GetText(), Is.EqualTo("Test E"));
	}

#if TEST_FAILS_ON_ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Editor control even if its IsReadOnly property is set to true.
	[Test]
	public void VerifyEditorMaxLengthWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLenght");
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

	[Test]
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

	[Test]
	public void VerifyEditorTextWhenTextColorSetCorrectly()
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

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29833
		[Test]
		public void VerifyEditorTextWhenIsTextPredictionEnabledTrueOrFalse()
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

		[Test]
		public void VerifyEditorTextWhenIsSpellCheckEnabledTrueOrFalse()
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

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID  // On iOS and Maccatalyst While updating CursorPosition and SelectionLength, the Editor text gets deleted. & On Android, changing CursorPosition keeps the cursor visible even when IsCursorVisible is set to false, which is unexpected.
	[Test, Order(5)]
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

	[Test]
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
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("11"));
	}

	[Test]
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
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("11"));
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("0"));
	}
#endif

#if TEST_FAILS_ON_WINDOWS // On Windows, cursor position and selection length still work when the Entry is set to read-only.
	[Test]
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

	[Test]
	public void VerifyEditorSelectionLenghtWhenIsReadOnlyTrue()
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

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS //keybord type is not supported on Windows and Maccatalyst platforms & On Android & IOS related issue:https://github.com/dotnet/maui/issues/26968
	[Test]
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
		VerifyScreenshot();
	}

	[Test]
	public void VerifyEditorTextWhenReturnTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Search");
		App.Tap("Search");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.Tap("TestEditor");
		VerifyScreenshot();
	}

#endif

#if TEST_FAILS_ON_ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsEnabled property is set to false.
	[Test]
	public void VerifyEditorControlWhenIsEnabledTrueOrFalse()
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

	[Test]
	public void VerifyEditorControlWhenIsVisibleTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEditor");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VisibleFalse");
		App.Tap("VisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("TestEditor");
	}

	[Test]
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
	[Test]
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


	[Test]
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

	[Test]
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
#endif

	[Test]
	public void VerifyEditorWhenTextChanged()
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

	[Test]
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

	[Test]
	public void VerifyEditorTextWhenTextTransFormSet()
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

	[Test]
	public void VerifyzEditorTextWhenAutoSizeTextChangesSet()
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
	public void VerifyzEditorTextWhenAutoSizeDisabled()
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

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29812
	[Test]
	public void VerifyEditor_WithShadow()
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
	[Test]
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

	[Test]
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
	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/30571
	[Test]
	public void VerifyzEditorPlaceholderWithAutoSizeDiabled()
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
	public void VerifyzEditorPlaceholderWithAutoSizeTextChanges()
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
#endif
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
}
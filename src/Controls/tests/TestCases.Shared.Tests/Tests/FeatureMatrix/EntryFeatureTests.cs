using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Entry)]
public class EntryFeatureTests : _GalleryUITest
{
	public const string EntryFeatureMatrix = "Entry Feature Matrix";

	public override string GalleryPageName => EntryFeatureMatrix;

#if IOS
	private const int CropBottomValue = 1550;
#elif ANDROID
	private const int CropBottomValue = 1150;
#elif WINDOWS
	private const int CropBottomValue = 400;
#else
	private const int CropBottomValue = 360;
#endif

	public EntryFeatureTests(TestDevice device)
		: base(device)
	{
	}
	// Note: FontAutoScaling states cannot currently be reliably covered in CI environments, as system font scaling settings are not consistently supported or controllable in automated runs.
	[Test, Order(1)]
	public void VerifyInitialEventStates()
	{
		App.WaitForElement("TestEntry");
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Not triggered"));
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Not triggered"));
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='', New='Test Entry'"));
	}

	[Test, Order(2)]
	public void VerifyEntryFocusedEvent()
	{
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		Assert.That(App.WaitForElement("FocusedLabel").GetText(), Is.EqualTo("Focused: Event Triggered"));
	}

	[Test, Order(3)]
	public void VerifyEntryTextChangedEvent()
	{

		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.EnterText("TestEntry", "New Text");

#if !ANDROID
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='New Tex', New='New Text'"));
#else
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='', New='New Text'"));
#endif
	}

	[Test, Order(4)]
	public void VerifyEntryUnfocusedEvent()
	{
		App.WaitForElement("TestEntry");
		App.WaitForElement("SelectionLengthEntry");
		App.Tap("SelectionLengthEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Event Triggered"));
	}

	[Test, Order(5)]
	public void VerifyEntryCompletedEvent()
	{
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.PressEnter();
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Event Triggered"));
	}

#if TEST_FAILS_ON_CATALYST //In CI, the clear button is visible even when the text is empty, which is not the expected behavior.
	[Test, Order(6)]
	public void VerifyClearButtonVisibilityWhenTextPresentOrEmpty()
	{
		Exception? exception = null;
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "ClearButtonVisibilityButton_TextPresent");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "ClearButtonVisibilityButton_TextEmpty");
		if (exception != null)
		{
			throw exception;
		}
	}
#endif

	[Test, Order(7)]
	public void VerifyClearButtonVisibilityWhenTextAlignedHorizontally()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandling();
	}

	[Test, Order(8)]
	public void VerifyClearButtonVisibilityWhenTextAlignedVertically()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandling();
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test, Order(9)]
	public void VerifyClearButtonVisibilityWhenIsPasswordTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandling();
	}
#endif

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test, Order(10)]
	public void VerifyClearButtonVisibilityWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandling();
	}
#endif

	[Test, Order(11)]
	public void VerifyClearVisibilityButtonWhenTextColorChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandling();
	}

	[Test, Order(12)]
	public void VerifyTextWhenClearButtonVisibleSetNever()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ClearButtonNever");
		App.Tap("ClearButtonNever");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandling();
	}


	[Test, Order(13)]
	public void VerifyTextWhenAlignedHorizontally()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(14)]
	public void VerifyTextWhenAlignedVertically()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test, Order(15)]
	public void VerifyTextEntryWhenSetAsReadOnly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		// On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("TestEntry", "123");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Test Entry"));
	}
#endif

	[Test, Order(16)]
	public void VerifyTextWhenFontFamilySetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(17)]
	public void VerifyTextWhenCharacterSpacingSetValues()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(18)]
	public void VerifyHorizontalTextAlignmentBasedOnCharacterSpacing()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(19)]
	public void VerifyVerticalTextAlignmentBasedOnCharacterSpacing()
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
		App.WaitForElement("TestEntry");
#if WINDOWS // On Windows, the Entry control cursor does not disappear when IsCursorVisible is set to false
		App.WaitForElement("TextChangedLabel");
		App.Tap("TextChangedLabel");
#endif
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test, Order(20)]
	public void VerifyIsPasswordBasedOnCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
#endif

	[Test, Order(21)]
	public void VerifyCharacterSpacingWhenFontFamily()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test, Order(22)]
	public void VerifyTextWhenIsPasswordTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(23)]
	public void VerifyIsPasswordBasedOnVerticalTextAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(24)]
	public void VerifyIsPasswordBasedOnHorizontalTextAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(25)]
	public void VerifyIsPasswordWhenMaxLengthSetValue()
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
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
#endif

	[Test, Order(26)]
	public void VerifyCharacterSpacingWhenMaxLengthSet()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(27)]
	public void VerifyTextWhenMaxLengthSetValue()
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
		App.WaitForElement("TestEntry");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Test E"));
		App.ClearText("TestEntry");
		App.EnterText("TestEntry", "1234567890");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("123456"));
	}

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test, Order(28)]
	public void VerifyMaxLengthWhenIsReadOnlyTrue()
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
		App.WaitForElement("TestEntry");
		// On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("TestEntry", "123");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Test E"));
	}
#endif

	[Test, Order(29)]
	public void VerifyHorizontalTextAlignmentWhenVerticalTextAlignmentSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(30)]
	public void VerifyTextWhenTextColorSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(31)]
	public void VerifyTextColorResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");

		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorDefault");
		App.Tap("TextColorDefault");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(32)]
	public void VerifyTextWhenFontSizeSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test, Order(33)]
	public void VerifyIsPasswordWhenFontSizeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
#endif

	[Test, Order(34)]
	[Ignore("This test is currently failing on All platforms. See issue link: https://github.com/dotnet/maui/issues/29833")]
	public void VerifyTextWhenIsTextPredictionEnabledTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextPredictionTrue");
		App.Tap("TextPredictionTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.EnterText("TestEntry", "Testig");
		App.EnterText("TestEntry", " ");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Testing "));
	}

	[Test, Order(35)]
	[Ignore("This test is currently failing on All platforms. See issue link: https://github.com/dotnet/maui/issues/29833")]
	public void VerifyTextWhenIsSpellCheckEnabledTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SpellCheckTrue");
		App.Tap("SpellCheckTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.EnterText("TestEntry", "Testig");
		App.EnterText("TestEntry", " ");
		VerifyScreenshotWithKeyboardHandling();
	}

	[Test, Order(36)]
	public void VerifyTextWhenSelectionLengthSetValue()
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
		App.DismissKeyboard();
		App.WaitForElement("SelectionLengthEntry");
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("5"));
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("0"));
	}

	[Test, Order(37)]
	public void VerifyTextWhenCursorPositionValueSet()
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
		App.DismissKeyboard();
		App.WaitForElement("CursorPositionEntry");
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("5"));
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("10"));
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test, Order(38)]
	public void VerifyIsPasswordWhenCursorPositionValueSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CursorPositionEntry");
		App.ClearText("CursorPositionEntry");
		App.EnterText("CursorPositionEntry", "5");
		App.DismissKeyboard();
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.DismissKeyboard();
		App.WaitForElement("CursorPositionEntry");
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("5"));
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("10"));
	}
#endif

	[Test, Order(39)]
	public void VerifyCursorPositionWhenSelectionLengthSetValue()
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
		App.DismissKeyboard();
		App.WaitForElement("CursorPositionEntry");
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("3"));
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("5"));
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("10"));
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // On Windows, cursor position and selection length still work when the Entry is set to read-only  
	//On android After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test, Order(40)]
	public void VerifyCursorPositionWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("0"));
	}

	[Test, Order(41)]
	public void VerifySelectionLengthWhenIsReadOnlyTrue()
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
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("3"));
	}
#endif

	[Test, Order(42)]
	public void VerifyTextWhenReturnCommandAndReturnCommandParameter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.EnterText("TestEntry", "Test");
		App.Tap("TestEntry");
		App.PressEnter();
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Command Executed with Parameter"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID //In Android related issue:https://github.com/dotnet/maui/issues/26968 and In mac and Windows keybord type is not supported.
	[Test, Order(43)]
	public void VerifyTextWhenKeyboardTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Numeric");
		App.Tap("Numeric");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshot();
	}

	[Test, Order(44)]
	public void VerifyTextWhenReturnTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Search");
		App.Tap("Search");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsEnabled property is set to false.
	[Test, Order(45)]
	public void VerifyEntryControlWhenIsEnabledTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.EnterText("TestEntry", "123");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("123"));
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("EnabledFalse");
		App.Tap("EnabledFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.EnterText("TestEntry", "123");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Test Entry"));

	}
#endif

	[Test, Order(46)]
	public void VerifyEntryControlWhenIsVisibleFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VisibleFalse");
		App.Tap("VisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("TestEntry");
	}

	[Test, Order(47)]
	public void VerifyEntryControlWhenFlowDirectionSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(48)]
	public void VerifyPlaceholderWhenFlowDirectionSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}


	[Test, Order(49)]
	public void VerifyEntryControlWhenPlaceholderTextSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.DismissKeyboard();
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(50)]
	public void VerifyEntryControlWhenPlaceholderColorSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(51)]
	public void VerifyPlaceholderColorResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");

		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorDefault");
		App.Tap("PlaceholderColorDefault");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(52)]
	public void VerifyEntryWhenTextDynamicallyUpdated()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "New Text Changed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("New Text Changed"));
	}

	[Test, Order(53)]
	public void VerifyTextWhenFontAttributesSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(54)]
	public void VerifyTextWhenTextTransformSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextTransformUppercase");
		App.Tap("TextTransformUppercase");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("TEST ENTRY"));
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29812
	[Test, Order(55)]
	public void VerifyEntry_WithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(56)]
	public void VerifyPlaceholderWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
#endif

	[Test, Order(57)]
	public void VerifyPlaceholderWithClearButtonVisible()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandling("PlaceholderWithClearButtonVisible");
	}

	[Test, Order(58)]
	public void VerifyPlaceholderWithPasswordTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your password");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(59)]
	public void VerifyPlaceholderWithHorizontalAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(60)]
	public void VerifyPlaceholderWithVerticalAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VStart");
		App.Tap("VStart");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30071
	[Test, Order(61)]
	public void VerifyPlaceholderWithCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
#endif

	[Test, Order(62)]
	public void VerifyPlaceholderWithFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(63)]
	public void VerifyPlaceholderWithFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(64)]
	public void VerifyPlaceholderWithFontAttributes()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(65)]
	public void VerifyEntryWhenWidthRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WidthRequest");
		App.ClearText("WidthRequest");
		App.EnterText("WidthRequest", "150");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(66)]
	public void VerifyEntryWhenHeightRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequest");
		App.ClearText("HeightRequest");
		App.EnterText("HeightRequest", "80");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(67)]
	public void VerifyEntryWhenHeightRequestAndWidthRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequest");
		App.ClearText("HeightRequest");
		App.EnterText("HeightRequest", "100");
		App.WaitForElement("WidthRequest");
		App.ClearText("WidthRequest");
		App.EnterText("WidthRequest", "150");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(68)]
	public void VerifyEntryWhenOpacitySet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(69)]
	public void VerifyEntryWhenOpacityResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "1.0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(70)]
	public void VerifyEntryWhenOpacitySetToZero()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(71)]
	public void VerifyEntryWhenBackgroundColorSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorLightBlue");
		App.Tap("BackgroundColorLightBlue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
	[Test, Order(72)]
	public void VerifyTextWhenBoldAndItalicFontAttributesSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(73)]
	public void VerifyPlaceholderTextWhenBoldAndItalicFontAttributesSet()
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
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.DismissKeyboard();
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(74)]
	public void VerifyEntryBackgroundColorWithTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(75)]
	public void VerifyEntryBackgroundColorWithPlaceholderText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorLightBlue");
		App.Tap("BackgroundColorLightBlue");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.DismissKeyboard();
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(76)]
	public void VerifyEntryBackgroundColorWithPlaceholderColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //related issue link: https://github.com/dotnet/maui/issues/34611
	[Test, Order(77)]
	public void VerifyEntryBackgroundColorResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorDefault");
		App.Tap("BackgroundColorDefault");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
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

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

	[Test, Order(0)]
	public void VerifyInitialEventStates()
	{
		App.WaitForElement("TestEntry");
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Not triggered"));
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Not triggered"));
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='', New='Test Entry'"));
	}

	[Test, Order(4)]
	public void VerifyEntryCompletedEvent()
	{
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.PressEnter();
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Event Triggered"));
	}

	[Test, Order(2)]
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


	[Test, Order(1)]
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
	public void VerifyEntryUnfocusedEvent()
	{
		App.WaitForElement("TestEntry");
		App.WaitForElement("SelectionLengthEntry");
		App.Tap("SelectionLengthEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Event Triggered"));
	}

	[Test]
	public void VerifyClearButtonVisiblityWhenTextPresentOrEmpty()
	{
		Exception? exception = null;
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "ClearButtonVisiblityButton_TextPresent");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "ClearButtonVisiblityButton_TextEmpty");
		if (exception != null)
		{
			throw exception;
		}
	}

	[Test]
	public void VerifyClearButtonVisiblityWhenTextAlignedHorizontally()
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

	[Test]
	public void VerifyClearButtonVisiblityWhenTextAlignedVertically()
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
	[Test]
	public void VerifyClearButtonVisiblityWhenIsPasswordTrueOrFalse()
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
	[Test]
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

	[Test]
	public void VerifyClearVisiblityButtonWhenTextColorChanged()
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

	[Test]
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


	[Test]
	public void VerifyTextWhenAlingnedHorizontally()
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

	[Test]
	public void VerifyTextWhenAlingnedVertically()
	{
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test]
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

	[Test]
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
	[Test]
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

	[Test]
	public void VerifyIsPasswordBasedOnVerticalTextAlignment()
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

	[Test]
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

	[Test]
	public void VerifyIsPasswordWhenMaxLenghtSetValue()
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
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
#endif

	[Test]
	public void VerifyCharacterSpacingWhenMaxLengthSet()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test]
	public void VerifyTextWhenMaxLengthSetValue()
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
		App.WaitForElement("TestEntry");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Test E"));
	}

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test]
	public void VerifyMaxLengthWhenIsReadOnlyTrue()
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
		App.WaitForElement("TestEntry");
		// On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("TestEntry", "123");
		Assert.That(App.WaitForElement("TestEntry").GetText(), Is.EqualTo("Test E"));

	}
#endif

	[Test]
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

	[Test]
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

	[Test]
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
	[Test]
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

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29833
		[Test]
		public void VerifyTextWhenIsTextPredictionEnabledTrueOrFalse()
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

		[Test]
		public void VerifyTextWhenIsSpellCheckEnabledTrueOrFalse()
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
#endif

	[Test]
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
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("0"));
	}

	[Test]
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
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("10"));
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test]
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
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("10"));
	}
#endif

	[Test]
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
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		Assert.That(App.WaitForElement("CursorPositionEntry").GetText(), Is.EqualTo("10"));
		Assert.That(App.WaitForElement("SelectionLengthEntry").GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // On Windows, cursor position and selection length still work when the Entry is set to read-only  
	//On android After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test]
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

	[Test]
	public void VerifySelectionLenghtWhenIsReadOnlyTrue()
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

	[Test]
	public void VerifyTextWhenReturmCommandAndReturnCommandParameter()
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

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //keybord type is not supported on Windows and Maccatalyst platforms
	[Test]
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
		VerifyScreenshotWithKeyboardHandling();
	}

#if TEST_FAILS_ON_ANDROID //related issue:https://github.com/dotnet/maui/issues/26968
	[Test]
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
		VerifyScreenshotWithKeyboardHandling();
	}

#endif
#endif

#if TEST_FAILS_ON_ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsEnabled property is set to false.
	[Test]
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

	[Test]
	public void VerifyEntryControlWhenIsVisibleTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VisibleFalse");
		App.Tap("VisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("TestEntry");
	}

	[Test]
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

	[Test]
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


	[Test]
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

	[Test]
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

	[Test]
	public void VerifyEntryWhenTextChanged()
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

	[Test]
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

	[Test]
	public void VerifyTextWhenTextTransFormSet()
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
	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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
	[Test]
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

	[Test]
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

	[Test]
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

	[Test]
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
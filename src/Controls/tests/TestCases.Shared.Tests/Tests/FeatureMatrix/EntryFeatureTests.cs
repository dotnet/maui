using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class EntryFeatureTests : UITest
{
	public const string EntryFeatureMatrix = "Entry Feature Matrix";
	private const int iOSKeyboardCropHeight = 1650;

	public EntryFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(EntryFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.Entry)]
	public void VerifyClearButtonVisiblityWhenTextPresentOrEmpty()
	{
		Exception? exception = null;
		App.WaitForElement("Entry Control");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "ClearButtonVisiblityButton_TextPresent");
		App.WaitForElement("EntryText");
		App.ClearText("EntryText");
		VerifyScreenshotWithKeyboardHandlingOrSetException(ref exception, "ClearButtonVisiblityButton_TextEmpty");
		if (exception != null)
		{
			throw exception;
		}
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyClearButtonVisiblityWhenTextAlignedHorizontally()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyClearButtonVisiblityWhenTextAlignedVertically()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyClearButtonVisiblityWhenIsPasswordTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}
#endif

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyClearButtonVisibilityWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}
#endif

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyClearVisiblityButtonWhenTextColorChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenClearButtonVisibleSetNever()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ClearButtonNever");
		App.Tap("ClearButtonNever");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}


	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenAlingnedHorizontally()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenAlingnedVertically()
	{
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextEntryWhenSetAsReadOnly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		// On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
	}
#endif

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenFontFamilySetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenCharacterSpacingSetValues()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
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
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
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
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test]
	[Category(UITestCategories.Entry)]
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
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.Entry)]
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
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenIsPasswordTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyIsPasswordBasedOnVerticalTextAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
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
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyIsPasswordWhenMaxLenghtSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyCharacterSpacingWhenMaxLengthSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenMaxLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test E"));
	}

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyMaxLengthWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		// On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test E"));

	}
#endif

	[Test]
	[Category(UITestCategories.Entry)]
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
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenTextColorSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenFontSizeSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyIsPasswordWhenFontSizeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29833
		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenIsTextPredictionEnabledTrueOrFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextPredictionTrue");
			App.Tap("TextPredictionTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.ClearText("EntryText");
			App.EnterText("EntryText", "Testig");
			App.EnterText("EntryText", " ");
			Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Testing "));
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenIsSpellCheckEnabledTrueOrFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("SpellCheckTrue");
			App.Tap("SpellCheckTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.ClearText("EntryText");
			App.EnterText("EntryText", "Testig");
			App.EnterText("EntryText", " ");
			VerifyScreenshotWithKeyboardHandling();
		}
#endif

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenSelectionLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SelectionLength");
		App.ClearText("SelectionLength");
		App.EnterText("SelectionLength", "5");
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		Assert.That(App.WaitForElement("SelectionLength").GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenCursorPositionValueSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CursorPosition");
		App.ClearText("CursorPosition");
		App.EnterText("CursorPosition", "5");
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		Assert.That(App.WaitForElement("CursorPosition").GetText(), Is.EqualTo("10"));
	}

#if TEST_FAILS_ON_IOS // When taking a screenshot of a password field (<Entry IsPassword="true" />), iOS hides the password text for security reasons.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyIsPasswordWhenCursorPositionValueSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CursorPosition");
		App.ClearText("CursorPosition");
		App.EnterText("CursorPosition", "5");
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		Assert.That(App.WaitForElement("CursorPosition").GetText(), Is.EqualTo("10"));
	}
#endif

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyCursorPositionWhenSelectionLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("CursorPosition");
		App.ClearText("CursorPosition");
		App.EnterText("CursorPosition", "3");
		App.WaitForElement("SelectionLength");
		App.ClearText("SelectionLength");
		App.EnterText("SelectionLength", "5");
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		Assert.That(App.WaitForElement("CursorPosition").GetText(), Is.EqualTo("10"));
		Assert.That(App.WaitForElement("SelectionLength").GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_ANDROID //After setting IsReadOnly to true, the Cursor remains visible on Android even when IsCursorVisible is set to false, which is not the expected behavior.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyCursorPositionWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		Assert.That(App.WaitForElement("CursorPosition").GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_WINDOWS // On Windows, Even when IsReadOnly is set to true, the Entry control still allows text selection, and SelectionLength continues to work.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifySelectionLenghtWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SelectionLength");
		App.ClearText("SelectionLength");
		App.EnterText("SelectionLength", "3");
		App.WaitForElement("UpdateCursorAndSelectionButton");
		App.Tap("UpdateCursorAndSelectionButton");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		Assert.That(App.WaitForElement("SelectionLength").GetText(), Is.EqualTo("3"));
	}
#endif
#endif

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenReturmCommandAndReturnCommandParameter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.ClearText("EntryText");
		App.EnterText("EntryText", "Test");
		App.Tap("EntryText");
		App.PressEnter();
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Command Executed with Parameter"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //keybord type is not supported on Windows and Maccatalyst platforms
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenKeyboardTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Numeric");
		App.Tap("Numeric");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}

#if TEST_FAILS_ON_ANDROID //related issue:https://github.com/dotnet/maui/issues/26968
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyTextWhenReturnTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Search");
		App.Tap("Search");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		VerifyScreenshotWithKeyboardHandling();
	}
    
	// On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsEnabled property is set to false.
	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntryControlWhenIsEnabledTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.ClearText("EntryText");
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("123"));
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("EnabledFalse");
		App.Tap("EnabledFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));

	}
#endif
#endif

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntryControlWhenIsVisibleTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VisibleFalse");
		App.Tap("VisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("EntryText");
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntryControlWhenFlowDirectionSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntryControlWhenPlaceholderTextSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.ClearText("EntryText");
		App.DismissKeyboard();
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntryControlWhenPlaceholderColorSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.ClearText("EntryText");
		App.DismissKeyboard();
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntry_WithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");

		App.WaitForElement("Apply");
		App.Tap("Apply");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyAllEntryEventsInSequence()
	{
		// Clear options first to ensure we have a clean state
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");

		// Wait for the entry control to be visible
		App.WaitForElement("EntryText");

		Assert.That(App.WaitForElement("FocusedLabel").GetText(), Is.EqualTo("Focused: Not triggered"));
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Not triggered"));
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Not triggered"));
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Not triggered"));


		// Step 1: Focus the entry (should trigger Focused event)
		App.Tap("EntryText");
		Assert.That(App.WaitForElement("FocusedLabel").GetText(), Is.EqualTo("Focused: Event Triggered"));
		// Step 2: Change the text (should trigger TextChanged event)
		App.ClearText("EntryText");
		App.EnterText("EntryText", "New Text");
#if !ANDROID
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='New Tex', New='New Text'"));
#else
		Assert.That(App.WaitForElement("TextChangedLabel").GetText(), Is.EqualTo("TextChanged: Old='', New='New Text'"));
#endif

		// Step 3: Press Enter (should trigger Completed event)
		App.PressEnter();
		Assert.That(App.WaitForElement("CompletedLabel").GetText(), Is.EqualTo("Completed: Event Triggered"));

		// Step 4: Unfocus the entry (should trigger Unfocused event)
		App.Tap("TextChangedLabel");
		Assert.That(App.WaitForElement("UnfocusedLabel").GetText(), Is.EqualTo("Unfocused: Event Triggered"));

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

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results.
		// As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		if (string.IsNullOrEmpty(screenshotName))
			VerifyScreenshot(cropBottom: iOSKeyboardCropHeight);
		else
			VerifyScreenshot(screenshotName, cropBottom: iOSKeyboardCropHeight);
#else
		if (string.IsNullOrEmpty(screenshotName))
			VerifyScreenshot();
		else
			VerifyScreenshot(screenshotName);
#endif
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

#if IOS
		VerifyScreenshotOrSetException(ref exception, screenshotName, cropBottom: iOSKeyboardCropHeight);
#else
		VerifyScreenshotOrSetException(ref exception, screenshotName);
#endif
	}
}

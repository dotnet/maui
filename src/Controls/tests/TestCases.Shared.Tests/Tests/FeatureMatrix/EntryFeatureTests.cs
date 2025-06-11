using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class EntryFeatureTests : UITest
{
	public const string EntryFeatureMatrix = "Entry Feature Matrix";

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
		App.Tap("EntryText");		App.Tap("EntryText");
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshotOrSetException(ref exception, "ClearButtonVisiblityButton_TextPresent", cropBottom: 1200);
#else
			VerifyScreenshotOrSetException(ref exception, "ClearButtonVisiblityButton_TextPresent");
#endif
		App.WaitForElement("EntryText");
		App.ClearText("EntryText");
		App.Tap("EntryText");
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshotOrSetException(ref exception, "ClearButtonVisiblityButton_TextEmpty", cropBottom: 1200);
#else
		VerifyScreenshotOrSetException(ref exception, "ClearButtonVisiblityButton_TextEmpty");
#endif
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
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
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
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
	}

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
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyClearButtonVisibilityWhenIsReadOnly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
	}

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
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
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
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
		// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
		// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
		VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshot();
	}

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
		App.ClearText("EntryText");
#if !ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
#endif
	}

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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
			VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyIsPasswordBasedOnCharacterSpacing()
	{
		Exception? exception = null;
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, "IsPassword_Before_CharacterSpacingSet");
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
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, "IsPassword_After_CharacterSpacingSet");
		if (exception != null)
		{
			throw exception;
		}
	}

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
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshot();
	}

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
		App.Tap("EntryText");
		App.DismissKeyboard();
			VerifyScreenshot();
	}

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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
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

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyIsPasswordWhenMaxLenghtSetValue()
	{
		Exception? exception = null;
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, "IsPassword_Before_MaxLengthSet");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, "IsPassword_After_MaxLengthSet");
		if (exception != null)
		{
			throw exception;
		}
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyMaxLengthWhenIsReadOnlyTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ReadOnlyTrue");
		App.Tap("ReadOnlyTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
#if !ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
#endif
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
		App.Tap("EntryText");
#if !ANDROID // On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsReadOnly property is set to true.
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test E"));
#endif
	}

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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
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
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyIsPasswordWhenFontSizeSet()
	{
		Exception? exception = null;
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, "FontSize_Default");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.Tap("Apply");
		App.WaitForElement("EntryText");
		App.Tap("EntryText");
		App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, "FontSize_20");
		if (exception != null)
		{
			throw exception;
		}
	}

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
#if ANDROID // Skip keyboard on Android to address CI flakiness, Keyboard is not needed validation.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif

#if IOS
			// On iOS, the virtual keyboard appears inconsistent with keyboard characters casing, can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
			// Using DismissKeyboard() would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
			VerifyScreenshot(cropBottom: 1200);  
#else
			VerifyScreenshot();
#endif
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
		VerifyScreenshot();
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
		VerifyScreenshot();
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
#if !ANDROID// On Android, using App.EnterText in UI tests (e.g., with Appium UITest) can programmatically enter text into an Entry control even if its IsEnabled property is set to false.
		App.EnterText("EntryText", "123");
		Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
#endif
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
}
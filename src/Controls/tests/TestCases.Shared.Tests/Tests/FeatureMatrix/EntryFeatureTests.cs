using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
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
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshotOrSetException(ref exception, "ClearButtonVisiblityButton_TextPresent");
			App.WaitForElement("EntryText");
			App.ClearText("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshotOrSetException(ref exception, "ClearButtonVisiblityButton_TextEmpty");
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyClearVisiblityButtonWhenTextColorChanged()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("Red");
			App.Tap("Red");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
			App.EnterText("EntryText", "123");
			Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}	
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenCharacterSpacingSetValues()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.ClearText("CharacterSpacing");
			App.EnterText("CharacterSpacing", "5");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyHorizontalTextAlignmentBasedOnCharacterSpacing()
		{
			App.Tap("Options");
			App.ClearText("CharacterSpacing");
			App.EnterText("CharacterSpacing", "5");
			App.WaitForElement("HCenter");
			App.Tap("HCenter");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyVerticalTextAlignmentBasedOnCharacterSpacing()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.ClearText("CharacterSpacing");
			App.EnterText("CharacterSpacing", "5");
			App.WaitForElement("VEnd");
			App.Tap("VEnd");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshotOrSetException(ref exception, "IsPassword_Before_CharacterSpacingSet");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.ClearText("CharacterSpacing");
			App.EnterText("CharacterSpacing", "5");
			App.WaitForElement("PasswordTrue");
			App.Tap("PasswordTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
			Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
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
			App.EnterText("EntryText", "123");
			Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test E"));
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenTextColorSetCorrectly()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("Red");
			App.Tap("Red");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenFontSizeSetCorrectly()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.ClearText("FontSizeEntry");
			App.EnterText("FontSizeEntry", "20");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
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
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshotOrSetException(ref exception, "FontSize_20");
			if (exception != null)
			{
				throw exception;
			}
		}

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
			App.EnterText("EntryText", "Testig123");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif

		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyIsPasswordWhenIsTextPredictionEnabledTrueOrFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextPredictionTrue");
			App.Tap("TextPredictionTrue");
			App.WaitForElement("PasswordTrue");
			App.Tap("PasswordTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.ClearText("EntryText");
			App.EnterText("EntryText", "Testig123");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
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
			App.EnterText("EntryText", "Testig123");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyIsPasswordWhenIsSpellCheckEnabledTrueOrFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("SpellCheckTrue");
			App.Tap("SpellCheckTrue");
			App.WaitForElement("PasswordTrue");
			App.Tap("PasswordTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.ClearText("EntryText");
			App.EnterText("EntryText", "Testig123");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenSelectionLengthSetValue()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.ClearText("SelectionLength");
			App.EnterText("SelectionLength", "5");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenCursorPositionValueSet()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("CursorVisibleTrue");
			App.Tap("CursorVisibleTrue");
			App.ClearText("CursorPosition");
			App.EnterText("CursorPosition", "5");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyIsPasswordWhenCursorPositionValueSet()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("CursorVisibleTrue");
			App.Tap("CursorVisibleTrue");
			App.ClearText("CursorPosition");
			App.EnterText("CursorPosition", "5");
			App.WaitForElement("PasswordTrue");
			App.Tap("PasswordTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyCursorPositionWhenSelectionLengthSetValue()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.ClearText("SelectionLength");
			App.EnterText("SelectionLength", "5");
			App.WaitForElement("CursorVisibleTrue");
			App.Tap("CursorVisibleTrue");
			App.WaitForElement("CursorPosition");
			App.ClearText("CursorPosition");
			App.EnterText("CursorPosition", "3");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
		}		
	}
}
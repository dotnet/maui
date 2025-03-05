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
			// Exception? exception = null;
			App.WaitForElement("Entry Control");
			App.EnterText("EntryText", "Test Entry");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_TextPresent");
			App.WaitForElement("EntryText", "Timed out waiting for EntryText element to appear", TimeSpan.FromSeconds(30));
			App.ClearText("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_TextEmpty");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}


		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyClearButtonVisiblityWhenTextAlignedHorizontally()
		{
			// Exception? exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_BeforeTextAlignedHorizontally");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
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
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_AfterTextAlignedHorizontally");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyClearButtonVisiblityWhenTextAlignedVertically()
		{
			// Exception? exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_BeforeTextAlignedVertically");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("VStart");
			App.Tap("VStart");
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
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_AfterTextAlignedVertically");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyClearButtonVisiblityWhenIsPasswordTrueOrFalse()
		{
			// Exception? exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");	
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
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_WhenIsPasswordTrue");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("PasswordFalse");
			App.Tap("PasswordFalse");
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
			// VerifyScreenshotOrSetException(ref exception,"ClearButtonVisiblityButton_WhenIsPasswordFalse");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenAlingnedHorizontally()
		{
			// Exception? exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			// VerifyScreenshotOrSetException(ref exception,"TextHorizontalAlignment_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
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
			// VerifyScreenshotOrSetException(ref exception,"TextHorizontalAlignment_End");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenAlingnedVertically()
		{
			// Exception exception = null;	
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			// VerifyScreenshotOrSetException(ref exception,"TextVerticalAlignment_Default");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
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
			// VerifyScreenshotOrSetException(ref exception,"TextVerticalAlignment_Start");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyHorizontalTextAlignmentBasedOnCharacterSpacing()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Character Spacing");
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
			// VerifyScreenshotOrSetException(ref exception,"TextHorizontalAlignment_CharacterSpacing_Default");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Character Spacing Test");
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
			// VerifyScreenshotOrSetException(ref exception,"TextHorizontalAlignment_CharacterSpacing_Center");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyVerticalTextAlignmentBasedOnCharacterSpacing()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Character Spacing Test.");
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
			// VerifyScreenshotOrSetException(ref exception,"TextVerticalAlignment_CharacterSpacing_Default");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Character Spacing Test");
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
			// VerifyScreenshotOrSetException(ref exception,"TextVerticalAlignment_CharacterSpacing_Center");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenIsPasswordTrueOrFalse()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");
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
			// VerifyScreenshotOrSetException(ref exception,"TextIsPassword_True");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("PasswordFalse");
			App.Tap("PasswordFalse");
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
			// VerifyScreenshotOrSetException(ref exception,"TextIsPassword_False");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyIsPasswordBasedOnVerticalTextAlignment()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");
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
			// VerifyScreenshotOrSetException(ref exception,"IsPassword_VerticalAlignment_Default");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("VEnd");
			App.Tap("VEnd");
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
			// VerifyScreenshotOrSetException(ref exception,"IsPassword_VerticalAlignment_End");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyIsPasswordBasedOnHorizontalTextAlignment()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");
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
			// VerifyScreenshotOrSetException(ref exception,"IsPassword_HorizontalAlignment_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("HEnd");
			App.Tap("HEnd");
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
			// VerifyScreenshotOrSetException(ref exception,"IsPassword_HorizontalAlignment_End");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenMaxLengthSetValue()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234abc");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "MaxLength_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234abc");
			App.ClearText("MaxLength");
			App.EnterText("MaxLength", "8");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "MaxLength_8");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyIsPasswordWhenMaxLenghtSet()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234abc");
			App.Tap("PasswordTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "IsPassword_MaxLength_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test@1234abc");
			App.ClearText("MaxLength");
			App.EnterText("MaxLength", "8");
			App.Tap("PasswordTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "IsPassword_MaxLength_8");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyHorizontalTextAlignmentWhenVerticalTextAlignmentSet()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "VerticalTextAlignment_HorizontalTextAlignment_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("VEnd");
			App.WaitForElement("HEnd");
			App.Tap("HEnd");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "VerticalTextAlignment_HorizontalTextAlignment_End");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenTextColorSetCorrectly()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "TextColor_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("Red");
			App.Tap("Red");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "TextColor_Red");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenFontSizeSetCorrectly()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "FontSize_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.ClearText("FontSizeEntry");
			App.EnterText("FontSizeEntry", "20");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "FontSize_20");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenIsTextPredictionEnabledTrueOrFalse()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("TextPredictionTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "IsTextPredictionEnabled_True");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("TextPredictionFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "IsTextPredictionEnabled_False");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenIsSpellCheckEnabledTrueOrFalse()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("SpellCheckTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "IsSpellCheckEnabled_True");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("SpellCheckFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "IsSpellCheckEnabled_False");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenCharacterSpacingSetValues()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "CharacterSpacing_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.ClearText("CharacterSpacing");
			App.EnterText("CharacterSpacing", "5");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "CharacterSpacing_5");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenSelectionLengthSetValue()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "SelectionLength_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.ClearText("SelectionLength");
			App.EnterText("SelectionLength", "5");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "SelectionLength_5");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextWhenCursorPositionValueSet()
		{
			// Exception exception = null;
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "CursorPosition_Default");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.WaitForElement("CursorVisibleTrue");
			App.Tap("CursorVisibleTrue");
			App.ClearText("CursorPosition");
			App.EnterText("CursorPosition", "5");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			App.Tap("EntryText");
			// VerifyScreenshotOrSetException(ref exception, "CursorPosition_5");
			// if (exception != null)
			// {
			// 	throw exception;
			// }
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyTextEntryWhenSetAsReadOnly()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("ReadOnlyTrue");
			App.EnterText("EntryText1", "TestEntry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("Test Entry"));
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EntryText1");
			App.EnterText("EntryText1", "Test Entry");
			App.Tap("ReadOnlyFalse");
			App.EnterText("EntryText1", "TestEntry");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("EntryText");
			Assert.That(App.WaitForElement("EntryText").GetText(), Is.EqualTo("TestEntry"));
			App.Tap("EntryText");
		}
	}
}
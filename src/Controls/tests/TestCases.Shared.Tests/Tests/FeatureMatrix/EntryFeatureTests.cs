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
			App.Tap("Options");
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
			App.EnterText("EntryText1", "Test@1234");	
			App.WaitForElement("True");
			App.Tap("True");
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
			App.Tap("Options");
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("False");
			App.Tap("False");
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
			App.Tap("Options");
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
			App.EnterText("EntryText1", "Character Spacing Test");
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
			App.EnterText("EntryText1", "Character Spacing Test");
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
			App.EnterText("EntryText1", "Character Spacing Test before vertical alignment.");
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
			App.EnterText("EntryText1", "Character Spacing Test after vertical alignment");
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
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("True");
			App.Tap("True");
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
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("False");
			App.Tap("False");
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
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("True");
			App.Tap("True");
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
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("VEnd");
			App.Tap("VEnd");
			App.Tap("True");
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
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("True");
			App.Tap("True");
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
			App.EnterText("EntryText1", "Test@1234");
			App.WaitForElement("HEnd");
			App.Tap("HEnd");
			App.Tap("True");
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
	}
}
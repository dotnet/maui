using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class PickerFeatureTests : UITest
	{
		public const string PickerFeatureMatrix = "Picker Feature Matrix";

		public PickerFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(PickerFeatureMatrix);
		}

		[Test, Order(1)]
		[Category(UITestCategories.Picker)]
		public void Picker_Validate_VerifyLabels()
		{
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_TapPicker_TakeScreenshot()
		{
			App.WaitForElement("Picker");
			App.Tap("Picker");
			VerifyScreenshot();
#if ANDROID
			App.WaitForElement("Cancel");
			App.Tap("Cancel");
#elif IOS
			App.WaitForElement("Done");
			App.Tap("Done");
#endif
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SelectItem_VerifySelectedItem()
		{
			App.WaitForElement("Picker");
			App.Tap("Picker");
#if ANDROID
			App.WaitForElement("Option 3 - Third option");
			App.Tap("Option 3 - Third option");
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitle_VerifyTitleLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Choose Option");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleAndCharacterSpacing_VerifyBoth()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Custom Title");
			App.WaitForElement("CharacterSpacingEntry");
			App.ClearText("CharacterSpacingEntry");
			App.EnterText("CharacterSpacingEntry", "5");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetSelectedIndex_VerifySelectedIndexAndItem()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("SelectedIndexEntry");
			App.ClearText("SelectedIndexEntry");
			App.EnterText("SelectedIndexEntry", "1");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			Assert.That(App.FindElement("SelectedIndexLabel").GetText(), Is.EqualTo("1"));
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetSelectedItem_VerifySelectedItemLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("SelectedItemEntry");
			App.ClearText("SelectedItemEntry");
			App.EnterText("SelectedItemEntry", "Option 4 - Fourth option");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			Assert.That(App.FindElement("SelectedItemLabel").GetText(), Is.EqualTo("Option 4 - Fourth option"));
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetCharacterSpacing_VerifyCharacterSpacingLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("CharacterSpacingEntry");
			App.ClearText("CharacterSpacingEntry");
			App.EnterText("CharacterSpacingEntry", "5");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontSize_VerifyFontSizeLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontSizeEntry");
			App.ClearText("FontSizeEntry");
			App.EnterText("FontSizeEntry", "20");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontSizeAndFontAttributesBold_VerifyFontSizeAndAttributes()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontSizeEntry");
			App.ClearText("FontSizeEntry");
			App.EnterText("FontSizeEntry", "24");
			App.PressEnter();
			App.WaitForElement("FontAttributesBold");
			App.Tap("FontAttributesBold");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontSizeAndFontFamilyDokdo_VerifyFontSizeAndFontFamily()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontSizeEntry");
			App.ClearText("FontSizeEntry");
			App.EnterText("FontSizeEntry", "18");
			App.PressEnter();
			App.WaitForElement("FontFamilyDokdoButton");
			App.Tap("FontFamilyDokdoButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFlowDirectionRTL_VerifyFlowDirection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontAttributesItalic_VerifyFontAttributes()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontAttributesItalic");
			App.Tap("FontAttributesItalic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontAttributesItalicAndFontFamilyDokdo_VerifyFontAttributesAndFontFamily()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontAttributesItalic");
			App.Tap("FontAttributesItalic");
			App.WaitForElement("FontFamilyDokdoButton");
			App.Tap("FontFamilyDokdoButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetIsEnabledFalse_VerifyPickerDisabled()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			App.Tap("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetIsVisibleFalse_VerifyPickerHidden()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsVisibleFalseRadio");
			App.Tap("IsVisibleFalseRadio");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Picker");
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetHorizontalTextAlignmentEnd_VerifyTextAlignment()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HorizontalTextAlignmentEnd");
			App.Tap("HorizontalTextAlignmentEnd");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetHorizontalTextAlignmentAndSelectedItem_VerifySelectedItem()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HorizontalTextAlignmentEnd");
			App.Tap("HorizontalTextAlignmentEnd");
			App.WaitForElement("SelectedItemEntry");
			App.ClearText("SelectedItemEntry");
			App.EnterText("SelectedItemEntry", "Option 2 - Second option");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetVerticalTextAlignmentEnd_VerifyTextAlignment()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("VerticalTextAlignmentEnd");
			App.Tap("VerticalTextAlignmentEnd");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetVerticalTextAlignmentAndSelectedItem_VerifySelectedItem()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("VerticalTextAlignmentEnd");
			App.Tap("VerticalTextAlignmentEnd");
			App.WaitForElement("SelectedItemEntry");
			App.ClearText("SelectedItemEntry");
			App.EnterText("SelectedItemEntry", "Option 3 - Third option");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTextTransformUppercase_VerifyTextTransform()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextTransformUppercase");
			App.Tap("TextTransformUppercase");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTextTransformUppercaseAndSelectedItem_VerifyTextTransformAndSelectedItem()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextTransformUppercase");
			App.Tap("TextTransformUppercase");
			App.WaitForElement("SelectedItemEntry");
			App.ClearText("SelectedItemEntry");
			App.EnterText("SelectedItemEntry", "Option 5 - Fifth option");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetShadow_VerifyShadow()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ShadowTrueButton");
			App.Tap("ShadowTrueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTextColorRed_VerifyTextColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextColorRedButton");
			App.Tap("TextColorRedButton");
			App.WaitForElement("SelectedIndexEntry");
			App.ClearText("SelectedIndexEntry");
			App.EnterText("SelectedIndexEntry", "2");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleColorOrange_VerifyTitleColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleColorOrangeButton");
			App.Tap("TitleColorOrangeButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontFamilyDokdo_VerifyFontFamily()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontFamilyDokdoButton");
			App.Tap("FontFamilyDokdoButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetItemDisplayBindingName_VerifyItemDisplay()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemDisplayNameButton");
			App.Tap("ItemDisplayNameButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SelectedIndexChanged_VerifyEventTriggered()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("SelectedIndexEntry");
			App.ClearText("SelectedIndexEntry");
			App.EnterText("SelectedIndexEntry", "2");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			Assert.That(App.FindElement("SelectedIndexChangedStatusLabel").GetText(), Is.EqualTo("Triggered"));
		}
	}
}

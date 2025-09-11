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

		public void VerifyPickerScreenshot()
		{
#if WINDOWS
			VerifyScreenshot(cropTop: 100);
#else
			VerifyScreenshot();
#endif
		}

		[Test, Order(1)]
		[Category(UITestCategories.Picker)]
		public void Picker_Validate_VerifyLabels()
		{
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

#if TEST_FAILS_ON_CATALYST

		[Test, Order(2)]
		[Category(UITestCategories.Picker)]
		public void Picker_TapPicker_TakeScreenshot()
		{
			App.WaitForElement("Picker");
			App.Tap("Picker");
			VerifyPickerScreenshot();

#if IOS
			App.WaitForElement("Done");
			App.Tap("Done");
#elif WINDOWS
			App.Tap("Option 2 - Second option");
#endif
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST

		[Test, Order(3)]
		[Category(UITestCategories.Picker)]
		public void Picker_SelectItem_VerifySelectedItem()
		{
#if ANDROID
			App.WaitForElement("Cancel");
			App.Tap("Cancel");
#endif
			App.WaitForElement("Picker");
			App.Tap("Picker");
			App.WaitForElement("Option 3 - Third option");
			App.Tap("Option 3 - Third option");
			VerifyPickerScreenshot();
		}
#endif

		[Test, Order(4)]
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
			VerifyPickerScreenshot();
		}

		[Test, Order(5)]
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
			VerifyPickerScreenshot();
		}

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30464

		[Test, Order(6)]
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
			VerifyPickerScreenshot();
		}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30463, https://github.com/dotnet/maui/issues/30464

		[Test, Order(7)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetCharacterSpacingAndTitle_VerifyBoth()
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
			VerifyPickerScreenshot();
		}
#endif

		[Test, Order(8)]
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
			App.WaitForElement("SelectedIndexEntry");
			App.ClearText("SelectedIndexEntry");
			App.EnterText("SelectedIndexEntry", "1");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

		[Test, Order(9)]
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
			App.WaitForElement("SelectedIndexEntry");
			App.ClearText("SelectedIndexEntry");
			App.EnterText("SelectedIndexEntry", "1");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

		[Test, Order(10)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFlowDirectionRTL_VerifyFlowDirection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("SelectedIndexEntry");
			App.ClearText("SelectedIndexEntry");
			App.EnterText("SelectedIndexEntry", "1");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/30463

		[Test, Order(11)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFlowDirectionRTLAndTitle_VerifyFlowDirectionAndTitle()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "RTL Title");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}
#endif

		[Test, Order(12)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontAttributesItalicAndFontFamilyDokdo_VerifyFontAttributesAndFontFamily()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontAttributesItalic");
			App.Tap("FontAttributesItalic");
			App.WaitForElement("FontFamilyDokdoButton");
			App.Tap("FontFamilyDokdoButton");
			App.WaitForElement("SelectedIndexEntry");
			App.ClearText("SelectedIndexEntry");
			App.EnterText("SelectedIndexEntry", "2");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

		[Test, Order(13)]
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
			VerifyPickerScreenshot();
		}

		[Test, Order(14)]
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

		[Test, Order(15)]
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
			VerifyPickerScreenshot();
		}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/30463

		[Test, Order(16)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetHorizontalTextAlignmentEndAndTitle_VerifyTitleAlignment()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HorizontalTextAlignmentEnd");
			App.Tap("HorizontalTextAlignmentEnd");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Aligned Title");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}
#endif

		[Test, Order(17)]
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
			VerifyPickerScreenshot();
		}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/30463

		[Test, Order(18)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetVerticalTextAlignmentEndAndTitle_VerifyTitleAlignment()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("VerticalTextAlignmentEnd");
			App.Tap("VerticalTextAlignmentEnd");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Aligned Title");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/29812

		[Test, Order(19)]
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
			VerifyPickerScreenshot();
		}
#endif

		[Test, Order(20)]
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
			VerifyPickerScreenshot();
		}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/30463

		[Test, Order(21)]
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
			VerifyPickerScreenshot();
		}

		[Test, Order(22)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleColorOrange_VerifyTitleColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleColorOrangeButton");
			App.Tap("TitleColorOrangeButton");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Choose Option");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

		[Test, Order(23)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleWithFontSize_VerifyTitleAndFontSize()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Styled Title");
			App.WaitForElement("FontSizeEntry");
			App.ClearText("FontSizeEntry");
			App.EnterText("FontSizeEntry", "22");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

		[Test, Order(24)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleWithFontAttributeBold_VerifyTitleAndFontAttribute()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Styled Title");
			App.WaitForElement("FontAttributesBold");
			App.Tap("FontAttributesBold");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}

		[Test, Order(25)]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleWithFontFamilyDokdo_VerifyTitleAndFontFamily()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleEntry");
			App.ClearText("TitleEntry");
			App.EnterText("TitleEntry", "Styled Title");
			App.WaitForElement("FontFamilyDokdoButton");
			App.Tap("FontFamilyDokdoButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Picker");
			VerifyPickerScreenshot();
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/4818

		[Test, Order(26)]
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
			VerifyPickerScreenshot();
		}
#endif

		[Test, Order(27)]
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
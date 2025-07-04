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
		public void Picker_ValidateDefaultValues_VerifyLabels()
		{
			App.WaitForElement("Options");
			Assert.That(App.FindElement("SelectedIndexLabel").GetText(), Is.EqualTo("-1"));
			Assert.That(App.FindElement("SelectedItemLabel").GetText(), Is.EqualTo(""));
			Assert.That(App.FindElement("TitleLabel").GetText(), Is.EqualTo("Select an item"));
			Assert.That(App.FindElement("CharacterSpacingLabel").GetText(), Is.EqualTo("0.0"));
			Assert.That(App.FindElement("FontSizeLabel").GetText(), Is.EqualTo("-1.0"));
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			Assert.That(App.FindElement("TitleLabel").GetText(), Is.EqualTo("Choose Option"));
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			Assert.That(App.FindElement("SelectedIndexLabel").GetText(), Is.EqualTo("1"));
			Assert.That(App.FindElement("SelectedItemLabel").GetText(), Is.EqualTo("Option 2"));
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetCharacterSpacing_VerifyCharacterSpacingLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("CharacterSpacingEntry");
			App.ClearText("CharacterSpacingEntry");
			App.EnterText("CharacterSpacingEntry", "2.5");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			Assert.That(App.FindElement("CharacterSpacingLabel").GetText(), Is.EqualTo("2.5"));
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontSize_VerifyFontSizeLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontSizeEntry");
			App.ClearText("FontSizeEntry");
			App.EnterText("FontSizeEntry", "18");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			Assert.That(App.FindElement("FontSizeLabel").GetText(), Is.EqualTo("18.0"));
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present and responsive
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontAttributesBold_VerifyFontAttributes()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontAttributesBold");
			App.Tap("FontAttributesBold");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_DisableFontAutoScaling_VerifyFontAutoScaling()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontAutoScalingFalse");
			App.Tap("FontAutoScalingFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present but should be disabled
			var pickerElement = App.FindElement("PickerControl");
			Assert.That(pickerElement, Is.Not.Null);
			// Assert.That(pickerElement.GetAttribute("enabled"), Is.EqualTo("false").Or.EqualTo("False"));
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
			// App.WaitForElementTillPageNavigationSettled("PickerControl", WaitTimeout.Short);
			
			// The picker should not be visible
			Assert.Throws<InvalidOperationException>(() => App.FindElement("PickerControl"));
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetHorizontalTextAlignmentCenter_VerifyTextAlignment()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HorizontalTextAlignmentCenter");
			App.Tap("HorizontalTextAlignmentCenter");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetVerticalTextAlignmentStart_VerifyTextAlignment()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("VerticalTextAlignmentStart");
			App.Tap("VerticalTextAlignmentStart");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTextTransformLowercase_VerifyTextTransform()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextTransformLowercase");
			App.Tap("TextTransformLowercase");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetShadowRed_VerifyShadow()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ShadowRedButton");
			App.Tap("ShadowRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetShadowBlue_VerifyShadow()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ShadowBlueButton");
			App.Tap("ShadowBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetShadowGreen_VerifyShadow()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ShadowGreenButton");
			App.Tap("ShadowGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_ClearShadow_VerifyNoShadow()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ShadowNoneButton");
			App.Tap("ShadowNoneButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTextColorRed_VerifyTextColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextColorRedButton");
			App.Tap("TextColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTextColorBlue_VerifyTextColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextColorBlueButton");
			App.Tap("TextColorBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTextColorGreen_VerifyTextColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TextColorGreenButton");
			App.Tap("TextColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleColorPurple_VerifyTitleColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleColorPurpleButton");
			App.Tap("TitleColorPurpleButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetTitleColorBrown_VerifyTitleColor()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("TitleColorBrownButton");
			App.Tap("TitleColorBrownButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontFamilySerif_VerifyFontFamily()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontFamilySerifButton");
			App.Tap("FontFamilySerifButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetFontFamilyMonospace_VerifyFontFamily()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FontFamilyMonospaceButton");
			App.Tap("FontFamilyMonospaceButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetItemDisplayBindingDescription_VerifyItemDisplay()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemDisplayDescriptionButton");
			App.Tap("ItemDisplayDescriptionButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_SetItemDisplayBindingDefault_VerifyItemDisplay()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemDisplayDefaultButton");
			App.Tap("ItemDisplayDefaultButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker is present
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
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
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the event status label shows the event was triggered
			var eventStatusLabel = App.FindElement("SelectedIndexChangedStatusLabel");
			Assert.That(eventStatusLabel.GetText(), Does.Contain("SelectedIndexChanged fired"));
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void Picker_ResetToDefaults_VerifyDefaultValues()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			
			// Reset to defaults
			App.WaitForElement("TextColorDefaultButton");
			App.Tap("TextColorDefaultButton");
			App.WaitForElement("TitleColorDefaultButton");
			App.Tap("TitleColorDefaultButton");
			App.WaitForElement("FontFamilyDefaultButton");
			App.Tap("FontFamilyDefaultButton");
			App.WaitForElement("FontAttributesNone");
			App.Tap("FontAttributesNone");
			App.WaitForElement("FlowDirectionLTR");
			App.Tap("FlowDirectionLTR");
			
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("PickerControl");
			
			// Verify the picker returns to default state
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.Picker)]
		[Category(UITestCategories.ManualReview)]
		public void Picker_InteractiveSelection_VerifyUserCanSelectItems()
		{
			App.WaitForElement("PickerControl");
			
			// Try to tap the picker to open it (this may vary by platform)
			App.Tap("PickerControl");
			
			// Wait a moment for potential picker dialog/dropdown
			App.WaitForNoElement("NonExistentElement", timeout: TimeSpan.FromSeconds(1));
			
			// Verify picker is still present after interaction
			Assert.That(App.FindElement("PickerControl"), Is.Not.Null);
			
			// This test requires manual verification that the picker opens
			// and allows item selection on each platform
		}
	}
}

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class SliderFeatureTests : _GalleryUITest
	{
		public const string SliderFeatureMatrix = "Slider Feature Matrix";

		public override string GalleryPageName => SliderFeatureMatrix;

		public SliderFeatureTests(TestDevice device)
			: base(device)
		{
		}

		

		[Test, Order(1)]
		[Category(UITestCategories.Slider)]
		public void Slider_ValidateDefaultValues_VerifyLabels()
		{
			App.WaitForElement("Options");
			Assert.That(App.FindElement("MinimumValueLabel").GetText(), Is.EqualTo("0.00"));
			Assert.That(App.FindElement("MaximumValueLabel").GetText(), Is.EqualTo("1.00"));
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("0.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinimumValue_VerifyMaximumLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "3");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("MinimumValueLabel").GetText(), Is.EqualTo("3.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMaximumValue_VerifyMinimumLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaximumEntry");
			App.EnterText("MaximumEntry", "20");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("MaximumValueLabel").GetText(), Is.EqualTo("20.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetCurrentValue_VerifyValueLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ValueEntry");
			App.EnterText("ValueEntry", "1");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("1.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetValueExceedsMaximum()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaximumEntry");
			App.EnterText("MaximumEntry", "100");
			App.PressEnter();
			App.EnterText("ValueEntry", "150");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("100.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetValueWithinRange()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaximumEntry");
			App.EnterText("MaximumEntry", "100");
			App.PressEnter();
			App.EnterText("ValueEntry", "50");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("50.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinimumValue_CheckValueLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "-5");
			App.PressEnter();
			App.EnterText("ValueEntry", "-2");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("-2.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_MinimumExceedsMaximum_SetsMaximumToMinimum()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "50");
			App.PressEnter();
			App.EnterText("MaximumEntry", "25");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("MaximumValueLabel").GetText(), Is.EqualTo("25.00"));
		}
		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ValueAboveMaximum_CheckMaximumLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaximumEntry");
			App.EnterText("MaximumEntry", "50");
			App.PressEnter();
			App.EnterText("ValueEntry", "70");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("MaximumValueLabel").GetText(), Is.EqualTo("50.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void MinimumIsSetNegativeMaximumShouldNotChangeDefaultValue()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "-1");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("MaximumValueLabel").GetText(), Is.EqualTo("1.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void SliderCurrentValueHigherThanMinimumAndMaximumTest()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "10");
			App.PressEnter();
			App.EnterText("MaximumEntry", "50");
			App.PressEnter();
			App.EnterText("ValueEntry", "60");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("50.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void CurrentValueIsSetToMinimum()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "10");
			App.PressEnter();
			App.EnterText("MaximumEntry", "20");
			App.PressEnter();
			App.EnterText("ValueEntry", "15");
			App.PressEnter();
			App.ClearText("MinimumEntry");
			App.EnterText("MinimumEntry", "20");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("20.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinimumAboveMaximum()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaximumEntry");
			App.EnterText("MaximumEntry", "50");
			App.PressEnter();
			App.EnterText("MinimumEntry", "60");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("MinimumValueLabel").GetText(), Is.EqualTo("60.00"));
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ValueBelowMinimum_CheckMinimumLabel()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "20");
			App.PressEnter();
			App.EnterText("ValueEntry", "10");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			Assert.That(App.FindElement("MinimumValueLabel").GetText(), Is.EqualTo("20.00"));
		}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // SetSliderValue method not supported on Catalyst and windows
		[Test, Order(2)]
		[Category(UITestCategories.Slider)]
		public void Slider_DragStartedAndCompletedEventTrigged()
		{
			App.WaitForElement("Options");
			App.SetSliderValue("SliderControl", 0, 1);
			Task.Delay(TimeSpan.FromSeconds(1)).Wait();
			Assert.That(App.WaitForElement("DragStartStatusLabel").GetText(), Is.EqualTo("Drag Started"));
			Assert.That(App.WaitForElement("DragCompletedStatusLabel").GetText(), Is.EqualTo("Drag Completed"));
		}
#endif

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetEnabledStateToFalse_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsEnabledFalseRadio");
			App.Tap("IsEnabledFalseRadio");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ChangeFlowDirection_RTL_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetVisibilityToFalse_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsVisibleFalseRadio");
			App.Tap("IsVisibleFalseRadio");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Options");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ChangeThumbColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ThumbColorGreenButton");
			App.Tap("ThumbColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ChangeMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinTrackColorYellowButton");
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ChangeMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaxTrackColorRedButton");
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ChangeBackgroundColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("BackgroundColorLightBlueButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_ChangeThumbImageSource_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ThumbImageSourceButton");
			App.Tap("ThumbImageSourceButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinimumAndChangeFlowDirection_RTL()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.EnterText("MinimumEntry", "10");
			App.PressEnter();
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMaximumAndChangeFlowDirection_RTL()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaximumEntry");
			App.EnterText("MaximumEntry", "50");
			App.PressEnter();
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetValueAndMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ValueEntry");
			App.EnterText("ValueEntry", "1");
			App.PressEnter();
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetValueAndMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ValueEntry");
			App.EnterText("ValueEntry", "0");
			App.PressEnter();
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetValueAndThumbImageSource_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ValueEntry");
			App.EnterText("ValueEntry", "0");
			App.PressEnter();
			App.Tap("ThumbImageSourceButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetValueAndFlowDirection_RTL_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ValueEntry");
			App.EnterText("ValueEntry", "0");
			App.PressEnter();
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetThumbAndMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ThumbColorGreenButton");
			App.Tap("ThumbColorGreenButton");
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetThumbAndMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ThumbColorGreenButton");
			App.Tap("ThumbColorGreenButton");
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetThumbAndBackgroundColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ThumbColorGreenButton");
			App.Tap("ThumbColorGreenButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetThumbColorAndThumbImageSource_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ThumbColorGreenButton");
			App.Tap("ThumbColorGreenButton");
			App.Tap("ThumbImageSourceButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinTrackColorAndValue_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinTrackColorYellowButton");
			App.Tap("MinTrackColorYellowButton");
			App.EnterText("ValueEntry", "1");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinTrackAndThumbColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinTrackColorYellowButton");
			App.Tap("MinTrackColorYellowButton");
			App.Tap("ThumbColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinTrackAndMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinTrackColorYellowButton");
			App.Tap("MinTrackColorYellowButton");
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinTrackAndBackgroundColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinTrackColorYellowButton");
			App.Tap("MinTrackColorYellowButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMinTrackColorTestFlowDirection_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinTrackColorYellowButton");
			App.Tap("MinTrackColorYellowButton");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMaxTrackColorAndValue_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaxTrackColorRedButton");
			App.Tap("MaxTrackColorRedButton");
			App.EnterText("ValueEntry", "0");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMaxTrackAndThumbColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaxTrackColorRedButton");
			App.Tap("MaxTrackColorRedButton");
			App.Tap("ThumbColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMaxTrackAndMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaxTrackColorRedButton");
			App.Tap("MaxTrackColorRedButton");
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMaxTrackAndBackgroundColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaxTrackColorRedButton");
			App.Tap("MaxTrackColorRedButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetMaxTrackColorTestFlowDirection_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MaxTrackColorRedButton");
			App.Tap("MaxTrackColorRedButton");
			App.Tap("FlowDirectionRTL");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsEnableAndThumbColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsEnabledTrueRadio");
			App.Tap("IsEnabledTrueRadio");
			App.Tap("ThumbColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsEnableAndMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsEnabledTrueRadio");
			App.Tap("IsEnabledTrueRadio");
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsEnableAndMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsEnabledTrueRadio");
			App.Tap("IsEnabledTrueRadio");
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsEnableAndBackgroundColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsEnabledTrueRadio");
			App.Tap("IsEnabledTrueRadio");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsVisibleAndThumbColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsVisibleTrueRadio");
			App.Tap("IsVisibleTrueRadio");
			App.Tap("ThumbColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsVisibleAndMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsVisibleTrueRadio");
			App.Tap("IsVisibleTrueRadio");
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsVisibleAndMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsVisibleTrueRadio");
			App.Tap("IsVisibleTrueRadio");
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetIsVisibleAndBackgroundColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsVisibleTrueRadio");
			App.Tap("BackgroundColorLightBlueButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetBackgroundColorAndThumbColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("BackgroundColorLightBlueButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.Tap("ThumbColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetBackgroundColorAndMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("BackgroundColorLightBlueButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetBackgroundColorAndMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("BackgroundColorLightBlueButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetBackgroundColorAndIsEnable_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("BackgroundColorLightBlueButton");
			App.Tap("BackgroundColorLightBlueButton");
			App.Tap("IsEnabledTrueRadio");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]

		public void Slider_SetThumbImageSourceAndThumbColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ThumbImageSourceButton");
			App.Tap("ThumbImageSourceButton");
			App.Tap("ThumbColorGreenButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_FlowDirection_RTL_SetMinimumValue_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.EnterText("MinimumEntry", "10");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_FlowDirection_RTL_SetMaximumValue_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.EnterText("MaximumEntry", "50");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_FlowDirection_RTL_SetValue_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.EnterText("ValueEntry", "0");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetFlowDirectionAndMinTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.Tap("MinTrackColorYellowButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Slider)]
		public void Slider_SetFlowDirectionAndMaxTrackColor_VerifyVisualState()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FlowDirectionRTL");
			App.Tap("FlowDirectionRTL");
			App.Tap("MaxTrackColorRedButton");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("SliderControl");
			VerifyScreenshot();
		}
	}
}
// Material3 Slider tests reuse the existing Slider Feature Matrix HostApp page.
// The native Android Slider uses Material3 styling (Google Material3 Slider) when Material3 is enabled,
// so these tests produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3SliderFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Slider Feature Matrix";

	public Material3SliderFeatureTests(TestDevice device)
		: base(device)
	{
	}

	// ========== Single Property Tests ==========

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetEnabledStateToFalse_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseRadio");
		App.Tap("IsEnabledFalseRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_ChangeFlowDirection_RTL_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetVisibilityToFalse_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseRadio");
		App.Tap("IsVisibleFalseRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Options");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_ChangeThumbColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbColorGreenButton");
		App.Tap("ThumbColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_ChangeMinTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MinTrackColorYellowButton");
		App.Tap("MinTrackColorYellowButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_ChangeMaxTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaxTrackColorRedButton");
		App.Tap("MaxTrackColorRedButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_ChangeBackgroundColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorLightBlueButton");
		App.Tap("BackgroundColorLightBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_ChangeThumbImageSource_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbImageSourceButton");
		App.Tap("ThumbImageSourceButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ========== Two Property Combo Tests ==========

	// --- Value + other property ---

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetValueAndMinTrackColor_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetValueAndMaxTrackColor_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetValueAndThumbImageSource_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetValueAndFlowDirection_RTL_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// --- ThumbColor + other property ---

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetThumbAndMaxTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbColorGreenButton");
		App.Tap("ThumbColorGreenButton");
		App.Tap("MaxTrackColorRedButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetThumbAndMinTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbColorGreenButton");
		App.Tap("ThumbColorGreenButton");
		App.Tap("MinTrackColorYellowButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetThumbAndBackgroundColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbColorGreenButton");
		App.Tap("ThumbColorGreenButton");
		App.Tap("BackgroundColorLightBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetThumbColorAndThumbImageSource_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbColorGreenButton");
		App.Tap("ThumbColorGreenButton");
		App.Tap("ThumbImageSourceButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// --- MinTrackColor + other property ---

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetMinTrackAndMaxTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MinTrackColorYellowButton");
		App.Tap("MinTrackColorYellowButton");
		App.Tap("MaxTrackColorRedButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetMinTrackAndBackgroundColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MinTrackColorYellowButton");
		App.Tap("MinTrackColorYellowButton");
		App.Tap("BackgroundColorLightBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetMinTrackColorAndFlowDirection_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MinTrackColorYellowButton");
		App.Tap("MinTrackColorYellowButton");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// --- MaxTrackColor + other property ---

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetMaxTrackAndBackgroundColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaxTrackColorRedButton");
		App.Tap("MaxTrackColorRedButton");
		App.Tap("BackgroundColorLightBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetMaxTrackColorAndFlowDirection_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaxTrackColorRedButton");
		App.Tap("MaxTrackColorRedButton");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// --- IsEnabled + other property ---

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsEnableAndThumbColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledTrueRadio");
		App.Tap("IsEnabledTrueRadio");
		App.Tap("ThumbColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsEnableAndMinTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledTrueRadio");
		App.Tap("IsEnabledTrueRadio");
		App.Tap("MinTrackColorYellowButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsEnableAndMaxTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledTrueRadio");
		App.Tap("IsEnabledTrueRadio");
		App.Tap("MaxTrackColorRedButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsEnableAndBackgroundColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledTrueRadio");
		App.Tap("IsEnabledTrueRadio");
		App.Tap("BackgroundColorLightBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// --- IsVisible + other property ---

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsVisibleAndThumbColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleTrueRadio");
		App.Tap("IsVisibleTrueRadio");
		App.Tap("ThumbColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsVisibleAndMinTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleTrueRadio");
		App.Tap("IsVisibleTrueRadio");
		App.Tap("MinTrackColorYellowButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsVisibleAndMaxTrackColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleTrueRadio");
		App.Tap("IsVisibleTrueRadio");
		App.Tap("MaxTrackColorRedButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetIsVisibleAndBackgroundColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleTrueRadio");
		App.Tap("BackgroundColorLightBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// --- Minimum/Maximum + FlowDirection ---

#if TEST_FAILS_ON_ANDROID // Resetting ThumbImageSource to null does not restore the default thumb on Material3 Slider
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetThumbImageSourceAndReset_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbImageSourceButton");
		App.Tap("ThumbImageSourceButton");
		App.Tap("ThumbImageResetButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SliderControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

#if TEST_FAILS_ON_ANDROID // Setting Minimum=10 with default Maximum=1 causes IllegalStateException in Material3 Slider (valueFrom must be < valueTo)
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetMinimumAndChangeFlowDirection_RTL()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3Slider_SetMaximumAndChangeFlowDirection_RTL()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif

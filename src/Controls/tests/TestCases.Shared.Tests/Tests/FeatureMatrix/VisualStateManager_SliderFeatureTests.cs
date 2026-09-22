using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
public class VisualStateManager_SliderFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerSliderFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerSliderFeatureTests;

	public VisualStateManager_SliderFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Slider_NormalState()
	{
		App.WaitForElement("VSMSliderButton");
		App.Tap("VSMSliderButton");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal | Value: 50"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_Slider_NormalOrUnfocusedState()
	{
		App.WaitForElement("SliderNormal");
		App.Tap("SliderNormal");
		App.WaitForElement("SliderState");
		var stateText1 = App.FindElement("SliderState").GetText();
		Assert.That(stateText1, Is.EqualTo("State: Normal/Unfocused | Value: 50"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyVSM_Slider_DisabledState()
	{
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled | Value: 50"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void VerifyVSM_Slider_ResetState()
	{
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal | Value: 50"));
		VerifyScreenshot();
	}

	[Test, Order(5)]
	public void VerifyVSM_Slider_FocusedState()
	{
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused | Value: 65"));
		VerifyScreenshot();
	}

	[Test, Order(17)]
	public void VerifyVSM_Slider_DragToFocus()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("VSMSlider");
		var sliderRect = App.WaitForElement("VSMSlider").GetRect();
		var startX = sliderRect.X + (sliderRect.Width * 50 / 100);
		var centerY = sliderRect.Y + (sliderRect.Height / 2);
		var endX = sliderRect.X + (sliderRect.Width * 35 / 100);
		App.DragCoordinates(startX, centerY, endX, centerY);
		App.WaitForElement("SliderState");
		var focusedStateText = App.FindElement("SliderState").GetText();
		Assert.That(focusedStateText, Does.Contain("State: Focused"));
	}

	[Test, Order(18)]
	public void VerifyVSM_Slider_DragWhileDisabled()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("VSMSlider");
		var sliderRect = App.WaitForElement("VSMSlider").GetRect();
		var startX = sliderRect.X + (sliderRect.Width * 50 / 100);
		var centerY = sliderRect.Y + (sliderRect.Height / 2);
		var endX = sliderRect.X + (sliderRect.Width * 35 / 100);
		App.DragCoordinates(startX, centerY, endX, centerY);
		App.WaitForElement("SliderState");
		var disabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(disabledStateText, Is.EqualTo("State: Disabled | Value: 50"));
	}
}


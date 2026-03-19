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

	[Test, Order(6)]
	public void VerifyVSM_Slider_ResetWhileFocused()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var initialStateText = App.FindElement("SliderState").GetText();
		Assert.That(initialStateText, Is.EqualTo("State: Focused | Value: 65"));
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal | Value: 50"));
	}

	[Test, Order(7)]
	public void VerifyVSM_Slider_ResetWhileUnfocused()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderNormal");
		App.Tap("SliderNormal");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unfocused | Value: 50"));
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var resetStateText = App.FindElement("SliderState").GetText();
		Assert.That(resetStateText, Is.EqualTo("State: Normal | Value: 50"));
	}

	[Test, Order(8)]
	public void VerifyVSM_Slider_ResetWhileDisabled()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled | Value: 50"));
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var resetStateText = App.FindElement("SliderState").GetText();
		Assert.That(resetStateText, Is.EqualTo("State: Normal | Value: 50"));
	}

	[Test, Order(9)]
	public void VerifyVSM_Slider_DisableWhileFocused()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var initialStateText = App.FindElement("SliderState").GetText();
		Assert.That(initialStateText, Is.EqualTo("State: Focused | Value: 65"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled | Value: 65"));
	}

	[Test, Order(10)]
	public void VerifyVSM_Slider_DisableWhileUnfocused()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderNormal");
		App.Tap("SliderNormal");
		App.WaitForElement("SliderState");
		var initialStateText = App.FindElement("SliderState").GetText();
		Assert.That(initialStateText, Is.EqualTo("State: Normal/Unfocused | Value: 50"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled | Value: 50"));
	}

	[Test, Order(11)]
	public void VerifyVSM_Slider_FocusedWhileDisabled()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var initialStateText = App.FindElement("SliderState").GetText();
		Assert.That(initialStateText, Is.EqualTo("State: Focused | Value: 65"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled | Value: 65"));
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var focusedDisabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(focusedDisabledStateText, Is.EqualTo("State: Disabled | Value: 65"));
	}

	[Test, Order(12)]
	public void VerifyVSM_Slider_UnfocusToFocus()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderNormal");
		App.Tap("SliderNormal");
		App.WaitForElement("SliderState");
		var initialStateText = App.FindElement("SliderState").GetText();
		Assert.That(initialStateText, Is.EqualTo("State: Normal/Unfocused | Value: 50"));
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var focusedStateText = App.FindElement("SliderState").GetText();
		Assert.That(focusedStateText, Is.EqualTo("State: Focused | Value: 65"));
	}

	[Test, Order(13)]
	public void VerifyVSM_Slider_FocusToUnFocus()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var focusedStateText = App.FindElement("SliderState").GetText();
		Assert.That(focusedStateText, Is.EqualTo("State: Focused | Value: 65"));
		App.WaitForElement("SliderNormal");
		App.Tap("SliderNormal");
		App.WaitForElement("SliderState");
		var unfocusedStateText = App.FindElement("SliderState").GetText();
		Assert.That(unfocusedStateText, Is.EqualTo("State: Normal/Unfocused | Value: 65"));
	}

	[Test, Order(14)]
	public void VerifyVSM_Slider_DisableAndEnableWhileUnfocused()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderNormal");
		App.Tap("SliderNormal");
		App.WaitForElement("SliderState");
		var unfocusedStateText = App.FindElement("SliderState").GetText();
		Assert.That(unfocusedStateText, Is.EqualTo("State: Normal/Unfocused | Value: 50"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var disabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(disabledStateText, Is.EqualTo("State: Disabled | Value: 50"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var unfocusedDisabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(unfocusedDisabledStateText, Is.EqualTo("State: Normal | Value: 50"));
	}

	[Test, Order(15)]
	public void VerifyVSM_Slider_DisableAndEnableWhileFocused()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var focusedStateText = App.FindElement("SliderState").GetText();
		Assert.That(focusedStateText, Is.EqualTo("State: Focused | Value: 65"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var disabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(disabledStateText, Is.EqualTo("State: Disabled | Value: 65"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var focusedEnabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(focusedEnabledStateText, Is.EqualTo("State: Normal | Value: 65"));
	}

	[Test, Order(16)]
	public void VerifyVSM_Slider_DisableAndEnableWhileReset()
	{
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var unfocusedStateText = App.FindElement("SliderState").GetText();
		Assert.That(unfocusedStateText, Is.EqualTo("State: Normal | Value: 50"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var disabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(disabledStateText, Is.EqualTo("State: Disabled | Value: 50"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var unfocusedEnabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(unfocusedEnabledStateText, Is.EqualTo("State: Normal | Value: 50"));
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


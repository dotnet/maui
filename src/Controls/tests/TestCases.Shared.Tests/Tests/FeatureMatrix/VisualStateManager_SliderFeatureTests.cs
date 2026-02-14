using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
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
		VerifyScreenshot("Slider_Initial_State");
	}

	[Test, Order(2)]
	public void VerifyVSM_Slider_NormalOrUnfocusedState()
	{
		App.WaitForElement("SliderNormal");
		App.Tap("SliderNormal");
		App.WaitForElement("SliderState");
		var stateText1 = App.FindElement("SliderState").GetText();
		Assert.That(stateText1, Is.EqualTo("State: Normal/Unfocused | Value: 50"));
		VerifyScreenshot("Slider_Unfocused_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_Slider_DisabledState()
	{
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled | Value: 50"));
		VerifyScreenshot("Slider_Disabled_State");
	}

	[Test, Order(4)]
	public void VerifyVSM_Slider_ResetState()
	{
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal | Value: 50"));
		VerifyScreenshot("Slider_Reset_State");
	}

	[Test, Order(5)]
	public void VerifyVSM_Slider_FocusedState()
	{
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");	
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused | Value: 65"));
		VerifyScreenshot("Slider_Focused_State");
	}

	[Test, Order(6)]
	public void VerifyVSM_Slider_FocusedAfterReset()
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
	public void VerifyVSM_Slider_UnfocusedAfterReset()
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
	public void VerifyVSM_Slider_DisabledAfterReset()
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
	public void VerifyVSM_Slider_FocusedAfterDisable()
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
	public void VerifyVSM_Slider_UnfocusedAfterDisable()
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
	public void VerifyVSM_Slider_DisableAfterFocus()
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
	public void VerifyVSM_Slider_UnfocusAfterFocus()
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
	public void VerifyVSM_Slider_FocusAfterUnFocus()
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
	public void VerifyVSM_Slider_DisableAfterUnFocus()
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
	public void VerifyVSM_Slider_FocusAfterDisableAndEnable()
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
	public void VerifyVSM_Slider_UnfocusAfterDisableAndEnable()
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
		var unfocusedEnabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(unfocusedEnabledStateText, Is.EqualTo("State: Normal | Value: 50"));
	}

	[Test, Order(17)]
	public void VerifyVSM_Slider_DisableAfterEnable()
	{
		Exception? exception = null;
		App.WaitForElement("VSMSlider");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var initialStateText = App.FindElement("SliderState").GetText();
		Assert.That(initialStateText, Is.EqualTo("State: Normal | Value: 50"));
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled | Value: 50"));
		VerifyScreenshotOrSetException(ref exception, "Slider_Reset_Disabled_State");
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		var enabledStateText = App.FindElement("SliderState").GetText();
		Assert.That(enabledStateText, Is.EqualTo("State: Normal | Value: 50"));
		VerifyScreenshotOrSetException(ref exception, "Slider_Reenabled_Normal_State");
		if (exception is not null)
		{
			throw exception;
		}
	}
}
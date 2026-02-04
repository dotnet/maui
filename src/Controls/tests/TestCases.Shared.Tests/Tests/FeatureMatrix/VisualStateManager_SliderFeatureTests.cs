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
	public void Slider_Focus_UpdatesStateLabelAndValue()
	{
		try { App.WaitForElement("VSMSliderButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMSliderButton"); } catch { }
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Focused"));
		Assert.That(stateText, Does.Contain("Value:"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void Slider_Reset_ShowsUnfocusedWithValue()
	{
		try { App.WaitForElement("VSMSliderButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMSliderButton"); } catch { }
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Unfocused"));
		Assert.That(stateText, Does.Contain("Value:"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	public void Slider_Disable_ShowsDisabledWithValue()
	{
		try { App.WaitForElement("VSMSliderButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMSliderButton"); } catch { }
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Disabled"));
		Assert.That(stateText, Does.Contain("Value:"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}
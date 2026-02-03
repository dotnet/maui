using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManagerFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerFeatureMatrix = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerFeatureMatrix;

	public VisualStateManagerFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VSM_Button_Disable_Reset_ShowsAlertWhenEnabled()
	{
		App.WaitForElement("DemoButton");

		// Tap when enabled should show alert
		App.Tap("DemoButton");
		App.TapDisplayAlertButton("OK");

		// Disable and verify IsEnabled false
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.False);

		// Reset and verify IsEnabled true, then alert shows again
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.True);
		App.Tap("DemoButton");
		App.TapDisplayAlertButton("OK");
	}

	[Test, Order(2)]
	public void VSM_Entry_ToggleIsEnabled()
	{
		App.WaitForElement("DemoEntry");
		App.WaitForElement("EntryDisable");

		// Disable Entry
		App.Tap("EntryDisable");
		Assert.That(App.FindElement("DemoEntry").IsEnabled(), Is.False);

		// Enable Entry
		App.Tap("EntryDisable");
		Assert.That(App.FindElement("DemoEntry").IsEnabled(), Is.True);
	}

	[Test, Order(3)]
	public void VSM_LabelContainer_ToggleIsEnabled()
	{
		App.WaitForElement("SelectableLabelContainer");
		App.WaitForElement("LabelDisable");

		// Disable container
		App.Tap("LabelDisable");
		Assert.That(App.FindElement("SelectableLabelContainer").IsEnabled(), Is.False);

		// Reset to normal
		App.WaitForElement("LabelReset");
		App.Tap("LabelReset");
		Assert.That(App.FindElement("SelectableLabelContainer").IsEnabled(), Is.True);
	}

	[Test, Order(4)]
	public void VSM_CheckBox_ToggleIsEnabled()
	{
		App.WaitForElement("DemoCheckBox");
		App.WaitForElement("CheckBoxDisable");

		// Disable checkbox
		App.Tap("CheckBoxDisable");
		Assert.That(App.FindElement("DemoCheckBox").IsEnabled(), Is.False);

		// Reset checkbox
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		Assert.That(App.FindElement("DemoCheckBox").IsEnabled(), Is.True);
	}

	[Test, Order(5)]
	public void VSM_Switch_ToggleIsEnabled()
	{
		App.WaitForElement("DemoSwitch");
		App.WaitForElement("SwitchDisable");

		// Disable switch
		App.Tap("SwitchDisable");
		Assert.That(App.FindElement("DemoSwitch").IsEnabled(), Is.False);

		// Reset switch
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		Assert.That(App.FindElement("DemoSwitch").IsEnabled(), Is.True);
	}

	[Test, Order(6)]
	public void VSM_Slider_ToggleIsEnabled()
	{
		App.WaitForElement("DemoSlider");
		App.WaitForElement("SliderDisable");

		// Disable slider
		App.Tap("SliderDisable");
		Assert.That(App.FindElement("DemoSlider").IsEnabled(), Is.False);

		// Reset slider
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		Assert.That(App.FindElement("DemoSlider").IsEnabled(), Is.True);
	}

	// Visual state screenshot validations

	[Test, Order(7)]
	public void VSM_Button_Disabled_And_Normal_VerifyScreenshot()
	{
		App.WaitForElement("DemoButton");
		// Normal state
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Disabled state via toggle
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.False);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Reset back to Normal
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.True);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	public void VSM_Entry_Focused_Unfocused_Disabled_VerifyScreenshot()
	{
		App.WaitForElement("DemoEntry");
		// Focused
		App.WaitForElement("EntryFocus");
		App.Tap("EntryFocus");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Unfocused (tap elsewhere — the DemoButton triggers alert we’ll dismiss)
		App.Tap("DemoButton");
		App.TapDisplayAlertButton("OK");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Disabled
		App.WaitForElement("EntryDisable");
		App.Tap("EntryDisable");
		Assert.That(App.FindElement("DemoEntry").IsEnabled(), Is.False);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	public void VSM_Label_Selected_Disabled_Reset_VerifyScreenshot()
	{
		App.WaitForElement("SelectableLabelContainer");
		// Normal
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Selected (tap the container)
		App.Tap("SelectableLabelContainer");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Disabled
		App.WaitForElement("LabelDisable");
		App.Tap("LabelDisable");
		Assert.That(App.FindElement("SelectableLabelContainer").IsEnabled(), Is.False);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Reset
		App.WaitForElement("LabelReset");
		App.Tap("LabelReset");
		Assert.That(App.FindElement("SelectableLabelContainer").IsEnabled(), Is.True);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	public void VSM_CheckBox_Checked_Unchecked_Disabled_VerifyScreenshot()
	{
		App.WaitForElement("DemoCheckBox");
		// Checked
		App.Tap("DemoCheckBox");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Disabled
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		Assert.That(App.FindElement("DemoCheckBox").IsEnabled(), Is.False);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Reset (returns to Unchecked + enabled)
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		Assert.That(App.FindElement("DemoCheckBox").IsEnabled(), Is.True);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	public void VSM_Switch_On_Off_Disabled_VerifyScreenshot()
	{
		App.WaitForElement("DemoSwitch");
		// On
		App.Tap("DemoSwitch");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Disabled
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		Assert.That(App.FindElement("DemoSwitch").IsEnabled(), Is.False);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Reset (Off + enabled)
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		Assert.That(App.FindElement("DemoSwitch").IsEnabled(), Is.True);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	public void VSM_Slider_Focused_Unfocused_Disabled_VerifyScreenshot()
	{
		App.WaitForElement("DemoSlider");
		// Focused
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Unfocused via Reset
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));

		// Disabled
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		Assert.That(App.FindElement("DemoSlider").IsEnabled(), Is.False);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}
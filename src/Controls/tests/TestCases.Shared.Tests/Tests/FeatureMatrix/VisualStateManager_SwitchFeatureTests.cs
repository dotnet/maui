using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_SwitchFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerSwitchFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerSwitchFeatureTests;

	public VisualStateManager_SwitchFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Switch_On_UpdatesStateLabel()
	{
		App.WaitForElement("VSMSwitchButton");
		App.Tap("VSMSwitchButton");
		App.WaitForElement("VSMSwitch");
		App.WaitForElement("SwitchState");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
	}

	[Test, Order(2)]
	public void VerifyVSM_Switch_Off_UpdatesStateLabel()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
	}

	[Test, Order(3)]
	public void VerifyVSM_Switch_Disable_UpdatesStateLabel()
	{
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(4)]
	public void VerifyVSM_Switch_Reset_ReturnsToOff()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
	}

	[Test, Order(5)]
	public void VerifyVSM_Switch_DisableWhileOn_ShowsDisabled()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		Assert.That(App.FindElement("SwitchState").GetText(), Is.EqualTo("State: On"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("SwitchDisable_WhileOn");
	}

	[Test, Order(6)]
	public void VerifyVSM_Switch_DisableWhileOff_ShowsDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		Assert.That(App.FindElement("SwitchState").GetText(), Is.EqualTo("State: Off"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("SwitchDisable_WhileOff");
	}
}
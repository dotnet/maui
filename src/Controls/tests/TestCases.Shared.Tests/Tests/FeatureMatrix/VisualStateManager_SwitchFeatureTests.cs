using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
public class VisualStateManager_SwitchFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerSwitchFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerSwitchFeatureTests;

	public VisualStateManager_SwitchFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Switch_InitialState()
	{
		App.WaitForElement("VSMSwitchButton");
		App.Tap("VSMSwitchButton");
		App.WaitForElement("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_Switch_On()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyVSM_Switch_Off()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void VerifyVSM_Switch_Reset()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

	[Test, Order(5)]
	public void VerifyVSM_Switch_OnToOff()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.Tap("VSMSwitch");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
	}

	[Test, Order(6)]
	public void VerifyVSM_Switch_OffToOn()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		App.Tap("VSMSwitch");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
	}

	[Test, Order(7)]
	public void VerifyVSM_Switch_DisableWhileOn()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(8)]
	public void VerifyVSM_Switch_DisableWhileOff()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(9)]
	public void VerifyVSM_Switch_ResetWhileDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(10)]
	public void VerifyVSM_Switch_DisableAndResetWhileOn()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(11)]
	public void VerifyVSM_Switch_DisableAndResetWhileOff()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(12)]
	public void VerifyVSM_Switch_OnWhileDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(13)]
	public void VerifyVSM_Switch_OffWhileDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(14)]
	public void VerifyVSM_Switch_OnWhileDisableAndEnable()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
	}

	[Test, Order(15)]
	public void VerifyVSM_Switch_OffWhileDisableAndEnable()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
	}

	[Test, Order(16)]
	public void VerifyVSM_Switch_ResetWhileOnAndDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(17)]
	public void VerifyVSM_Switch_ResetWhileOffAndDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(18)]
	public void VerifyVSM_Switch_ResetWhileOn()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
}


using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
public class VisualStateManager_CheckBoxFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerCheckBoxFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerCheckBoxFeatureTests;

	public VisualStateManager_CheckBoxFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_CheckBox_InitialState()
	{
		App.WaitForElement("VSMCheckBoxButton");
		App.Tap("VSMCheckBoxButton");
		App.WaitForElement("CheckBoxState");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_CheckBox_Disable()
	{
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyVSM_CheckBox_Reset()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void VerifyVSM_CheckBox_Checked()
	{
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
		VerifyScreenshot();
	}

	[Test, Order(5)]
	public void VerifyVSM_CheckBox_UnCheckedWhileChecked()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unchecked"));
	}

	[Test, Order(6)]
	public void VerifyVSM_CheckBox_DisableWhileChecked()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(7)]
	public void VerifyVSM_CheckBox_CheckedWhileDisable()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(8)]
	public void VerifyVSM_CheckBox_ResetWhileDisable()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(9)]
	public void VerifyVSM_CheckBox_UnCheckedWhileDisable()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText1 = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText1, Is.EqualTo("State: Disabled"));
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(10)]
	public void VerifyVSM_CheckBox_DisableAndEnableWhileChecked()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText1 = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText1, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
	}

	[Test, Order(11)]
	public void VerifyVSM_CheckBox_DisableAndEnableWhileUnchecked()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unchecked"));
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText1 = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText1, Is.EqualTo("State: Disabled"));
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unchecked"));
	}

	[Test, Order(12)]
	public void VerifyVSM_CheckBox_ResetWhileChecked()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(13)]
	public void VerifyVSM_CheckBox_ResetWhileUnchecked()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		App.WaitForElement("VSMCheckBox");
		App.Tap("VSMCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unchecked"));
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
}


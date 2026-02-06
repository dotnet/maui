using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_CheckBoxFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerCheckBoxFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerCheckBoxFeatureTests;

	public VisualStateManager_CheckBoxFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_CheckBox_Checked_UpdatesStateLabel()
	{
		App.WaitForElement("VSMCheckBoxButton");
		App.Tap("VSMCheckBoxButton");
		App.WaitForElement("DemoCheckBox");
		App.Tap("DemoCheckBox");
		App.WaitForElement("CheckBoxState");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
	}

	[Test, Order(2)]
	public void VerifyVSM_CheckBox_Disable_UpdatesStateLabel()
	{
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("CheckBox_Disabled_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_CheckBox_Reset_ReturnsToUnchecked()
	{
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unchecked"));
	}
}
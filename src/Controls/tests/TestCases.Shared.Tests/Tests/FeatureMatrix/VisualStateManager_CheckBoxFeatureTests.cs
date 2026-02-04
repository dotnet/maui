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
	public void CheckBox_Checked_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMCheckBoxButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMCheckBoxButton"); } catch { }
		App.WaitForElement("DemoCheckBox");
		App.WaitForElement("CheckBoxState");
		App.Tap("DemoCheckBox");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Checked"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void CheckBox_Disable_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMCheckBoxButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMCheckBoxButton"); } catch { }
		App.WaitForElement("CheckBoxDisable");
		App.Tap("CheckBoxDisable");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	public void CheckBox_Reset_ReturnsToUnchecked()
	{
		try { App.WaitForElement("VSMCheckBoxButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMCheckBoxButton"); } catch { }
		App.WaitForElement("CheckBoxReset");
		App.Tap("CheckBoxReset");
		var stateText = App.FindElement("CheckBoxState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unchecked"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}
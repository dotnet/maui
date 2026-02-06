using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_ButtonFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerButtonFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerButtonFeatureTests;

	public VisualStateManager_ButtonFeatureTests(TestDevice device)
		: base(device)
	{
	}
#if TEST_FAILS_ON_ANDROID
	[Test, Order(1)]
	public void VerifyVSM_Button_Click_UpdatesStateLabel()
	{
		App.WaitForElement("VSMButton");
		App.Tap("VSMButton");
		App.WaitForElement("DemoButton");
		App.Tap("DemoButton");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Clicked"));
	}

	[Test, Order(2)]
	public void VerifyVSM_Button_Disable_UpdatesStateLabel()
	{
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("Button_Disabled_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_Button_Reset_ReturnsToNormal()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.True);
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
#endif

	[Test, Order(4)]
	public void VerifyVSM_Button_DisableAndEnable()
	{
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(5)]
	public void VerifyVSM_Button_ResetFromDisabled()
	{
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.Tap("ButtonReset");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
}
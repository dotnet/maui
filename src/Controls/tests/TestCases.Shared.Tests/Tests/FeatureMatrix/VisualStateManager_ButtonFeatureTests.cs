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

	[Test, Order(1)]
	public void Button_Click_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMButton"); } catch { }
		App.WaitForElement("DemoButton");
		App.WaitForElement("ButtonStateLabel");
		App.Tap("DemoButton");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Clicked"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void Button_Disable_SetsIsEnabledFalse()
	{
		try { App.WaitForElement("VSMButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMButton"); } catch { }
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.False);
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	public void Button_Reset_ReturnsToNormal()
	{
		try { App.WaitForElement("VSMButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMButton"); } catch { }
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.True);
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}
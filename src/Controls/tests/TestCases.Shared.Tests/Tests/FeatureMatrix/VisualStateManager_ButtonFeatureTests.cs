using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
public class VisualStateManager_ButtonFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerButtonFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerButtonFeatureTests;

	public VisualStateManager_ButtonFeatureTests(TestDevice device)
		: base(device)
	{
	}
// PointerOver states cannot currently be reliably covered in CI environments, as hover/pointer interactions are not consistently supported in automated runs. Therefore, these states are validated manually on Mac and Windows, and PointerOver-related tests have not been included in the automated test cases.
	[Test, Order(1)]
	public void VerifyVSM_Button_InitialState()
	{
		App.WaitForElement("VSMButton");
		App.Tap("VSMButton");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_Button_Disable()
	{
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyVSM_Button_Reset()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		Assert.That(App.FindElement("DemoButton").IsEnabled(), Is.True);
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID // Related issue link: https://github.com/dotnet/maui/issues/19289
	[Test, Order(4)]
	public void VerifyVSM_Button_PressedAndReleased()
	{
		App.WaitForElement("DemoButton");
		App.Tap("DemoButton");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Released"));
		VerifyScreenshot();
	}

	[Test, Order(5)]
	public void VerifyVSM_Button_DisableWhilePressedAndReleased()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		App.WaitForElement("DemoButton");
		App.Tap("DemoButton");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Released"));
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(6)]
	public void VerifyVSM_Button_ResetWhilePressedAndReleased()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		App.WaitForElement("DemoButton");
		App.Tap("DemoButton");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Released"));
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		App.WaitForElement("ButtonStateLabel");
		stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
#endif

	[Test, Order(7)]
	public void VerifyVSM_Button_ResetWhileDisabled()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		App.WaitForElement("ButtonStateLabel");
		stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

#if TEST_FAILS_ON_ANDROID // Related issue link: https://github.com/dotnet/maui/issues/19289
	[Test, Order(8)]
	public void VerifyVSM_Button_PressedAndReleasedWhileDisabled()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("DemoButton");
		App.Tap("DemoButton");
		App.WaitForElement("ButtonStateLabel");
		stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}
#endif

	[Test, Order(9)]
	public void VerifyVSM_Button_DisableAndEnable()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
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

#if TEST_FAILS_ON_ANDROID // Related issue link: https://github.com/dotnet/maui/issues/19289
	[Test, Order(10)]
	public void VerifyVSM_Button_DisableAndEnableWhilePressedAndReleased()
	{
		App.WaitForElement("ButtonReset");
		App.Tap("ButtonReset");
		App.WaitForElement("DemoButton");
		App.Tap("DemoButton");
		App.WaitForElement("ButtonStateLabel");
		var stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Released"));
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("ButtonDisable");
		App.Tap("ButtonDisable");
		App.WaitForElement("ButtonStateLabel");
		stateText = App.FindElement("ButtonStateLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
#endif
}


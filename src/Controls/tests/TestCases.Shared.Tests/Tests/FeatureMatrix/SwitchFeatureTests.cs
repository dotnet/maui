using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Switch)]
public class SwitchFeatureTests : _GalleryUITest
{
	public const string SwitchFeatureMatrix = "Switch Feature Matrix";

	public override string GalleryPageName => SwitchFeatureMatrix;

	public SwitchFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void Switch_InitialState_VerifyVisualState()
	{
		App.WaitForElement("SwitchControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void Switch_Click_VerifyVisualState()
	{
		App.WaitForElement("SwitchControl");
		App.Tap("SwitchControl");
		App.WaitForElement("ToggledEventLabel");
		Assert.That(App.FindElement("ToggledEventLabel").GetText(), Is.EqualTo("True"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(3)]
	public void Switch_SetFlowDirectionAndToggled_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeftCheckBox");
		App.Tap("FlowDirectionRightToLeftCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwitchControl");
		App.Tap("SwitchControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(4)]
	public void Switch_SetEnabled_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseCheckBox");
		App.Tap("IsEnabledFalseCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwitchControl");
		App.Tap("SwitchControl");
		App.WaitForElement("ToggledEventLabel");
		Assert.That(App.FindElement("ToggledEventLabel").GetText(), Is.EqualTo("False"));
	}

	[Test]
	[Order(5)]
	public void Switch_SetVisibleAndToggled_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseCheckBox");
		App.Tap("IsVisibleFalseCheckBox");
		App.WaitForElement("IsToggledTrueCheckBox");
		App.Tap("IsToggledTrueCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("SwitchControl");
	}

	[Test]
	[Order(6)]
	public void Switch_SetToggledAndOnColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsToggledTrueCheckBox");
		App.Tap("IsToggledTrueCheckBox");
		App.WaitForElement("OnColorRedCheckBox");
		App.Tap("OnColorRedCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("SwitchControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(7)]
	public void Switch_SetOnColorAndThumbColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OnColorRedCheckBox");
		App.Tap("OnColorRedCheckBox");
		App.WaitForElement("ThumbColorGreenCheckBox");
		App.Tap("ThumbColorGreenCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwitchControl");
		App.Tap("SwitchControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30046, https://github.com/dotnet/maui/issues/29812
    [Test]
	[Order(8)]
    public void Switch_SetShadowAndToggled_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("ShadowTrueCheckBox");
        App.Tap("ShadowTrueCheckBox");
        App.WaitForElement("IsToggledTrueCheckBox");
        App.Tap("IsToggledTrueCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
	[Order(9)]
    public void Switch_SetShadowAndOnColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("ShadowTrueCheckBox");
        App.Tap("ShadowTrueCheckBox");
        App.WaitForElement("OnColorRedCheckBox");
        App.Tap("OnColorRedCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl");
        App.Tap("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
	[Order(10)]
    public void Switch_SetShadowAndThumbColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("ShadowTrueCheckBox");
        App.Tap("ShadowTrueCheckBox");
        App.WaitForElement("ThumbColorGreenCheckBox");
        App.Tap("ThumbColorGreenCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl");
        App.Tap("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
#endif

	[Test]
	[Order(11)]
	public void Switch_SetThumbColorAndOnColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThumbColorRedCheckBox");
		App.Tap("ThumbColorRedCheckBox");
		App.WaitForElement("OnColorGreenCheckBox");
		App.Tap("OnColorGreenCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwitchControl");
		App.Tap("SwitchControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(12)]
	public void Switch_SetOffColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OffColorRedCheckBox");
		App.Tap("OffColorRedCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwitchControl"); // IsToggled=false by default → OffColor is visible
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Order(13)]
	public void Switch_SetOffColorAndOnColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OffColorRedCheckBox");
		App.Tap("OffColorRedCheckBox");
		App.WaitForElement("OnColorGreenCheckBox");
		App.Tap("OnColorGreenCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwitchControl");
		App.Tap("SwitchControl"); // toggle to On → OnColor (green) should show
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}

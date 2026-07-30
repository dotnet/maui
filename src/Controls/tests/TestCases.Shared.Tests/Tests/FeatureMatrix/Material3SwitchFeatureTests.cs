// Material3 Switch tests reuse the existing Switch Feature Matrix HostApp page.
// The native Android view differs (MaterialSwitch vs SwitchCompat), so these tests
// produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Material3)]
public class Material3SwitchFeatureTests : _GalleryUITest
{
    public override string GalleryPageName => "Switch Feature Matrix";

    public Material3SwitchFeatureTests(TestDevice device)
        : base(device)
    {
    }

    [Test, Order(1)]
    public void Material3Switch_InitialState_VerifyVisualState()
    {
        App.WaitForElement("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(2)]
    public void Material3Switch_Click_VerifyVisualState()
    {
        App.WaitForElement("SwitchControl");
        App.Tap("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    public void Material3Switch_SetFlowDirectionAndToggled_VerifyVisualState()
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
    public void Material3Switch_SetToggledAndOnColor_VerifyVisualState()
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
    [Order(6)]
    public void Material3Switch_SetOffColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("OffColorRedCheckBox");
        App.Tap("OffColorRedCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Order(7)]
    public void Material3Switch_SetOffColorAndOnColor_VerifyVisualState()
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
        App.Tap("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Order(9)]
    public void Material3Switch_SetEnabled_VerifyVisualState()
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
    [Order(10)]
    public void Material3Switch_SetVisibleAndToggled_VerifyVisualState()
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
    [Order(11)]
    public void Material3Switch_SetEnabledFalseAndToggled_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsEnabledFalseCheckBox");
        App.Tap("IsEnabledFalseCheckBox");
        App.WaitForElement("IsToggledTrueCheckBox");
        App.Tap("IsToggledTrueCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl"); // disabled but toggled on
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Order(12)]
    public void Material3Switch_SetEnabledFalseAndOnColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsEnabledFalseCheckBox");
        App.Tap("IsEnabledFalseCheckBox");
        App.WaitForElement("IsToggledTrueCheckBox");
        App.Tap("IsToggledTrueCheckBox");
        App.WaitForElement("OnColorRedCheckBox");
        App.Tap("OnColorRedCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl"); // disabled + on + OnColor red
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Order(14)]
    public void Material3Switch_SetFlowDirectionAndOnColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FlowDirectionRightToLeftCheckBox");
        App.Tap("FlowDirectionRightToLeftCheckBox");
        App.WaitForElement("IsToggledTrueCheckBox");
        App.Tap("IsToggledTrueCheckBox");
        App.WaitForElement("OnColorRedCheckBox");
        App.Tap("OnColorRedCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl"); // RTL + on + OnColor red
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Order(15)]
    public void Material3Switch_SetAllProperties_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FlowDirectionRightToLeftCheckBox");
        App.Tap("FlowDirectionRightToLeftCheckBox");
        App.WaitForElement("IsToggledTrueCheckBox");
        App.Tap("IsToggledTrueCheckBox");
        App.WaitForElement("OnColorRedCheckBox");
        App.Tap("OnColorRedCheckBox");
        App.WaitForElement("OffColorGreenCheckBox");
        App.Tap("OffColorGreenCheckBox");
        App.WaitForElement("ThumbColorGreenCheckBox");
        App.Tap("ThumbColorGreenCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl"); // kitchen-sink: RTL + on + OnColor + OffColor + ThumbColor
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Order(16)]
    [Ignore("Fails on Android — see https://github.com/dotnet/maui/issues/19883")]
    public void Material3Switch_SetOnColorAndThumbColor_VerifyVisualState()
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

    [Test]
    [Order(17)]
    [Ignore("Fails on Android — see https://github.com/dotnet/maui/issues/19883")]
    public void Material3Switch_SetThumbColorAndOnColor_VerifyVisualState()
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
    [Order(18)]
    [Ignore("Fails on Android — see https://github.com/dotnet/maui/issues/19883")]
    public void Material3Switch_SetOffColorAndThumbColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("OffColorRedCheckBox");
        App.Tap("OffColorRedCheckBox");
        App.WaitForElement("ThumbColorGreenCheckBox");
        App.Tap("ThumbColorGreenCheckBox");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl"); // IsToggled=false by default → OffColor + ThumbColor visible
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }
}
#endif

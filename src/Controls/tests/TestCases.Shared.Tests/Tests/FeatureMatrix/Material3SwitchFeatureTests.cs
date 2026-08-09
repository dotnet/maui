// Material3 Switch tests reuse the existing Switch Feature Matrix HostApp page.
// The native Android view differs (MaterialSwitch vs SwitchCompat), so these tests
// produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3SwitchFeatureTests : _GalleryUITest
{
    public override string GalleryPageName => "Switch Feature Matrix";

    public Material3SwitchFeatureTests(TestDevice device)
        : base(device)
    {
    }

    [Test, Order(1)]
    [Category(UITestCategories.Material3)]
    public void Material3Switch_InitialState_VerifyVisualState()
    {
        App.WaitForElement("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(2)]
    [Category(UITestCategories.Material3)]
    public void Material3Switch_Click_VerifyVisualState()
    {
        App.WaitForElement("SwitchControl");
        App.Tap("SwitchControl");
        VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    [Category(UITestCategories.Material3)]
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
    [Category(UITestCategories.Material3)]
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
    [Category(UITestCategories.Material3)]
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
    [Category(UITestCategories.Material3)]
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
}
#endif
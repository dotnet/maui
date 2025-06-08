using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class SwitchFeatureTests : UITest
{
    public const string SwitchFeatureMatrix = "Switch Feature Matrix";

    public SwitchFeatureTests(TestDevice device)
        : base(device)
    {
    }

    protected override void FixtureSetup()
    {
        base.FixtureSetup();
        App.NavigateToGallery(SwitchFeatureMatrix);
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetToggledAndOnColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsToggledTrueButton");
        App.Tap("IsToggledTrueButton");
        App.WaitForElement("OnColorRedButton");
        App.Tap("OnColorRedButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetOnColorAndThumbColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("OnColorRedButton");
        App.Tap("OnColorRedButton");
        App.WaitForElement("ThumbColorGreenButton");
        App.Tap("ThumbColorGreenButton");
        App.WaitForElement("IsToggledTrueButton");
        App.Tap("IsToggledTrueButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetToggledAndFlowDirection_RTL_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsToggledTrueButton");
        App.Tap("IsToggledTrueButton");
        App.WaitForElement("FlowDirectionRightToLeftButton");
        App.Tap("FlowDirectionRightToLeftButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetBackgroundColorAndOnColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("BackgroundColorOrangeButton");
        App.Tap("BackgroundColorOrangeButton");
        App.WaitForElement("OnColorGreenButton");
        App.Tap("OnColorGreenButton");
        App.WaitForElement("IsToggledTrueButton");
        App.Tap("IsToggledTrueButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetBackgroundColorAndThumbColor_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("BackgroundColorBlueButton");
        App.Tap("BackgroundColorBlueButton");
        App.WaitForElement("ThumbColorGreenButton");
        App.Tap("ThumbColorGreenButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetEnabled_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsEnabledFalseButton");
        App.Tap("IsEnabledFalseButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("SwitchControl");
        App.Tap("SwitchControl");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetVisibleAndToggled_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("IsVisibleFalseButton");
        App.Tap("IsVisibleFalseButton");
        App.WaitForElement("IsToggledTrueButton");
        App.Tap("IsToggledTrueButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Switch)]
    public void Switch_SetShadowOpacityAndToggled_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("ShadowTrueButton");
        App.Tap("ShadowTrueButton");
        App.WaitForElement("IsToggledTrueButton");
        App.Tap("IsToggledTrueButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(4000).Wait();
        // VerifyScreenshot();
    }
}
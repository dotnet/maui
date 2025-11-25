using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32731 : _IssuesUITest
{
    public Issue32731(TestDevice device) : base(device) { }

    public override string Issue => "Applying Shadow property affects the properties in Visual Transform Matrix";

    [Test, Order(1)]
    [Category(UITestCategories.Border)]
    public void ScaleXWithShadowRendersCorrectly()
    {
        App.Tap("ScaleXButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }

    [Test, Order(2)]
    [Category(UITestCategories.Border)]
    public void ScaleYWithShadowRendersCorrectly()
    {
        App.Tap("ResetButton");
        App.Tap("ScaleYButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }

    [Test, Order(3)]
    [Category(UITestCategories.Border)]
    public void TranslationXWithShadowRendersCorrectly()
    {
        App.Tap("ResetButton");
        App.Tap("TranslationXButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }

    [Test, Order(4)]
    [Category(UITestCategories.Border)]
    public void TranslationYWithShadowRendersCorrectly()
    {
        App.Tap("ResetButton");
        App.Tap("TranslationYButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
}
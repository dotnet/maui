# if TEST_FAILS_ON_ANDROID      //More Info: https://github.com/dotnet/maui/issues/32731
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
 
namespace Microsoft.Maui.TestCases.Tests.Issues;
 
public class Issue32724 : _IssuesUITest
{
    public Issue32724(TestDevice device) : base(device) { }
 
    public override string Issue => "Applying Shadow property affects the properties in Visual Transform Matrix";
 
    [Test, Order(1)]
    [Category(UITestCategories.Border)]
    public void VerifyScaleAndShadow()
    {
        App.WaitForElement("ScaleButton");
        App.Tap("ScaleButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(2)]
    [Category(UITestCategories.Border)]
    public void VerifyScaleXAndShadow()
    {
        App.Tap("ResetButton");
        App.WaitForElement("ScaleXButton");
        App.Tap("ScaleXButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(3)]
    [Category(UITestCategories.Border)]
    public void VerifyScaleYAndShadow()
    {
        App.Tap("ResetButton");
        App.WaitForElement("ScaleYButton");
        App.Tap("ScaleYButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(4)]
    [Category(UITestCategories.Border)]
    public void VerifyTranslationXAndShadow()
    {
        App.Tap("ResetButton");
        App.WaitForElement("TranslationXButton");
        App.Tap("TranslationXButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(5)]
    [Category(UITestCategories.Border)]
    public void VerifyTranslationYAndShadow()
    {
        App.Tap("ResetButton");
        App.WaitForElement("TranslationYButton");
        App.Tap("TranslationYButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(6)]
    [Category(UITestCategories.Border)]
    public void VerifyRotationAndShadow()
    {
        App.Tap("ResetButton");
        App.Tap("RotationButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(7)]
    [Category(UITestCategories.Border)]
    public void VerifyRotationXAndShadow()
    {
        App.Tap("ResetButton");
        App.Tap("RotationXButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(8)]
    [Category(UITestCategories.Border)]
    public void VerifyRotationYAndShadow()
    {
        App.Tap("ResetButton");
        App.Tap("RotationYButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(9)]
    [Category(UITestCategories.Border)]
    public void VerifyAnchorXAndShadow()
    {
        App.Tap("ResetButton");
        App.Tap("AnchorXButton");
        App.Tap("RotationButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
    [Test, Order(10)]
    [Category(UITestCategories.Border)]
    public void VerifyAnchorYAndShadow()
    {
        App.Tap("ResetButton");
        App.Tap("AnchorYButton");
        App.Tap("RotationButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
 
    [Test, Order(11)]
    [Category(UITestCategories.Border)]
    public void VerifyAnchorXAndAnchorYShadow()
    {
        App.Tap("ResetButton");
        App.Tap("AnchorXButton");
        App.Tap("AnchorYButton");
        App.Tap("RotationButton");
        App.Tap("ToggleShadowButton");
        VerifyScreenshot();
    }
}
#endif
 
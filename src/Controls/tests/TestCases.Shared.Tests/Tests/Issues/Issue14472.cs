using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14472 : _IssuesUITest
{
    public Issue14472(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Slider is very broken, Value is a mess when setting Minimum";

    [Test]
    [Category(UITestCategories.Slider)]
    public void SliderShouldInitializeCorrectly()
    {
        App.WaitForElement("MauiSlider");
        VerifyScreenshot();
    }
}

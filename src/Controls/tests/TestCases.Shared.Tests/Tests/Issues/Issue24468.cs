#if ANDROID || IOS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue24468 : _IssuesUITest
{
    public Issue24468(TestDevice device) : base(device)
    {
    }

    public override string Issue => "FlyoutPage toolbar button not updating on orientation change on Android";
    [Test]
    [Category(UITestCategories.Navigation)]
    public void FlyoutPageToolbarButtonUpdatesOnOrientationChange()
    {
        App.WaitForElement("ContentPage");

        try
        {
            App.SetOrientationLandscape();

            var text = App.FindElement("StatusLabel").GetText();
            Assert.That(text, Contains.Substring("ShouldShowToolbarButton is called"));
        }
        finally
        {
            App.SetOrientationPortrait();
        }
    }
}
#endif

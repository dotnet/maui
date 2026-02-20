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
        App.SetOrientationLandscape();
        App.WaitForElement("ContentPage");

        try
        {
            App.WaitForElement("EventLabel");
            var eventText = App.FindElement("EventLabel").GetText();
            Assert.That(eventText, Is.EqualTo("ShouldShowToolbarButton called"));
            
            var callCount = int.Parse(App.FindElement("CountLabel").GetText() ?? "0");
            Assert.That(callCount, Is.GreaterThan(1).And.LessThan(5), 
                $"Expected call count between 2-4, but got {callCount}. Method should not be called excessively.");
        }
        finally
        {
            App.SetOrientationPortrait();
        }
    }
}
#endif

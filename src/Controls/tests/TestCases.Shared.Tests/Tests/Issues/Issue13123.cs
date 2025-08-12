// Test fails on Windows because the Maps control is not implemented on this platform.
// Test fails on Android because a valid API key is required to render the map.
#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue13123 : _IssuesUITest
    {
        public Issue13123(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "[iOS] Map Pin InfoWindowClicked event never fires";

        [Test]
        [Category(UITestCategories.Maps)]
        public void InfoWindowClickedEventShouldFire()
        {
            App.WaitForElement("Map");
            TapMapPin();
            App.WaitForElement("Marker Clicked: Yes");
            TapInfoWindow();
            App.WaitForElement("Test Result: PASSED");
        }

        private void TapMapPin()
        {
            var mapElement = App.FindElement("Map");
            var rect = mapElement.GetRect();
            var centerX = rect.X + rect.Width / 2;
            var centerY = rect.Y + rect.Height / 2;
            App.TapCoordinates(centerX, centerY);
        }

        private void TapInfoWindow()
        {
            var mapElement = App.FindElement("Map");
            var rect = mapElement.GetRect();
#if IOS
            var centerX = rect.X + rect.Width / 2;
            var infoWindowY = rect.Y + rect.Height / 2 - 30;
             App.TapCoordinates(centerX, infoWindowY);
#elif MACCATALYST
            var centerX = rect.X + rect.Width / 2 + 20;
            var infoWindowY = rect.Y + rect.Height / 2;
            App.TapCoordinates(centerX, infoWindowY);
#endif
        }

    }
}
#endif

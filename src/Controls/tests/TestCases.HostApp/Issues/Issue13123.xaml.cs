using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 13123, "[iOS] Map Pin InfoWindowClicked event never fires",
        PlatformAffected.iOS)]
    public partial class Issue13123 : ContentPage
    {
        private Pin _testPin;

        public Issue13123()
        {
            InitializeComponent();
            SetupMap();
        }

        private void SetupMap()
        {
            _testPin = new Pin
            {
                Label = "Test Pin",
                Type = PinType.Place,
                Location = new Location(-37.8136, 144.9631),
                Address = "Melbourne, Australia\nTap to test InfoWindowClicked"
            };
            _testPin.MarkerClicked += (s, e) =>
            {
                MarkerClickedLabel.Text = "Marker Clicked: Yes";
            };
            _testPin.AutomationId = "Test Pin";
            _testPin.InfoWindowClicked += OnInfoWindowClicked;
            TestMap.Pins.Add(_testPin);
            var mapSpan = MapSpan.FromCenterAndRadius(_testPin.Location, Distance.FromKilometers(10));
            TestMap.MoveToRegion(mapSpan);
        }

        private void OnInfoWindowClicked(object sender, PinClickedEventArgs e)
        {
            InfoWindowClickedLabel.Text = "InfoWindow Clicked: Yes";
            StatusLabel.Text = "Status: SUCCESS - InfoWindowClicked event fired!";
            TestResultLabel.Text = "Test Result: PASSED";
            TestResultLabel.TextColor = Colors.Green;
            e.HideInfoWindow = true;
        }
    }
}

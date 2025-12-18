using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 13123, "[iOS] Map Pin InfoWindowClicked event never fires",
		PlatformAffected.iOS)]
	public partial class Issue13123 : ContentPage
	{
		Pin _testPin;
		Label _markerClickedLabel;
		Label _testResultLabel;
		Map _testMap;

		public Issue13123()
		{

			_markerClickedLabel = new Label
			{
				Text = "Marker Clicked: No",
				AutomationId = "MarkerClickedLabel"
			};

			_testResultLabel = new Label
			{
				Text = "Test Result: Pending",
				FontSize = 14,
				FontAttributes = FontAttributes.Bold,
				TextColor = Colors.Orange,
				AutomationId = "TestResultLabel"
			};

			var titleLabel = new Label
			{
				Text = "iOS Maps Pin InfoWindowClicked Event Test",
				FontSize = 16,
				FontAttributes = FontAttributes.Bold
			};
			var statusStack = new StackLayout
			{
				Padding = 10,
				BackgroundColor = Colors.LightGray,
				Children =
			{
				titleLabel,
				_markerClickedLabel,
				_testResultLabel
			}
			};
			_testMap = new Map
			{
				MapType = MapType.Street,
				HeightRequest = 400,
				AutomationId = "Map"
			};
			var mainGrid = new Grid
			{
				RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto }
			}
			};

			mainGrid.Add(statusStack, 0, 0);
			mainGrid.Add(_testMap, 0, 1);
			this.Content = mainGrid;
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
				_markerClickedLabel.Text = "Marker Clicked: Yes";
			};
			_testPin.AutomationId = "Test Pin";
			_testPin.InfoWindowClicked += OnInfoWindowClicked;
			_testMap.Pins.Add(_testPin);
			var mapSpan = MapSpan.FromCenterAndRadius(_testPin.Location, Distance.FromKilometers(10));
			_testMap.MoveToRegion(mapSpan);
		}

		private void OnInfoWindowClicked(object sender, PinClickedEventArgs e)
		{
			_testResultLabel.Text = "Test Result: PASSED";
			e.HideInfoWindow = true;
		}
	}
}

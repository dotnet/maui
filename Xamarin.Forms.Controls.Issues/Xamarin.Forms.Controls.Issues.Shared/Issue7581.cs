using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7581, "UWP Map not moving to location when map hidden then shown", PlatformAffected.UWP)]
	public class Issue7581 : TestContentPage
	{
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			var _map = new Map
			{
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsVisible = false
			};

			_map.MoveToRegion(MapSpan.FromCenterAndRadius(
				new Position(-41.342112, 173.179948), Distance.FromMiles(1))); // Nelson NZ

			var pos1 = new Position(-41.342112, 173.179948);
			var pin1 = new Pin
			{
				Type = PinType.Place,
				Position = pos1,
				Label = "Test Pin 1",
				Address = "15 Hunt Street, Nelson"
			};
			_map.Pins.Add(pin1);

			var pos2 = new Position(-41.341861, 173.193816);
			var pin2 = new Pin
			{
				Type = PinType.Place,
				Position = pos2,
				Label = "Test Pin 2",
				Address = "29 William Street, Nelson"
			};
			_map.Pins.Add(pin2);

			var toggleButton = new Button
			{
				Text = "Toggle Map Visibility"
			};
			toggleButton.Clicked += (sender, e) => { _map.IsVisible = !_map.IsVisible; };

			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute) });
			grid.Children.Add(_map, 0, 0);
			grid.Children.Add(toggleButton, 0, 1);

			Content = grid;
		}
	}
}
using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;


namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38284, "when creating a map in iOS, if the map is not visible when the page is created the zoom level is offn")]
	public class Bugzilla38284 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		Map map1;
		Map map2;
		double Latitude = 28.032005;
		double Longitude = -81.948931;
		string LocationTitle = "Someplace Cool";
		string StreetAddress = "";

		protected override void Init()
		{
			var stack = new StackLayout();

			map1 = new Maps.Map
			{
				IsShowingUser = false,
				WidthRequest = 320,
				HeightRequest = 200
			};

			map2 = new Maps.Map
			{
				IsShowingUser = false,
				WidthRequest = 320,
				HeightRequest = 200
			};


			var btn = new Button { Text = "Show" };
			btn.Clicked += (sender, e) =>
			{
				map2.IsVisible = !map2.IsVisible;
			};

			stack.Children.Add(map1);
			stack.Children.Add(map2);
			stack.Children.Add(btn);
			DisplayMaps();
			Content = stack;
		}

		public void DisplayMaps()
		{
			map2.IsVisible = false;
			var mapPinPosition = new Position(Latitude, Longitude);

			var type = MapType.Satellite;
			map1.MapType = type;
			map2.MapType = type;
			var pin = new Pin
			{
				Type = PinType.Place,
				Position = mapPinPosition,
				Label = LocationTitle,
				Address = StreetAddress
			};
			map1.Pins.Add(pin);
			map2.Pins.Add(pin);

			// Move the map to center on the map location with the proper zoom level
			var lldegrees = 360 / (Math.Pow(2, 16));
			map1.MoveToRegion(new MapSpan(map1.Pins[0].Position, lldegrees, lldegrees));
			map2.MoveToRegion(new MapSpan(map2.Pins[0].Position, lldegrees, lldegrees));
		}
	}
}

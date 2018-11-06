using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1755, "Initializing a map with a different MapType other than default has no effect", PlatformAffected.WinPhone)]
	public class Issue1755
		: ContentPage
	{
		public Issue1755()
		{
			var map = new Map (MapSpan.FromCenterAndRadius (new Position (54.767683, -1.571671), Distance.FromMiles (5))) {
				IsShowingUser = false,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand, 
				MapType = MapType.Hybrid
			};

			var switchMapTypeButton = new Button { Text = "Switch to hybrid" };
			switchMapTypeButton.Clicked += (sender, args) => map.MapType = map.MapType == MapType.Hybrid ? MapType.Street : MapType.Hybrid;

			var stack = new StackLayout { Spacing = 0 };
            stack.Children.Add(map);
            stack.Children.Add(switchMapTypeButton);
            Content = stack;
		}
	}
}

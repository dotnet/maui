using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1755, "Initializing a map with a different MapType other than default has no effect", PlatformAffected.WinPhone)]
	public class Issue1755
		: ContentPage
	{
		public Issue1755()
		{
			var map = new Map(MapSpan.FromCenterAndRadius(new Devices.Sensors.Location(54.767683, -1.571671), Distance.FromMiles(5)))
			{
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

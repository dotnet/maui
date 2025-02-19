using System.Diagnostics;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 886, "Map scrolling problems", PlatformAffected.Android)]
	public class Issue886 : ContentPage
	{
		public Issue886()
		{
			var isScrollEnabledSwitch = new Switch
			{
				IsToggled = true
			};
			var isScrollEnabledLabel = new Label
			{
				Text = "Scroll Enabled"
			};
			var isScrollEnabledLabelOn = new Label
			{
				Text = isScrollEnabledSwitch.IsToggled.ToString()
			};

			var isZoomEnabledSwitch = new Switch
			{
				IsToggled = true
			};
			var isZoomEnabledLabel = new Label
			{
				Text = "Zoom Enabled"
			};
			var isZoomEnabledLabelOn = new Label
			{
				Text = isZoomEnabledSwitch.IsToggled.ToString()
			};

			var map = new Map();
			isScrollEnabledSwitch.Toggled += (sender, e) =>
			{
				isScrollEnabledLabelOn.Text = e.Value.ToString();
				map.IsScrollEnabled = e.Value;
			};

			isZoomEnabledSwitch.Toggled += (sender, e) =>
			{
				isZoomEnabledLabelOn.Text = e.Value.ToString();
				map.IsZoomEnabled = e.Value;
			};

			Content = new StackLayout
			{
				Children ={
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							isScrollEnabledSwitch,
							isScrollEnabledLabel,
							isScrollEnabledLabelOn
						}
					},
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							isZoomEnabledSwitch,
							isZoomEnabledLabel,
							isZoomEnabledLabelOn
						}
					},
					map
				}
			};

		}

	}

}

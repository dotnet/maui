using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Maps;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Issue(IssueTracker.Bugzilla, 29017, "Pin clicked does not work on iOS maps")]
	public class Issue29017 : TestContentPage // or TestFlyoutPage, etc ...
	{
		Label _lbl;

		protected override void Init()
		{
			var map = new Map
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			_lbl = new Label
			{
				Text = "Not Clicked"
			};

			Content = new StackLayout
			{
				Children = {
					new Button {
						Text = "Add pins",
						Command = new Command (() => {
							foreach (var pin in map.Pins) {
								(pin as Pin).MarkerClicked -= PinClicked;
							}

							map.Pins.Clear ();

							for (int i = 0; i < 100; i++) {
								var rnd = new Random ();
								var lat = rnd.NextDouble () / 10;
								var lng = rnd.NextDouble () / 10;

								if (i % 2 == 0) {
									lat = -lat;
									lng = -lng;
								}

								var pin = new Pin {
									Address = "address",
									Label = "label",
									Type = PinType.Place,
									Location = new Devices.Sensors.Location (map.VisibleRegion.Center.Latitude + lat, map.VisibleRegion.Center.Longitude + lng)
								};

								pin.MarkerClicked += PinClicked;

								map.Pins.Add (pin);
							}
						})
					},
					_lbl,
					map
				}
			};
		}

		void PinClicked(object sender, EventArgs e)
		{
			_lbl.Text = "Click " + DateTime.Now.ToLocalTime();
		}
	}
}

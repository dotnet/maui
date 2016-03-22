using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class LayoutOptionsGallery : ContentPage
	{
		public LayoutOptionsGallery ()
		{
			Build ();
		}

		void Build ()
		{
			var mainLayout = new StackLayout {
				BackgroundColor = Color.Silver,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					BuildLayoutRegion (),
					BuildLayoutRegion ()
				}
			};

			var changeOrientationButton = new Button {Text = "Change Orientation"};
			changeOrientationButton.Clicked += (sender, args) => mainLayout.Orientation = (mainLayout.Orientation == StackOrientation.Horizontal) ? StackOrientation.Vertical : StackOrientation.Horizontal;

			Content = new StackLayout {
				Children = {
					changeOrientationButton,
					mainLayout
				}
			};
		}

		LayoutOptions StringToLayoutOptions (string options)
		{
			switch (options) {
				case "Start":
					return LayoutOptions.Start;
				case "StartAndExpand":
					return LayoutOptions.StartAndExpand;
				case "Center":
					return LayoutOptions.Center;
				case "CenterAndExpand":
					return LayoutOptions.CenterAndExpand;
				case "End":
					return LayoutOptions.End;
				case "EndAndExpand":
					return LayoutOptions.EndAndExpand;
				case "Fill":
					return LayoutOptions.Fill;
				case "FillAndExpand":
					return LayoutOptions.FillAndExpand;
			}
			throw new InvalidDataException ();
		}

		View BuildLayoutRegion ()
		{
			// Set these to fill and expand so they just fill their parent which is the thing we actually want to play with.
			var horizontalButton = new Button {
				Text = "H Options",
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};

			var verticalButton = new Button {
				Text = "V Options",
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};

			var result = new StackLayout {
				BackgroundColor = Color.Gray,
				Children = {
					horizontalButton,
					verticalButton
				}
			};

			horizontalButton.Clicked += async (sender, args) => {
				var selection = await DisplayActionSheet ("Select Horizontal Options", null, null,
				                                          "Start", "StartAndExpand", 
														  "Center", "CenterAndExpand",
				                                          "End", "EndAndExpand", 
														  "Fill", "FillAndExpand");

				result.HorizontalOptions = StringToLayoutOptions (selection);
			};

			verticalButton.Clicked += async (sender, args) => {
				var selection = await DisplayActionSheet ("Select Horizontal Options", null, null,
				                                          "Start", "StartAndExpand", 
														  "Center", "CenterAndExpand",
				                                          "End", "EndAndExpand", 
														  "Fill", "FillAndExpand");

				result.VerticalOptions = StringToLayoutOptions (selection);
			};

			return result;
		}
	}
}

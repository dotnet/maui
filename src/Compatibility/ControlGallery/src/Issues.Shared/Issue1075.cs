using System.Diagnostics;
using System.Reflection;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.BoxView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1075, "Does not update Color", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1075 : ContentPage
	{
		// Issue1075
		// BoxView doesn't update color
		public Issue1075()
		{
			var instructions = new Label
			{
				Text = "Tap the 'Change to blue' button below. If the BoxView does not " +
				"turn blue, this test has failed."
			};

			Label header = new Label
			{
				Text = "Picker",
				FontAttributes = FontAttributes.Bold,
				FontSize = 50,
				HorizontalOptions = LayoutOptions.Center
			};

			Picker picker = new Picker
			{
				Title = "Color",
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			foreach (string color in new string[]
				{
					"Aqua", "Black", "Blue", "Fuchsia",
					"Gray", "Green", "Lime", "Maroon",
					"Navy", "Olive", "Purple", "Red",
					"Silver", "Teal", "White", "Yellow"
				})
			{
				picker.Items.Add(color);
			}

			// Create BoxView for displaying pickedColor
			BoxView boxView = new BoxView
			{
				BackgroundColor = Colors.Gray,
				WidthRequest = 150,
				HeightRequest = 150,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			var button = new Button
			{
				Text = "Change to blue",
				Command = new Command(() => boxView.BackgroundColor = Colors.Blue)
			};

			picker.SelectedIndexChanged += (sender, args) =>
			{
				if (picker.SelectedIndex == -1)
				{
					boxView.Color = null;
				}
				else
				{
					string selectedItem = picker.Items[picker.SelectedIndex];
					FieldInfo colorField = typeof(Color).GetTypeInfo().GetDeclaredField(selectedItem);
					boxView.Color = (Color)colorField.GetValue(null);
				}
			};

			// Accomodate iPhone status bar.
			Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(10, 20, 10, 0) : new Thickness(10, 0);

			// Build the page.
			Content = new StackLayout
			{
				Children =
				{
					instructions,
					header,
					picker,
					boxView,
					button
				}
			};
		}
	}

}

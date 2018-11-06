using System.Diagnostics;
using System.Reflection;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.BoxView)]
#endif

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1075, "Does not update Color", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1075 : ContentPage
	{
		// Issue1075
		// BoxView doesn't update color
		public Issue1075 ()
		{
			Label header = new Label
			{
				Text = "Picker",
#pragma warning disable 618
				Font = Font.BoldSystemFontOfSize(50),
#pragma warning restore 618
				HorizontalOptions = LayoutOptions.Center
			};

			Picker picker = new Picker
			{
				Title = "Color",
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			foreach (string color in new string[]
				{
					"Aqua", "Black", "Blue", "Fuschia",
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
				WidthRequest = 150,
				HeightRequest = 150,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			var button = new Button {
				Text = "Change to blue",
				Command = new Command (() => boxView.BackgroundColor = Color.Aqua)
			};

			picker.SelectedIndexChanged += (sender, args) =>
			{
				if (picker.SelectedIndex == -1)
				{
					boxView.Color = Color.Default;
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
					header,
					picker,
					boxView,
					button
				}
			};
		}
	}
		
}

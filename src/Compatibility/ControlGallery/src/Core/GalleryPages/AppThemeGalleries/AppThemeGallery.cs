//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.AppThemeGalleries
{
	public class AppThemeGallery : ContentPage
	{
		public AppThemeGallery()
		{
			var descriptionLabel =
				   new Label { Text = "AppTheme Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "AppTheme Galleries";

			var button = new Button
			{
				Text = "Enable AppTheme",
				AutomationId = "EnableAppTheme"
			};
			button.Clicked += ButtonClicked;

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						button,
						GalleryBuilder.NavButton("AppTheme Code", () =>
							new AppThemeCodeGallery(), Navigation),
						GalleryBuilder.NavButton("AppTheme XAML", () =>
							new AppThemeXamlGallery(), Navigation),
					}
				}
			};
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "AppTheme Enabled!";
			button.TextColor = Colors.Black;
			button.IsEnabled = false;
		}
	}
}
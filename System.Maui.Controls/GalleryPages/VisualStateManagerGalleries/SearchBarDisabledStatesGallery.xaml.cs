using System;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchBarDisabledStatesGallery : ContentPage
	{
		public SearchBarDisabledStatesGallery ()
		{
			InitializeComponent ();

			Button0.Text = $"Toggle IsEnabled (Currently {Search0.IsEnabled})";
			Button1.Text = $"Toggle IsEnabled (Currently {Search1.IsEnabled})";
			Button2.Text = $"Toggle IsEnabled (Currently {Search2.IsEnabled})";
			Button3.Text = $"Toggle IsEnabled (Currently {Search3.IsEnabled})";
		}

		void Button0_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Search0, button);
		}

		void Button1_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Search1, button);
		}

		void Button2_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Search2, button);
		}

		void Button3_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Search3, button);
		}


		void ToggleIsEnabled(SearchBar searchBar, Button toggleButton)
		{
			searchBar.IsEnabled = !searchBar.IsEnabled;

			if (toggleButton != null)
			{
				toggleButton.Text = $"Toggle IsEnabled (Currently {searchBar.IsEnabled})";
			}
		}
	}
}
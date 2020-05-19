using System;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ButtonDisabledStatesGallery : ContentPage
	{
		public ButtonDisabledStatesGallery ()
		{
			InitializeComponent ();
		}

		void Button0_Toggle_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Button0, button);
		}

		void Button1_Toggle_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Button1, button);
		}

		void Button2_Toggle_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Button2, button);
		}

		void Button3_Toggle_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Button3, button);
		}

		void ToggleIsEnabled(Button button, Button toggleButton)
		{
			button.IsEnabled = !button.IsEnabled;

			if (toggleButton != null)
			{
				toggleButton.Text = $"Toggle IsEnabled (Currently {button.IsEnabled})";
			}
		}
	}
}
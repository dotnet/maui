using System;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EditorDisabledStatesGallery : ContentPage
	{
		public EditorDisabledStatesGallery()
		{
			InitializeComponent();

			Button0.Text = $"Toggle IsEnabled (Currently {Editor0.IsEnabled})";
			Button1.Text = $"Toggle IsEnabled (Currently {Editor1.IsEnabled})";
			Button2.Text = $"Toggle IsEnabled (Currently {Editor2.IsEnabled})";
			Button3.Text = $"Toggle IsEnabled (Currently {Editor3.IsEnabled})";
		}

		void Button0_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Editor0, button);
		}

		void Button1_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Editor1, button);
		}

		void Button2_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Editor2, button);
		}

		void Button3_OnClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			ToggleIsEnabled(Editor3, button);
		}


		void ToggleIsEnabled(Editor editor, Button toggleButton)
		{
			editor.IsEnabled = !editor.IsEnabled;

			if (toggleButton != null)
			{
				toggleButton.Text = $"Toggle IsEnabled (Currently {editor.IsEnabled})";
			}
		}
	}
}
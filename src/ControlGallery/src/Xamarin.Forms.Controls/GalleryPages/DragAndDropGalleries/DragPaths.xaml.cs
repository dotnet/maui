using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.DragAndDropGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DragPaths : ContentPage
	{
		public DragPaths()
		{
			InitializeComponent();
		}

		void OnMonkeyDragStarting(object sender, DragStartingEventArgs e)
		{
			e.Data.Text = "Monkey";
		}

		void OnCatDragStarting(object sender, DragStartingEventArgs e)
		{
			e.Data.Text = "Cat";
		}

		async void OnDrop(object sender, DropEventArgs e)
		{
			string text = await e.Data.GetTextAsync();

			if (text.Equals("Cat"))
			{
				await DisplayAlert("Correct", "Congratulations!", "OK");
			}
			else if (text.Equals("Monkey"))
			{
				await DisplayAlert("Incorrect", "Try again.", "OK");
			}
		}
	}
}
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.DragAndDropGalleries
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
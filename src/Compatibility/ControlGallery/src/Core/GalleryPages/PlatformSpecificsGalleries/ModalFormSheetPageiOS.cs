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

using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PlatformSpecificsGalleries
{
	public class ModalFormSheetPageiOS : ContentPage
	{
		public ModalFormSheetPageiOS()
		{
			Title = "Modal FormSheet";
			BackgroundColor = Colors.Azure;

			On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => Navigation.PopModalAsync();

			Content = new StackLayout
			{
				Children =
				{
					restoreButton
				}
			};
		}
	}
}
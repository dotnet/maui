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

using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class SimpleApp : Application
	{
		public SimpleApp()
		{
			var label = new Label { VerticalOptions = LayoutOptions.CenterAndExpand };

			var labelText = Preferences.Get("LabelText", string.Empty);
			if (!string.IsNullOrEmpty(labelText))
			{
				label.Text = labelText + " Restored!";
				Debug.WriteLine("Initialized");
			}
			else
			{
				Preferences.Set("LabelText", "Wowza");
				label.Text = Preferences.Get("LabelText", string.Empty) + " Set!";
				Debug.WriteLine("Saved");
			}

			MainPage = new ContentPage
			{
				Content = new StackLayout
				{
					Children =
					{
						label
					}
				}
			};
		}
	}
}
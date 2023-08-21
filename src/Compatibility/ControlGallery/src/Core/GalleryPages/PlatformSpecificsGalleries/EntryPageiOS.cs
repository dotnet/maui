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
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PlatformSpecificsGalleries
{
	public class EntryPageiOS : ContentPage
	{
		public EntryPageiOS()
		{
			var entry = new Entry
			{
				FontSize = 22,
				Placeholder = "Type and toggle AdjustsFontSizeToFitWidth"
			};

			Content = new StackLayout
			{
				Children =
				{
					entry,
					new Button
					{
						Text = "Toggle AdjustsFontSizeToFitWidth",
						Command = new Command(() => entry.On<iOS>().SetAdjustsFontSizeToFitWidth(!entry.On<iOS>().AdjustsFontSizeToFitWidth()))
					}
				}
			};
		}
	}
}

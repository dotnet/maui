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
	public class SearchBariOS : ContentPage
	{
		public SearchBariOS()
		{
			var prominent = new SearchBar { Placeholder = "Prominent" };
			prominent.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Prominent);

			var minimal = new SearchBar { Placeholder = "Minimal" };
			minimal.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Minimal);

			var prominentBackground = new SearchBar { Placeholder = "Prominent on colored background" };
			prominentBackground.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Prominent);

			var minimalBackground = new SearchBar { Placeholder = "Minimal on colored background" };
			minimalBackground.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Minimal);

			Content = new StackLayout
			{
				Children =
				{
					prominent,
					minimal,
					new StackLayout()
					{
						BackgroundColor = Colors.Red,
						Children =
						{
							prominentBackground,
							minimalBackground
						}
					}
				}
			};
		}
	}
}

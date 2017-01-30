using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
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

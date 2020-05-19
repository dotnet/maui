using System;
using System.Maui;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.iOSSpecific;

namespace System.Maui.Controls.GalleryPages.PlatformSpecificsGalleries
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

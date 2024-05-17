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

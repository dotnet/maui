using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	public static class GalleryBuilder
	{
		public static Button NavButton(string galleryName, Func<Page> gallery, INavigation nav)
		{
			var automationId = System.Text.RegularExpressions.Regex.Replace(galleryName, " |\\(|\\)", string.Empty);
			var button = new Button { Text = $"{galleryName}", AutomationId = automationId, FontSize = 10, HeightRequest = DeviceInfo.Platform == DevicePlatform.Android ? 40 : 30 };
			button.Clicked += (sender, args) => { nav.PushAsync(gallery()); };
			return button;
		}
	}
}
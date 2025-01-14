using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Pages
{
	public static class GalleryBuilder
	{
		public static Button NavButton(string galleryName, Func<Page> gallery, INavigation nav)
		{
			var automationId = RegexHelper.AutomationIdRegex.Replace(galleryName, string.Empty);
			var button = new Button
			{
				Text = $"{galleryName}",
				AutomationId = automationId,
				FontSize = 10,
				HeightRequest = DeviceInfo.Platform == DevicePlatform.Android ? 40 : 30,
				HorizontalOptions = LayoutOptions.Fill,
				Margin = 5,
				Padding = 5
			};
			button.Clicked += async (sender, args) =>
			{
				await nav.PushAsync(gallery());
			};
			return button;
		}
	}

	internal static partial class RegexHelper
	{
		#if NET7_0_OR_GREATER
		[GeneratedRegex (" |\\(|\\)", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
		internal static partial Regex AutomationIdRegex
		{
			get;
		}
		#else
		internal static readonly Regex AutomationIdRegex =
										new (
											" |\\(|\\)",
											RegexOptions.Compiled,		
											TimeSpan.FromMilliseconds(1000)							// against malicious input
											);
		#endif
	}
}
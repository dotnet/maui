using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	public static class GalleryBuilder
	{
		public static Button NavButton(string galleryName, Func<Page> gallery, INavigation nav)
		{
			var automationId = RegexHelper.AutomationIdRegex.Replace(galleryName, string.Empty);
			var button = new Button { Text = $"{galleryName}", AutomationId = automationId, FontSize = 10, HeightRequest = DeviceInfo.Platform == DevicePlatform.Android ? 40 : 30 };
			button.Clicked += (sender, args) => { nav.PushAsync(gallery()); };
			return button;
		}
	}

	internal static partial class RegexHelper
	{
		static readonly string AutomationIdRegexPattern = " |\\(|\\)";
		#if NET7_0_OR_GREATER
		[GeneratedRegex (AutomationIdRegexPattern, RegexOptions.None, matchTimeoutMilliseconds: 1000)]
		internal static partial Regex AutomationIdRegex
		{
			get;
		}
		#else
		internal static readonly Regex AutomationIdRegex =
										new (
											AutomationIdRegexPattern,
											RegexOptions.Compiled,		
											TimeSpan.FromMilliseconds(1000)							// against malicious input
											);
		#endif
	}
}
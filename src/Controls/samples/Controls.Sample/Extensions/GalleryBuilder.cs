using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
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
				HorizontalOptions = LayoutOptions.Fill,
				Margin = 5
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
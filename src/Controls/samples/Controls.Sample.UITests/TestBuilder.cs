using System;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests
{
	public static class TestBuilder
	{
		public static Button NavButton(string galleryName, Func<Page> gallery, INavigation nav)
		{
			var automationId = System.Text.RegularExpressions.Regex.Replace(galleryName, " |\\(|\\)", string.Empty);
			
			var button = new Button
			{
				Text = $"{galleryName}",
				AutomationId = automationId,
				FontSize = 10,
				HeightRequest = 30,
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
}
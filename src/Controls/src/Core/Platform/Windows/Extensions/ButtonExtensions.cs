#nullable enable

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateContentLayout(this MauiButton mauiButton, Button button)
		{
			// If the Content isn't the StackPanel setup by Maui.Core then
			// The user has set a custom Content or the content isn't a mix of text/images
			if (mauiButton.Content is not StackPanel container)
				return;

			var image = mauiButton.GetImage();
			var textBlock = mauiButton.GetTextBlock();

			// If either of these are null then the user has taken control of the content
			// and we don't know how to apply our changes
			if (image == null || textBlock == null)
				return;

			container.Children.Clear();
			var layout = button.ContentLayout;
			var spacing = layout.Spacing;

			container.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
			container.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;

			switch (layout.Position)
			{
				case Button.ButtonContentLayout.ImagePosition.Top:
					container.Orientation = Orientation.Vertical;
					image.Margin = WinUIHelpers.CreateThickness(0, 0, 0, spacing);					
					container.Children.Add(image);
					container.Children.Add(textBlock);
					break;
				case Button.ButtonContentLayout.ImagePosition.Bottom:
					container.Orientation = Orientation.Vertical;
					image.Margin = WinUIHelpers.CreateThickness(0, spacing, 0, 0);
					container.Children.Add(textBlock);
					container.Children.Add(image);
					break;
				case Button.ButtonContentLayout.ImagePosition.Right:
					container.Orientation = Orientation.Horizontal;
					image.Margin = WinUIHelpers.CreateThickness(spacing, 0, 0, 0);
					container.Children.Add(textBlock);
					container.Children.Add(image);
					break;
				default:
					// Defaults to image on the left
					container.Orientation = Orientation.Horizontal;
					image.Margin = WinUIHelpers.CreateThickness(0, 0, spacing, 0);
					container.Children.Add(image);
					container.Children.Add(textBlock);
					break;
			}
		}
	}
}
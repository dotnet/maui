using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateContentLayout(this UI.Xaml.Controls.Button mauiButton, Button button)
		{
			if (mauiButton.Content is not DefaultMauiButtonContent content)
			{
				// If the content is the default for Maui.Core, then
				// The user has set a custom Content or the content isn't a mix of text/images
				return;
			}

			var layout = button.ContentLayout;
			var spacing = layout.Spacing;

			switch (layout.Position)
			{
				case Button.ButtonContentLayout.ImagePosition.Top:
					content.LayoutImageTop(spacing);
					break;
				case Button.ButtonContentLayout.ImagePosition.Bottom:
					content.LayoutImageBottom(spacing);
					break;
				case Button.ButtonContentLayout.ImagePosition.Right:
					content.LayoutImageRight(spacing);
					break;
				default:
					// Defaults to image on the left
					content.LayoutImageLeft(spacing);
					break;
			}

			content.InvalidateMeasure();
		}

		public static void UpdateLineBreakMode(this Microsoft.UI.Xaml.Controls.Button platformButton, Button button)
		{
			if (platformButton.GetContent<TextBlock>() is TextBlock textBlock)
			{
				textBlock.UpdateLineBreakMode(button.LineBreakMode);
			}
		}
	}
}
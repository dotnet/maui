using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class FrameExtensions
	{
		public static void UpdateContent(this Border border, IFrame frame, IMauiContext? mauiContext)
		{
			var content = frame.Content;

			if (content == null || mauiContext == null)
				return;

			border.Child = content.ToNative(mauiContext);
		}

		public static void UpdateBackgroundColor(this Border border, IFrame frame)
		{
			Color backgroundColor = frame.BackgroundColor;
							
			border.Background = backgroundColor.IsDefault() ?	
				new UI.Xaml.Media.SolidColorBrush((Windows.UI.Color)border.Resources["SystemAltHighColor"]) : 
				backgroundColor.ToNative();
		}

		public static void UpdateBorderColor(this Border border, IFrame frame)
		{
			if (frame.BorderColor.IsDefault())
			{
				border.BorderBrush = new Color(0, 0, 0, 0).ToNative();
			}
			else
			{
				border.BorderBrush = frame.BorderColor.ToNative();
				border.BorderThickness = WinUIHelpers.CreateThickness(1);
			}
		}

		public static void UpdateCornerRadius(this Border border, IFrame frame)
		{
			float cornerRadius = frame.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // Default corner radius

			border.CornerRadius = WinUIHelpers.CreateCornerRadius(cornerRadius);
		}
	}
}
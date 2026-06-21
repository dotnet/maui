#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using static Microsoft.Maui.Controls.Button;
using TButton = Tizen.UIExtensions.NUI.Button;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this TButton platformButton, Button button)
		{
			var text = TextTransformUtilities.GetTransformedText(button.Text, button.TextTransform);
			platformButton.Text = text;
		}

		public static void UpdateContentLayout(this TButton nativeButton, Button button)
		{
			if (button.ImageSource != null)
			{
				var contentLayout = button.ContentLayout;
				nativeButton.IconPadding = (ushort)Math.Max(contentLayout.Spacing.ToScaledPixel(), 0);
				switch (contentLayout.Position)
				{
					case ButtonContentLayout.ImagePosition.Top:
						nativeButton.IconRelativeOrientation = TButton.IconOrientation.Top;
						break;
					case ButtonContentLayout.ImagePosition.Bottom:
						nativeButton.IconRelativeOrientation = TButton.IconOrientation.Bottom;
						break;
					case ButtonContentLayout.ImagePosition.Left:
						nativeButton.IconRelativeOrientation = TButton.IconOrientation.Left;
						break;
					case ButtonContentLayout.ImagePosition.Right:
						nativeButton.IconRelativeOrientation = TButton.IconOrientation.Right;
						break;
				}
			}
			else
			{
				nativeButton.IconPadding = 0;
			}
		}
	}
}
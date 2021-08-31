using System;
using CoreGraphics;
using Foundation;
using UIKit;
using static Microsoft.Maui.Controls.Button;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		static CGRect GetTitleBoundingRect(this UIButton nativeButton)
		{
			if (nativeButton.CurrentAttributedTitle != null ||
					   nativeButton.CurrentTitle != null)
			{
				var title =
					   nativeButton.CurrentAttributedTitle ??
					   new NSAttributedString(nativeButton.CurrentTitle, new UIStringAttributes { Font = nativeButton.TitleLabel.Font });

				return title.GetBoundingRect(
					nativeButton.Bounds.Size,
					NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
					null);
			}

			return CGRect.Empty;
		}

		public static void UpdatePadding(this UIButton nativeButton, Button button)
		{
			double spacingVertical = 0;
			double spacingHorizontal = 0;

			if (button.ImageSource != null)
			{
				if (button.ContentLayout.IsHorizontal())
				{
					spacingHorizontal = button.ContentLayout.Spacing;
				}
				else
				{
					var imageHeight = nativeButton.ImageView.Image?.Size.Height ?? 0f;

					if (imageHeight < nativeButton.Bounds.Height)
					{
						spacingVertical = button.ContentLayout.Spacing +
							nativeButton.GetTitleBoundingRect().Height;
					}

				}
			}

			nativeButton.ContentEdgeInsets = new UIEdgeInsets(
				(float)(button.Padding.Top + spacingVertical / 2),
				(float)(button.Padding.Left + spacingHorizontal / 2),
				(float)(button.Padding.Bottom + spacingVertical / 2),
				(float)(button.Padding.Right + spacingHorizontal / 2));
		}

		public static void UpdateContentLayout(this UIButton nativeButton, Button button)
		{
			if (nativeButton.Bounds.Width == 0)
			{
				return;
			}

			var imageInsets = new UIEdgeInsets();
			var titleInsets = new UIEdgeInsets();

			var layout = button.ContentLayout;
			var spacing = (nfloat)layout.Spacing;

			var image = nativeButton.CurrentImage;


			// if the image is too large then we just position at the edge of the button
			// depending on the position the user has picked
			// This makes the behavior consistent with android
			var contentMode = UIViewContentMode.Center;

			if (image != null && !string.IsNullOrEmpty(nativeButton.CurrentTitle))
			{
				// TODO: Do not use the title label as it is not yet updated and
				//       if we move the image, then we technically have more
				//       space and will require a new layout pass.

				var titleRect = nativeButton.GetTitleBoundingRect();
				var titleWidth = titleRect.Width;
				var imageWidth = image.Size.Width;
				var imageHeight = image.Size.Height;
				var buttonWidth = nativeButton.Bounds.Width;
				var buttonHeight = nativeButton.Bounds.Height;

				// These are just used to shift the image and title to center
				// Which makes the later math easier to follow
				imageInsets.Left += titleWidth / 2;
				imageInsets.Right -= titleWidth / 2;
				titleInsets.Left -= imageWidth / 2;
				titleInsets.Right += imageWidth / 2;

				if (layout.Position == ButtonContentLayout.ImagePosition.Top)
				{
					if (imageHeight > buttonHeight)
					{
						contentMode = UIViewContentMode.Top;
					}
					else
					{
						imageInsets.Top -= (buttonHeight - imageHeight) / 2;
						imageInsets.Bottom += (buttonHeight - imageHeight) / 2;

						titleInsets.Top += (imageHeight / 2 + spacing);
						titleInsets.Bottom -= imageHeight / 2;
					}
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					if (imageHeight > buttonHeight)
					{
						contentMode = UIViewContentMode.Bottom;
					}
					else
					{
						imageInsets.Top += (buttonHeight - imageHeight) / 2;
						imageInsets.Bottom -= (buttonHeight - imageHeight) / 2;
					}

					titleInsets.Top -= (imageHeight / 2 + spacing);
					titleInsets.Bottom += imageHeight / 2;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Left)
				{
					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Left;
					}
					else
					{
						imageInsets.Left -= (buttonWidth - imageWidth) / 2;
						imageInsets.Right += (buttonWidth - imageWidth) / 2;
					}

					titleInsets.Left += (imageWidth / 2);
					titleInsets.Right -= (imageWidth / 2 + spacing);
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Right)
				{
					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Right;
					}
					else
					{
						imageInsets.Left += (buttonWidth - imageWidth) / 2;
						imageInsets.Right -= (buttonWidth - imageWidth) / 2;
					}

					titleInsets.Left -= (imageWidth / 2 + spacing);
					titleInsets.Right += (imageWidth / 2);
				}
			}

			nativeButton.ImageView.ContentMode = contentMode;

			// This is used to match the behavior between platforms.
			// If the image is too big then we just hide the label because
			// the image is pushing the title out of the visible view.
			// We can't use insets because then the title shows up outside the 
			// bounds of the UIButton. We could set the UIButton to clip bounds
			// but that feels like it might cause confusing side effects
			if (contentMode == UIViewContentMode.Center)
				nativeButton.TitleLabel.Layer.Hidden = false;
			else
				nativeButton.TitleLabel.Layer.Hidden = true;

			nativeButton.UpdatePadding(button);

			if (nativeButton.ImageEdgeInsets != imageInsets ||
				nativeButton.TitleEdgeInsets != titleInsets)
			{
				nativeButton.ImageEdgeInsets = imageInsets;
				nativeButton.TitleEdgeInsets = titleInsets;
				nativeButton.Superview?.SetNeedsLayout();
			}
		}
	}
}
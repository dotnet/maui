#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using UIKit;
using static Microsoft.Maui.Controls.Button;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		static CGRect GetTitleBoundingRect(this UIButton platformButton)
		{
			if (platformButton.CurrentAttributedTitle != null ||
					   platformButton.CurrentTitle != null)
			{
				var title =
					   platformButton.CurrentAttributedTitle ??
					   new NSAttributedString(platformButton.CurrentTitle, new UIStringAttributes { Font = platformButton.TitleLabel.Font });

				return title.GetBoundingRect(
					platformButton.Bounds.Size,
					NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
					null);
			}

			return CGRect.Empty;
		}

		public static void UpdatePadding(this UIButton platformButton, Button button)
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
					var imageHeight = platformButton.ImageView.Image?.Size.Height ?? 0f;

					if (imageHeight < platformButton.Bounds.Height)
					{
						spacingVertical = button.ContentLayout.Spacing +
							platformButton.GetTitleBoundingRect().Height;
					}

				}
			}

			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

			padding += new Thickness(spacingHorizontal / 2, spacingVertical / 2);

			platformButton.UpdatePadding(padding);
		}

		public static void UpdateContentLayout(this UIButton platformButton, Button button)
		{
			if (platformButton.Bounds.Width == 0)
			{
				return;
			}

			var imageInsets = new UIEdgeInsets();
			var titleInsets = new UIEdgeInsets();

			var layout = button.ContentLayout;
			var spacing = (nfloat)layout.Spacing;

			var image = platformButton.CurrentImage;


			// if the image is too large then we just position at the edge of the button
			// depending on the position the user has picked
			// This makes the behavior consistent with android
			var contentMode = UIViewContentMode.Center;
			if (platformButton.Configuration is UIButtonConfiguration startingConfig)
			{
				startingConfig.ImagePlacement = NSDirectionalRectEdge.None;
				platformButton.Configuration = startingConfig;
			}

			if (image != null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				// TODO: Do not use the title label as it is not yet updated and
				//       if we move the image, then we technically have more
				//       space and will require a new layout pass.

				var titleRect = platformButton.GetTitleBoundingRect();
				var titleWidth = titleRect.Width;
				var titleHeight = titleRect.Height;
				var imageWidth = image.Size.Width;
				var imageHeight = image.Size.Height;
				var buttonWidth = platformButton.Bounds.Width;
				var buttonHeight = platformButton.Bounds.Height;
				var sharedSpacing = spacing / 2;

				// These are just used to shift the image and title to center
				// Which makes the later math easier to follow
				imageInsets.Left += titleWidth / 2;
				imageInsets.Right -= titleWidth / 2;
				titleInsets.Left -= imageWidth / 2;
				titleInsets.Right += imageWidth / 2;

				if (layout.Position == ButtonContentLayout.ImagePosition.Top)
				{
					if (platformButton.Configuration is UIButtonConfiguration config)
					{
						config.ImagePlacement = NSDirectionalRectEdge.Top;
						platformButton.Configuration = config;
					}

					if (imageHeight > buttonHeight)
					{
						contentMode = UIViewContentMode.Top;
					}
					else
					{
						imageInsets.Top -= (titleHeight / 2) + sharedSpacing;
						imageInsets.Bottom += titleHeight / 2;

						titleInsets.Top += imageHeight / 2;
						titleInsets.Bottom -= (imageHeight / 2) + sharedSpacing;
					}
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					if (platformButton.Configuration is UIButtonConfiguration config)
					{
						config.ImagePlacement = NSDirectionalRectEdge.Bottom;
						platformButton.Configuration = config;
					}

					if (imageHeight > buttonHeight)
					{
						contentMode = UIViewContentMode.Bottom;
					}
					else
					{
						imageInsets.Top += titleHeight / 2;
						imageInsets.Bottom -= (titleHeight / 2) + sharedSpacing;
					}

					titleInsets.Top -= (imageHeight / 2) + sharedSpacing;
					titleInsets.Bottom += imageHeight / 2;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Left)
				{
					if (platformButton.Configuration is UIButtonConfiguration config)
					{
						if ((button.Parent as VisualElement)?.FlowDirection == FlowDirection.RightToLeft)
							config.ImagePlacement = NSDirectionalRectEdge.Trailing;
						else
							config.ImagePlacement = NSDirectionalRectEdge.Leading;

						platformButton.Configuration = config;
					}

					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Left;
					}
					else
					{
						imageInsets.Left -= (titleWidth / 2) + sharedSpacing;
						imageInsets.Right += titleWidth / 2;
					}

					titleInsets.Left += imageWidth / 2;
					titleInsets.Right -= (imageWidth / 2) + sharedSpacing;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Right)
				{
					if (platformButton.Configuration is UIButtonConfiguration config)
					{
						if ((button.Parent as VisualElement)?.FlowDirection == FlowDirection.RightToLeft)
							config.ImagePlacement = NSDirectionalRectEdge.Leading;
						else
							config.ImagePlacement = NSDirectionalRectEdge.Trailing;

						platformButton.Configuration = config;
					}

					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Right;
					}
					else
					{
						imageInsets.Left += titleWidth / 2;
						imageInsets.Right -= (titleWidth / 2) + sharedSpacing;
					}

					titleInsets.Left -= (imageWidth / 2) + sharedSpacing;
					titleInsets.Right += imageWidth / 2;
				}
			}

			platformButton.ImageView.ContentMode = contentMode;

			// This is used to match the behavior between platforms.
			// If the image is too big then we just hide the label because
			// the image is pushing the title out of the visible view.
			// We can't use insets because then the title shows up outside the 
			// bounds of the UIButton. We could set the UIButton to clip bounds
			// but that feels like it might cause confusing side effects
			if (contentMode == UIViewContentMode.Center)
				platformButton.TitleLabel.Layer.Hidden = false;
			else
				platformButton.TitleLabel.Layer.Hidden = true;

			if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is UIButtonConfiguration buttonConfig)
			{
				// If there is an image above or below the Title, the top and bottom insets will be updated and need to be redrawn.
				if ((buttonConfig.ImagePlacement == NSDirectionalRectEdge.Top || buttonConfig.ImagePlacement == NSDirectionalRectEdge.Bottom)
					&& ((button.Padding.IsNaN && buttonConfig.ContentInsets == ConvertPaddingToInsets(ButtonHandler.DefaultPadding)) 
						|| buttonConfig.ContentInsets == ConvertPaddingToInsets(button.Padding)
						|| (IsCloseToZero(buttonConfig.ContentInsets) && button.Padding == Thickness.Zero)))
				{
					platformButton.UpdatePadding(button);
					platformButton.Superview?.SetNeedsLayout();
					return;
				}
			}

			platformButton.UpdatePadding(button);

			if (!OperatingSystem.IsIOSVersionAtLeast(15))
			{
				if (platformButton.ImageEdgeInsets != imageInsets ||
					platformButton.TitleEdgeInsets != titleInsets)
				{
					platformButton.ImageEdgeInsets = imageInsets;
					platformButton.TitleEdgeInsets = titleInsets;
					platformButton.Superview?.SetNeedsLayout();
				}
			}
		}

		static NSDirectionalEdgeInsets ConvertPaddingToInsets(Thickness thickness) =>
		new NSDirectionalEdgeInsets((nfloat)thickness.Top, (nfloat)thickness.Left, (nfloat)thickness.Bottom, (nfloat)thickness.Right);

		static bool IsCloseToZero(NSDirectionalEdgeInsets inset)
		{
			// The system sets the Top and Bottom insets to very small fractions when set to zero
			if (inset.Top < 0.0001 && inset.Leading < 0.0001 && inset.Bottom < 0.0001 && inset.Trailing < 0.0001)
				return true;

			return false;
		}

		public static void UpdateText(this UIButton platformButton, Button button)
		{
			var text = TextTransformUtilites.GetTransformedText(button.Text, button.TextTransform);
			platformButton.SetTitle(text, UIControlState.Normal);

			// Content layout depends on whether or not the text is empty; changing the text means
			// we may need to update the content layout
			platformButton.UpdateContentLayout(button);
		}

		public static void UpdateLineBreakMode(this UIButton nativeButton, Button button)
		{
			nativeButton.TitleLabel.LineBreakMode = button.LineBreakMode switch
			{
				LineBreakMode.NoWrap => UILineBreakMode.Clip,
				LineBreakMode.WordWrap => UILineBreakMode.WordWrap,
				LineBreakMode.CharacterWrap => UILineBreakMode.CharacterWrap,
				LineBreakMode.HeadTruncation => UILineBreakMode.HeadTruncation,
				LineBreakMode.TailTruncation => UILineBreakMode.TailTruncation,
				LineBreakMode.MiddleTruncation => UILineBreakMode.MiddleTruncation,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
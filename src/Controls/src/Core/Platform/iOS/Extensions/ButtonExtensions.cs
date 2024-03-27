#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
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

			if ((!OperatingSystem.IsIOSVersionAtLeast(15) || platformButton.Configuration is null) && button.ImageSource != null)
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

			object configuration = OperatingSystem.IsIOSVersionAtLeast(15) ? platformButton.Configuration : null;

			if (configuration is UIButtonConfiguration config)
			{
				config.ImagePadding = spacing;
				platformButton.Configuration = config;
			}

			var image = platformButton.CurrentImage;

			NSDirectionalRectEdge? originalContentMode = null;
			if (configuration is UIButtonConfiguration config1)
			{
				originalContentMode = config1.ImagePlacement;
			}

			// if the image is too large then we just position at the edge of the button
			// depending on the position the user has picked
			// This makes the behavior consistent with android
			var contentMode = UIViewContentMode.Center;
			if (configuration is UIButtonConfiguration config2)
			{
				config2.ImagePlacement = NSDirectionalRectEdge.None;
				platformButton.Configuration = config2;
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
					if (configuration is UIButtonConfiguration config3)
					{
						config3.ImagePlacement = NSDirectionalRectEdge.Top;
						platformButton.Configuration = config3;
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
					if (configuration is UIButtonConfiguration config4)
					{
						config4.ImagePlacement = NSDirectionalRectEdge.Bottom;
						platformButton.Configuration = config4;
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
					if (configuration is UIButtonConfiguration config5)
					{
						if ((button.Parent as VisualElement)?.FlowDirection == FlowDirection.RightToLeft)
							config5.ImagePlacement = NSDirectionalRectEdge.Trailing;
						else
							config5.ImagePlacement = NSDirectionalRectEdge.Leading;

						platformButton.Configuration = config5;
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
					if (configuration is UIButtonConfiguration config5)
					{
						if ((button.Parent as VisualElement)?.FlowDirection == FlowDirection.RightToLeft)
							config5.ImagePlacement = NSDirectionalRectEdge.Leading;
						else
							config5.ImagePlacement = NSDirectionalRectEdge.Trailing;

						platformButton.Configuration = config5;
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

			ResizeImageIfNecessary(platformButton, button, image);

			if (configuration is UIButtonConfiguration config6)
			{
				// If there is an image above or below the Title, the button will need to be redrawn the first time.
				if ((config6.ImagePlacement == NSDirectionalRectEdge.Top || config6.ImagePlacement == NSDirectionalRectEdge.Bottom)
					&& originalContentMode != config6.ImagePlacement)
				{
					platformButton.UpdatePadding(button);
					platformButton.Superview?.SetNeedsLayout();
					return;
				}
			}

			platformButton.UpdatePadding(button);

			if (configuration is null)
			{
				// ImageButton still will use the deprecated UIEdgeInsets for now.
#pragma warning disable CA1422 // Validate platform compatibility
				if (platformButton.ImageEdgeInsets != imageInsets ||
					platformButton.TitleEdgeInsets != titleInsets)
				{
					platformButton.ImageEdgeInsets = imageInsets;
					platformButton.TitleEdgeInsets = titleInsets;
					platformButton.Superview?.SetNeedsLayout();
				}
#pragma warning restore CA1422 // Validate platform compatibility
			}
		}

		static void ResizeImageIfNecessary(UIButton platformButton, Button button, UIImage image)
		{
			nfloat availableHeight = 0;
			nfloat availableWidth = 0;

			if (platformButton.Bounds != CGRect.Empty
				&& (button.Height != double.NaN || button.Width != double.NaN))
			{
				// If using iOS 15+ UIButtonConfiguration APIs
				if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is UIButtonConfiguration config
					&& config.ContentInsets is NSDirectionalEdgeInsets contentInsets)
				{
					// Case where the image is on top or bottom of the Title text.
					if (config.ImagePlacement == NSDirectionalRectEdge.Top || config.ImagePlacement == NSDirectionalRectEdge.Bottom)
					{
						availableHeight = platformButton.Bounds.Height - (contentInsets.Top + contentInsets.Bottom + config.ImagePadding + platformButton.GetTitleBoundingRect().Height);
						availableWidth = platformButton.Bounds.Width - (contentInsets.Leading + contentInsets.Trailing);
					}

					// Case where the image is on the left or right of the Title text.
					else
					{
						availableHeight = platformButton.Bounds.Height - (contentInsets.Top + contentInsets.Bottom);
						availableWidth = platformButton.Bounds.Width - (contentInsets.Leading + contentInsets.Trailing + config.ImagePadding + platformButton.GetTitleBoundingRect().Width);
					}
				}

				// If using the deprecated iOS 14 and below APIs
				else if (!OperatingSystem.IsIOSVersionAtLeast(15))
				{
					var contentEdgeInsets = platformButton.ContentEdgeInsets;
					var titleEdgeInsets = platformButton.TitleEdgeInsets;
					var imageEdgeInsets = platformButton.ImageEdgeInsets;

					// Case where the image is on top or bottom of the Title text.
					if (button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Top || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Bottom)
					{
						availableHeight = platformButton.Bounds.Height - (contentEdgeInsets.Top + contentEdgeInsets.Bottom + titleEdgeInsets.Top + titleEdgeInsets.Bottom + imageEdgeInsets.Top + imageEdgeInsets.Bottom);
						availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right + imageEdgeInsets.Left + imageEdgeInsets.Right);
					}

					// Case where the image is on the left or right of the Title text.
					else
					{
						availableHeight = platformButton.Bounds.Height - (contentEdgeInsets.Top + contentEdgeInsets.Bottom + imageEdgeInsets.Top + imageEdgeInsets.Bottom);
						availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right + titleEdgeInsets.Left + titleEdgeInsets.Right + imageEdgeInsets.Left + imageEdgeInsets.Right);
					}
				}
			}

			availableHeight = (nfloat)Math.Max(availableHeight, 0);
			availableWidth = (nfloat)Math.Max(availableWidth, 0);

			try
			{
				if (image.Size.Height > availableHeight || image.Size.Width > availableWidth)
				{
					image = ResizeImageSource(image, availableWidth, availableHeight);
				}
				else
				{
					return;
				}

				image = image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				platformButton.SetImage(image, UIControlState.Normal);

				platformButton.SetNeedsLayout();
			}
			catch (Exception)
			{
				button.Handler.MauiContext?.CreateLogger<ButtonHandler>()?.LogWarning("Can not load Button ImageSource");
			}
		}

		static UIImage ResizeImageSource(UIImage sourceImage, nfloat maxWidth, nfloat maxHeight)
		{
			if (sourceImage is null || sourceImage.CGImage is null)
				return null;

			var sourceSize = sourceImage.Size;
			float maxResizeFactor = (float)Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			if (maxResizeFactor > 1)
				return sourceImage;

			return UIImage.FromImage(sourceImage.CGImage, sourceImage.CurrentScale / maxResizeFactor, sourceImage.Orientation);
		}

		public static void UpdateText(this UIButton platformButton, Button button)
		{
			var text = TextTransformUtilites.GetTransformedText(button.Text, button.TextTransform);
			platformButton.SetTitle(text, UIControlState.Normal);
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

			if (OperatingSystem.IsIOSVersionAtLeast(15) && nativeButton.Configuration is UIButtonConfiguration config)
			{
				// If there is an image above or below the Title, the button will need to be redrawn the first time.
				config.TitleLineBreakMode = nativeButton.TitleLabel.LineBreakMode;
				nativeButton.Configuration = config;
			}

		}
	}
}
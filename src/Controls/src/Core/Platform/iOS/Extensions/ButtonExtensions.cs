#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
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
				config1.ImagePlacement = NSDirectionalRectEdge.None;
			}

			// if the image is too large then we just position at the edge of the button
			// depending on the position the user has picked
			// This makes the behavior consistent with android
			var contentMode = UIViewContentMode.Center;

			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

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

				// If the image is resized to take up too much of the button's space, we may want to leave the image centered and hide the title
				var availableWidth = buttonWidth - imageWidth - spacing - (nfloat)padding.Left - (nfloat)padding.Right;
				var availableHeight = buttonHeight - imageHeight - spacing - (nfloat)padding.Top - (nfloat)padding.Bottom;

				var hideTitleDistance = nfloat.MaxValue / 2;

				if (layout.Position == ButtonContentLayout.ImagePosition.Top)
				{
					if (configuration is UIButtonConfiguration configLayout)
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
					if (configuration is UIButtonConfiguration configLayout)
					{
						if (availableHeight <= 0 && button.HeightRequest != -1)
							configLayout.Title = string.Empty;

						configLayout.ImagePlacement = NSDirectionalRectEdge.Bottom;
						platformButton.Configuration = configLayout;
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
					if (configuration is UIButtonConfiguration configLayout)
					{
						if (availableWidth <= 0)
							configLayout.Title = string.Empty;

						if ((button.Parent as VisualElement)?.FlowDirection == FlowDirection.RightToLeft)
							configLayout.ImagePlacement = NSDirectionalRectEdge.Trailing;
						else
							configLayout.ImagePlacement = NSDirectionalRectEdge.Leading;

						platformButton.Configuration = configLayout;
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
					if (configuration is UIButtonConfiguration configLayout)
					{
						if (availableWidth <= 0)
							configLayout.Title = string.Empty;

						if ((button.Parent as VisualElement)?.FlowDirection == FlowDirection.RightToLeft)
							configLayout.ImagePlacement = NSDirectionalRectEdge.Leading;
						else
							configLayout.ImagePlacement = NSDirectionalRectEdge.Trailing;

						platformButton.Configuration = configLayout;
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

#pragma warning disable CA1416, CA1422 // TODO: [UnsupportedOSPlatform("ios15.0")]
			if (platformButton.ImageEdgeInsets != imageInsets ||
				platformButton.TitleEdgeInsets != titleInsets)
			{
				platformButton.ImageEdgeInsets = imageInsets;
				platformButton.TitleEdgeInsets = titleInsets;
				platformButton.Superview?.SetNeedsLayout();
			}
#pragma warning restore CA1416, CA1422

			var titleRectHeight = platformButton.GetTitleBoundingRect(padding).Height;

			var buttonContentHeight = 
				+ (nfloat)Math.Max(titleRectHeight, platformButton.CurrentImage?.Size.Height ?? 0)
				+ (nfloat)padding.Top
				+ (nfloat)padding.Bottom;

			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				if (layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					buttonContentHeight += spacing;
					buttonContentHeight += (nfloat)Math.Min(titleRectHeight, platformButton.CurrentImage?.Size.Height ?? 0);
				}

#pragma warning disable CA1416, CA1422
				// If the button's content is larger than the button, we need to adjust the ContentEdgeInsets.
				// Apply a small buffer to the image size comparison since iOS can return a size that is off by a fraction of a pixel
				if (configuration is not UIButtonConfiguration && buttonContentHeight - button.Height > 1 && button.HeightRequest == -1)
				{
					var contentInsets = platformButton.ContentEdgeInsets;

					var additionalVerticalSpace = (buttonContentHeight - button.Height) / 2;

					platformButton.ContentEdgeInsets = new UIEdgeInsets(
						(nfloat)(additionalVerticalSpace + (nfloat)padding.Top),
						contentInsets.Left,
						(nfloat)(additionalVerticalSpace + (nfloat)padding.Bottom),
						contentInsets.Right);

					platformButton.Superview?.SetNeedsLayout();
					platformButton.Superview?.LayoutIfNeeded();
				}
#pragma warning restore CA1416, CA1422
			}
		}

		static bool ResizeImageIfNecessary(UIButton platformButton, Button button, UIImage image, nfloat spacing, Thickness padding)
		{
			// If the image is on the left or right, we still have an implicit width constraint
			if (button.HeightRequest == -1 && button.WidthRequest == -1 && (button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Top || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Bottom))
			{
				return false;
			}

			nfloat availableHeight = nfloat.MaxValue;
			nfloat availableWidth = nfloat.MaxValue;

			// Apply a small buffer to the image size comparison since iOS can return a size that is off by a fraction of a pixel.
			var buffer = 0.1;

			if (platformButton.Bounds != CGRect.Empty
				&& (button.Height != double.NaN || button.Width != double.NaN))
			{
				var contentWidth = platformButton.Bounds.Width - (nfloat)padding.Left - (nfloat)padding.Right;
					
				if (image.Size.Width - contentWidth > buffer)
				{
					availableWidth = contentWidth;
				}

				var contentHeight = platformButton.Bounds.Height - ((nfloat)padding.Top + (nfloat)padding.Bottom);
				if (image.Size.Height - contentHeight > buffer)
				{
					availableHeight = contentHeight;
				}
			}

			availableHeight = button.HeightRequest == -1 ? nfloat.PositiveInfinity : (nfloat)Math.Max(availableHeight, 0);
			// availableWidth = button.WidthRequest == -1 ? platformButton.Bounds.Width : (nfloat)Math.Max(availableWidth, 0);

			availableWidth = (nfloat)Math.Max(availableWidth, 0);

			try
			{
				if (image.Size.Height - availableHeight > buffer || image.Size.Width - availableWidth > buffer)
				{
					image = ResizeImageSource(image, availableWidth, availableHeight);
				}
				else
				{
					return false;
				}

				image = image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				platformButton.SetImage(image, UIControlState.Normal);

				platformButton.Superview?.SetNeedsLayout();

				return true;
			}
			catch (Exception)
			{
				button.Handler.MauiContext?.CreateLogger<ButtonHandler>()?.LogWarning("Can not load Button ImageSource");
			}

			return false;
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

			if (OperatingSystem.IsIOSVersionAtLeast(15) && nativeButton.Configuration is UIButtonConfiguration config)
			{
				config.TitleLineBreakMode = nativeButton.TitleLabel.LineBreakMode;
				nativeButton.Configuration = config;
			}
		}
	}
}
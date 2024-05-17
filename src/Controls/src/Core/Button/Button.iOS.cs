#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using CoreGraphics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		CGSize _originalImageSize = CGSize.Empty;

		protected override Size ArrangeOverride(Rect bounds)
		{
			var result = base.ArrangeOverride(bounds);

			var button = this;
			var platformButton = Handler?.PlatformView as UIButton;

			if (button == null || platformButton == null)
			{
				return result;
			}

			if (platformButton.Bounds.Width == 0)
			{
				return result;
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

			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

			// If the button's image takes up too much space, we will want to hide the title
			var hidesTitle = false;

			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				// Save the original image size the first time before resizing
				if (_originalImageSize == CGSize.Empty)
				{
					_originalImageSize = platformButton.CurrentImage.Size;
				}

				// Resize the image if necessary and then update the image variable
				if (ResizeImageIfNecessary(platformButton, button, image, spacing, padding, bounds, _originalImageSize))
				{
					image = platformButton.CurrentImage;
				}

				// TODO: Do not use the title label as it is not yet updated and
				//       if we move the image, then we technically have more
				//       space and will require a new layout pass.
				var titleRect = platformButton.GetTitleBoundingRect(padding);
				var titleWidth = titleRect.Width;
				var titleHeight = titleRect.Height;
				var imageWidth = image.Size.Width;
				var imageHeight = image.Size.Height;
				var buttonWidth = platformButton.Bounds.Width;
				var buttonHeight = platformButton.Bounds.Height;
				var sharedSpacing = spacing / 2;

				// The titleWidth will include the part of the title that is potentially truncated. Let's figure out the max width of the title in the button for our calculations.
				// Note: we do not calculate spacing in maxTitleWidth since the original button laid out by iOS will not contain the spacing in the measurements.
				var maxTitleWidth = platformButton.Bounds.Width - (imageWidth + (nfloat)padding.Left + (nfloat)padding.Right);
				var titleWidthMove = (nfloat)Math.Min(maxTitleWidth, titleWidth);

				// These are just used to shift the image and title to center
				// Which makes the later math easier to follow
				imageInsets.Left += titleWidthMove / 2;
				imageInsets.Right -= titleWidthMove / 2;
				titleInsets.Left -= imageWidth / 2;
				titleInsets.Right += imageWidth / 2;

				if (layout.Position == ButtonContentLayout.ImagePosition.Top)
				{
					if (imageHeight > buttonHeight)
					{
						contentMode = UIViewContentMode.Top;
					}

					imageInsets.Top -= (titleHeight / 2) + sharedSpacing;
					imageInsets.Bottom += (titleHeight / 2) + sharedSpacing;

					titleInsets.Top += (imageHeight / 2) + sharedSpacing;
					titleInsets.Bottom -= (imageHeight / 2) + sharedSpacing;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					if (imageHeight > buttonHeight)
					{
						contentMode = UIViewContentMode.Bottom;
					}

					imageInsets.Top += (titleHeight / 2) + sharedSpacing;
					imageInsets.Bottom -= (titleHeight / 2) + sharedSpacing;

					titleInsets.Top -= (imageHeight / 2) + sharedSpacing;
					titleInsets.Bottom += (imageHeight / 2) + sharedSpacing;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Left)
				{
					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Left;
					}

					imageInsets.Left -= (titleWidthMove / 2) + sharedSpacing;
					imageInsets.Right += (titleWidthMove / 2) + sharedSpacing;

					titleInsets.Left += (imageWidth / 2) + sharedSpacing;
					titleInsets.Right -= (imageWidth / 2) + sharedSpacing;

				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Right)
				{
					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Right;
					}

					imageInsets.Left += (titleWidthMove / 2) + sharedSpacing;
					imageInsets.Right -= (titleWidthMove / 2) + sharedSpacing;

					titleInsets.Left -= (imageWidth / 2) + sharedSpacing;
					titleInsets.Right += (imageWidth / 2) + sharedSpacing;
				}
			}

			// If we just have an image, we can still resize it here
			else if (image is not null)
			{
				ResizeImageIfNecessary(platformButton, button, image, 0, padding, bounds, _originalImageSize);
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

			platformButton.UpdatePadding(button);

#pragma warning disable CA1416, CA1422
			if (platformButton.ImageEdgeInsets != imageInsets ||
				platformButton.TitleEdgeInsets != titleInsets)
			{
				platformButton.ImageEdgeInsets = imageInsets;
				platformButton.TitleEdgeInsets = titleInsets;
				platformButton.Superview?.SetNeedsLayout();
				platformButton.Superview?.LayoutIfNeeded();
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
					if (!hidesTitle)
					{
						buttonContentHeight += spacing;
						buttonContentHeight += (nfloat)Math.Min(titleRectHeight, platformButton.CurrentImage?.Size.Height ?? 0);
					}
					// If the title is hidden, we don't need to add the spacing or the title to this measurement
					else
					{
						if (titleRectHeight > platformButton.CurrentImage.Size.Height)
						{
							buttonContentHeight -= titleRectHeight;
							buttonContentHeight += platformButton.CurrentImage.Size.Height;
						}
					}
				}

#pragma warning disable CA1416, CA1422

				var maxButtonHeight = Math.Min(buttonContentHeight, bounds.Height);

				// If the button's content is larger than the button, we need to adjust the ContentEdgeInsets.
				// Apply a small buffer to the image size comparison since iOS can return a size that is off by a fraction of a pixel
				if (maxButtonHeight - button.Height > 1 && button.HeightRequest == -1)
				{
					var contentInsets = platformButton.ContentEdgeInsets;

					var additionalVerticalSpace = (maxButtonHeight - button.Height) / 2;

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

			return result;
		}

		static bool ResizeImageIfNecessary(UIButton platformButton, Button button, UIImage image, nfloat spacing, Thickness padding, Rect bounds, CGSize originalImageSize)
		{
			var currentImageWidth = image.Size.Width;
			var currentImageHeight = image.Size.Height;

			nfloat availableWidth = (nfloat)bounds.Width;
			nfloat availableHeight = (nfloat)bounds.Height;

			// Apply a small buffer to the image size comparison since iOS can return a size that is off by a fraction of a pixel.
			var buffer = 0.1;

			if (bounds != Rect.Zero && (!double.IsNaN(bounds.Height) || !double.IsNaN(bounds.Width)))
			{
				var contentWidth = (nfloat)bounds.Width - (nfloat)padding.Left - (nfloat)padding.Right;

				if (currentImageWidth - contentWidth > buffer)
				{
					availableWidth = contentWidth;
				}

				var contentHeight = (nfloat)bounds.Height - ((nfloat)padding.Top + (nfloat)padding.Bottom);
				if (currentImageHeight - contentHeight > buffer)
				{
					availableHeight = contentHeight;
				}
			}

			// make sure we do not have negative values
			availableWidth = (nfloat)Math.Max(availableWidth, 0.1f);
			availableHeight = (nfloat)Math.Max(availableHeight, 0.1f);

			try
			{
				// if the image is too large then we will size it smaller
				if (currentImageHeight - availableHeight > buffer || currentImageWidth - availableWidth > buffer)
				{
					image = ResizeImageSource(image, availableWidth, availableHeight, originalImageSize);
				}
				// if the image is already sized down but now has more space, we will size it up no more than the original image size
				else if (availableHeight - ((nfloat)padding.Top + (nfloat)padding.Bottom) - currentImageHeight > buffer && availableWidth - (nfloat)padding.Left - (nfloat)padding.Right - currentImageWidth > buffer
					&& currentImageHeight != originalImageSize.Height && currentImageWidth != originalImageSize.Width)
				{
					image = ResizeImageSource(image, availableWidth, availableHeight, originalImageSize, true);
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

		static UIImage ResizeImageSource(UIImage sourceImage, nfloat maxWidth, nfloat maxHeight, CGSize originalImageSize, bool shouldScaleUp = false)
		{
			if (sourceImage is null || sourceImage.CGImage is null)
				return null;

			maxWidth = (nfloat)Math.Min(maxWidth, originalImageSize.Width);
			maxHeight = (nfloat)Math.Min(maxHeight, originalImageSize.Height);

			var sourceSize = sourceImage.Size;

			float maxResizeFactor = (float)Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			if (maxResizeFactor > 1 && !shouldScaleUp)
				return sourceImage;

			return UIImage.FromImage(sourceImage.CGImage, sourceImage.CurrentScale / maxResizeFactor, sourceImage.Orientation);
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		private static void MapPadding(IButtonHandler handler, Button button)
		{
			handler.PlatformView.UpdatePadding(button);
		}

		public static void MapText(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}
	}
}

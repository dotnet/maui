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

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			var button = this;

			var platformButton = Handler?.PlatformView as UIButton;

			if (button is null || platformButton is null)
			{
				return Size.Zero;
			}

			var layout = button.ContentLayout;
			var spacing = (nfloat)layout.Spacing;
			var borderWidth = button.BorderWidth < 0 ? 0 : button.BorderWidth;

			// if the image is too large then we just position at the edge of the button
			// depending on the position the user has picked
			// This makes the behavior consistent with android
			var contentMode = UIViewContentMode.Center;

			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

			var buttonWidthConstraint = button.WidthRequest == -1 ? widthConstraint : Math.Min(button.WidthRequest, widthConstraint);
			var buttonHeightConstraint = button.HeightRequest == -1 ? heightConstraint : Math.Min(button.HeightRequest, heightConstraint);

			var titleWidthConstraint = buttonWidthConstraint - padding.Left - padding.Right - borderWidth * 2;
			var titleHeightConstraint = buttonHeightConstraint - padding.Top - padding.Bottom - borderWidth * 2;

			var image = platformButton.CurrentImage;

			if (image is not null)
			{
				// Save the original image size for later image resizing
				if (_originalImageSize == CGSize.Empty)
				{
					_originalImageSize = image.Size;
				}

				// Resize the image if necessary and then update the image variable
				if (ResizeImageIfNecessary(platformButton, button, image, 0, padding, borderWidth, buttonWidthConstraint, buttonHeightConstraint, _originalImageSize))
				{
					image = platformButton.CurrentImage;
				}
			}

			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				// In non-UIButtonConfiguration setups, the title will always be truncated by the image's width
				// even when the image is on top or bottom.
				titleWidthConstraint -= image.Size.Width;

				// When the image is on top or bottom and the HorizontalOptions is not fill,
				// the title will only be as large as the borderwidth if there is no explicit width request.
				// This shouldn't be an issue with Configuration APIs later down the line.
				if ((layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom)
					&& button.WidthRequest == -1 && button.HorizontalOptions != LayoutOptions.Fill)
				{
					titleWidthConstraint = borderWidth * 2;
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

			platformButton.UpdatePadding(button);

			var titleRect = platformButton.GetTitleBoundingRect(titleWidthConstraint, titleHeightConstraint);
			var titleRectWidth = titleRect.Width;
			var titleRectHeight = titleRect.Height;

			var buttonContentWidth =
				+ (nfloat)Math.Max(titleRectWidth, platformButton.CurrentImage?.Size.Width ?? 0)
				+ (nfloat)padding.Left
				+ (nfloat)padding.Right
				+ (nfloat)borderWidth * 2;

			var buttonContentHeight =
				+ (nfloat)Math.Max(titleRectHeight, platformButton.CurrentImage?.Size.Height ?? 0)
				+ (nfloat)padding.Top
				+ (nfloat)padding.Bottom
				+ (nfloat)borderWidth * 2;

			// if we have both an image and title, add the smaller of the two to the calculation as well
			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				if (layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					buttonContentHeight += spacing;
					buttonContentHeight += (nfloat)Math.Min(titleRectHeight, platformButton.CurrentImage?.Size.Height ?? 0);
				}

				else
				{
					buttonContentWidth += spacing;
					buttonContentWidth += (nfloat)Math.Min(titleRectWidth, platformButton.CurrentImage?.Size.Width ?? 0);
				}
			}

			var ret = new Size(button.WidthRequest == -1 ? Math.Min(buttonContentWidth, buttonWidthConstraint) : button.WidthRequest,
							button.HeightRequest == -1 ? Math.Min(buttonContentHeight, buttonHeightConstraint) : button.HeightRequest);

			// Rounding the values up to the nearest whole number to match UIView.SizeThatFits
			ret = new Size((int)Math.Ceiling(ret.Width), (int)Math.Ceiling(ret.Height));

			return ret;
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			var button = this;
			var platformButton = Handler?.PlatformView as UIButton;

			// Layout the image and title of the button
			LayoutButton(platformButton, button, bounds);

			return base.ArrangeOverride(bounds);
		}

		void LayoutButton(UIButton platformButton, Button button, Rect size)
		{
			var layout = button.ContentLayout;
			var spacing = (nfloat)layout.Spacing;
			var borderWidth = button.BorderWidth < 0 ? 0 : button.BorderWidth;

			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

			var buttonWidthConstraint = button.WidthRequest == -1 ? size.Width : Math.Min(button.WidthRequest, size.Width);
			var buttonHeightConstraint = button.HeightRequest == -1 ? size.Height : Math.Min(button.HeightRequest, size.Height);

			var titleWidthConstraint = buttonWidthConstraint - (nfloat)padding.Left + (nfloat)padding.Right + ((nfloat)borderWidth * 2);
			var titleHeightConstraint = buttonHeightConstraint - (nfloat)padding.Top + (nfloat)padding.Bottom + ((nfloat)borderWidth * 2);

			var imageInsets = new UIEdgeInsets();
			var titleInsets = new UIEdgeInsets();

			var image = platformButton.CurrentImage;

			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				// In non-UIButtonConfiguration setups, the title will always be truncated by the image's width
				// even when the image is on top or bottom.
				titleWidthConstraint -= image.Size.Width;

				// When the image is on top or bottom and the HorizontalOptions is not fill,
				// the title will only be as large as the borderwidth if there is no explicit width request.
				// This shouldn't be an issue with Configuration APIs later down the line.
				if ((layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom)
					&& button.WidthRequest == -1 && button.HorizontalOptions != LayoutOptions.Fill)
				{
					titleWidthConstraint = borderWidth * 2;
				}

				var titleRect = platformButton.GetTitleBoundingRect(titleWidthConstraint, titleHeightConstraint);
				var titleWidth = titleRect.Width;
				var titleHeight = titleRect.Height;
				var imageWidth = image.Size.Width;
				var imageHeight = image.Size.Height;
				var sharedSpacing = spacing / 2;

				// These are just used to shift the image and title to center
				// Which makes the later math easier to follow
				imageInsets.Left += titleWidth / 2;
				imageInsets.Right -= titleWidth / 2;
				titleInsets.Left -= imageWidth / 2;
				titleInsets.Right += imageWidth / 2;

				if (layout.Position == ButtonContentLayout.ImagePosition.Top)
				{
					imageInsets.Top -= (titleHeight / 2) + sharedSpacing;
					imageInsets.Bottom += (titleHeight / 2) + sharedSpacing;

					titleInsets.Top += (imageHeight / 2) + sharedSpacing;
					titleInsets.Bottom -= (imageHeight / 2) + sharedSpacing;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					imageInsets.Top += (titleHeight / 2) + sharedSpacing;
					imageInsets.Bottom -= (titleHeight / 2) + sharedSpacing;

					titleInsets.Top -= (imageHeight / 2) + sharedSpacing;
					titleInsets.Bottom += (imageHeight / 2) + sharedSpacing;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Left)
				{
					imageInsets.Left -= (titleWidth / 2) + sharedSpacing;
					imageInsets.Right += (titleWidth / 2) + sharedSpacing;

					titleInsets.Left += (imageWidth / 2) + sharedSpacing;
					titleInsets.Right -= (imageWidth / 2) + sharedSpacing;

				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Right)
				{
					imageInsets.Left += (titleWidth / 2) + sharedSpacing;
					imageInsets.Right -= (titleWidth / 2) + sharedSpacing;

					titleInsets.Left -= (imageWidth / 2) + sharedSpacing;
					titleInsets.Right += (imageWidth / 2) + sharedSpacing;
				}
			}

			platformButton.UpdatePadding(button);

#pragma warning disable CA1416, CA1422
			if (platformButton.ImageEdgeInsets != imageInsets ||
				platformButton.TitleEdgeInsets != titleInsets)
			{
				platformButton.ImageEdgeInsets = imageInsets;
				platformButton.TitleEdgeInsets = titleInsets;
			}
#pragma warning restore CA1416, CA1422
		}

		static bool AreSignificantlyDifferent(double value1, double value2, double buffer = 0.1)
		{
				return Math.Abs(value1 - value2) > buffer;
		}

		static bool ResizeImageIfNecessary(UIButton platformButton, Button button, UIImage image, nfloat spacing, Thickness padding, double borderWidth, double widthConstraint, double heightConstraint, CGSize originalImageSize)
		{
			var currentImageWidth = image.Size.Width;
			var currentImageHeight = image.Size.Height;

			nfloat availableWidth = (nfloat)widthConstraint;
			nfloat availableHeight = (nfloat)heightConstraint;

			var additionalHorizontalSpace = (nfloat)padding.Left + (nfloat)padding.Right + ((nfloat)borderWidth * 2);
			var additionalVerticalSpace = (nfloat)padding.Top + (nfloat)padding.Bottom + ((nfloat)borderWidth * 2);

			// Apply a small buffer to the image size comparison since iOS can return a size that is off by a fraction of a pixel.
			var buffer = 0.1;

			if (!double.IsNaN(widthConstraint) || !double.IsNaN(heightConstraint))
			{
				var contentWidth = (nfloat)widthConstraint - additionalHorizontalSpace;

				if (currentImageWidth - contentWidth > buffer)
				{
					availableWidth = contentWidth;
				}

				var contentHeight = (nfloat)heightConstraint - additionalVerticalSpace;
				if (currentImageHeight - contentHeight > buffer)
				{
					availableHeight = contentHeight;
				}
			}

			// make sure we have values greater than 0
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
				else if (availableHeight - additionalVerticalSpace - currentImageHeight > buffer
					&& availableWidth - additionalHorizontalSpace - currentImageWidth > buffer
					&& currentImageHeight != originalImageSize.Height
					&& currentImageWidth != originalImageSize.Width)
				{
					image = ResizeImageSource(image, (nfloat)widthConstraint - additionalHorizontalSpace, (nfloat)heightConstraint - additionalVerticalSpace, originalImageSize, true);
				}
				else
				{
					return false;
				}

				image = image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				platformButton.SetImage(image, UIControlState.Normal);

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

		internal static void MapBorderWidth(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateContentLayout(button);
		}
	}
}

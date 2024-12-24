#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Button : ICrossPlatformLayout
	{
		// _originalImage and _originalImageSize are used to ensure we don't resize the image larger than the original image size
		// and to ensure if a new image is loaded, we use that image's size for resizing.
		CGImage _originalCGImage = null;
		CGSize _originalImageSize = CGSize.Empty;

		/// <summary>
		/// Measure the desired size of the button based on the image and title size taking into account
		/// the padding, spacing, margins, borders, and image placement.
		/// This method will also try to resize the image if it is too large for the button.
		/// </summary>
		/// <param name="widthConstraint"></param>
		/// <param name="heightConstraint"></param>
		/// <returns>Returns a <see cref="Size"/> representing the width and height of the button.</returns>
		/// <remarks>This method is used to pseudo-override the SizeThatFits() on the UIButton since we cannot override the UIButton class.</remarks>
		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
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

			var image = platformButton.CurrentImage;

			// We can use the ContentEdgeInsets to provide space for the border.
			// If the image is on the left or right, we can also incorporate the padding and spacing in the ContentEdgeInsets so that the title does not try to use that extra space.
			var contentEdgeInsets = new Thickness(borderWidth);
			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle)
				&& (layout.Position == ButtonContentLayout.ImagePosition.Left || layout.Position == ButtonContentLayout.ImagePosition.Right))
			{
				contentEdgeInsets.Left += (nfloat)padding.Left + layout.Spacing / 2;
				contentEdgeInsets.Right += (nfloat)padding.Right + layout.Spacing / 2;
				contentEdgeInsets.Top += (nfloat)padding.Top;
				contentEdgeInsets.Bottom += (nfloat)padding.Bottom;
			}
			else if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle)
				&& (layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom))
			{
				// in this scenario, we won't add the Left and Right padding to the ContentEdgeInsets because the button will use that space before moving the image and title
				contentEdgeInsets.Top += (nfloat)padding.Top + layout.Spacing / 2;
				contentEdgeInsets.Bottom += (nfloat)padding.Bottom + layout.Spacing / 2;
			}
			else if (image is null || string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				contentEdgeInsets.Left += (nfloat)padding.Left;
				contentEdgeInsets.Right += (nfloat)padding.Right;
				contentEdgeInsets.Top += (nfloat)padding.Top;
				contentEdgeInsets.Bottom += (nfloat)padding.Bottom;
			}
			platformButton.UpdateContentEdgeInsets(button, contentEdgeInsets);

			if (image is not null)
			{
				// Resize the image if necessary and then update the image variable
				if (ResizeImageIfNecessary(platformButton, button, image, padding, spacing, borderWidth, widthConstraint, heightConstraint))
				{
					image = platformButton.CurrentImage;
				}
			}

			else
			{
				_originalCGImage = null;
				_originalImageSize = CGSize.Empty;
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

			var titleRect = ComputeTitleRect(platformButton, button, image, widthConstraint, heightConstraint, borderWidth, padding, true);

			var titleRectWidth = titleRect.Width;
			var titleRectHeight = titleRect.Height;

			var buttonContentWidth =
				+(nfloat)Math.Max(titleRectWidth, platformButton.CurrentImage?.Size.Width ?? 0)
				+ (nfloat)padding.Left
				+ (nfloat)padding.Right
				+ (nfloat)borderWidth * 2;

			var buttonContentHeight =
				+(nfloat)Math.Max(titleRectHeight, platformButton.CurrentImage?.Size.Height ?? 0)
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

			// if the image is on top or bottom, let's make sure the title is not cut off by ensuring we have enough padding for the image and title.
			if (image is not null
				&& (layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom))
			{
				var maxTitleRect = ComputeTitleRect(platformButton, button, image, double.PositiveInfinity, double.PositiveInfinity, borderWidth, padding, true);

				var smallerWidth = (nfloat)Math.Min(maxTitleRect.Width, platformButton.CurrentImage?.Size.Width ?? 0);
				if (padding.HorizontalThickness < smallerWidth)
				{
					buttonContentWidth += (nfloat)(smallerWidth - padding.HorizontalThickness);
				}
			}

			var returnSize = new Size(Math.Min(buttonContentWidth, widthConstraint),
							Math.Min(buttonContentHeight, heightConstraint));

			// Rounding the values up to the nearest whole number to match UIView.SizeThatFits
			return new Size((int)Math.Ceiling(returnSize.Width), (int)Math.Ceiling(returnSize.Height));
		}

		/// <summary>
		/// Arrange the button and layout the image and title.
		/// </summary>
		/// <param name="bounds"></param>
		/// <returns>Returns a <see cref="Size"/> representing the width and height of the button.</returns>
		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			bounds = this.ComputeFrame(bounds);

			var platformButton = Handler?.PlatformView as UIButton;

			// Layout the image and title of the button
			LayoutButton(platformButton, this, bounds);

			return new Size(bounds.Width, bounds.Height);
		}

		/// <summary>
		/// Calculate and layout the image and title insets of the UIButton.
		/// </summary>
		/// <param name="platformButton"></param>
		/// <param name="button"></param>
		/// <param name="size"></param>
		/// <remarks>TitleEdgeInsets and ImageEdgeInsets are deprecated in iOS 15. The layout process will change with UIButton.Configuration API in the future.</remarks>
		void LayoutButton(UIButton platformButton, Button button, Rect size)
		{
			var layout = button.ContentLayout;
			var spacing = (nfloat)layout.Spacing;
			var borderWidth = button.BorderWidth < 0 ? 0 : button.BorderWidth;

			var imageInsets = new UIEdgeInsets();
			var titleInsets = new UIEdgeInsets();

			var image = platformButton.CurrentImage;

			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				var padding = button.Padding;
				if (padding.IsNaN)
				{
					padding = ButtonHandler.DefaultPadding;
				}
				var titleRect = ComputeTitleRect(platformButton, button, image, size.Width, size.Height, borderWidth, padding, false);

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

#pragma warning disable CA1416, CA1422
			if (platformButton.ImageEdgeInsets != imageInsets ||
				platformButton.TitleEdgeInsets != titleInsets)
			{
				platformButton.ImageEdgeInsets = imageInsets;
				platformButton.TitleEdgeInsets = titleInsets;
			}
#pragma warning restore CA1416, CA1422
		}

		/// <summary>
		/// Estimate the size of the rect containing the title text.
		/// </summary>
		/// <param name="platformButton"></param>
		/// <param name="button"></param>
		/// <param name="image"></param>
		/// <param name="widthConstraint"></param>
		/// <param name="heightConstraint"></param>
		/// <param name="borderWidth"></param>
		/// <param name="padding"></param>
		/// <param name="isMeasuring"></param>
		/// <returns>Returns a <see cref="CGRect"/> that contains the title text.</returns>
		CGRect ComputeTitleRect(UIButton platformButton, Button button, UIImage image, double widthConstraint, double heightConstraint, double borderWidth, Thickness padding, bool isMeasuring)
		{
			if (string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				return CGRect.Empty;
			}

			var titleWidthConstraint = widthConstraint - ((nfloat)borderWidth * 2);
			var titleHeightConstraint = heightConstraint - ((nfloat)borderWidth * 2);

			if (image is not null && !string.IsNullOrEmpty(platformButton.CurrentTitle) && titleWidthConstraint != double.PositiveInfinity)
			{
				// In non-UIButtonConfiguration setups, the title will always be truncated by the image's width
				// even when the image is on top or bottom.
				titleWidthConstraint -= image.Size.Width;
			}

			if (image is not null && button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Left || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Right)
			{
				titleWidthConstraint -= (nfloat)(button.ContentLayout.Spacing + padding.Left + padding.Right);
			}

			else if (image is null)
			{
				titleWidthConstraint -= (nfloat)(padding.Left + padding.Right);
			}

			var titleRect = platformButton.GetTitleBoundingRect(titleWidthConstraint, titleHeightConstraint);

			var currentTitleText = platformButton.CurrentTitle;

			// We will only do this for buttons with image on left and right because the left and right padding are handled differently
			// when the image is on the top or bottom
			if (currentTitleText.Length > 0 && button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Left || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Right)
			{
				// Measure the width of the first character in the string using the same font as the TitleLabel. If a character cannot fit in the titleRect, let's use a zero size.
				var minimumCharacterWidth = new Foundation.NSString(currentTitleText.Substring(0, 1)).GetSizeUsingAttributes(new UIStringAttributes { Font = platformButton.TitleLabel.Font });
				if (double.IsNaN(titleRect.Width) || double.IsNaN(titleRect.Height) || titleRect.Width < minimumCharacterWidth.Width)
				{
					titleRect = Rect.Zero;
				}
			}

			return titleRect;
		}

		/// <summary>
		/// See if the button's image fits within the constraints and resize it if necessary.
		/// </summary>
		/// <param name="platformButton"></param>
		/// <param name="button"></param>
		/// <param name="image"></param>
		/// <param name="padding"></param>
		/// <param name="spacing"></param>
		/// <param name="borderWidth"></param>
		/// <param name="widthConstraint"></param>
		/// <param name="heightConstraint"></param>
		/// <returns></returns>
		bool ResizeImageIfNecessary(UIButton platformButton, Button button, UIImage image, Thickness padding, double spacing, double borderWidth, double widthConstraint, double heightConstraint)
		{
			// Save the original image for later image resizing
			if (_originalImageSize == CGSize.Empty || _originalCGImage is null || image.CGImage != _originalCGImage)
			{
				_originalCGImage = image.CGImage;
				_originalImageSize = image.Size;
			}

			var currentImageWidth = image.Size.Width;
			var currentImageHeight = image.Size.Height;

			nfloat availableWidth = (nfloat)widthConstraint;
			nfloat availableHeight = (nfloat)heightConstraint;

			var additionalHorizontalSpace = (nfloat)padding.Left + (nfloat)padding.Right + ((nfloat)borderWidth * 2);
			var additionalVerticalSpace = (nfloat)padding.Top + (nfloat)padding.Bottom + ((nfloat)borderWidth * 2);

			if (!string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				if (button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Left || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Right)
				{
					additionalHorizontalSpace += (nfloat)spacing;
				}
				else
				{
					additionalVerticalSpace += (nfloat)spacing;
				}
			}

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
					image = ResizeImageSource(image, availableWidth, availableHeight, _originalImageSize);
				}
				// if the image is already sized down but now has more space, we will size it up no more than the original image size
				else if (availableHeight - additionalVerticalSpace - currentImageHeight > buffer
					&& availableWidth - additionalHorizontalSpace - currentImageWidth > buffer
					&& currentImageHeight != _originalImageSize.Height
					&& currentImageWidth != _originalImageSize.Width)
				{
					image = ResizeImageSource(image, (nfloat)widthConstraint - additionalHorizontalSpace, (nfloat)heightConstraint - additionalVerticalSpace, _originalImageSize, true);
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

		/// <summary>
		/// Resize the image to fit within the constraints.
		/// </summary>
		/// <param name="sourceImage"></param>
		/// <param name="maxWidth"></param>
		/// <param name="maxHeight"></param>
		/// <param name="originalImageSize"></param>
		/// <param name="shouldScaleUp"></param>
		/// <returns></returns>
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
			handler.PlatformView.UpdateContentLayout(button);
		}

		public static void MapText(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		internal static void MapBorderWidth(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateContentLayout(button);
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);
			_originalImageSize = CGSize.Empty;
			_originalCGImage = null;
		}
	}
}

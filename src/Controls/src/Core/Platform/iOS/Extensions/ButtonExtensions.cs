#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Controls.Button;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		static CGRect GetTitleBoundingRect(this UIButton platformButton, Thickness padding)
		{
			if (platformButton.CurrentAttributedTitle != null ||
					   platformButton.CurrentTitle != null)
			{
				var title =
					   platformButton.CurrentAttributedTitle ??
					   new NSAttributedString(platformButton.CurrentTitle, new UIStringAttributes { Font = platformButton.TitleLabel.Font });

				// Use the available height when calculating the bounding rect
				var lineHeight = platformButton.TitleLabel.Font.LineHeight;
				var availableHeight = platformButton.Bounds.Size.Height;

				// If the line break mode is one of the truncation modes, limit the height to the line height
				if (platformButton.TitleLabel.LineBreakMode == UILineBreakMode.HeadTruncation ||
					platformButton.TitleLabel.LineBreakMode == UILineBreakMode.MiddleTruncation ||
					platformButton.TitleLabel.LineBreakMode == UILineBreakMode.TailTruncation ||
					platformButton.TitleLabel.LineBreakMode == UILineBreakMode.Clip)
				{
					availableHeight = lineHeight;
				}

				var availableSize = new CGSize(platformButton.Bounds.Size.Width, availableHeight);

				var boundingRect = title.GetBoundingRect(
					availableSize,
					NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesDeviceMetrics,
					null);

				// NSStringDrawingOptions.UsesDeviceMetrics can split at characters instead of words but ignore the height. Pass the height constraint back in.
				return new CGRect(boundingRect.Location, new CGSize(boundingRect.Width, availableHeight));
			}

			return CGRect.Empty;
		}

		public static void UpdatePadding(this UIButton platformButton, Button button)
		{
			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

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

			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

			// If the button's image takes up too much space, we will want to hide the title
			var hidesTitle = false;

			if (image != null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				// TODO: Do not use the title label as it is not yet updated and
				//       if we move the image, then we technically have more
				//       space and will require a new layout pass.

				// Resize the image if necessary and then update the image variable
				if (ResizeImageIfNecessary(platformButton, button, image, spacing, padding))
				{
					image = platformButton.CurrentImage;
				}

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
				ResizeImageIfNecessary(platformButton, button, image, 0, padding);
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
				return;
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
				// If the button's content is larger than the button, we need to adjust the ContentEdgeInsets.
				// Apply a small buffer to the image size comparison since iOS can return a size that is off by a fraction of a pixel
				if (buttonContentHeight - button.Height > 1 && button.HeightRequest == -1)
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
		}
	}
}

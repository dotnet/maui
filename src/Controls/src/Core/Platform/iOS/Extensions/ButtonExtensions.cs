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
		static CGRect GetTitleBoundingRect(this UIButton platformButton)
		{
			if (platformButton.CurrentAttributedTitle != null ||
					   platformButton.CurrentTitle != null)
			{
				var title =
					   platformButton.CurrentAttributedTitle ??
					   new NSAttributedString(platformButton.CurrentTitle, new UIStringAttributes { Font = platformButton.TitleLabel.Font });

				// Calculate the available width for the title
				var imageWidth = platformButton.CurrentImage?.Size.Width ?? 0;
#pragma warning disable CA1422 // Validate platform compatibility
				var spacing = platformButton.ContentEdgeInsets.Left + platformButton.ContentEdgeInsets.Right;
				// var spacing = platformButton.ContentEdgeInsets.Right + platformButton.ContentEdgeInsets.Left;
				// var spacing = 0;
#pragma warning restore CA1422 // Validate platform compatibility
				var availableWidth = (nfloat)Math.Max(platformButton.Bounds.Size.Width - imageWidth - spacing, 0.1);
				
				// Use the available width when calculating the bounding rect
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

				var availableSize = new CGSize(availableWidth, availableHeight);

				return title.GetBoundingRect(
					availableSize,
					NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
					null);

				// return title.GetBoundingRect(
				// 	platformButton.Bounds.Size,
				// 	NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
				// 	null);
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
				// padding = new Thickness(30,30);
				padding = ButtonHandler.DefaultPadding;

			// padding += new Thickness(spacingHorizontal / 2, spacingVertical / 2);

			platformButton.UpdatePadding(padding);
		}

		public static void UpdateContentLayout(this UIButton platformButton, Button button)
		{
			if (platformButton.Bounds.Width == 0)
			{
				return;
			}


			var buttonW = button.Width;
			var buttonH = button.Height;

			var imageInsets = new UIEdgeInsets();
			var titleInsets = new UIEdgeInsets();

			var layout = button.ContentLayout;
			var spacing = (nfloat)layout.Spacing;

			var image = platformButton.CurrentImage;

			// if the image is too large then we just position at the edge of the button
			// depending on the position the user has picked
			// This makes the behavior consistent with android
			var contentMode = UIViewContentMode.Center;

			if (image != null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				// TODO: Do not use the title label as it is not yet updated and
				//       if we move the image, then we technically have more
				//       space and will require a new layout pass.

				// platformButton.ContentMode = UIViewContentMode.ScaleAspectFit;

				var titleRect = platformButton.GetTitleBoundingRect();
				var titleWidth = titleRect.Width;
				var titleHeight = titleRect.Height;
				var imageWidth = image.Size.Width;
				var imageHeight = image.Size.Height;
				var buttonWidth = platformButton.Bounds.Width;
				var buttonHeight = platformButton.Bounds.Height;
				var sharedSpacing = spacing / 2;

				if (image.Size.Width > buttonWidth || image.Size.Height > buttonHeight)
				{
					ResizeImageIfNecessary(platformButton, button, image, spacing);
					image = platformButton.CurrentImage;
					imageWidth = image.Size.Width;
					imageHeight = image.Size.Height;
					titleRect = platformButton.GetTitleBoundingRect();
					titleWidth = titleRect.Width;
					titleHeight = titleRect.Height;
				}

				// These are just used to shift the image and title to center
				// Which makes the later math easier to follow
				imageInsets.Left += titleWidth / 2;
				imageInsets.Right -= titleWidth / 2;
				titleInsets.Left -= imageWidth / 2;
				titleInsets.Right += imageWidth / 2;

				// var titleFrame = platformButton.TitleLabel.Frame;
				// platformButton.TitleLabel.Frame = new CGRect(titleFrame.X, titleFrame.Y, 20, titleFrame.Height);
				// platformButton.TitleLabel.BackgroundColor = UIColor.Red;

				// var title = new NSString(platformButton.CurrentTitle);
				// var titleAttributes = new UIStringAttributes { Font = platformButton.TitleLabel.Font };
				// var titleSize = title.GetSizeUsingAttributes(titleAttributes);
				// var titleWidth1 = titleSize.Width;

				if (layout.Position == ButtonContentLayout.ImagePosition.Top)
				{
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


			platformButton.UpdatePadding(button);

			ResizeImageIfNecessary(platformButton, button, image, spacing);

#pragma warning disable CA1422 // Validate platform compatibility
			if (platformButton.ImageEdgeInsets != imageInsets ||
				platformButton.TitleEdgeInsets != titleInsets)
			{
				platformButton.ImageEdgeInsets = imageInsets;
				platformButton.TitleEdgeInsets = titleInsets;
				platformButton.Superview?.SetNeedsLayout();
			}

			var titleHeight1 = platformButton.GetTitleBoundingRect().Height;
			var buttonContentHeight = platformButton.CurrentImage.Size.Height + platformButton.ContentEdgeInsets.Top
				+ platformButton.ContentEdgeInsets.Top + platformButton.GetTitleBoundingRect().Height;
			if (layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom)
			{
				buttonContentHeight += spacing;
			}

			// seems to be really close some times but not exact
			buttonContentHeight = (nfloat)Math.Round(buttonContentHeight, 0);

			if (buttonContentHeight > button.Height)
			{
				var contentInsets = platformButton.ContentEdgeInsets;
				platformButton.ContentEdgeInsets = new UIEdgeInsets(
					(nfloat)(contentInsets.Top + (buttonContentHeight - button.Height) / 2), 
					contentInsets.Left, 
					(nfloat)(contentInsets.Bottom + (buttonContentHeight - button.Height) / 2), 
					contentInsets.Right);
				platformButton.Superview?.SetNeedsLayout();

			}
#pragma warning restore CA1422 // Validate platform compatibility


		}

		static void ResizeImageIfNecessary(UIButton platformButton, Button button, UIImage image, nfloat spacing)
		{
			if (button.HeightRequest == -1 && button.WidthRequest == -1)
			{
				return;
			}

			nfloat availableHeight = 0;
			nfloat availableWidth = 0;

			if (platformButton.Bounds != CGRect.Empty
				&& (button.Height != double.NaN || button.Width != double.NaN))
			{
#pragma warning disable CA1422 // Validate platform compatibility
				var contentEdgeInsets = platformButton.ContentEdgeInsets;
				var titleEdgeInsets = platformButton.TitleEdgeInsets;
				var imageEdgeInsets = platformButton.ImageEdgeInsets;
				var titleRect = platformButton.GetTitleBoundingRect();

				// Case where the image is on top or bottom of the Title text.
				if (button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Top || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					availableHeight = platformButton.Bounds.Height - (contentEdgeInsets.Top + contentEdgeInsets.Bottom + titleRect.Height + spacing);
					availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right);
				}

				// Case where the image is on the left or right of the Title text.
				else
				{
					availableHeight = platformButton.Bounds.Height - (contentEdgeInsets.Top + contentEdgeInsets.Bottom);
					availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right + titleRect.Width + spacing);
				}

				// // Case where the image is on top or bottom of the Title text.
				// if (button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Top || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Bottom)
				// {
				// 	availableHeight = platformButton.Bounds.Height - (contentEdgeInsets.Top + contentEdgeInsets.Bottom + titleEdgeInsets.Top + titleEdgeInsets.Bottom + imageEdgeInsets.Top + imageEdgeInsets.Bottom);
				// 	availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right + imageEdgeInsets.Left + imageEdgeInsets.Right);
				// }

				// // Case where the image is on the left or right of the Title text.
				// else
				// {
				// 	availableHeight = platformButton.Bounds.Height - (contentEdgeInsets.Top + contentEdgeInsets.Bottom + imageEdgeInsets.Top + imageEdgeInsets.Bottom);
				// 	availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right + titleEdgeInsets.Left + titleEdgeInsets.Right + imageEdgeInsets.Left + imageEdgeInsets.Right);
				// }
			}
#pragma warning restore CA1422 // Validate platform compatibility


			availableHeight = button.HeightRequest == -1 ? nfloat.PositiveInfinity : (nfloat)Math.Max(availableHeight, 0);
			availableWidth = button.WidthRequest == -1 ? nfloat.PositiveInfinity : (nfloat)Math.Max(availableWidth, 0);

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
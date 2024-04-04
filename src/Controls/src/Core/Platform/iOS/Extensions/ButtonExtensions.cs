#nullable disable
using System;
using System.Text;
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

				var availableSize = new CGSize(platformButton.Bounds.Size.Width, availableHeight);
				// var availableSize = new CGSize(availableWidth, availableHeight);

				// return title.GetBoundingRect(
				// 	availableSize,
				// 	NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
				// 	null);

				return title.GetBoundingRect(
					availableSize,
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

			// var originalEdgeInsets = platformButton.ContentEdgeInsets;

#pragma warning disable CA1422 // Validate platform compatibility
			var contentInsets = platformButton.ContentEdgeInsets;
#pragma warning restore CA1422 // Validate platform compatibility

			// if (contentInsets != UIEdgeInsets.Zero 
			// 	&& contentInsets != new UIEdgeInsets(
			// 		(nfloat)ButtonHandler.DefaultPadding.Top,
			// 		(nfloat)ButtonHandler.DefaultPadding.Left,
			// 		(nfloat)ButtonHandler.DefaultPadding.Bottom,
			// 		(nfloat)ButtonHandler.DefaultPadding.Right))
			// {
			// 	return;
			// }

			var padding = button.Padding;
			if (padding.IsNaN)
				// padding = new Thickness(30,30);
				padding = ButtonHandler.DefaultPadding;

			// padding += new Thickness(spacingHorizontal / 2, spacingVertical / 2);

			platformButton.UpdatePadding(padding);

// #pragma warning disable CA1422 // Validate platform compatibility
// 			if (contentInsets != platformButton.ContentEdgeInsets)
// 				platformButton?.Superview.SetNeedsLayout();
// #pragma warning restore CA1422 // Validate platform compatibility
							  
			// platformButton.SetNeedsLayout();
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

			var padding = button.Padding;
			if (padding.IsNaN)
				padding = ButtonHandler.DefaultPadding;

			if (image != null && !string.IsNullOrEmpty(platformButton.CurrentTitle))
			{
				// TODO: Do not use the title label as it is not yet updated and
				//       if we move the image, then we technically have more
				//       space and will require a new layout pass.

				// platformButton.ContentMode = UIViewContentMode.ScaleAspectFit;

				// TODO see if this is being resized more than once per button and figure out why?
				bool resized = ResizeImageIfNecessary(platformButton, button, image, spacing, padding);
				if (resized)
				{
					// if (platformButton.TitleLabel.Text.Contains("2", StringComparison.OrdinalIgnoreCase))
					Console.WriteLine($"~~~~ Resized: {platformButton.TitleLabel.Text}");
					image = platformButton.CurrentImage;
				}
				

				var titleRect = platformButton.GetTitleBoundingRect();
				var titleWidth = titleRect.Width;
				var titleHeight = titleRect.Height;
				var imageWidth = image.Size.Width;
				var imageHeight = image.Size.Height;
				var buttonWidth = platformButton.Bounds.Width;
				var buttonHeight = platformButton.Bounds.Height;
				// var buttonWidth = button.Width;
				// var buttonHeight = button.Height;
				var sharedSpacing = spacing / 2;

				var buttonFrame = platformButton.Frame;
				var imageFrame = platformButton.ImageView?.Frame;
				var titleFrame = platformButton.TitleLabel?.Frame;

#pragma warning disable CA1422 // Validate platform compatibility
				// if (platformButton.TitleLabel.Text.Contains("2", StringComparison.OrdinalIgnoreCase))
				// Console.WriteLine($"Entered: {platformButton.TitleLabel.Text}\nButtonWidth:{buttonWidth}\nTitleWidth:{titleWidth}\nImageWidth:{imageWidth}\nplatformButton.ImageEdgeInsets.Left:{platformButton.ImageEdgeInsets.Left}\nplatformButton.TitleEdgeInsets.Left:{platformButton.TitleEdgeInsets.Left}\nimageFrame.Value.Left:{imageFrame.Value.Left}\ntitleFrame.Value.Left:{titleFrame.Value.Left}");//\nButtonHeight{buttonHeight}\nTitleHeight{titleHeight}\nImageHeight{imageHeight}\nButtonFrame{buttonFrame}\nImageFrame{imageFrame}\nTitleFrame{titleFrame}\nContentEdgeInsets{platformButton.ContentEdgeInsets}\nImageEdgeInsets{platformButton.ImageEdgeInsets}\nTitleEdgeInsets{platformButton.TitleEdgeInsets}\nBounds{platformButton.Bounds}\nFrame{platformButton.Frame}\n");
#pragma warning restore CA1422 // Validate platform compatibility


				// if (image.Size.Width > buttonWidth || image.Size.Height > buttonHeight)
				// {
				// 	ResizeImageIfNecessary(platformButton, button, image, spacing);
				// 	image = platformButton.CurrentImage;
				// 	imageWidth = image.Size.Width;
				// 	imageHeight = image.Size.Height;
				// 	titleRect = platformButton.GetTitleBoundingRect();
				// 	titleWidth = titleRect.Width;
				// 	titleHeight = titleRect.Height;
				// }

				// These are just used to shift the image and title to center
				// Which makes the later math easier to follow
				// imageInsets.Left += titleWidth / 2;
				// imageInsets.Right -= titleWidth / 2;
				// titleInsets.Left -= imageWidth / 2;
				// titleInsets.Right += imageWidth / 2;


				double centerX = buttonWidth / 2.0; // 50

				// Calculate the horizontal offset for the image and title
#pragma warning disable CA1422 // Validate platform compatibility

// 				var imageXLeftmostPosition = imageFrame.HasValue ? Math.Round(imageFrame.Value.Left,0) : platformButton.ContentEdgeInsets.Left;
// 				var titleXLeftmostPosition = titleFrame.HasValue ? Math.Round(titleFrame.Value.Left,0) : platformButton.ContentEdgeInsets.Left;
// 				double imageOffsetX = centerX + (platformButton.ImageEdgeInsets.Left - platformButton.ImageEdgeInsets.Right) - (imageWidth / 2.0) - imageXLeftmostPosition;
// 				double titleOffsetX = centerX + (platformButton.TitleEdgeInsets.Left - platformButton.TitleEdgeInsets.Right) - (titleWidth / 2.0) - titleXLeftmostPosition;
				
// #pragma warning restore CA1422 // Validate platform compatibility

// 				// Adjust the image and title insets
// 				imageInsets.Left += (nfloat)imageOffsetX / 2;
// 				imageInsets.Right -= (nfloat)imageOffsetX / 2;
// 				titleInsets.Left += (nfloat)titleOffsetX / 2;
// 				titleInsets.Right -= (nfloat)titleOffsetX / 2;



				// ROUNDING
				// var imageXLeftmostPosition = imageFrame.HasValue ? Math.Round(imageFrame.Value.Left,0) : platformButton.ContentEdgeInsets.Left;
				// var titleXLeftmostPosition = titleFrame.HasValue ? Math.Round(titleFrame.Value.Left,0) : platformButton.ContentEdgeInsets.Left;
				var imageXLeftmostPosition = imageFrame.HasValue ? imageFrame.Value.Left : platformButton.ContentEdgeInsets.Left;
				var titleXLeftmostPosition = titleFrame.HasValue ? titleFrame.Value.Left : platformButton.ContentEdgeInsets.Left;
				var titleXRightmostPosition = titleFrame.HasValue ? titleFrame.Value.Right : platformButton.ContentEdgeInsets.Right;
				double imageOffsetX = centerX - (imageXLeftmostPosition + (imageWidth / 2.0) - platformButton.ImageEdgeInsets.Left);
				double titleOffsetX = centerX - (titleXLeftmostPosition + (titleWidth / 2.0) - platformButton.TitleEdgeInsets.Left - padding.Left);
				double maxTitleOffsetStartX = titleXLeftmostPosition - padding.Left - platformButton.TitleEdgeInsets.Left;
				double maxTitleOffsetEndX = buttonWidth - titleXRightmostPosition - padding.Right - platformButton.TitleEdgeInsets.Right;

				// TODO in the case for when the width request is set, we probably want to adjust both sides of the titleOffsetX and not just the left one

				// We start out centering the image.
				// imageInsets.Left += (nfloat)imageOffsetX;
				// imageInsets.Right -= (nfloat)imageOffsetX;
				
				

				// titleInsets.Left += (nfloat)titleOffsetX;
				

				var titleOffsetCenterX = titleXLeftmostPosition + (titleWidth / 2) - (buttonWidth / 2) - padding.Left - platformButton.TitleEdgeInsets.Left;


#pragma warning restore CA1422 // Validate platform compatibility

				// var titleOffsetStartX = Math.Min(maxTitleOffsetStartX, titleOffsetCenterX);
				// var titleOffsetEndX = Math.Min(maxTitleOffsetEndX, titleOffsetCenterX);

				// titleInsets.Left -= (nfloat)maxTitleOffsetStartX;
				// titleInsets.Right += (nfloat)maxTitleOffsetEndX;

				// if (false && titleWidth + padding.Left + padding.Right < buttonWidth)
				// {
				// 	titleInsets.Left += (nfloat)titleOffsetX;
				// }
				// else
				// {
				// 	titleInsets.Left -= (nfloat)maxTitleOffsetStartX;
				// 	titleInsets.Right += (nfloat)maxTitleOffsetEndX;
				// }


				if (layout.Position == ButtonContentLayout.ImagePosition.Top)
				{
					if (imageHeight > buttonHeight)
					{
						contentMode = UIViewContentMode.Top;
					}
					else
					{
						// var verticalSpace = titleHeight + imageHeight + spacing;
						// imageInsets.Top -= verticalSpace / 2;
						// titleInsets.Bottom -= verticalSpace / 2;

						imageInsets.Top -= (titleHeight / 2) + sharedSpacing;
						imageInsets.Bottom += titleHeight / 2 + sharedSpacing;

						titleInsets.Top += imageHeight / 2 + sharedSpacing;
						titleInsets.Bottom -= (imageHeight / 2) + sharedSpacing;

						var calculatedImageInsets = MoveImageInsetsHorizontally(platformButton, button, image, padding, spacing, layout.Position);
						imageInsets.Left += calculatedImageInsets.left;
						imageInsets.Right -= calculatedImageInsets.right;

						var calulatedTitleInsets = MoveTitleInsetsHorizontally(platformButton, button, padding, spacing, layout.Position);

						titleInsets.Left -= calulatedTitleInsets.left;
						titleInsets.Right += calulatedTitleInsets.right;
						
						// titleInsets.Left -= (nfloat)maxTitleOffsetStartX;
						// titleInsets.Right += (nfloat)maxTitleOffsetEndX;
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
						imageInsets.Top += titleHeight / 2 + sharedSpacing;
						imageInsets.Bottom -= (titleHeight / 2) + sharedSpacing;
					}

					var calculatedImageInsets = MoveImageInsetsHorizontally(platformButton, button, image, padding, spacing, layout.Position);
					imageInsets.Left += calculatedImageInsets.left;
					imageInsets.Right -= calculatedImageInsets.right;

					titleInsets.Top -= (imageHeight / 2) + sharedSpacing;
					titleInsets.Bottom += imageHeight / 2 + sharedSpacing;

					var calulatedTitleInsets = MoveTitleInsetsHorizontally(platformButton, button, padding, spacing, layout.Position);

					titleInsets.Left -= calulatedTitleInsets.left;
					titleInsets.Right += calulatedTitleInsets.right;

					// titleInsets.Left -= (nfloat)maxTitleOffsetStartX;
					// titleInsets.Right += (nfloat)maxTitleOffsetEndX;
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Left)
				{
					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Left;
					}
					else
					{
						// image needs to be bound to the left if too large

						// imageInsets.Left -= (titleWidth / 2) + sharedSpacing;
						// imageInsets.Right += titleWidth / 2 + sharedSpacing;
					}

					// Center the image first
					var calculatedImageInsets = MoveImageInsetsHorizontally(platformButton, button, image, padding, spacing, layout.Position, true);
					imageInsets.Left += calculatedImageInsets.left;
					imageInsets.Right -= calculatedImageInsets.right;

					
					
					
					var calulatedTitleInsets = MoveTitleInsetsHorizontally(platformButton, button, padding, spacing, layout.Position);

					titleInsets.Left -= calulatedTitleInsets.left;
					titleInsets.Right += calulatedTitleInsets.right;

					// if the title is not hidden, we can center the image and title together
					if (!calulatedTitleInsets.isHiden)
					{
						var calculatedImageInsets2 = MoveImageInsetsHorizontally(platformButton, button, image, padding, spacing, layout.Position);
						imageInsets.Left += calculatedImageInsets2.left;
						imageInsets.Right -= calculatedImageInsets2.right;
					}


					// var isWidthTooWide = titleWidth + padding.Left + padding.Right + imageWidth + spacing > buttonWidth;


					// titleInsets.Left += imageWidth / 2 + sharedSpacing;
					// titleInsets.Right -= (imageWidth / 2) + sharedSpacing;

					// var isWidthTooWide = titleWidth + padding.Left + padding.Right + imageWidth + spacing > buttonWidth;
					
					// var calulatedTitleInsets = MoveTitleInsetsHorizontally(platformButton, button, padding, spacing, layout.Position, !isWidthTooWide);


					// actually if we just position the imageFrame, we can just add the text after the image
					// if (isWidthTooWide)
					// {

					// }
				}
				else if (layout.Position == ButtonContentLayout.ImagePosition.Right)
				{
					if (imageWidth > buttonWidth)
					{
						contentMode = UIViewContentMode.Right;
					}
					else
					{
						// imageInsets.Left += titleWidth / 2 + sharedSpacing;
						// imageInsets.Right -= (titleWidth / 2) + sharedSpacing;
					}

					var calculatedImageInsets = MoveImageInsetsHorizontally(platformButton, button, image, padding, spacing, layout.Position, true);
					imageInsets.Left += calculatedImageInsets.left;
					imageInsets.Right -= calculatedImageInsets.right;

					var calulatedTitleInsets = MoveTitleInsetsHorizontally(platformButton, button, padding, spacing, layout.Position);
					titleInsets.Left -= calulatedTitleInsets.left;
					titleInsets.Right += calulatedTitleInsets.right;

					// if the title is not hidden, we can center the image and title together
					if (!calulatedTitleInsets.isHiden)
					{
						var calculatedImageInsets2 = MoveImageInsetsHorizontally(platformButton, button, image, padding, spacing, layout.Position);
						imageInsets.Left += calculatedImageInsets2.left;
						imageInsets.Right -= calculatedImageInsets2.right;
					}



					// var calculatedImageInsets = MoveImageInsetsHorizontally(platformButton, button, image, padding, spacing, layout.Position, true);
					// imageInsets.Left += calculatedImageInsets.left;
					// imageInsets.Right -= calculatedImageInsets.right;

					// // titleInsets.Left -= (imageWidth / 2) + sharedSpacing;
					// // titleInsets.Right += imageWidth / 2 + sharedSpacing;

					// var calulatedTitleInsets = MoveTitleInsetsHorizontally(platformButton, button, padding, spacing, layout.Position);

					// titleInsets.Left -= calulatedTitleInsets.left;
					// titleInsets.Right += calulatedTitleInsets.right;
				}
			}

			platformButton.ImageView.ContentMode = contentMode;

			// This is used to match the behavior between platforms.
			// If the image is too big then we just hide the label because
			// the image is pushing the title out of the visible view.
			// We can't use insets because then the title shows up outside the 
			// bounds of the UIButton. We could set the UIButton to clip bounds
			// but that feels like it might cause confusing side effects
			// if (contentMode == UIViewContentMode.Center)
			// 	platformButton.TitleLabel.Layer.Hidden = false;
			// else
			// 	platformButton.TitleLabel.Layer.Hidden = true;


			// platformButton.UpdatePadding(button);


			// TODO what if instead of checking if the insets have changed, see if the distance from 
			// title/image center to the center 

			var testImageFrame = platformButton.ImageView!.Frame;
			var testTitleFrame = platformButton.TitleLabel!.Frame;
			var LeftSubtraction = testImageFrame.Left - imageInsets.Left;
			var LeftAddition = testImageFrame.Left + imageInsets.Left;
			var RightAddition = testImageFrame.Right + imageInsets.Right;
			var RightSubtraction = testImageFrame.Right - imageInsets.Right;

			Console.WriteLine ($@"

			~~~\n
			ImageFrameLeft: {testImageFrame.Left}
			ImageFrameRight: {testImageFrame.Right}
			imageInsets.Left: {imageInsets.Left}
			imageInsets.Right: {imageInsets.Right}
			LeftSubtraction: {LeftSubtraction}
			LeftAddition: {LeftAddition}
			RightAddition: {RightAddition}
			RightSubtraction: {RightSubtraction}
			titleInsets.Left: {titleInsets.Left}
			imageInsets.Right: {titleInsets.Right}
			
								");

			// Maybe if there is a significant difference between the insets, we can calculate how far the center of the title or image is currently from the center of the button?
#pragma warning disable CA1422 // Validate platform compatibility
			// ROUNDING
			if (IsSignificantlyDifferent(platformButton.ImageEdgeInsets, imageInsets, 0.5) ||
				IsSignificantlyDifferent(platformButton.TitleEdgeInsets, titleInsets, 0.5))// && counter < 10)
			// if (platformButton.ImageEdgeInsets != imageInsets || platformButton.TitleEdgeInsets!= titleInsets)
			{

				// DebugInsets("imageInsets" + platformButton.TitleLabel.Text.Substring(0,5), platformButton.ImageEdgeInsets, imageInsets);
				// DebugInsets("titleInsets" + platformButton.TitleLabel.Text.Substring(0,5), platformButton.TitleEdgeInsets, titleInsets);
				
				// if (platformButton.TitleLabel.Text.Contains("2", StringComparison.OrdinalIgnoreCase))
				// Console.WriteLine($"image and title insets: {platformButton.TitleLabel.Text}\nplatformButton.ImageEdgeInsets{platformButton.ImageEdgeInsets}\nimageInsets{imageInsets}\nplatformButton.TitleEdgeInsets{platformButton.TitleEdgeInsets}\ntitleInsets{titleInsets}");
				platformButton.ImageEdgeInsets = imageInsets;
				platformButton.TitleEdgeInsets = titleInsets;
				// counter++;
				// platformButton.SetNeedsLayout();

				// platformButton.TitleLabel.Layer.Hidden = true;
				// platformButton.TitleLabel.Hidden = true;

				// Wanted to remove this, but parent needs to be called or we can have extra padding in cases. Could be made into a separate case, but will probably need to run most times :/
				platformButton.Superview?.SetNeedsLayout();
				// return;
			}

			var titleHeight1 = platformButton.GetTitleBoundingRect().Height;
			var buttonContentHeight = 
				+ (nfloat)Math.Max(platformButton.GetTitleBoundingRect().Height, platformButton.CurrentImage?.Size.Height ?? 0)
				+ (nfloat)padding.Top
				+ (nfloat)padding.Bottom;
				
				// var buttonContentHeight = platformButton.CurrentImage.Size.Height
				// + platformButton.ContentEdgeInsets.Top 
				// + platformButton.ContentEdgeInsets.Bottom 
				// + platformButton.GetTitleBoundingRect().Height
				// + (nfloat)padding.Top
				// + (nfloat)padding.Bottom;
			if (layout.Position == ButtonContentLayout.ImagePosition.Top || layout.Position == ButtonContentLayout.ImagePosition.Bottom)
			{
				buttonContentHeight += spacing;
				buttonContentHeight += (nfloat)Math.Min(platformButton.GetTitleBoundingRect().Height, platformButton.CurrentImage?.Size.Height ?? 0);
			}

			// ROUNDING
			// if (buttonContentHeight - button.Height > 1 && counter < 10)
			if (buttonContentHeight - button.Height > 1 && button.HeightRequest == -1 && counter < 2)
			// if (buttonContentHeight > button.Height && button.HeightRequest == -1 && counter < 2)
			{
				// if (counter == 0){

				// }

				// platformButton.SetContentHuggingPriority(1, UILayoutConstraintAxis.Vertical);

				// counter++;
				// if (platformButton.TitleLabel.Text.Contains("6", StringComparison.OrdinalIgnoreCase)){
				// Console.WriteLine($"Make longer: {platformButton.TitleLabel.Text}");

				// } 
				var contentInsets = platformButton.ContentEdgeInsets;
				// ROUNDING
				var additionalVerticalSpace = (buttonContentHeight - button.Height) / 2 + 2;
				// nfloat verticalPadding =(nfloat)(buttonContentHeight - button.Height) / 2;

				// if we change these here, we need a way to not change back to default in UpdatePadding
				// platformButton.ContentEdgeInsets = new UIEdgeInsets(
				// 	(nfloat)(additionalVerticalSpace + (nfloat)padding.Top), 
				// 	contentInsets.Left, 
				// 	(nfloat)(additionalVerticalSpace + (nfloat)padding.Bottom), 
				// 	contentInsets.Right);

				platformButton.ContentEdgeInsets = new UIEdgeInsets(
					(nfloat)(additionalVerticalSpace + (nfloat)padding.Top), 
					contentInsets.Left, 
					(nfloat)(additionalVerticalSpace + (nfloat)padding.Bottom), 
					contentInsets.Right);
				var newButtonHeight = button.Height + additionalVerticalSpace + additionalVerticalSpace + (nfloat)padding.Top + (nfloat)padding.Bottom;
				
				// platformButton.Superview?.LayoutIfNeeded();
				// platformButton.Superview?.SetNeedsLayout(); // top and bottom are 39.22 // 8.06 // 39.22
				
				// platformButton.InvalidateIntrinsicContentSize();
				// platformButton.TitleLabel.Layer.Hidden = true;
				// platformButton.TitleLabel.Hidden = true;

				platformButton.Superview?.SetNeedsLayout();
			}

			// platformButton.TitleLabel.Layer.Hidden = true;
			// platformButton.TitleLabel.Hidden = true;
			// platformButton.SetNeedsLayout();

			// if (counter < 5)
			// {
			// 	platformButton.Superview?.SetNeedsLayout();
			// 	counter++;
			// }
			// if (counter == 0)
			// {
			// 	var contentInsets = platformButton.ContentEdgeInsets;
			// 	var verticalPadding = 50;

			// 	platformButton.ContentEdgeInsets = new UIEdgeInsets(
			// 		(nfloat)(contentInsets.Top + verticalPadding), 
			// 		contentInsets.Left, 
			// 		(nfloat)(contentInsets.Bottom + verticalPadding), 
			// 		contentInsets.Right);
			// 	var newButtonHeight = button.Height;
			// 	counter++;
			// 	platformButton.Superview?.SetNeedsLayout();
			// 	return;
			// }
#pragma warning restore CA1422 // Validate platform compatibility
		}

		static int counter = 0;

		static bool IsSignificantlyDifferent (UIEdgeInsets edgeInsets1, UIEdgeInsets edgeInsets2, double threshold = 0.5)
		{
			var diff =  Math.Abs(edgeInsets1.Top - edgeInsets2.Top) > threshold ||
				Math.Abs(edgeInsets1.Left - edgeInsets2.Left) > threshold ||
				Math.Abs(edgeInsets1.Bottom - edgeInsets2.Bottom) > threshold ||
				Math.Abs(edgeInsets1.Right - edgeInsets2.Right) > threshold ;

			return diff;
		}

		// maybe we will always want to center horizontally and only have a special case
		// for when the text comes all the way to the edge?
		
		// In the top/bottom scenario, we want to center but clip at the end
		
		// In the left right examples, we want to:
		// * Center the image and text horizontally and then split using the titleWidth and imageWidth
		// * Special case, the text and image are wider than the available space
		//     - then we only want to move image all the way to the left and then use padding and spacing to stop the title at the right side
		
		// if shouldCenterText == true, we will center the text here and then move the image and text insets together later
		static (nfloat left, nfloat right, bool isHiden) MoveTitleInsetsHorizontally (UIButton platformButton, Button button, Thickness padding, double spacing, ButtonContentLayout.ImagePosition imagePosition, bool shouldCenterText = false)
		{
			if (platformButton.TitleLabel.Text == "2 ~ short"){
				
			}

			var imageFrame = platformButton.ImageView!.Frame;
			var titleFrame = platformButton.TitleLabel!.Frame;

#pragma warning disable CA1422 // Validate platform compatibility

			// var requestedStart = -1.0;
			// var requestedEnd = -1.0;

			var buttonWidth = platformButton.Bounds.Width;


			// the leftmost position of where we can start the title relative to the button
			var leftmostPosition = padding.Left;
			var rightmostPosition = buttonWidth - padding.Right;

			var titleRect = platformButton.GetTitleBoundingRect();
			var titleWidth = titleRect.Width;
			var titleHeight = titleRect.Height;

			var prospectiveStartingX = double.MinValue;
			var prospectiveEndingX = double.MaxValue;

			var centerX = buttonWidth / 2.0;
			var centerTitle = titleWidth / 2.0;

			var hideTitle = false;

			// find the positions that will center the title 
			if (imagePosition == ButtonContentLayout.ImagePosition.Top || imagePosition == ButtonContentLayout.ImagePosition.Bottom)
			{
				prospectiveStartingX = centerX - centerTitle;
				prospectiveEndingX = centerX + centerTitle;
			}

			if (imagePosition == ButtonContentLayout.ImagePosition.Left || imagePosition == ButtonContentLayout.ImagePosition.Right)
			{
				// // if shouldCenterText == true, we will center the text here and then move the image and text insets together outside this method
				// if (shouldCenterText)
				// {
				// 	prospectiveStartingX = centerX - centerTitle;
				// 	prospectiveEndingX = centerX + centerTitle;
				// }

				// else we will have the title go all the way to the left after the image and spacing and stretch to the rightmost bound
				if (imagePosition == ButtonContentLayout.ImagePosition.Left)
				{
					prospectiveStartingX = imageFrame.Right + spacing;
					prospectiveEndingX = prospectiveStartingX + titleWidth;

					if ((prospectiveStartingX < leftmostPosition && prospectiveEndingX < rightmostPosition)
						|| (prospectiveStartingX > buttonWidth && prospectiveEndingX > buttonWidth))
					{
						hideTitle = true;
					}
				}
				else
				{
					// if both of these values are negative or past the width of the button, we will not want to see the title
					prospectiveEndingX = imageFrame.Left - spacing;
					prospectiveStartingX = prospectiveEndingX - titleWidth;

					if ((prospectiveStartingX < leftmostPosition && prospectiveEndingX < rightmostPosition)
						|| (prospectiveStartingX > buttonWidth && prospectiveEndingX > buttonWidth))
					{
						hideTitle = true;
					}
				}
			}

			
			// cap the prospective positions to the left and right padding
			var startingX = Math.Max(leftmostPosition, prospectiveStartingX);
			var endingX = Math.Min(rightmostPosition, prospectiveEndingX);

			if (hideTitle)
			{
				startingX = -5 - titleWidth;
				endingX = -5;
			}

			var startingMove = titleFrame.Left - startingX;
			var endingMove = buttonWidth - titleFrame.Right - (buttonWidth - endingX);

				
			double startingOffset = startingMove - platformButton.TitleEdgeInsets.Left;
			double endingOffset = endingMove - platformButton.TitleEdgeInsets.Right;

			return ((nfloat)startingOffset, (nfloat)endingOffset, hideTitle);
#pragma warning restore CA1422 // Validate platform compatibility
		}

		static void CheckToHideTitle(UIButton platformButton, double startingX, double endingX, double buttonWidth)
		{
			if ((startingX < 0 && endingX < 0)
				|| (startingX > buttonWidth && endingX > buttonWidth))
			{
				platformButton.TitleLabel.Layer.Hidden = true;
			}
			else
			{
				platformButton.TitleLabel.Layer.Hidden = false;
			}
		}



		static (nfloat left, nfloat right) MoveImageInsetsHorizontally (UIButton platformButton, Button button, UIImage image, Thickness padding, double spacing, ButtonContentLayout.ImagePosition imagePosition, bool shouldCenterText = false)
		{
			var imageFrame = platformButton.ImageView!.Frame;
			var titleFrame = platformButton.TitleLabel!.Frame;

#pragma warning disable CA1422 // Validate platform compatibility

			var buttonWidth = platformButton.Bounds.Width;

			// the leftmost position of where we can start the title relative to the button
			var leftmostPosition = padding.Left;
			var rightmostPosition = buttonWidth - padding.Right;

			var imageWidth = image.Size.Width;
			var imageHeight = image.Size.Height;

			var titleRect = platformButton.GetTitleBoundingRect();
			var titleWidth = titleRect.Width;
			var titleHeight = titleRect.Height;

			var prospectiveStartingX = double.MinValue;
			var prospectiveEndingX = double.MaxValue;

			var centerX = buttonWidth / 2.0;
			var centerImage = imageWidth / 2.0;

			var sharedSpacing = spacing / 2;

			// // find the positions that will center the title 
			// if (imagePosition == ButtonContentLayout.ImagePosition.Top || imagePosition == ButtonContentLayout.ImagePosition.Bottom)
			// {
			// 	prospectiveStartingX = centerX - centerImage;
			// 	prospectiveEndingX = centerX + centerImage;
			// }

			// always start with a centered image
			prospectiveStartingX = centerX - centerImage;
			prospectiveEndingX = centerX + centerImage;

			// if the image is on the left or right, try to move the image over using the size of the title
			if ((imagePosition == ButtonContentLayout.ImagePosition.Left || imagePosition == ButtonContentLayout.ImagePosition.Right)
				&& !shouldCenterText)
			{
				// imageInsets.Left += titleWidth / 2 + sharedSpacing;
				// imageInsets.Right -= (titleWidth / 2) + sharedSpacing;
				if (imagePosition == ButtonContentLayout.ImagePosition.Left)
				{
					prospectiveStartingX -= titleWidth / 2 + sharedSpacing;
					prospectiveEndingX -= titleWidth / 2 + sharedSpacing;
				}
				else
				{
					prospectiveStartingX += titleWidth / 2 + sharedSpacing;
					prospectiveEndingX += titleWidth / 2 + sharedSpacing;
				}
				
			}

			double startingX = prospectiveStartingX;
			double endingX = prospectiveEndingX;
			// cap the prospective positions to the left and right padding
			if (leftmostPosition > prospectiveStartingX)
			{
				startingX = leftmostPosition;
				endingX = leftmostPosition + imageWidth;
			}
			else if (rightmostPosition < prospectiveEndingX)
			{
				startingX = rightmostPosition - imageWidth;
				endingX = rightmostPosition;
			}
			// var startingX = Math.Max(leftmostPosition, prospectiveStartingX);
			// var endingX = Math.Min(rightmostPosition, prospectiveEndingX);


			// TODO if one of these change, we'll have to keep the image size the same and move the other
			var startingMove = imageFrame.Left - startingX;
			var endingMove = buttonWidth - imageFrame.Right - (buttonWidth - endingX);

				
			double startingOffset = startingMove - platformButton.ImageEdgeInsets.Left;
			double endingOffset = endingMove - platformButton.ImageEdgeInsets.Right;

			return ((nfloat)startingOffset, (nfloat)endingOffset);
#pragma warning restore CA1422 // Validate platform compatibility
		}

		static void DebugInsets(string title, UIEdgeInsets platformInset, UIEdgeInsets insets)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"** {title}");
			var sbSize = sb.Length;

			if (platformInset.Top != insets.Top)
				sb.AppendLine($"Top Difference {platformInset.Top - insets.Top}: {platformInset.Top} -> {insets.Top}");

			if (platformInset.Left != insets.Left)
				sb.AppendLine($"Left Difference {platformInset.Left - insets.Left}: {platformInset.Left} -> {insets.Left}");

			if (platformInset.Bottom != insets.Bottom)
				sb.AppendLine($"Bottom Difference {platformInset.Bottom - insets.Bottom}: {platformInset.Bottom} -> {insets.Bottom}");

			if (platformInset.Right != insets.Right)
				sb.AppendLine($"Right Difference {platformInset.Right - insets.Right}: {platformInset.Right} -> {insets.Right}");

			if (sb.Length > sbSize)
				Console.WriteLine(sb.ToString());
		}

		static bool ResizeImageIfNecessary(UIButton platformButton, Button button, UIImage image, nfloat spacing, Thickness padding)
		{
			// if the button is on the left or right, we still have an implicit width constraint
			if (button.HeightRequest == -1 && button.WidthRequest == -1 && button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Top || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Bottom)
			{
				return false;
			}

			nfloat availableHeight = nfloat.MaxValue;
			nfloat availableWidth = nfloat.MaxValue;

			if (platformButton.Bounds != CGRect.Empty
				&& (button.Height != double.NaN || button.Width != double.NaN))
			{
#pragma warning disable CA1422 // Validate platform compatibility
				var contentEdgeInsets = platformButton.ContentEdgeInsets;
				var titleEdgeInsets = platformButton.TitleEdgeInsets;
				var imageEdgeInsets = platformButton.ImageEdgeInsets;
				var titleRect = platformButton.GetTitleBoundingRect();

				// If the image is larger than the button, we will ignore the text
				// what about the spacing?
				// Maybe if the image is larger than the button - spacing, then it can ignore the spacing a text because the text wouldn't show anyways?
				// In this case, we will care if the image is vertical or horizontal
				// If we make the image take up the whole space, we'll need to tell the title to not show / be zero to not mess with other calculations?


				// Case where the image is on top or bottom of the Title text.
				if (button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Top || button.ContentLayout.Position == ButtonContentLayout.ImagePosition.Bottom)
				{
					var contentHeightWithoutTextSpacing = platformButton.Bounds.Height - padding.Top - padding.Bottom;
					var contentHeightWithTextSpacing = platformButton.Bounds.Height - (titleRect.Height + spacing + (nfloat)padding.Top + (nfloat)padding.Bottom);
					// Case where the image is big enough to fill the whole button
					if (image.Size.Height > contentHeightWithoutTextSpacing)
					{
						availableHeight = (nfloat)contentHeightWithoutTextSpacing;
					}

					else if (image.Size.Height - spacing > contentHeightWithTextSpacing)
					{
						availableHeight = (nfloat)contentHeightWithTextSpacing;
					}

					availableWidth = platformButton.Bounds.Width - ((nfloat)padding.Left + (nfloat)padding.Right);
					// availableHeight = platformButton.Bounds.Height - (contentEdgeInsets.Top + contentEdgeInsets.Bottom + titleRect.Height + spacing + (nfloat)padding.Top + (nfloat)padding.Bottom); //49 // 49.29296875
					// availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right + (nfloat)padding.Left + (nfloat)padding.Right);

					// we do not care about width of the image if there are not width constraints
					availableWidth = button.WidthRequest == -1 ? nfloat.PositiveInfinity : (nfloat)Math.Max(availableWidth, 0);
				}

				// Case where the image is on the left or right of the Title text.
				else
				{
					var contentWidthWithoutTextSpacing = platformButton.Bounds.Width - (nfloat)padding.Left - (nfloat)padding.Right;
					var contentWidthWithTextSpacing = platformButton.Bounds.Width - (spacing + (nfloat)padding.Left + (nfloat)padding.Right);
					// Case where the image is big enough to fill the whole button
					if (image.Size.Width > contentWidthWithoutTextSpacing)
					{
						availableWidth = contentWidthWithoutTextSpacing;
					}

					else if (image.Size.Width - spacing > contentWidthWithTextSpacing)
					{
						availableWidth = contentWidthWithTextSpacing;
					}

					var contentHeight = platformButton.Bounds.Height - ((nfloat)padding.Top + (nfloat)padding.Bottom);
					if (image.Size.Height - contentHeight > 0.1)
					{
						availableHeight = contentHeight;
					}

					// availableHeight = platformButton.Bounds.Height - ((nfloat)padding.Top + (nfloat)padding.Bottom);
					// availableWidth = platformButton.Bounds.Width - (contentEdgeInsets.Left + contentEdgeInsets.Right + titleRect.Width + spacing + (nfloat)padding.Left + (nfloat)padding.Right);

					// let's remove the title rect for now since we have a max width
					
					// at a max, the image should not take up more than half the button
					// availableWidth = (nfloat)Math.Min(availableWidth, button.Width / 2);
				}
			}
#pragma warning restore CA1422 // Validate platform compatibility


			availableHeight = button.HeightRequest == -1 ? nfloat.PositiveInfinity : (nfloat)Math.Max(availableHeight, 0);
			

			try
			{
				// ROUNDING
				// if (image.Size.Height - availableHeight > 0.5 || image.Size.Width - availableWidth > 0.5)
				if (image.Size.Height > availableHeight || image.Size.Width > availableWidth)
				{
					image = ResizeImageSource(image, availableWidth, availableHeight);
				}
				else
				{
					return false;
				}

				image = image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				platformButton.SetImage(image, UIControlState.Normal);

				platformButton.SetNeedsLayout();

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
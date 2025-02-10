using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class ImageButton : ICrossPlatformLayout
	{
		/// <summary>
		/// Measure the desired size of the ImageButton based on the image size taking into account the padding.
		/// </summary>
		/// <param name="widthConstraint"></param>
		/// <param name="heightConstraint"></param>
		/// <returns>Returns a <see cref="Size"/> representing the width and height of the ImageButton.</returns>
		/// <remarks>This method is used to override the SizeThatFitsImage() on wrapper view.</remarks>
		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if (Handler?.PlatformView is not UIButton platformButton)
				return Size.Zero;

			Thickness padding = Padding.IsNaN ? default(Thickness) : Padding;

			CGSize imageSize = platformButton.CurrentImage?.Size ?? CGSize.Empty;

			double contentWidth = imageSize.Width + padding.HorizontalThickness;
			double contentHeight = imageSize.Height + padding.VerticalThickness;

			double constrainedWidth = Math.Min(contentWidth, widthConstraint);
			double constrainedHeight = Math.Min(contentHeight, heightConstraint);

			double widthRatio = constrainedWidth / imageSize.Width;
			double heightRatio = constrainedHeight / imageSize.Height;

			// In cases where the image must fit within its given constraints, 
			// it should be scaled down based on the smallest dimension (scale factor) that allows it to fit.
			if (Aspect == Aspect.AspectFit)
			{
				double scaleFactor = Math.Min(widthRatio, heightRatio);
				return new Size(imageSize.Width * scaleFactor, imageSize.Height * scaleFactor);
			}

			// Cases where AspectMode is ScaleToFill or Center
			return new Size(constrainedWidth, constrainedHeight);
		}

		/// <summary>
		/// Returns the size of the ImageButton as a Size based on the specified bounds.
		/// </summary>
		/// <param name="bounds"></param>
		/// <returns>Returns a <see cref="Size"/> representing the width and height of the ImageButton.</returns>
		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			return bounds.Size;
		}
	}
}

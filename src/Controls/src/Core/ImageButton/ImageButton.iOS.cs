using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
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
			if (Handler?.PlatformView is not UIButton platformButton || platformButton.ImageView is null)
				return Size.Zero;

			CGSize boundsSize = platformButton.ImageView.SizeThatFitsImage(
				new CGSize(widthConstraint, heightConstraint),
				Padding.IsNaN ? null : Padding);

			// If we set the size to zero for measurement, it meets the size constraints as zero falls within the bounds. 
			// so, we need to calculate the size based on the image dimension. if image not set, return zero.
			if (boundsSize.AsSize().IsZero)
			{
				boundsSize = platformButton.SizeThatFits(new CGSize(widthConstraint, heightConstraint));
			}

			return new Size(boundsSize.Width, boundsSize.Height);
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

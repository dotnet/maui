#nullable disable
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
			UIButton platformButton = Handler?.PlatformView as UIButton;
			if (platformButton is null)
				return Size.Zero;

			ImageButton imageButton = this;

			Thickness padding = imageButton.Padding.IsNaN ? default(Thickness) : imageButton.Padding;

			Thickness contentEdgeInsets = new Thickness
			{
				Left = padding.Left,
				Right = padding.Right,
				Top = padding.Top,
				Bottom = padding.Bottom
			};

			CGSize imageSize = platformButton.CurrentImage?.Size ?? CGSize.Empty;

			double contentWidth = imageSize.Width + contentEdgeInsets.Left + contentEdgeInsets.Right;
			double contentHeight = imageSize.Height + contentEdgeInsets.Top + contentEdgeInsets.Bottom;

			double constrainedWidth = Math.Min(contentWidth, widthConstraint);
			double constrainedHeight = Math.Min(contentHeight, heightConstraint);

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

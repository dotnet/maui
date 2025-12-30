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
		/// <summary>
		/// Compute the <see cref="CGRect"/> for the title of the button.
		/// </summary>
		/// <param name="platformButton"></param>
		/// <param name="widthConstraint"></param>
		/// <param name="heightConstraint"></param>
		/// <returns>Returns the <see cref="CGRect"/> that contains the button's title.</returns>
		/// <remarks>The <see cref="NSStringDrawingOptions.UsesDeviceMetrics"/> flag is useful for when there is truncation in the title so we will take the max width of the two bounding rects. The GetBoundingRect method does not always give a great representation of what characters will be visible on the button but will not be needed when the UIButton.Configuration API is implemented. </remarks>
		internal static CGRect GetTitleBoundingRect(this UIButton platformButton, double widthConstraint, double heightConstraint)
		{
			if ((platformButton.CurrentAttributedTitle != null || platformButton.CurrentTitle != null)
				&& widthConstraint > 0 && heightConstraint > 0)
			{
				var title =
					   platformButton.CurrentAttributedTitle ??
					   new NSAttributedString(platformButton.CurrentTitle, new UIStringAttributes { Font = platformButton.TitleLabel.Font });

				// Use the available height when calculating the bounding rect
				var lineHeight = platformButton.TitleLabel.Font.LineHeight;
				var availableHeight = heightConstraint;

				// If the line break mode is one of the truncation modes, limit the height to the line height
				if (platformButton.TitleLabel.LineBreakMode == UILineBreakMode.HeadTruncation ||
					platformButton.TitleLabel.LineBreakMode == UILineBreakMode.MiddleTruncation ||
					platformButton.TitleLabel.LineBreakMode == UILineBreakMode.TailTruncation ||
					platformButton.TitleLabel.LineBreakMode == UILineBreakMode.Clip)
				{
					availableHeight = Math.Min(lineHeight, heightConstraint);
				}

				var availableSize = new CGSize(widthConstraint, availableHeight);

				var boundingRect = title.GetBoundingRect(
					availableSize,
					NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
					null);

				// The width is more usually more accurate when using the device metrics with truncatated text
				var boundingRectWithDeviceMetrics = title.GetBoundingRect(
					availableSize,
					NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesDeviceMetrics,
					null);

				return new CGRect(boundingRectWithDeviceMetrics.Location,
					new CGSize(Math.Ceiling(Math.Max(boundingRect.Width, boundingRectWithDeviceMetrics.Width)),
						Math.Ceiling(Math.Min(availableHeight, boundingRect.Height))));
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

		internal static void UpdateContentEdgeInsets(this UIButton platformButton, Button button, Thickness thickness)
		{
			platformButton.UpdateContentEdgeInsets(thickness);
		}

		public static void UpdateContentLayout(this UIButton platformButton, Button button)
		{
			(button as IView)?.InvalidateMeasure();
		}

		public static void UpdateText(this UIButton platformButton, Button button)
		{
			var text = TextTransformUtilities.GetTransformedText(button.Text, button.TextTransform);
			platformButton.SetTitle(text, UIControlState.Normal);

			// The TitleLabel retains its previous text value even after a new value is assigned. As a result, the label does not display the updated text and reverts to the old value when the button is re-measured
			if (string.IsNullOrEmpty(button.Text))
			{
				platformButton.TitleLabel.Text = string.Empty;
			}

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

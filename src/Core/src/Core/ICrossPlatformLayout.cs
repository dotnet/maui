using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface ICrossPlatformLayout
	{
		/// <summary>
		/// Measures the desired size of the ICrossPlatformLayout within the given constraints.
		/// </summary>
		/// <param name="widthConstraint">The width limit for measuring the ICrossPlatformLayout.</param>
		/// <param name="heightConstraint">The height limit for measuring the ICrossPlatformLayout.</param>
		/// <returns>The desired size of the ILayout.</returns>
#if NETSTANDARD2_0
		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);
#else
		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return this switch
			{
				IContentView contentView => contentView.CrossPlatformMeasure(widthConstraint, heightConstraint),
				ILayout layout => layout.CrossPlatformMeasure(widthConstraint, heightConstraint),
				_ => Size.Zero,
			};
		}
#endif

		/// <summary>
		/// Arranges the children of the ICrossPlatformLayout within the given bounds.
		/// </summary>
		/// <param name="bounds">The bounds in which the ICrossPlatformLayout's children should be arranged.</param>
		/// <returns>The actual size of the arranged ICrossPlatformLayout.</returns>
#if NETSTANDARD2_0
		Size CrossPlatformArrange(Rect bounds);
#else
		Size CrossPlatformArrange(Rect bounds) 
		{
			return this switch
			{
				IContentView contentView => contentView.CrossPlatformArrange(bounds),
				ILayout layout => layout.CrossPlatformArrange(bounds),
				_ => Size.Zero,
			};
		}
#endif
	}
}

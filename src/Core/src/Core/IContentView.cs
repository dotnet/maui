using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// A View that contains another View.
	/// </summary>
	public interface IContentView : IView, IPadding, ICrossPlatformLayout
	{
		/// <summary>
		/// Gets the raw content of this view.
		/// </summary>
		object? Content { get; }

		/// <summary>
		/// Gets the content of this view as it will be rendered in the user interface, including any transformations or applied templates.
		/// </summary>
		IView? PresentedContent { get; }

		/// <summary>
		/// This interface method is provided for backward compatibility with previous versions. 
		/// Implementing classes should implement the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// This interface method is provided for backward compatibility with previous versions. 
		/// Implementing classes should implement the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformArrange(Rect bounds);

#if !NETSTANDARD2_0
		Size ICrossPlatformLayout.CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds) => CrossPlatformArrange(bounds);
		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint) => CrossPlatformMeasure(widthConstraint, heightConstraint);
#endif
	}
}
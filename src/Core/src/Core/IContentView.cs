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

#if NETSTANDARD2_0
		/// <summary>
		/// This interface method is provided as a stub for .NET Standard
		/// </summary>
		new Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// This interface method is provided as a stub for .NET Standard
		/// </summary>
		new Size CrossPlatformArrange(Rect bounds);
#else
		/// <summary>
		/// This implementation is provided as a bridge for previous versions. Implementing classes should implement 
		/// the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return (this as ICrossPlatformLayout).CrossPlatformMeasure(widthConstraint, heightConstraint);
		}

		/// <summary>
		/// This implementation is provided as a bridge for previous versions. Implementing classes should implement 
		/// the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformArrange(Rect bounds)
		{
			return (this as ICrossPlatformLayout).CrossPlatformArrange(bounds);
		}
#endif
	}
}
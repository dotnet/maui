using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// A View that contains another View.
	/// </summary>
	public interface IContentView : IView
	{
		/// <summary>
		/// Gets the content of this view.
		/// </summary>
		object? Content { get; }

		/// <summary>
		/// Gets the content of this view it will be rendered in the user interface.
		/// </summary>
		IView? PresentedContent { get; }

		/// <summary>
		/// The space between the outer edge of the IContentViews's content area and its content.
		/// </summary>
		Thickness Padding { get; }

		// TODO ezhart Document this
		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);
		Size CrossPlatformArrange(Rectangle bounds);
	}
}
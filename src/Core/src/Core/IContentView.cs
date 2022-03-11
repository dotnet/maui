using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// A View that contains another View.
	/// </summary>
	public interface IContentView : IView, IPadding
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
		/// Measures the desired size of the IContentView within the given constraints.
		/// </summary>
		/// <param name="widthConstraint">The width limit for measuring the IContentView.</param>
		/// <param name="heightConstraint">The height limit for measuring the IContentView.</param>
		/// <returns>The desired size of the IContentView.</returns>
		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// Arranges the content of the IContentView within the given bounds.
		/// </summary>
		/// <param name="bounds">The bounds in which the IContentView's content should be arranged.</param>
		/// <returns>The actual size of the arranged IContentView.</returns>
		Size CrossPlatformArrange(Rect bounds);
	}
}
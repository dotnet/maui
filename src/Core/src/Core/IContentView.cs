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
	}
}
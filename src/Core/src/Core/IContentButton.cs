namespace Microsoft.Maui
{
	/// <summary>
	/// A native button which presents a single view instead of text and an image.
	/// </summary>
	/// <remarks>
	/// Content is presentation-only. Interactive descendants are not supported.
	/// Provide an accessible name for the button through its semantics.
	/// </remarks>
	public interface IContentButton : IButton, IContentView
	{
	}
}

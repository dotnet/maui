namespace Microsoft.Maui
{   /// <summary>
	/// Represents a .NET MAUI Page.
	/// </summary>
	public interface IPage : IContentView
	{
		/// <summary>
		/// Get the Background Image of the page.
		/// </summary>
		IImageSource? BackgroundImageSource { get; }
	}
}
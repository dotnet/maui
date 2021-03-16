namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to align Text.
	/// </summary>
	public interface ITextAlignment : IView
	{
		/// <summary>
		/// Gets the horizontal text alignment.
		/// </summary>
		TextAlignment HorizontalTextAlignment { get; }
	}
}
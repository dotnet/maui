namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize Padding.
	/// </summary>
	public interface IPadding
	{
		/// <summary>
		/// The space between the outer edge of the control and its content.
		/// </summary>
		Thickness Padding { get; }
	}
}

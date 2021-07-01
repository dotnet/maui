using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to use a Placeholder.
	/// </summary>
	public interface IPlaceholder
	{
		/// <summary>
		/// Gets the placeholder or hint text.
		/// </summary>
		string Placeholder { get; }

		/// <summary>
		/// Gets the color of the placeholder text.
		/// </summary>
		Color PlaceholderColor { get; }
	}
}
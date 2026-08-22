namespace Microsoft.Maui
{
	/// <summary>
	/// Provides a reusable safe area configuration contract for visual elements.
	/// </summary>
	/// <remarks>
	/// Controls implementations can use the shared safe area bindable property provided by Microsoft.Maui.Controls.
	/// </remarks>
	public interface ISafeAreaElement
	{
		/// <summary>
		/// Gets the safe area behavior for each edge of the element.
		/// </summary>
		SafeAreaEdges SafeAreaEdges { get; }

		/// <summary>
		/// Gets the default value for <see cref="SafeAreaEdges"/>.
		/// </summary>
		/// <returns>The default safe area behavior for the element.</returns>
		SafeAreaEdges GetDefaultSafeAreaEdges();
	}
}
namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the properties for a row in a GridLayout.
	/// </summary>
	public interface IGridRowDefinition
	{
		/// <summary>
		/// Gets the height of the row.
		/// </summary>
		GridLength Height { get; }

		/// <summary>
		/// Gets the minimum height of the row.
		/// </summary>
		double MinHeight { get; }

		/// <summary>
		/// Gets the maximum height of the row.
		/// </summary>
		double MaxHeight { get; }
	}
}
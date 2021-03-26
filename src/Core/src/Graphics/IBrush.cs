namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines objects used to fill the interiors of Views. 
	/// </summary>
	public partial interface IBrush
	{
		/// <summary>
		/// Returns a value that indicates whether the Brush is empty.
		/// </summary>
		bool IsEmpty { get; }
	}
}
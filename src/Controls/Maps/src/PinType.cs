namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Enumeration specifying the various kinds of <see cref="Pin"/>.
	/// </summary>
	public enum PinType
	{
		/// <summary>
		/// A generic pin.
		/// </summary>
		Generic,
		/// <summary>
		/// Pin for a place.
		/// </summary>
		Place,
		/// <summary>
		/// Pin for a saved location.
		/// </summary>
		SavedPin,
		/// <summary>
		/// Pin for a search result.
		/// </summary>
		SearchResult
	}
}
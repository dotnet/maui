namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Specify which visual representation should be shown for a map.
	/// </summary>
	public enum MapType
	{
		/// <summary>
		/// Shows the <see cref="IMap" /> as a schematic overview of all roads, streets, etc.
		/// </summary>
		Street,

		/// <summary>
		/// Shows the <see cref="IMap" /> with satellite imagery.
		/// </summary>
		Satellite,

		/// <summary>
		/// Shows the <see cref="IMap" /> with satellite imagery and a street map overlay.
		/// </summary>
		Hybrid
	}
}
namespace Microsoft.Maui.Media
{
	/// <summary>
	/// Describes the behavior of a device ui when multi-picking files
	/// </summary>
	public enum MultiPickingBehaviour
	{
		/// <summary>
		/// Сan select only one file at a time
		/// </summary>
		AlwaysSingle,
		/// <summary>
		/// Can select limited count of files
		/// </summary>
		Limit,
		/// <summary>
		/// Can select unlimited count of files
		/// </summary>
		UnLimit,
	}
}
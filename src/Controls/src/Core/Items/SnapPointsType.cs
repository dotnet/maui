namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies the snap points behavior when scrolling through items in an items layout.
	/// </summary>
	/// <remarks>
	/// Snap points control how scrolling behaves when the user stops interacting with the view.
	/// They create discrete stopping points, commonly used for paging or carousel-style navigation.
	/// </remarks>
	public enum SnapPointsType
	{
		/// <summary>
		/// No snap points. Scrolling stops wherever the user releases, with standard inertia.
		/// </summary>
		None,
		
		/// <summary>
		/// Scrolling snaps to the nearest snap point, but the user can scroll past multiple snap points in a single gesture.
		/// After scrolling stops, the view snaps to the nearest item.
		/// </summary>
		Mandatory,
		
		/// <summary>
		/// Scrolling snaps to exactly one adjacent snap point per gesture.
		/// The user can only advance or go back by one item at a time, regardless of scroll velocity.
		/// This is commonly used for <see cref="CarouselView"/> to ensure precise item-by-item navigation.
		/// </summary>
		MandatorySingle
	}
}
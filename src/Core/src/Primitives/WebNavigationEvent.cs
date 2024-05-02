namespace Microsoft.Maui
{
	/// <summary>
	/// Contains values that indicate why a navigation event was raised.
	/// </summary>
	public enum WebNavigationEvent
	{
		/// <summary>
		/// Indicates that the navigation resulted from the user going back to a previous page in the navigation history.
		/// </summary>
		Back = 1,
		/// <summary>
		/// Indicates that the navigation resulted from the user going forward to a later page in the navigation history.
		/// </summary>
		Forward = 2,
		/// <summary>
		/// Indicates that the navigation was to a previously unvisited page, according to the navigation history.
		/// </summary>
		NewPage = 3,
		/// <summary>
		/// Indicates that the navigation resulted from a page refresh.
		/// </summary>
		Refresh = 4
	}
}
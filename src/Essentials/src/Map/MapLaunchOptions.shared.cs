namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Launch options for opening the installed map application.
	/// </summary>
	public class MapLaunchOptions
	{
		/// <summary>
		/// Gets or sets the navigation mode to use in the map application.
		/// </summary>
		public NavigationMode NavigationMode { get; set; } = NavigationMode.None;

		/// <summary>
		/// Gets or sets the name of destination to display to user.
		/// </summary>
		public string Name { get; set; } = string.Empty;
	}
}

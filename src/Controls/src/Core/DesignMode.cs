namespace Microsoft.Maui.Controls
{
	/// <summary>Static class that developers can use to determine if the application is running in a previewer.</summary>
	public static class DesignMode
	{
		/// <summary>Indicates whether the application is being run in a previewer.</summary>
		public static bool IsDesignModeEnabled { get; internal set; }
	}
}
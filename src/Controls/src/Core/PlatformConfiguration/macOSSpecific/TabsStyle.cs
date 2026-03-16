namespace Microsoft.Maui.Controls
{
	/// <summary>Enumerates the display styles for tabs in a <see cref="TabbedPage"/> on macOS.</summary>
	public enum TabsStyle
	{
		/// <summary>The default tab display style.</summary>
		Default = 0,
		/// <summary>Tabs are hidden.</summary>
		Hidden = 1,
		/// <summary>Tabs are displayed as icons only.</summary>
		Icons = 2,
		/// <summary>Tabs are displayed only during navigation.</summary>
		OnNavigation = 3,
		/// <summary>Tabs are displayed at the bottom of the page.</summary>
		OnBottom = 4
	}
}

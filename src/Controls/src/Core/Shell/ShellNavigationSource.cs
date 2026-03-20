namespace Microsoft.Maui.Controls
{
	/// <summary>Indicates how Shell navigation was initiated.</summary>
	public enum ShellNavigationSource
	{
		/// <summary>The navigation source is unknown.</summary>
		Unknown = 0,
		/// <summary>Navigation was initiated by pushing a page onto the stack.</summary>
		Push,
		/// <summary>Navigation was initiated by popping a page from the stack.</summary>
		Pop,
		/// <summary>Navigation was initiated by popping to the root page.</summary>
		PopToRoot,
		/// <summary>Navigation was initiated by inserting a page into the stack.</summary>
		Insert,
		/// <summary>Navigation was initiated by removing a page from the stack.</summary>
		Remove,
		/// <summary>Navigation was initiated by changing the active <see cref="ShellItem"/>.</summary>
		ShellItemChanged,
		/// <summary>Navigation was initiated by changing the active <see cref="ShellSection"/>.</summary>
		ShellSectionChanged,
		/// <summary>Navigation was initiated by changing the active <see cref="ShellContent"/>.</summary>
		ShellContentChanged,
	}
}
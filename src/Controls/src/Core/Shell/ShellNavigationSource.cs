using System;

namespace Microsoft.Maui.Controls
{
	public enum ShellNavigationSource
	{
		Unknown = 0,
		Push,
		Pop,
		PopToRoot,
		Insert,
		Remove,
		ShellItemChanged,
		ShellSectionChanged,
		ShellContentChanged,
	}
}
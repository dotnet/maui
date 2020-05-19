using System;

namespace System.Maui
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
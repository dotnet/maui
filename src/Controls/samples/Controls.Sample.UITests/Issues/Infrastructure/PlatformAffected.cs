using System;

namespace Maui.Controls.Sample
{
	[Flags]
	public enum PlatformAffected
	{
		iOS = 1 << 0,
		Android = 1 << 1,
		WinPhone = 1 << 2,
		WinRT = 1 << 3,
		UWP = 1 << 4,
		WPF = 1 << 5,
		macOS = 1 << 6,
		Gtk = 1 << 7,
		All = ~0,
		Default = 0
	}
}
using System;
using Microsoft.Maui.Handlers;
using static Microsoft.Maui.Controls.Button;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		[MissingMapper]
		public static void UpdateContentLayout(this Gtk.Button nativeButton, Button button)
		{
		}
	}
}
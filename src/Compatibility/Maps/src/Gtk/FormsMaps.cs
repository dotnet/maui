using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.Maps.Gtk;

namespace Microsoft.Maui.Controls
{
	public static class FormsMaps
	{
		public static bool IsInitialized { get; private set; }

		public static void Init()
		{
			if (IsInitialized)
				return;
			IsInitialized = true;
		}
	}
}
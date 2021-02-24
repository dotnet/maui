using System;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public interface INavigationDrawer
	{
		event EventHandler Toggled;

		EvasObject TargetView { get; }

		EvasObject NavigationView { get; set; }

		EvasObject Main { get; set; }

		bool IsOpen { get; set; }
	}
}

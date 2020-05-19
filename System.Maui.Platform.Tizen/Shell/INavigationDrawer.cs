using System;
using ElmSharp;

namespace System.Maui.Platform.Tizen
{
	public interface INavigationDrawer
	{
		event EventHandler Toggled;

		EvasObject NavigationView { get; set; }

		EvasObject Main { get; set; }

		bool IsOpen { get; set; }
	}
}

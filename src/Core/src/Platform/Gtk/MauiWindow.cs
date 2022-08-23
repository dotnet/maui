using System;
using Gtk;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Platform
{

	[Obsolete("use MauiGtkApplication")]
	public class MauiWindow : Window
	{

		public MauiWindow() : base(WindowType.Toplevel)
		{ }

	}

}
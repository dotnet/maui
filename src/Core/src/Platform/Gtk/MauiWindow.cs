using System;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Native
{

	[Obsolete("use MauiGtkApplication")]
	public class MauiWindow : Window
	{

		public MauiWindow() : base(WindowType.Toplevel)
		{ }

	}

}
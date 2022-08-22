using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{

	public partial class MauiContext_ : IMauiContext
	{

		public Gtk.Window? Window { get; internal set; }

	}

}
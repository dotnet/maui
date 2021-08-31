#define UseSimpleSample

using System;
using System.Threading.Tasks;
using GLib;
using Maui.SimpleSampleApp;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Controls.Sample.Gtk
{

	class Program
	{

		static void Main(string[] args)
		{

			var app =
#if UseSimpleSample
				new SimpleSampleGtkApplication();
#else
				new MauiGtkApplication<Maui.Controls.Sample.Startup>();
#endif
			app.Run();

		}

	}

}
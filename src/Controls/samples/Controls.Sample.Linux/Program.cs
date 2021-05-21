using System;
using System.Threading.Tasks;
using GLib;
using Maui.SimpleSampleApp;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Controls.Sample.Linux
{

	class Program
	{

		static void Main(string[] args)
		{

			var app = new SimpleSampleGtkApplication();
			app.Run();

		}

	}

}
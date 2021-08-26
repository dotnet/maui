using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

namespace MauiApp1
{
	class Program : MauiApplication<Startup>
	{
		protected override void OnCreate()
		{
			base.OnCreate();
		}

		static void Main(string[] args)
		{
			var app = new Program();
			app.Run(args);
		}
	}
}

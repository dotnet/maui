using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample.Tizen
{
	class Program : MauiApplication<Startup>
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			//Microsoft.Maui.Controls.Essentials.Platform.Init(this);
		}

		static void Main(string[] args)
		{
			var app = new Program();
			app.Run(args);
		}
	}
}

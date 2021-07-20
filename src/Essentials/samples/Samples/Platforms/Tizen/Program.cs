using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Tizen.NET.MaterialComponents;
using Xamarin.Forms.Platform.Tizen;

namespace Samples.Tizen
{
	class Program : MauiApplication<Startup>
	{
		protected override void OnCreate()
		{
			base.OnCreate();

			MaterialComponents.Init(DirectoryInfo.Resource);
			Microsoft.Maui.Essentials.Platform.Init(CoreUIAppContext.GetInstance(this).MainWindow);
		}

		static void Main(string[] args)
		{
			var app = new Program();
			app.Run(args);
		}
	}
}

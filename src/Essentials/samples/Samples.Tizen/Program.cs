using Tizen.NET.MaterialComponents;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xamarin.Forms.Platform.Tizen;

namespace Samples.Tizen
{
	class Program : FormsApplication
	{
		static App formsApp;

		protected override void OnCreate()
		{
			base.OnCreate();

			MaterialComponents.Init(DirectoryInfo.Resource);
			Microsoft.Maui.Essentials.Platform.Init(MainWindow);
			LoadApplication(formsApp ??= new App());
		}

		static void Main(string[] args)
		{
			var app = new Program();
			Forms.Init(app);
			Microsoft.Maui.Essentials.Platform.MapServiceToken = "MAP_SERVICE_KEY";
			app.Run(args);
		}
	}
}

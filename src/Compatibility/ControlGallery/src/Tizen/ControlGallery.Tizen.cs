using ElmSharp;
using Tizen.Applications;
using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
{
	class MainApplication : MauiApplication
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

		protected override void OnCreate()
		{
			base.OnCreate();
			MaterialComponents.Init(DirectoryInfo.Resource);
		}

		static void Main(string[] args)
		{
			var app = new MainApplication();
			FormsMaps.Init("HERE", "write-your-API-key-here");
			//FormsMaterial.Init();
			app.Run(args);
		}
	}
}

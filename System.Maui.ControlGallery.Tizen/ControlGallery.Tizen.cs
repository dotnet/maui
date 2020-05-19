using System.Maui;
using System.Maui.Platform.Tizen;
using System.Maui.Controls;
using ElmSharp;
using Tizen.Applications;
using Tizen.NET.MaterialComponents;

namespace System.Maui.ControlGallery.Tizen
{
	class MainApplication : FormsApplication
	{
		internal static EvasObject NativeParent { get; private set; }
		protected override void OnCreate()
		{
			base.OnCreate();
			MaterialComponents.Init(DirectoryInfo.Resource);
			NativeParent = MainWindow;
			LoadApplication(new App());
		}

		static void Main(string[] args)
		{
			var app = new MainApplication();
			FormsMaps.Init("HERE", "write-your-API-key-here");
			System.Maui.Maui.SetFlags("CollectionView_Experimental", "Shell_Experimental", "MediaElement_Experimental", "IndicatorView_Experimental");
			System.Maui.Maui.Init(app);
			FormsMaterial.Init();
			app.Run(args);
		}
	}
}

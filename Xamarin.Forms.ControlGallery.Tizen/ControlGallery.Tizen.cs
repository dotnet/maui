using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Controls;
using ElmSharp;
using Tizen.Applications;
using Tizen.NET.MaterialComponents;

namespace Xamarin.Forms.ControlGallery.Tizen
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
			Forms.SetFlags("CollectionView_Experimental", "Shell_Experimental", "MediaElement_Experimental");
			Forms.Init(app);
			FormsMaterial.Init();
			app.Run(args);
		}
	}
}

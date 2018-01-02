using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Controls;

namespace Xamarin.Forms.ControlGallery.Tizen
{
	class Program : FormsApplication
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			LoadApplication(new App());
		}

		static void Main(string[] args)
		{
			var app = new Program();
			FormsMaps.Init("HERE", "write-your-API-key-here");
			global::Xamarin.Forms.Platform.Tizen.Forms.Init(app);
			app.Run(args);
		}
	}
}

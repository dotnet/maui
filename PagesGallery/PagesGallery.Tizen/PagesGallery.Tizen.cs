using System.Maui;
using System.Maui.Platform.Tizen;

namespace PagesGallery.Tizen
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
			System.Maui.Maui.Init(app);
			app.Run(args);
		}
	}
}

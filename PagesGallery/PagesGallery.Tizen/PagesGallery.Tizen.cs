using Xamarin.Forms.Platform.Tizen;

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
			global::Xamarin.Forms.Platform.Tizen.Forms.Init(app);
			app.Run(args);
		}
	}
}
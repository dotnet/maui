
using System.Maui;


[assembly: ExportFont("CuteFont-Regular.ttf", Alias = "Foo")]
[assembly: ExportFont("PTM55FT.ttf")]
[assembly: ExportFont("Dokdo-Regular.ttf")]
[assembly: ExportFont("fa-regular-400.ttf")]
//[assembly: ExportFont("CuteFont-Regular.ttf",
//	EmbeddedFontResourceId = "PagesGallery.Resources.CuteFont-Regular.ttf")]
//[assembly: ExportFont("CuteFont-Regular.ttf",
//	EmbeddedFontResourceId = "PagesGallery.Resources.CuteFont-Regular.ttf",
//	Alias ="Bar")]

namespace PagesGallery
{

	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			var eventsPage = new EventsPage();
			var speakersPage = new SpeakersPage();
			MainPage = new NavigationPage(new TabbedPage
			{
				Title = "Xamarin Evolve 2016",
				Children = {
					eventsPage,
					speakersPage
				}
			});
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}

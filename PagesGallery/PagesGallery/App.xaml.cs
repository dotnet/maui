using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace PagesGallery
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent ();

			var eventsPage = new EventsPage ();
			var speakersPage = new SpeakersPage ();
			MainPage = new NavigationPage (new TabbedPage {
				Title = "Xamarin Evolve 2016",
				Children = {
					eventsPage,
					speakersPage
				}
			});
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;

namespace Microsoft.Maui.Controls.ControlGallery
{
	// Not quite sure how to turn this into a test case, effectively replace the normal Application with this to repro issues reported.
	// Full repro requires assignment to MainPage, hence the issue
	public class NavReproApp : Application
	{
		NavigationPage navPage1 = new NavigationPage();

		public NavReproApp()
		{

			var btn = new Button() { Text = "Start" };

			btn.Clicked += Btn_Clicked;

			navPage1.PushAsync(new ContentPage() { Content = btn });

			MainPage = navPage1;

		}

		async void Btn_Clicked(object sender, EventArgs e)
		{
			await navPage1.PushAsync(new ContentPage() { Content = new Label() { Text = "Page 2" } });
			await navPage1.PushAsync(new ContentPage() { Content = new Label() { Text = "Page 3" } });


			var navPage2 = new NavigationPage();

			var btn = new Button() { Text = "Start Next" };
			btn.Clicked += Btn_Clicked1;

			await navPage2.PushAsync(new ContentPage() { Content = btn });

			MainPage = navPage2;


		}

		async void Btn_Clicked1(object sender, EventArgs e)
		{
			MainPage = navPage1;
			await navPage1.PopAsync();


			await navPage1.PushAsync(new ContentPage() { Content = new Label() { Text = "Page 3a" } });
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
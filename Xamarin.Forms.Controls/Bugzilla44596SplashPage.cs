using System;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class Bugzilla44596SplashPage : ContentPage
	{
		Action FinishedLoading { get; set; }


		public Bugzilla44596SplashPage(Action finishedLoading)
		{
			BackgroundColor = Color.Blue;
			FinishedLoading = finishedLoading;
		}


		protected async override void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(500);
			FinishedLoading?.Invoke();
		}
	}
}

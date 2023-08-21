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
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class Bugzilla44596SplashPage : ContentPage
	{
		Action FinishedLoading { get; set; }


		public Bugzilla44596SplashPage(Action finishedLoading)
		{
			BackgroundColor = Colors.Blue;
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

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
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public partial class MyAbout : ContentPage
	{
		public MyAbout()
		{
			InitializeComponent();

			twitter.GestureRecognizers.Add(new TapGestureRecognizer()
			{
				Command = new Command(async () =>
				{

					await this.Navigation.PushAsync(new WebsiteView("https://m.twitter.com/shanselman", "@shanselman"));
				})
			});

			facebook.GestureRecognizers.Add(new TapGestureRecognizer()
			{
				Command = new Command(async () =>
				{

					await this.Navigation.PushAsync(new WebsiteView("https://facebook.com/scott.hanselman", "Scott @Facebook"));
				})
			});

			instagram.GestureRecognizers.Add(new TapGestureRecognizer()
			{
				Command = new Command(async () =>
				{

					await this.Navigation.PushAsync(new WebsiteView("https://instagram.com/shanselman", "Scott @Instagram"));
				})
			});
		}
	}
}
